#!/usr/bin/env python
import sys
import logging
import socket
from threading import Thread
from util import *
socket.SO_REUSEPORT = socket.SO_REUSEADDR

from flask import Flask, Response,request

logger = logging.getLogger()
clients = {}
app = Flask(__name__)
addressConnected=[]

def main(host='localhost', port=8080):
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    s.bind((host, port))
    s.listen(1)
    s.settimeout(30)

    while True:
        try:
            conn, addr = s.accept()
            print("indirizzo addr[0]"+addr[0])
            if addr[0] in addressConnected:
                addressConnected.remove(addr[0])
            else:
                continue
        except socket.timeout:
            continue

        logger.info('connection address: %s', addr)
        data = recv_msg(conn)
        priv_addr = msg_to_addr(data)
        send_msg(conn, addr_to_msg(addr))
        data = recv_msg(conn)
        data_addr = msg_to_addr(data)
        if data_addr == addr:
            logger.info('client reply matches')
            clients[addr] = Client(conn, addr, priv_addr)
        else:
            logger.info('client reply did not match')
            conn.close()

        logger.info('server - received data: %s', data)

        if len(clients) == 2:
            (addr1, c1), (addr2, c2) = clients.items()
            logger.info('server - send client info to: %s', c1.pub)
            send_msg(c1.conn, c2.peer_msg())
            logger.info('server - send client info to: %s', c2.pub)
            send_msg(c2.conn, c1.peer_msg())
            clients.pop(addr1)
            clients.pop(addr2)


@app.route("/holePunch/<path:userInfo>",methods=['GET'])
def holePunchControl(userInfo):
    if userInfo!= "1":
        return "info errate"
    else:
        addressConnected.append(request.remote_addr)
        return "ok"
host=""
if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(message)s')
    holeThread=Thread(target=main,args=(*addr_from_args(sys.argv),))
    holeThread.start()
    #hostname = socket.gethostname()
    #host=socket.gethostbyname_ex(hostname)[2][0]
    app.run(debug=True, use_reloader=False,port=80)
