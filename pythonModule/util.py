#!/usr/bin/env python
from collections import namedtuple
import json
import os
import logging
import socket
import struct
from threading import Event, Thread
import threading
import time
from util import *
import requests
import base64
logger = logging.getLogger('client')
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(message)s')

DOWNLOADDIRECTORY=os.getenv("downloadDirectory","../downloadDirectory")
TURNSERVER=os.getenv("turnServer","192.168.1.64") #server di gestione delle richieste di controllo e scambio messaggi tramite server
#funzione ausiliare per vedere se si sta richiedendo un path valido
def stringInsideAList(path,paths):
    for temp in paths:
        if temp in path:
            return True
    return False

def addr_from_args(args, host='127.0.0.1', port=9999):
    if len(args) >= 3:
        host, port = args[1], int(args[2])
    elif len(args) == 2:
        host, port = host, int(args[1])
    else:
        host, port = host, port
    return host, port

def keep_numbers_and_dot(input_str):
    result = ''.join(char for char in input_str if char.isdigit() or char == '.')

    return result

def msg_to_addr(data):
    ip, port = data.decode('utf-8').strip().split(':')
    ip=keep_numbers_and_dot(ip)
    port=keep_numbers_and_dot(port)
    return (ip, int(port))


def addr_to_msg(addr):
    return '{}:{}'.format(addr[0], str(addr[1])).encode('utf-8')


def send_msg(sock, msg):
    # Prefix each message with a 4-byte length (network byte order)
    msg = struct.pack('>I', len(msg)) + msg
    sock.sendall(msg)


def recvall(sock, n):
    # Helper function to recv n bytes or return None if EOF is hit
    data = b''
    logging.info("60-"+str(sock))
    sock.settimeout(5)
    logging.info("62-sock.settimeout(5)")

    while len(data) < n:
        logging.info("65-sock.recv(%s - len(data))",str(n))

        packet = sock.recv(n - len(data))
        if not packet:
            logging.info("69-return None")
            return None
        data += packet
    return data


def recv_msg(sock):
    # Read message length and unpack it into an integer
    logging.info("71-raw_msglen = recvall(sock, 4)")

    raw_msglen = recvall(sock, 4)
    logging.info("80-if not raw_msglen")

    if not raw_msglen:
        logging.info("83-return None")

        return None

    msglen = struct.unpack('>I', raw_msglen)[0]
    logging.info("msg Len=%s",str(msglen))

    # Read the message data
    return recvall(sock, msglen)


class Client(namedtuple('Client', 'conn, pub, priv')):

    def peer_msg(self):
        return addr_to_msg(self.pub) + b'|' + addr_to_msg(self.priv)

# class connector:
#     #classe creata allo scopo di incampsulare le operazioni del nostro "protocollo"
#     def __init__(self,user, code, role):#separare in classe Client e classe server
#         self.user = user
#         self.code = code
#         self.role = role
#         #client
#         self.operation=0
#         self.ans=""
#         self.ansReady=False
#         self.__mutex__=threading.Lock()
#         self.holeCreated=False
#         self.requestWithOutHole=0
        
#         self.path=None
#         self.paths=[]

#     #funzione che implementa l'algoritmo di hole punching tcp aggiungendo dei messaggi di controllo
#     #per l'ottenimento dei permessi dal server
#     def __create_hole__(self,host=TURNSERVER, port=5000,peer_username="",peer_code=""):
#         try:
#             if self.holeCreated:
#                 return "True"
#             if self.requestWithOutHole>0:
#                 self.requestWithOutHole-=1
#                 return ["/error"]
#             if self.role=="client":
#                 self.__getPermission__(peer_username,peer_code)
        
#             sa = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
#             sa.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
#             data={
#                     "username":self.user,
#                     "code":self.code,
#                     "peer_username":peer_username,
#                     "peer_code":peer_code,
#                     "operation":4,
#                     "path":""
#                 }
            
#             #print(port)
#             sa.connect((host, port))
#             send_msg(sa,json.dumps(data).encode("utf-8"))

#             priv_addr = sa.getsockname()
            
#             send_msg(sa, addr_to_msg(priv_addr))
#             data = recv_msg(sa)
            
#             logger.info("client %s %s - received data: %s", priv_addr[0], priv_addr[1], data)
#             pub_addr = msg_to_addr(data)
#             logger.info("pub addr")
#             print(pub_addr)
#             send_msg(sa, addr_to_msg(pub_addr))
#             logger.info("data = recv_msg(sa)")
#             data = recv_msg(sa)
#             logger.info("all data")

#             print(data)
#             logger.info("pubdata, privdata = data.split")
#             pubdata, privdata = data.split(b'|')
#             client_pub_addr = msg_to_addr(pubdata)
#             client_priv_addr = msg_to_addr(privdata)
#             logger.info(
#                 "client public is %s and private is %s, peer public is %s private is %s",
#                 pub_addr, priv_addr, client_pub_addr, client_priv_addr,
#             )
#             #lancio quattro thread per sopperire ad ogni possibile caso della connessione
#             threads = {
#                 '0_accept': Thread(target=self.__accept__, args=(priv_addr[1],)),
#                 '1_accept': Thread(target=self.__accept__, args=(pub_addr[1],)),
#                 '2_connect': Thread(target=self.__connect__, args=(priv_addr, client_pub_addr,)),
#                 '3_connect': Thread(target=self.__connect__, args=(priv_addr, client_priv_addr,)),
#             }
#             for name in sorted(threads.keys()):
#                 logger.info('start thread %s', name)
#                 threads[name].start()
#             #aspetto che almeno un thread riesca e in caso contrario ritorno il fallimento
#             while threads:
#                 keys = list(threads.keys())
#                 for name in keys:
#                     try:
#                         threads[name].join(1)
#                     except TimeoutError:
#                         continue
#                     if not threads[name].is_alive():
#                         logger.info("112:threads.pop(name)")

