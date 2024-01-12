import socket
import threading

def receive_messages(sock):
    while True:
        data = sock.recv(1024)
        print(f"Received from server: {data.decode('utf-8')}")

def main():
    # Connect to the mediator server
    mediator_address = ('localhost', 8080)
    with socket.create_connection(mediator_address) as mediator_sock:

    # Get local port of the client
        local_port = mediator_sock.getsockname()[1]
        print(f"Local port of client: {local_port}")

        # Send local port to the mediator
        mediator_sock.sendall(str(local_port).encode('utf-8'))

        # Receive external port from the mediator
        external_port = int(mediator_sock.recv(1024).decode('utf-8'))
        print(f"External port assigned by mediator: {external_port}")

        # Close the connection to the mediator
        mediator_sock.close()

        # Connect to the server using the external port
        server_address = ('localhost', external_port)
        while True:
            try:
                server_sock = socket.create_connection(server_address)
                break
            except:
                pass
        # Start a thread to receive messages from the server
        threading.Thread(target=receive_messages, args=(server_sock,), daemon=True).start()

        while True:
            # Send messages to the server
            message = input("Enter message to send: ")
            server_sock.sendall(message.encode('utf-8'))

if __name__ == "__main__":
    main()
