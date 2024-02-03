import logging
import socket
import sys
import time
from util import *

logger = logging.getLogger()

def main(host='151.54.53.122', port=80):
    sock = socket.socket(socket.AF_INET, # Internet
                         socket.SOCK_DGRAM) # UDP

    while True:
        sock.sendto(b'0', (host, port))

        data, addr = sock.recvfrom(1024)
        print('client received: {} {}'.format(addr, data))
        addr = msg_to_addr(data)
        print(addr)
        try:
            sock.sendto(b'0', addr)
            print("invio a "+addr)
            sock.sendto(b'0', addr)
            data, addr = sock.recvfrom(1024)
        except:
            sock.sendto(b'0', (host, port))

            time.sleep(1)
            continue
        print('client received: {} {}'.format(addr, data))


if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(message)s')
    main()