#                         threads.pop(name)
#                     if self.holeCreated:
#                         logger.info("116:self.holeCreated")
#                         return "True"
#             #Operazione utilizzata per non avere continuo ritardo in caso di fallimento
#             self.requestWithOutHole=5
#             return ["/error"]
#         except:
#             self.requestWithOutHole=5
#             return ["/error"]
#     #funzione che implementa l'accettazione  del primo messaggio
#     def __accept__(self,port):
#         try:
#             logger.info("accept %s", port)
#             s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
#             s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
#             s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEPORT, 1)
#             s.bind(('', port))
#             s.listen(1)
#             logger.info("130:s.settimeout(5)")
#             s.settimeout(5)                
#             try:
#                 logger.info("135:conn, addr = s.accept()")
#                 conn, addr = s.accept()
#                 logger.info("Accept %s connected!", addr)
#                 self.__handleHole__(conn,self.role)
#             except socket.timeout:
#                 if self.holeCreated:
#                     logger.info("141:self.holeCreated")
#                     return True
#                 else:
#                     return False
                        
#         except Exception as e:
#             logger.info("funzione"+"__accept__")
#             logger.info(e)
#             return False 
# #funzione che implementa l'invio  del primo messaggio per l'hole punching

#     def __connect__(self,local_addr, addr):
#         try:
#             logger.info("connect from %s to %s", local_addr, addr)
#             s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
#             s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
#             s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEPORT, 1)
#             s.bind(local_addr)
#             try:
#                 s.connect(addr)
#                 logger.info("connected from %s to %s success!", local_addr, addr)

#                 self.__handleHole__(s,self.role)
#             except socket.error:
#                 if self.holeCreated:
#                     logger.info("168:self.holeCreated:")
#                     return True
#                 else:
#                     return False
#             else:
#                 logger.info("connected from %s to %s success!", local_addr, addr)
#         except Exception as e:
#             logger.info("funzione"+"__connect__")
#             logger.info(e)
#             return False

# #funzione ausiliaria che ritorna un dizionario avente come chiavi i nomi dei file in un certo
# #path e come valore se è un file o una cartella
#     def get_all_files_in_directory(self,directory_path):
#         file_names = {}
    
#     # List all files in the directory
#         try:
#             with os.scandir(directory_path) as entries:
#                 for entry in entries:
#                     if entry.is_file():
#                         file_names[os.path.join(directory_path,entry.name)]="file"#fare una lista
#                     else:
#                         file_names[os.path.join(directory_path,entry.name)]="directory"
                        
#             return file_names
#         except:
#             return {}
#     #gestisce l'invio del primo messaggio (serie di path names) o la ricezione degli stessi
#     def __handleFirstMessage__(self,s,role):
#         if role == "server":
#             send_msg(s,json.dumps(self.paths).encode("utf-8"))
#             return "Sent first message"
#         else:
#             temp=recv_msg(s)
#             if temp is None:
#                 return None
#             return(json.loads(temp.decode("utf-8")))
#     #funzione che cambia self.operation in connector per cambiare il comportamento del thread in 
#     # continuo polling durante l'hole punching
#     def __newClientOperation__(self,operation,path):
#         #print("client operation")
#         if operation>0 and operation<4:
#             self.operation=operation
#             self.path=path
#         else:
#             return ["/error"]
#         while not self.ansReady and self.holeCreated:
#             time.sleep(1)
#             print("waiting"+str(self.ansReady))
#         else:
#             ret=self.ans
#             self.ansReady=False
#             return ret
        
#     #gestrione del download di un singolo file
#     def __handleFileDownload__(self,s,path,role,subPath=""):
#         if role == "server":
#             with open(path, 'rb') as file:
#                 while True:
#                     file_data = file.read()
#                     if(len(file_data)==0):
#                         break
#                     temp={}
#                     temp["chunk"]=str(base64.b64encode(file_data),"UTF-8")
#                     temp["last"]=False
#                     print(len(file_data))
#                     #print(temp)

#                     send_msg(s,json.dumps(temp).encode("utf-8"))
#             temp={}
#             temp["last"]="True"
#             send_msg(s,json.dumps(temp).encode("utf-8"))
#             return "SentFile"
#         else:
#             send_msg(s,(path+"?file").encode("utf-8"))
#             #print(os.path.join( DOWNLOADDIRECTORY,subPath,path.split(os.path.sep)[-1]))

#             with open(os.path.join( DOWNLOADDIRECTORY,subPath,path.split(os.path.sep)[-1]), 'wb') as received_file:
#                 file_data=b''
#                 cond=True
#                 while(cond):
#                     a=recv_msg(s)
#                     #print(a)
#                     if a is None:
#                         return "error in file download"
#                     temp=json.loads(a.decode("utf-8"))
#                     print("templen:")
#                     print(len(temp))

#                     if(len(temp)<2):
#                         break
#                     print(cond)
#                     time.sleep(1)
#                     file_data=base64.b64decode(temp["chunk"])
#                     received_file.write(file_data)

#             print("finish download")  
#             return "received File"
        
#     #Sincronizzazione di un'intera cartella richiamando ricorsivamente __handleDirectoryFiles__ e __handleFileDownload__
#     def __handleSincronizeDownload__(self,s,path,role,subPath=""):
#             files=self.__handleDirectoryFiles__(s,path,role)
            
#             if len(path.split("/"))>1:
#                 dirName=path.split("/")[-1]
#             else:
#                 dirName=path.split("\\")[-1]
#             print(os.path.join(DOWNLOADDIRECTORY,subPath,dirName))
#             os.makedirs(os.path.join(DOWNLOADDIRECTORY,subPath,dirName),exist_ok=True)
#             print("Create directory:"+os.path.join(DOWNLOADDIRECTORY,subPath,dirName))
#             subPath=os.path.join(subPath,dirName)
            
#             for file in files:
#                 if file=="error":
#                     return s
#                 print("file:"+file)
#                 print("is:"+files[file])
#                 requestPath=file
#                 print("requestPath:"+requestPath)
#                 print("subPath:"+subPath)
#                 if files[file]=="file":

