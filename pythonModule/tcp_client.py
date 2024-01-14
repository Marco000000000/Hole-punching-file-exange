#!/usr/bin/env python
import json
import os
import sys
import logging
import socket
import struct
from threading import Event, Thread
import time
from util import *
import requests
import base64
socket.SO_REUSEPORT = socket.SO_REUSEADDR
DOWNLOADPATH=os.getenv("downloadPath","download")
DOWNLOADDIRECTORY=os.getenv("downloadDirectory","downloadDirectory")
if not os.path.exists(DOWNLOADDIRECTORY):
        # Create the directory
        os.makedirs(DOWNLOADDIRECTORY)
logger = logging.getLogger('client')
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(message)s')
STOP = Event()
STOP.clear()

def accept(port):
    logger.info("accept %s", port)
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEPORT, 1)
    s.bind(('', port))
    s.listen(1)
    s.settimeout(5)
    while not STOP.is_set():
        try:
            conn, addr = s.accept()
            logger.info("Accept %s connected!", addr)
            makeThing(conn,"accept","server")
        except socket.timeout:
            continue
def get_all_files_in_directory(directory_path):
    file_names = {}
    
    # List all files in the directory
    with os.scandir(directory_path) as entries:
        for entry in entries:
            if entry.is_file():
                file_names[entry.name]="file"
            else:
                file_names[entry.name]="directory"
    
    return file_names
def handleFirstMessage(s,role):#penso si possa fare una classe
    if role == "server":
        send_msg(s,json.dumps(get_all_files_in_directory(DOWNLOADPATH)).encode("utf-8"))
        return "Sent first message"
    else:
        return(json.loads(recv_msg(s).decode("utf-8")))
        
def handleFileDownload(s,path,role):
    if role == "server":
        with open(path, 'rb') as file:
            file_data = file.read()
            temp={}
            temp["chunk"]=str(base64.b64encode(file_data),"UTF-8")
            temp["last"]=file.tell()>0

            print(temp)

            send_msg(s,json.dumps(temp).encode("utf-8"))
            s.sendall(file_data)
           
        return "Sent File"
    else:
        send_msg(s,(path+"?file").encode("utf-8"))
        print(os.path.join( DOWNLOADDIRECTORY,path.split(os.path.sep)[-1]))

        with open(os.path.join( DOWNLOADDIRECTORY,path.split(os.path.sep)[-1]), 'wb') as received_file:
            file_data=b''
            cond=True
            while(cond):
                temp=json.loads(recv_msg(s).decode("utf-8"))
                print(temp)

                cond=not(temp['last'])
                file_data=file_data+base64.b64decode(temp["chunk"])
                received_file.write(file_data)
                
        return "received File"
    

def handleArchiveDownload(s,path,role):
    if role == "server":
        send_msg(s,json.dumps(get_all_files_in_directory(path)).encode("utf-8"))
        return "Sent Archive"
    else:
        send_msg(s,(path+"?directory").encode("utf-8"))
        return(json.loads(recv_msg(s).decode("utf-8")))
    
def handleDirectoryFiles(s,path,role):
    if role == "server":
        send_msg(s,json.dumps(get_all_files_in_directory(path)).encode("utf-8"))
        return "Sent DirectoryFiles"
    else:
        send_msg(s,(path+"?directory").encode("utf-8"))
        return(json.loads(recv_msg(s).decode("utf-8")))
    

def makeThing(s,type,role):
    if type=="connect":
        while True:
            variable=1
            path="download"+os.path.sep+"tcp_server.py"
            send_msg(s,"salve".encode("utf-8"))
            logger.info("inviato: %s","salve")
            print(handleFirstMessage(s,role))
            if variable>0:
                if variable==1:
                    print(handleFileDownload(s,path,"client"))
                    return
                elif variable == 2:
                    return
                elif variable == 3:
                    print(handleDirectoryFiles(s,path,"client"))
                    return
            time.sleep(5)
    else:
        while True:
                msg=recv_msg(s)
                msg=msg.decode("utf-8")
                logger.info("ricevuto: %s",msg)
                if msg=="salve":
                   print(handleFirstMessage(s,role)) 
                else:
                    msg=msg.split("?")
                    print(msg)
                    if len(msg)!=2:
                        continue
                    elif msg[1]=="file":
                        print(handleFileDownload(s,msg[0],"server"))
                        pass
                    elif msg[1]=="archive":
                        handleArchiveDownload(s,msg[0])
                        pass
                    elif msg[1]=="directory":
                        handleDirectoryFiles(s,msg[0],"server")
                        pass
def connect(local_addr, addr):
    logger.info("connect from %s to %s", local_addr, addr)
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEPORT, 1)
    s.bind(local_addr)
    while not STOP.is_set():
        try:
            s.connect(addr)
            logger.info("connected from %s to %s success!", local_addr, addr)

            makeThing(s,"connect","client")
            STOP.set()
        except socket.error:
            continue
        # except Exception as exc:
        #     logger.exception("unexpected exception encountered")
        #     break
    else:
        logger.info("connected from %s to %s success!", local_addr, addr)
        


def main(host="127.0.0.1", port=5000):
    sa = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sa.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    httpPort="80"
    print("http://"+host+":"+httpPort+"/holePunch/1")
    firstCall=requests.get("http://"+host+":"+httpPort+"/holePunch/1")
    print(firstCall.text)
    print(port)
    sa.connect(("127.0.0.1", port))
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
        '0_accept': Thread(target=accept, args=(priv_addr[1],)),
        '1_accept': Thread(target=accept, args=(pub_addr[1],)),
        '2_connect': Thread(target=connect, args=(priv_addr, client_pub_addr,)),
        '3_connect': Thread(target=connect, args=(priv_addr, client_priv_addr,)),
    }
    for name in sorted(threads.keys()):
        logger.info('start thread %s', name)
        threads[name].start()

    while threads:
        keys = list(threads.keys())
        for name in keys:
            try:
                threads[name].join(1)
            except TimeoutError:
                continue
            if not threads[name].is_alive():
                threads.pop(name)



if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO, message='%(asctime)s %(message)s')
    main()
