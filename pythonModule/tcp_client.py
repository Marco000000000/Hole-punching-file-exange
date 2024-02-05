#!/usr/bin/env python
import json
import os
import sys
import logging
import socket
import struct
from threading import Event, Thread
import threading
import time
from util import *
import requests
import base64
from flask import Flask, request,jsonify
app = Flask(__name__)
if socket.SO_REUSEPORT is None:
    socket.SO_REUSEPORT = socket.SO_REUSEADDR
DOWNLOADPATH=os.getenv("downloadPath","download")
DOWNLOADDIRECTORY=os.getenv("downloadDirectory","downloadDirectory")
TURNSERVER="37.102.123.139"
if not os.path.exists(DOWNLOADDIRECTORY):
        # Create the directory
        os.makedirs(DOWNLOADDIRECTORY)
logger = logging.getLogger('client')
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(message)s')

class connector:
    def __init__(self,user, code, role):#separare in classe Client e classe server
        self.user = user
        self.code = code
        self.role = role
        self.permission=False
        self.__mutex__=threading.Lock()
        self.holeCreated=False
        self.maxTimeForHole=5
        self.HoleFailed=False
        self.STOP = Event()
        self.STOP.clear()
        self.operation=0
        self.path=None

    def __create_hole__(self,host=TURNSERVER, port=5000):
        if self.holeCreated:
            return True
        elif self.HoleFailed:
            return False
        self.__getPermission__()
        sa = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sa.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    
        print(port)
        sa.connect((host, port))
        priv_addr = sa.getsockname()
        
        send_msg(sa, addr_to_msg(priv_addr))
        data = recv_msg(sa)
        print(data.decode("utf-8"))
        logger.info("client %s %s - received data: %s", priv_addr[0], priv_addr[1], data)
        pub_addr = msg_to_addr(data)
        send_msg(sa, addr_to_msg(pub_addr))

        data = recv_msg(sa)
        print(data)

        pubdata, privdata = data.split(b'|')
        client_pub_addr = msg_to_addr(pubdata)
        client_priv_addr = msg_to_addr(privdata)
        logger.info(
            "client public is %s and private is %s, peer public is %s private is %s",
            pub_addr, priv_addr, client_pub_addr, client_priv_addr,
        )

        threads = {
            '0_accept': Thread(target=self.__accept__, args=(priv_addr[1],)),
            '1_accept': Thread(target=self.__accept__, args=(pub_addr[1],)),
            '2_connect': Thread(target=self.__connect__, args=(priv_addr, client_pub_addr,)),
            '3_connect': Thread(target=self.__connect__, args=(priv_addr, client_priv_addr,)),
        }
        for name in sorted(threads.keys()):
            logger.info('start thread %s', name)
            threads[name].start()
        timer=time.time()
        while threads:
            keys = list(threads.keys())
            for name in keys:
                try:
                    threads[name].join(1)
                except TimeoutError:
                    continue
                if not threads[name].is_alive():
                    threads.pop(name)
                if self.holeCreated:
                    return True
                
        return False
    def __accept__(self,port):
        logger.info("accept %s", port)
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEPORT, 1)
        s.bind(('', port))
        s.listen(1)
        s.settimeout(5)
        temp_timer=time.time()
        while not self.STOP.is_set():
            
            try:
                conn, addr = s.accept()
                logger.info("Accept %s connected!", addr)
                self.__makeThing__(conn,"accept",self.role)
            except socket.timeout:
                if self.holeCreated:
                    return True
                else:
                    delta_time=time.time()-temp_timer
                    if delta_time>self.maxTimeForHole:
                        return False
                    continue

    def __connect__(self,local_addr, addr):
        logger.info("connect from %s to %s", local_addr, addr)
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEPORT, 1)
        s.bind(local_addr)
        temp_timer=time.time()

        while not self.STOP.is_set():
            try:
                s.connect(addr)
                logger.info("connected from %s to %s success!", local_addr, addr)

                self.__makeThing__(s,"connect",self.role)
                self.STOP.set()
            except socket.error:
                if self.holeCreated:
                    return True
                else:
                    delta_time=time.time()-temp_timer
                    if delta_time>self.maxTimeForHole:
                        return False
                    continue
        else:
            logger.info("connected from %s to %s success!", local_addr, addr)
    def get_all_files_in_directory(directory_path):
        file_names = {}
    
    # List all files in the directory
        with os.scandir(directory_path) as entries:
            for entry in entries:
                if entry.is_file():
                    file_names[entry.name]="file"#fare una lista
                else:
                    file_names[entry.name]="directory"
                    
        return file_names
    
    def __handleFirstMessage__(self,s,role):
        if role == "server":
            send_msg(s,json.dumps(self.get_all_files_in_directory(DOWNLOADPATH)).encode("utf-8"))
            return "Sent first message"
        else:
            return(json.loads(recv_msg(s).decode("utf-8")))
    def __newClientOperation__(self,operation,path):
        if operation>0 and operation<4:
            self.operation=operation
            self.path=path
    def __handleFileDownload__(self,s,path,role,subPath=""):
        if role == "server":
            with open(path, 'rb') as file:
                file_data = file.read()
                temp={}
                temp["chunk"]=str(base64.b64encode(file_data),"UTF-8")
                temp["last"]=file.tell()>0

                print(temp)

                send_msg(s,json.dumps(temp).encode("utf-8"))
            
            return "Sent File"
        else:
            send_msg(s,(path+"?file").encode("utf-8"))
            print(os.path.join( DOWNLOADDIRECTORY,subPath,path.split(os.path.sep)[-1]))

            with open(os.path.join( DOWNLOADDIRECTORY,subPath,path.split(os.path.sep)[-1]), 'wb') as received_file:
                file_data=b''
                cond=True
                while(cond):
                    a=recv_msg(s)
                    print(a)
                    temp=json.loads(a.decode("utf-8"))
                    print(temp)

                    cond=not(temp['last'])
                    file_data=file_data+base64.b64decode(temp["chunk"])
                    received_file.write(file_data)
                    
            return "received File"
        

    def __handleSincronizeDownload__(self,s,path,role,subPath=""):
            files=self.__handleDirectoryFiles__(s,path,role)
            for file in files:
                print("file:"+file)
                print("is:"+files[file])
                requestPath=os.path.join(path,file)
                print("requestPath:"+requestPath)
                print("subPath:"+subPath)
                if files[file]=="file":
                    print(self.__handleFileDownload__(s,requestPath,role,subPath))
                else:
                    os.makedirs(os.path.join(DOWNLOADDIRECTORY,subPath,file),exist_ok=True)
                    print("Create directory:"+os.path.join(DOWNLOADDIRECTORY,subPath,file))
                    s=self.__handleSincronizeDownload__(s,requestPath,role,os.path.join(subPath,file))
            return s
        
    def __handleDirectoryFiles__(self,s,path,role):
        if role == "server":
            send_msg(s,json.dumps(self.get_all_files_in_directory(path)).encode("utf-8"))
            return "Sent DirectoryFiles"
        else:
            send_msg(s,(path+"?directory").encode("utf-8"))
            return(json.loads(recv_msg(s).decode("utf-8")))

    def __handleHearthBit__(self,s,role):
        if role == "server":
            send_msg(s,b'')
            return "Sent HeartBit"
        else:
            send_msg(s,("/"+"?heartBit").encode("utf-8"))
            return recv_msg(s)    

    def __makeThing__(self,s,type,role):
        self.__mutex__.acquire()
        try:
            self.holeCreated=True
        finally:
            self.__mutex__.release()
        while True:
            if type=="connect":
                while True:
                    try:
                        # path="download"
                        # send_msg(s,"salve".encode("utf-8"))
                        # logger.info("inviato: %s","salve")
                        print(self.__handleFirstMessage__(s,role))
                        if self.operation>0:
                            if self.operation==1:
                                print(self.__handleFileDownload__(s,self.path,"client"))
                                self.operation=0
                                
                            elif self.operation == 2:
                                print(self.__handleSincronizeDownload__(s,self.path,"client"))
                                self.operation=0
                                
                            elif self.operation == 3:
                                print(self.__handleDirectoryFiles__(s,self.path,"client"))
                                self.operation=0
                        else:
                            self.__handleHearthBit__(s,"client")     
                    except:
                        self.__closeHole__()
                        return

                    time.sleep(1)
                    
            else:
                while True:
                    try:
                        msg=recv_msg(s)
                        msg=msg.decode("utf-8")
                        logger.info("ricevuto: %s",msg)
                        if msg=="salve":
                            print(self.__handleFirstMessage__(s,role)) 
                        else:
                            msg=msg.split("?")
                            print(msg)
                            if len(msg)!=2:
                                continue
                            elif msg[1]=="file":
                                print(self.__handleFileDownload__(s,msg[0],"server"))
                            elif msg[1]=="directory":
                                self.__handleDirectoryFiles__(s,msg[0],"server")
                            elif msg[1]=="heartBit":
                                self.__handleHearthBit__(s,"server")
                    except:
                        self.__closeHole__()
                        return
                
    def __closeHole__(self):
        self.__mutex__.acquire()
        try:
            self.holeCreated=False
        finally:
            self.__mutex__.release()
            
    def __handleHttpHearthBit__(self):
        response=requests.get("http://"+TURNSERVER+"/hearthBit/"+self.code)
        data=response.json()
        if "operation" in data:
            self._handleTurnOperation_(data["path"],data["operation"],data["code"])

    def _handleTurnOperation_(self,path,operation,code):
        if operation==1: 
            files = {path.split("/")[-1]: open(path, 'rb')}
            requests.post("http://"+TURNSERVER+"/response/"+code, files=files)
        elif operation==3:
            requests.post("http://"+TURNSERVER+"/response/"+code, files=json.dumps(self.get_all_files_in_directory(path)))
        else:
            requests.post("http://"+TURNSERVER+"/response/"+code, files=json.dumps({"error":"Bad Operation"}))

            

    def __getPermission__(self):
        httpPort="80"
        print("http://"+TURNSERVER+":"+httpPort+"/holePunch/"+self.code)
        firstCall=requests.get("http://"+TURNSERVER+":"+httpPort+"/holePunch/"+self.code)
        return firstCall.json
    

    def __turnDownload__(self,data,subPath=""):
        try:
            response= requests.post( "http://"+TURNSERVER+"/request/", files=json.dumps(data))
            with open(os.path.join( DOWNLOADDIRECTORY,subPath,data["path"].split(os.path.sep)[-1]), 'wb') as received_file:
                received_file.write(response.content)
            return True
        except:
            return False

    def __turnFilenames__(self,data):
        response= requests.post( "http://"+TURNSERVER+"/request/", files=json.dumps(data))
        return json.loads(response.content.decode("utf-8"))

    def __turnSincronizeDirectory__(self,data,subPath=""):
            files=self.__turnFilenames__(data)
            for file in files:
                print("file:"+file)
                print("is:"+files[file])
                requestPath=os.path.join(data["path"],file)
                print("requestPath:"+requestPath)
                print("subPath:"+subPath)
                if files[file]=="file":
                    print(self.__turnDownload__(data,subPath))
                else:
                    os.makedirs(os.path.join(DOWNLOADDIRECTORY,subPath,file),exist_ok=True)
                    print("Create directory:"+os.path.join(DOWNLOADDIRECTORY,subPath,file))
                    self.__turnSincronizeDirectory__(data,os.path.join(subPath,file))
            return True

    def turnOperation(self,user,code,peer_username,peer_code,operation,path):
        data={
                "username":user,
                "code":code,
                "peer_username":peer_username,
                "peer_code":peer_code,
                "operation":operation,
                "path":path
            }
        if operation==1:
            return self.__turnDownload__(data)
        elif operation==2:
            return self.__turnSincronizeDirectory__(data)
        else:
            return self.__turnFilenames__(data)
    def handleOperation(self,peer_username,peer_code,path,operation):
        if self.holeCreated:
            self.__newClientOperation__(operation,path)
        else:
            if self.__create_hole__(peer_username,peer_code):
                self.__newClientOperation__(operation,path)
                return True
            else:
                return self.turnOperation(self.user,self.code,peer_username,peer_code,operation,path)