#                     print(self.__handleFileDownload__(s,requestPath,role,subPath))
#                 else:
#                     #os.makedirs(os.path.join(DOWNLOADDIRECTORY,subPath,file),exist_ok=True)
#                     #print("Create directory:"+os.path.join(DOWNLOADDIRECTORY,subPath,file))
#                     s=self.__handleSincronizeDownload__(s,requestPath,role,subPath)
#             return s
#     #gestisce il ritorno di tutti i file sotto una cartella 
#     def __handleDirectoryFiles__(self,s,path,role):
#         if role == "server":
#             send_msg(s,json.dumps(self.get_all_files_in_directory(path)).encode("utf-8"))
#             return "Sent DirectoryFiles"
#         else:
#             send_msg(s,(path+"?directory").encode("utf-8"))
#             temp=recv_msg(s)
#             if temp is None:
#                 return None
#             return(json.loads(temp.decode("utf-8")))
#     #utilizzo di un messaggio vuoto per mantenere la connessione
#     def __handleHearthBit__(self,s,role):
#         if role == "server":
#             send_msg(s,b'')
#             return "Sent HeartBit"
#         else:
#             send_msg(s,("/"+"?heartBit").encode("utf-8"))
#             return recv_msg(s)    

#     #gestore della connessione dopo aver aperto la connessione con l'hole punching
#     def __handleHole__(self,s,role):
#         self.__mutex__.acquire()
#         try:
#             logger.info("317:self.holeCreated=True")
#             self.holeCreated=True
#         finally:
#             self.__mutex__.release()
#         if role=="client":
#             timer=time.time()
#             while True:
#                 try:
                   
#                     print(self.path)
#                     if time.time()-timer>60:
#                         self.__closeHole__()
#                         s.close()
#                         return
#                     if self.operation>0:
#                         timer=time.time()
#                         if self.operation==1:
#                             print(self.__handleFileDownload__(s,self.path,role))
#                             self.ans="True"
#                             self.ansReady=True
#                             print(self.ansReady)
#                             self.operation=0
                            
#                         elif self.operation == 2:
#                             print(self.__handleSincronizeDownload__(s,self.path,role))
#                             self.ans="True"
#                             self.ansReady=True
#                             self.operation=0
                            
#                         elif self.operation == 3:
#                             temp=self.__handleDirectoryFiles__(s,self.path,role)
#                             if type(temp)==list:
#                                 self.ans=temp
#                             else:
#                                 self.ans=self.ans=list(temp.keys())
                            
#                             self.ansReady=True
#                             #return parameter
#                             self.operation=0
#                     else:
#                         print(self.__handleHearthBit__(s,role))     
#                 except Exception as e:
#                     logger.info("funzione"+"makeThing parte client")

#                     logger.info((e))
#                     self.__closeHole__()
#                     s.close()
#                     return

#                 time.sleep(1)
                
#         else:
#             while True:
#                 try:
#                     msg=recv_msg(s)
#                     if msg is None:
#                         return
#                     msg=msg.decode("utf-8")
#                     logger.info("ricevuto: %s",msg)
                
#                     msg=msg.split("?")
#                     #print(msg)
#                     if len(msg)!=2:
#                         continue
#                     elif msg[1]=="file":
#                         print(self.__handleFileDownload__(s,msg[0],role))
#                     elif msg[1]=="directory":
#                         if msg[0]=="/":
#                             self.__handleFirstMessage__(s,role)
#                         else:
#                             self.__handleDirectoryFiles__(s,msg[0],role)
#                     elif msg[1]=="heartBit":
#                         self.__handleHearthBit__(s,role)
#                 except Exception as e:
#                     logger.info("funzione"+"makeThing parte server")

#                     logger.info((e))
#                     self.__closeHole__()

#                     s.close()
#                     return
    
#     #Chiudo l'hole punching
            
#     def __closeHole__(self):
#         self.__mutex__.acquire()
#         try:
#             self.holeCreated=False
#         finally:
#             self.__mutex__.release()
#     #gestisce lato server lo stato di online e ritorna le richieste da smaltire       
#     def __handleHttpHearthBit__(self):
#         while True:
#             try:
#                 data={
#                 "username":self.user,
#                 "code":self.code,
#                 }
#                 response=requests.get("http://"+TURNSERVER+"/hearthBit",json=data)
#                 response=response.json()
#                 print("hearthbit")
#                 for data in response:
#                     if "operation" in data:
#                         #print(data)
#                         self.__handleTurnOperation__(data["path"],data["operation"],data["code"])
#             except:
#                 print("exception in handle HTTP hearth bit")
#             time.sleep(2) 


            
#     #gestisce ogni tipo di operazione ricevuta in __handleHttpHearthBit__
#     def __handleTurnOperation__(self,path,operation,code):
#         #inserire una get per l'accettazione
#         path=str(path)
#         if operation==1: 
#             if stringInsideAList(path,self.paths):

#                 with open(path, 'rb') as file:
#                     file_data = file.read()
#                 request=requests.post("http://"+TURNSERVER+"/response/"+code,data=file_data)
#                 ##print(request.json)
#             else:
#                 requests.post("http://"+TURNSERVER+"/response/"+code, json={"error":"path not allowed"})

#         elif operation==4:
#             requests.post("http://"+TURNSERVER+"/response/"+code, json={"AcceptHole":True})
#             thread=threading.Thread(target=self.__create_hole__())
#             time.sleep(0.1)
#             thread.start()
#         elif operation==3:
#             if path=="/":
#                 requests.post("http://"+TURNSERVER+"/response/"+code, json=self.paths)

#             else:
#                 if stringInsideAList(path,self.paths):
#                     #print(path)
#                     requests.post("http://"+TURNSERVER+"/response/"+code, json=(self.get_all_files_in_directory(path)))
#                 else:
#                     requests.post("http://"+TURNSERVER+"/response/"+code, json={"error":"path not allowed"})

