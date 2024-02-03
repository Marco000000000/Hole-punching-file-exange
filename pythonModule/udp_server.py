import logging
import socket
import sys
from util import *

logger = logging.getLogger()
addresses = []

def get_local_ip():
    try:
        # Create a socket object
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

        # Connect to an external server (doesn't actually send any data)
        s.connect(('8.8.8.8', 80))

        # Get the local IP address
        local_ip = s.getsockname()[0]

        # Close the socket
        s.close()

        return local_ip
    except Exception as e:
        print(f"Error: {e}")
        return None
    
def main(host='127.0.0.1', port=80):
    host=get_local_ip()
    print((host, port))
    sock = socket.socket(socket.AF_INET, # Internet
                         socket.SOCK_DGRAM) # UDP
    sock.bind((host, port))
    while True:
        data, addr = sock.recvfrom(1024) # buffer size is 1024 bytes
        logger.info("connection from: %s", addr)
        if(addr not in addresses):
            addresses.append(addr)
            print(addresses)
        if len(addresses) >= 2:
            logger.info("server - send client info to: %s", addresses[0])
            sock.sendto(addr_to_msg(addresses[1]), addresses[0])
            logger.info("server - send client info to: %s", addresses[1])
            sock.sendto(addr_to_msg(addresses[0]), addresses[1])
            addresses.pop(1)
            addresses.pop(0)


if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(message)s')
    main()