@app.route('/', methods=['GET'])
def handleMessage():
    if request.remote_addr != '127.0.0.1':
        return "Forbidden: Only localhost connections are allowed.", 403

    
    query_type = request.args.get('query')
    user=request.args.get('user')
    code=request.args.get('code')
    path=request.path
    
    global clientConnector
    global serverConnector
    if query_type=="download":
        if clientConnector is None:
            clientConnector=connector(user,code,"client")
        peer_username=request.args.get('peer_username')
        peer_code=request.args.get('peer_code','')
        if "." in path.split("/")[-1]:
            clientConnector.handleOperation(peer_username,peer_code,path,1)
        else:
            clientConnector.handleOperation(peer_username,peer_code,path,2)
    elif query_type=="names":
        if clientConnector is None:
            clientConnector=connector(user,code,"client")
        peer_username=request.args.get('peer_username')
        peer_code=request.args.get('peer_code','')
        clientConnector.handleOperation(peer_username,peer_code,path,3)
    elif query_type=="start_share":
        if serverConnector is None:
            serverConnector=connector(user,code,"server")
    else:
        return jsonify({"error":"bad query"}),400

clientConnector=None
serverConnector=None

if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO, message='%(asctime)s %(message)s')
    
    hostname = socket.gethostname()

    app.run(debug=False,host=socket.gethostbyname_ex(hostname)[2][0],port=80,threaded=True)