#         else:
#             requests.post("http://"+TURNSERVER+"/response/"+code, json={"error":"Bad Operation"})


              
#     #funzione che richiede il permesso di poter eseguire l'hole punch
#     def __getPermission__(self,peer_username,peer_code):#farla sempre come /request
#         httpPort="80"
#         data={
#                 "username":self.user,
#                 "code":self.code,
#                 "peer_username":peer_username,
#                 "peer_code":peer_code,
#                 "operation":4,
#                 "path":""
#             }
#         #print("http://"+TURNSERVER+":"+httpPort+"/request/")
#         firstCall=requests.post( "http://"+TURNSERVER+"/request", json=(data),timeout=5)
#         return firstCall.json
    
#     #richiesta di un file tramite server 
#     def __turnDownload__(self,data,subPath=""):
#         #print("Data:")
#         #print(data)
#         #print("subpath:")
#         #print(subPath)
#         try:
#             response= requests.post( "http://"+TURNSERVER+"/request", json=data)
#             #print(response.text)
#             file=data["path"]
#             if len(file.split("/"))>1:
#                 fileName=file.split("/")[-1]
#             else:
#                 fileName=file.split("\\")[-1]
#             #print("newPath:")
#             #print(os.path.join( DOWNLOADDIRECTORY,subPath,fileName))
#             with open(os.path.join( DOWNLOADDIRECTORY,subPath,fileName), 'wb') as received_file:
#                 ##print(response.content)
#                 received_file.write(response.content)
#             return "True"
#         except:
#             return ["/error"]
   
#     #richiesta dei nomi dei file dentro una cartella tramite server 

#     def __turnFilenames__(self,data):
#         response= requests.post( "http://"+TURNSERVER+"/request", json=data)
#         return response.json()
#     #richiesta di sincronizzazione dei file dentro una cartella tramite server 

#     def __turnSincronizeDirectory__(self,data,subPath=""):
#             try:
#                 datafile=data
#                 datafile["operation"]=3
#                 #print("data")
#                 #print(data)
#                 dirPath=data["path"]
#                 if len(dirPath.split("/"))>1:
#                     dirName=dirPath.split("/")[-1]
#                 else:
#                     dirName=dirPath.split("\\")[-1]
#                 os.makedirs(os.path.join(DOWNLOADDIRECTORY,subPath,dirName),exist_ok=True)
#                 print("Create directory:"+os.path.join(DOWNLOADDIRECTORY,subPath,dirName))
#                 subPath=os.path.join(subPath,dirName)
#                 files=self.__turnFilenames__(datafile)
#                 print("files")
#                 print(files)
#                 for file in files:

#                     print("file:"+file)
#                     print("is:"+files[file])
#                     requestPath=file
#                     print("requestPath:"+requestPath)
#                     print("subPath:"+subPath)
#                     dataTemp=data
#                     dataTemp["path"]=file
#                     dataTemp["operation"]=1
#                     if files[file]=="file":
                        
#                         print(self.__turnDownload__(dataTemp,subPath))
#                     else:
#                         print("dentro cose che non dovrebbe")
#                         print(dataTemp)
#                         self.__turnSincronizeDirectory__(dataTemp,subPath)
#                 return "True"
#             except:
#                 return ["/error"]
#     #creazione della struttura dati da mandare al server go dalla richiesta ricevuta sul server locale flask
#     def turnOperation(self,user,code,peer_username,peer_code,operation,path):
#         data={
#                 "username":user,
#                 "code":code,
#                 "peer_username":peer_username,
#                 "peer_code":peer_code,
#                 "operation":operation,
#                 "path":path
#             }
#         if operation==1:
#             return self.__turnDownload__(data)
#         elif operation==2:
#             return self.__turnSincronizeDirectory__(data)
#         else:
#             temp=self.__turnFilenames__(data)
#             if type(temp) == list:
#                 return temp
#             return list((temp).keys())
        
#     #gestisce il flusso e sceglie se passare l'operazione tramite il server o usando l'hole punch
#     def handleOperation(self,peer_username,peer_code,path,operation):
#         #versione con il solo server
#         #return self.turnOperation(self.user,self.code,peer_username,peer_code,operation,path)
#         logging.info("self.holeCreated=_%s",str(self.holeCreated))
#         try:
#             if self.holeCreated:
#                 #print("hole")
#                 return self.__newClientOperation__(operation,path)
#             else:
#                 if self.__create_hole__(peer_username=peer_username,peer_code=peer_code)=="True":
#                     return self.__newClientOperation__(operation,path)
#                 else:
#                     logging.info("handle %s",str(operation))
#                     return self.turnOperation(self.user,self.code,peer_username,peer_code,operation,path)
#         except:
#             logging.info("handle %s after exception",str(operation))

#             return self.turnOperation(self.user,self.code,peer_username,peer_code,operation,path)


#     #funzione che inizia il thread di gestione del server  
#     def __startServer__(self):
#         thread=threading.Thread(target=self.__handleHttpHearthBit__)
#         thread.start()



