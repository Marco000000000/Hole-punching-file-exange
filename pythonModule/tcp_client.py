#!/usr/bin/env python
import os
import logging
import socket
from util import *
from flask import Flask, request,jsonify
app = Flask(__name__)
if not hasattr( socket,"SO_REUSEPORT") :#dipende dai sistemi operativi
    socket.SO_REUSEPORT = socket.SO_REUSEADDR

if not os.path.exists(DOWNLOADDIRECTORY):
        # Create the directory
        os.makedirs(DOWNLOADDIRECTORY)
logger = logging.getLogger('client')
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(message)s')



#route di gestione messaggi dal frontend
@app.route('/', methods=['POST'])
def handleMessage():
    if request.remote_addr != '127.0.0.1':
        return jsonify({"error":"Forbidden: Only localhost connections are allowed"}), 403

    data = request.json
    #print(data)
    query_type = data["query"]
    user=data["username"]
    code=data["code"]
    path=data["path"]
    
    global clientconnector
    global serverconnector
    
    if query_type=="download":

        if clientconnector is None:
            clientconnector=clientConnector(user,code)
        peer_username=data["peer_username"]
        peer_code=data["peer_code"]
        print("path: "+path.split(os.sep)[-1][0])

        if "." in path.split(os.sep)[-1] and path.split(os.sep)[-1][0]!=".":
            return clientconnector.handleOperation(peer_username,peer_code,path,1)
        else:

            return clientconnector.handleOperation(peer_username,peer_code,path,2)
    elif query_type=="names":

        if clientconnector is None:
            clientconnector=clientConnector(user,code)
        peer_username=data["peer_username"]
        peer_code=data["peer_code"]
        return clientconnector.handleOperation(peer_username,peer_code,path,3)
    elif query_type=="start_share":
        if serverconnector is None:
            serverconnector=serverConnector(user,code)
            serverconnector.__startServer__()
        tempPaths=[]
        for tempPath in path:
            tempPaths.append(tempPath)
        serverconnector.paths=tempPaths
        return jsonify({"ok":tempPaths})
    else:
        return jsonify({"error":"bad query"}),400

clientconnector=None
serverconnector=None

if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO, message='%(asctime)s %(message)s')
    
    hostname = socket.gethostname()

    app.run(debug=False,port=80,threaded=True)
