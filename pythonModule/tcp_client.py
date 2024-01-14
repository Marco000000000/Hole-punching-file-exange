#!/usr/bin/env python
import sys
import logging
import socket
import struct
from threading import Event, Thread
import time
from util import *
import requests

socket.SO_REUSEPORT = socket.SO_REUSEADDR

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
            makeThing(conn,"accept")
        except socket.timeout:
            continue


def makeThing(s,type):
    if type=="connect":
        while True:

            s.send("salve".encode("utf-8"))
            logger.info("inviato: %s","salve")

            time.sleep(5)
    else:
        while True:
                msg=s.recv(1000)
                logger.info("ricevuto: %s",msg.decode("utf-8"))
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

            makeThing(s,"connect")
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