class serverConnector:
    #classe creata allo scopo di incampsulare le operazioni del nostro "protocollo"
    def __init__(self,user, code):#separare in classe Client e classe server
        self.user = user
        self.code = code
        #client
        self.operation=0
        self.ans=""
        self.ansReady=False
        self.__mutex__=threading.Lock()
        self.holeCreated=False
        self.requestWithOutHole=0
        
        self.path=None
        self.paths=[]

    #funzione che implementa l'algoritmo di hole punching tcp aggiungendo dei messaggi di controllo
    #per l'ottenimento dei permessi dal server
    def __create_hole__(self,host=TURNSERVER, port=5000,peer_username="",peer_code=""):
        try:
            if self.holeCreated:
                return "True"
           

        
            sa = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sa.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            data={
                    "username":self.user,
                    "code":self.code,
                    "peer_username":peer_username,
                    "peer_code":peer_code,
                    "operation":4,
                    "path":""
                }
            
            #print(port)
            sa.connect((host, port))
            send_msg(sa,json.dumps(data).encode("utf-8"))

            priv_addr = sa.getsockname()
            
            send_msg(sa, addr_to_msg(priv_addr))
            data = recv_msg(sa)
            
            logger.info("client %s %s - received data: %s", priv_addr[0], priv_addr[1], data)
            pub_addr = msg_to_addr(data)
            logger.info("pub addr")
            print(pub_addr)
            send_msg(sa, addr_to_msg(pub_addr))
            logger.info("data = recv_msg(sa)")
            data = recv_msg(sa)
            logger.info("all data")

            print(data)
            logger.info("pubdata, privdata = data.split")
            pubdata, privdata = data.split(b'|')
            client_pub_addr = msg_to_addr(pubdata)
            client_priv_addr = msg_to_addr(privdata)
            logger.info(
                "client public is %s and private is %s, peer public is %s private is %s",
                pub_addr, priv_addr, client_pub_addr, client_priv_addr,
            )
            #lancio quattro thread per sopperire ad ogni possibile caso della connessione
            threads = {
                '0_accept': Thread(target=self.__accept__, args=(priv_addr[1],)),
                '1_accept': Thread(target=self.__accept__, args=(pub_addr[1],)),
                '2_connect': Thread(target=self.__connect__, args=(priv_addr, client_pub_addr,)),
                '3_connect': Thread(target=self.__connect__, args=(priv_addr, client_priv_addr,)),
            }
            for name in sorted(threads.keys()):
                logger.info('start thread %s', name)
                threads[name].start()
            #aspetto che almeno un thread riesca e in caso contrario ritorno il fallimento
            timer=time.time()
            while threads:
                keys = list(threads.keys())
                for name in keys:
                    try:
                        threads[name].join(1)
                    except TimeoutError:
                        continue
                    if not threads[name].is_alive():
                        logger.info("112:threads.pop(name)")

                        threads.pop(name)
                    if self.holeCreated:
                        logger.info("116:self.holeCreated")
                        return "True"
                    # if time.time()-timer>5:
                    #     threads[name].st
                    #     break
            #Operazione utilizzata per non avere continuo ritardo in caso di fallimento
            self.requestWithOutHole=5
            return ["/error"]
        except:
            self.requestWithOutHole=5
            return ["/error"]
    #funzione che implementa l'accettazione  del primo messaggio
    def __accept__(self,port):
        try:
            logger.info("accept %s", port)
            s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEPORT, 1)
            s.bind(('', port))
            s.listen(1)
            logger.info("130:s.settimeout(5)")
            s.settimeout(5)                
            try:
                logger.info("135:conn, addr = s.accept()")
                conn, addr = s.accept()
                logger.info("Accept %s connected!", addr)
                self.__handleHole__(conn)
            except socket.timeout:
                if self.holeCreated:
                    logger.info("141:self.holeCreated")
                    return True
                else:
                    return False
                        
        except Exception as e:
            logger.info("funzione"+"__accept__")
            logger.info(e)
            return False 
#funzione che implementa l'invio  del primo messaggio per l'hole punching

    def __connect__(self,local_addr, addr):
        try:
            logger.info("connect from %s to %s", local_addr, addr)
            s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEPORT, 1)
            s.bind(local_addr)
            try:
                s.connect(addr)
                logger.info("connected from %s to %s success!", local_addr, addr)

                self.__handleHole__(s)
            except socket.error:
                if self.holeCreated:
                    logger.info("168:self.holeCreated:")
                    return True
                else:
                    return False
            else:
                logger.info("connected from %s to %s success!", local_addr, addr)
        except Exception as e:
            logger.info("funzione"+"__connect__")
            logger.info(e)
            return False

#funzione ausiliaria che ritorna un dizionario avente come chiavi i nomi dei file in un certo
#path e come valore se è un file o una cartella
    def get_all_files_in_directory(self,directory_path):
        file_names = {}
    
    # List all files in the directory
        try:
            with os.scandir(directory_path) as entries:
                for entry in entries:
                    if entry.is_file():
                        file_names[os.path.join(directory_path,entry.name)]="file"#fare una lista
                    else:
                        file_names[os.path.join(directory_path,entry.name)]="directory"
                        
            return file_names
        except:
            return {}
    #gestisce l'invio del primo messaggio (serie di path names) o la ricezione degli stessi
    def __handleFirstMessage__(self,s):
        send_msg(s,json.dumps(self.paths).encode("utf-8"))
        return "Sent first message"
    
    #funzione che cambia self.operation in connector per cambiare il comportamento del thread in 
    # continuo polling durante l'hole punching
    def __newClientOperation__(self,operation,path):
        #print("client operation")
        if operation>0 and operation<4:
            self.operation=operation
            self.path=path
        else:
            return ["/error"]
        while not self.ansReady and self.holeCreated:
            time.sleep(1)
            print("waiting"+str(self.ansReady))
        else:
            ret=self.ans
            self.ansReady=False
            return ret
        
    #gestrione del download di un singolo file
    def __handleFileDownload__(self,s,path,subPath=""):
        with open(path, 'rb') as file:
            while True:
                file_data = file.read()
                if(len(file_data)==0):
                    break
                temp={}
                temp["chunk"]=str(base64.b64encode(file_data),"UTF-8")
                temp["last"]=False
                print(len(file_data))
                #print(temp)

                send_msg(s,json.dumps(temp).encode("utf-8"))
        temp={}
        temp["last"]="True"
        send_msg(s,json.dumps(temp).encode("utf-8"))
        return "SentFile"

    #gestisce il ritorno di tutti i file sotto una cartella 
    def __handleDirectoryFiles__(self,s,path):
        send_msg(s,json.dumps(self.get_all_files_in_directory(path)).encode("utf-8"))
        return "Sent DirectoryFiles"
        
    #utilizzo di un messaggio vuoto per mantenere la connessione
    def __handleHearthBit__(self,s):
        send_msg(s,b'')
        return "Sent HeartBit"
        

    #gestore della connessione dopo aver aperto la connessione con l'hole punching
    def __handleHole__(self,s):
        self.__mutex__.acquire()
        try:
            logger.info("317:self.holeCreated=True")
            self.holeCreated=True
        finally:
            self.__mutex__.release()
        timer=time.time()
        while True:
            try:
                if time.time()-timer>60:
                    self.__closeHole__()
                    s.close()
                    return
                msg=recv_msg(s)
                if msg is None:
                    return
                msg=msg.decode("utf-8")
                logger.info("ricevuto: %s",msg)
            
                msg=msg.split("?")
                #print(msg)
                if len(msg)!=2:
                    continue
                elif msg[1]=="file":
                    timer=time.time()
                    print(self.__handleFileDownload__(s,msg[0]))
                elif msg[1]=="directory":
                    timer=time.time()
                    if msg[0]=="/":
                        self.__handleFirstMessage__(s)
                    else:
                        self.__handleDirectoryFiles__(s,msg[0])
                elif msg[1]=="heartBit":
                    self.__handleHearthBit__(s)
            except Exception as e:
                logger.info("funzione"+"makeThing parte server")

                logger.info((e))
                self.__closeHole__()

                s.close()
                return
            
    
    #Chiudo l'hole punching
            
    def __closeHole__(self):
        self.__mutex__.acquire()
        try:
            self.holeCreated=False
        finally:
            self.__mutex__.release()
    #gestisce lato server lo stato di online e ritorna le richieste da smaltire       
    def __handleHttpHearthBit__(self):
        while True:
            try:
                data={
                "username":self.user,
                "code":self.code,
                }
                response=requests.get("http://"+TURNSERVER+"/hearthBit",json=data)
                response=response.json()
                print("hearthbit")
                for data in response:
                    if "operation" in data:
                        #print(data)
                        self.__handleTurnOperation__(data["path"],data["operation"],data["code"])
            except:
                print("exception in handle HTTP hearth bit")
            time.sleep(2) 


            
    #gestisce ogni tipo di operazione ricevuta in __handleHttpHearthBit__
    def __handleTurnOperation__(self,path,operation,code):
        #inserire una get per l'accettazione
        path=str(path)
        if operation==1: 
            if stringInsideAList(path,self.paths):

                with open(path, 'rb') as file:
                    file_data = file.read()
                request=requests.post("http://"+TURNSERVER+"/response/"+code,data=file_data)
                logging.info("respose download data=file_data")
            else:
                logging.info("respose download &s",str({"error":"path not allowed"}))
                requests.post("http://"+TURNSERVER+"/response/"+code, json={"error":"path not allowed"})

        elif operation==4:
            requests.post("http://"+TURNSERVER+"/response/"+code, json={"AcceptHole":True})
            thread=threading.Thread(target=self.__create_hole__())
            time.sleep(0.1)
            thread.start()
        elif operation==3:
            if path=="/":
                requests.post("http://"+TURNSERVER+"/response/"+code, json=self.paths)

            else:
                if stringInsideAList(path,self.paths):
                    #print(path)
                    requests.post("http://"+TURNSERVER+"/response/"+code, json=(self.get_all_files_in_directory(path)))
                else:
                    requests.post("http://"+TURNSERVER+"/response/"+code, json={"error":"path not allowed"})

        else:
            requests.post("http://"+TURNSERVER+"/response/"+code, json={"error":"Bad Operation"})

    #funzione che inizia il thread di gestione del server  
    def __startServer__(self):
        thread=threading.Thread(target=self.__handleHttpHearthBit__)
        thread.start()
    
class clientConnector:
    #classe creata allo scopo di incampsulare le operazioni del nostro "protocollo"
    def __init__(self,user, code):#separare in classe Client e classe server
        self.user = user
        self.code = code
        #client
        self.operation=0
        self.ans=""
        self.ansReady=False
        self.__mutex__=threading.Lock()
        self.holeCreated=False
        self.requestWithOutHole=0
        
        self.path=None
        self.paths=[]

    #funzione che implementa l'algoritmo di hole punching tcp aggiungendo dei messaggi di controllo
    #per l'ottenimento dei permessi dal server
    def __create_hole__(self,host=TURNSERVER, port=5000,peer_username="",peer_code=""):
        try:
            if self.holeCreated:
                return "True"
            if self.requestWithOutHole>0:
                self.requestWithOutHole-=1
                return ["/error"]
            control=self.__getPermission__(peer_username,peer_code)
            logging.info(str(control))
            
            sa = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sa.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            data={
                    "username":self.user,
                    "code":self.code,
                    "peer_username":peer_username,
                    "peer_code":peer_code,
                    "operation":4,
                    "path":""
                }
            
            #print(port)
            sa.connect((host, port))
            send_msg(sa,json.dumps(data).encode("utf-8"))

            priv_addr = sa.getsockname()
            
            send_msg(sa, addr_to_msg(priv_addr))
            data = recv_msg(sa)
            
            logger.info("client %s %s - received data: %s", priv_addr[0], priv_addr[1], data)
            pub_addr = msg_to_addr(data)
            logger.info("pub addr")
            print(pub_addr)
            send_msg(sa, addr_to_msg(pub_addr))
            logger.info("data = recv_msg(sa)")
            data = recv_msg(sa)
            logger.info("all data")

            print(data)
            logger.info("pubdata, privdata = data.split")
            pubdata, privdata = data.split(b'|')
            client_pub_addr = msg_to_addr(pubdata)
            client_priv_addr = msg_to_addr(privdata)
            logger.info(
                "client public is %s and private is %s, peer public is %s private is %s",
                pub_addr, priv_addr, client_pub_addr, client_priv_addr,
            )
            #lancio quattro thread per sopperire ad ogni possibile caso della connessione
            threads = {
                '0_accept': Thread(target=self.__accept__, args=(priv_addr[1],)),
                '1_accept': Thread(target=self.__accept__, args=(pub_addr[1],)),
                '2_connect': Thread(target=self.__connect__, args=(priv_addr, client_pub_addr,)),
                '3_connect': Thread(target=self.__connect__, args=(priv_addr, client_priv_addr,)),
            }
            for name in sorted(threads.keys()):
                logger.info('start thread %s', name)
                threads[name].start()
            #aspetto che almeno un thread riesca e in caso contrario ritorno il fallimento
            while threads:
                keys = list(threads.keys())
                for name in keys:
                    try:
                        threads[name].join(1)
                    except TimeoutError:
                        continue
                    if not threads[name].is_alive():
                        logger.info("112:threads.pop(name)")

                        threads.pop(name)
                    if self.holeCreated:
                        logger.info("116:self.holeCreated")
                        return "True"
            #Operazione utilizzata per non avere continuo ritardo in caso di fallimento
            self.requestWithOutHole=5
            return ["/error"]
        except:
            self.requestWithOutHole=5
            return ["/error"]
    #funzione che implementa l'accettazione  del primo messaggio
    def __accept__(self,port):
        try:
            logger.info("accept %s", port)
            s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEPORT, 1)
            s.bind(('', port))
            s.listen(1)
            logger.info("130:s.settimeout(5)")
            s.settimeout(5)                
            try:
                logger.info("135:conn, addr = s.accept()")
                conn, addr = s.accept()
                logger.info("Accept %s connected!", addr)
                self.__handleHole__(conn)
            except socket.timeout:
                if self.holeCreated:
                    logger.info("141:self.holeCreated")
                    return True
                else:
                    return False
                        
        except Exception as e:
            logger.info("funzione"+"__accept__")
            logger.info(e)
            return False 
#funzione che implementa l'invio  del primo messaggio per l'hole punching

    def __connect__(self,local_addr, addr):
        try:
            logger.info("connect from %s to %s", local_addr, addr)
            s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEPORT, 1)
            s.bind(local_addr)
            try:
                s.connect(addr)
                logger.info("connected from %s to %s success!", local_addr, addr)

                self.__handleHole__(s)
            except socket.error:
                if self.holeCreated:
                    logger.info("168:self.holeCreated:")
                    return True
                else:
                    return False
            else:
                logger.info("connected from %s to %s success!", local_addr, addr)
        except Exception as e:
            logger.info("funzione"+"__connect__")
            logger.info(e)
            return False

#funzione ausiliaria che ritorna un dizionario avente come chiavi i nomi dei file in un certo
#path e come valore se è un file o una cartella
    def get_all_files_in_directory(self,directory_path):
        file_names = {}
    
    # List all files in the directory
        try:
            with os.scandir(directory_path) as entries:
                for entry in entries:
                    if entry.is_file():
                        file_names[os.path.join(directory_path,entry.name)]="file"#fare una lista
                    else:
                        file_names[os.path.join(directory_path,entry.name)]="directory"
                        
            return file_names
        except:
            return {}
    #gestisce l'invio del primo messaggio (serie di path names) o la ricezione degli stessi
    def __handleFirstMessage__(self,s):
        temp=recv_msg(s)
        if temp is None:
            return None
        return(json.loads(temp.decode("utf-8")))
    #funzione che cambia self.operation in connector per cambiare il comportamento del thread in 
    # continuo polling durante l'hole punching
    def __newClientOperation__(self,operation,path):
        #print("client operation")
        if operation>0 and operation<4:
            self.operation=operation
            self.path=path
        else:
            return ["/error"]
        while not self.ansReady and self.holeCreated:
            time.sleep(1)
            print("waiting"+str(self.ansReady))
        else:
            ret=self.ans
            self.ansReady=False
            return ret
        
    #gestrione del download di un singolo file
    def __handleFileDownload__(self,s,path,subPath=""):

        send_msg(s,(path+"?file").encode("utf-8"))
        #print(os.path.join( DOWNLOADDIRECTORY,subPath,path.split(os.path.sep)[-1]))

        with open(os.path.join( DOWNLOADDIRECTORY,subPath,path.split(os.path.sep)[-1]), 'wb') as received_file:
            file_data=b''
            cond=True
            while(cond):
                a=recv_msg(s)
                #print(a)
                if a is None:
                    return "error in file download"
                temp=json.loads(a.decode("utf-8"))
                print("templen:")
                print(len(temp))

                if(len(temp)<2):
                    break
                print(cond)
                time.sleep(1)
                file_data=base64.b64decode(temp["chunk"])
                received_file.write(file_data)

            print("finish download")  
            return "received File"
        
    #Sincronizzazione di un'intera cartella richiamando ricorsivamente __handleDirectoryFiles__ e __handleFileDownload__
    def __handleSincronizeDownload__(self,s,path,subPath=""):
            files=self.__handleDirectoryFiles__(s,path)
            
            if len(path.split("/"))>1:
                dirName=path.split("/")[-1]
            else:
                dirName=path.split("\\")[-1]
            print(os.path.join(DOWNLOADDIRECTORY,subPath,dirName))
            os.makedirs(os.path.join(DOWNLOADDIRECTORY,subPath,dirName),exist_ok=True)
            print("Create directory:"+os.path.join(DOWNLOADDIRECTORY,subPath,dirName))
            subPath=os.path.join(subPath,dirName)
            
            for file in files:
                if file=="error":
                    return s
                print("file:"+file)
                print("is:"+files[file])
                requestPath=file
                print("requestPath:"+requestPath)
                print("subPath:"+subPath)
                if files[file]=="file":

                    print(self.__handleFileDownload__(s,requestPath,subPath))
                else:
                    #os.makedirs(os.path.join(DOWNLOADDIRECTORY,subPath,file),exist_ok=True)
                    #print("Create directory:"+os.path.join(DOWNLOADDIRECTORY,subPath,file))
                    s=self.__handleSincronizeDownload__(s,requestPath,subPath)
            return s
    #gestisce il ritorno di tutti i file sotto una cartella 
    def __handleDirectoryFiles__(self,s,path):
        send_msg(s,(path+"?directory").encode("utf-8"))
        temp=recv_msg(s)
        if temp is None:
            return ["error"]
        return(json.loads(temp.decode("utf-8")))
    
    #utilizzo di un messaggio vuoto per mantenere la connessione
    def __handleHearthBit__(self,s):
        send_msg(s,("/"+"?heartBit").encode
                 ("utf-8"))
        return recv_msg(s)    

    #gestore della connessione dopo aver aperto la connessione con l'hole punching
    def __handleHole__(self,s):
        self.__mutex__.acquire()
        try:
            logger.info("317:self.holeCreated=True")
            self.holeCreated=True
        finally:
            self.__mutex__.release()
        timer=time.time()
        while True:
            try:
                
                print(self.path)
                if time.time()-timer>60:
                    self.__closeHole__()
                    s.close()
                    return
                if self.operation>0:
                    timer=time.time()
                    if self.operation==1:
                        print(self.__handleFileDownload__(s,self.path))
                        self.ans="True"
                        self.ansReady=True
                        print(self.ansReady)
                        self.operation=0
                        
                    elif self.operation == 2:
                        print(self.__handleSincronizeDownload__(s,self.path))
                        self.ans="True"
                        self.ansReady=True
                        self.operation=0
                        
                    elif self.operation == 3:
                        temp=self.__handleDirectoryFiles__(s,self.path)
                        if type(temp)==list:
                            self.ans=temp
                        else:
                            self.ans=list(temp.keys())
                        
                        self.ansReady=True
                        #return parameter
                        self.operation=0
                else:
                    print(self.__handleHearthBit__(s))     
            except Exception as e:
                logger.info("funzione"+"makeThing parte client")

                logger.info((e))
                self.__closeHole__()
                s.close()
                return

            time.sleep(1)
                
        
    
    #Chiudo l'hole punching
            
    def __closeHole__(self):
        self.__mutex__.acquire()
        try:
            self.holeCreated=False
        finally:
            self.__mutex__.release()

   

              
    #funzione che richiede il permesso di poter eseguire l'hole punch
    def __getPermission__(self,peer_username,peer_code):#farla sempre come /request
        httpPort="80"
        data={
                "username":self.user,
                "code":self.code,
                "peer_username":peer_username,
                "peer_code":peer_code,
                "operation":4,
                "path":""
            }
        #print("http://"+TURNSERVER+":"+httpPort+"/request/")
        firstCall=requests.post( "http://"+TURNSERVER+"/request", json=(data),timeout=5)
        return firstCall.json
    
    #richiesta di un file tramite server 
    def __turnDownload__(self,data,subPath=""):
        #print("Data:")
        #print(data)
        #print("subpath:")
        #print(subPath)
        try:
            response= requests.post( "http://"+TURNSERVER+"/request", json=data)
            #print(response.text)
            file=data["path"]
            if len(file.split("/"))>1:
                fileName=file.split("/")[-1]
            else:
                fileName=file.split("\\")[-1]
            #print("newPath:")
            try:
                temp=response.json()
                if "/error" in temp or "error" in temp or response.content==None:
                    return ["error"]
            except:
                pass

            #print(os.path.join( DOWNLOADDIRECTORY,subPath,fileName))
            with open(os.path.join( DOWNLOADDIRECTORY,subPath,fileName), 'wb') as received_file:
                ##print(response.content)
                received_file.write(response.content)
            return ["True"]
        except:
            return ["error"]
   
    #richiesta dei nomi dei file dentro una cartella tramite server 

    def __turnFilenames__(self,data):
        response= requests.post( "http://"+TURNSERVER+"/request", json=data)
        if response.content!=None:
            return response.json()
        else:
            return ["error"]

    #richiesta di sincronizzazione dei file dentro una cartella tramite server 

    def __turnSincronizeDirectory__(self,data,subPath=""):
            try:
                datafile=data
                datafile["operation"]=3
                #print("data")
                #print(data)
                dirPath=data["path"]
                if len(dirPath.split("/"))>1:
                    dirName=dirPath.split("/")[-1]
                else:
                    dirName=dirPath.split("\\")[-1]
                os.makedirs(os.path.join(DOWNLOADDIRECTORY,subPath,dirName),exist_ok=True)
                print("Create directory:"+os.path.join(DOWNLOADDIRECTORY,subPath,dirName))
                subPath=os.path.join(subPath,dirName)
                files=self.__turnFilenames__(datafile)
                print("files")
                print(files)
                for file in files:

                    print("file:"+file)
                    print("is:"+files[file])
                    requestPath=file
                    print("requestPath:"+requestPath)
                    print("subPath:"+subPath)
                    dataTemp=data
                    dataTemp["path"]=file
                    dataTemp["operation"]=1
                    if files[file]=="file":
                        
                        print(self.__turnDownload__(dataTemp,subPath))
                    else:
                        print("dentro cose che non dovrebbe")
                        print(dataTemp)
                        self.__turnSincronizeDirectory__(dataTemp,subPath)
                return "True"
            except:
                return ["/error"]
    #creazione della struttura dati da mandare al server go dalla richiesta ricevuta sul server locale flask
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
            temp=self.__turnFilenames__(data)
            if type(temp) == list:
                return temp
            return list((temp).keys())
        
    #gestisce il flusso e sceglie se passare l'operazione tramite il server o usando l'hole punch
    def handleOperation(self,peer_username,peer_code,path,operation):
        #versione con il solo server
        #return self.turnOperation(self.user,self.code,peer_username,peer_code,operation,path)
        logging.info("self.holeCreated=_%s",str(self.holeCreated))
        try:
            
            if self.__create_hole__(peer_username=peer_username,peer_code=peer_code)=="True":
                logging.info("1448- hole created ")
                return self.__newClientOperation__(operation,path)
            else:
                logging.info("handle %s",str(operation))
                return self.turnOperation(self.user,self.code,peer_username,peer_code,operation,path)
        except:
            logging.info("handle %s after exception",str(operation))

            return self.turnOperation(self.user,self.code,peer_username,peer_code,operation,path)


