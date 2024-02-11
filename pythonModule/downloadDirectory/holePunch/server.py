import socket
import threading

def handle_client(client_socket, clients):
    # Receive local address and port from the client
    client_address_port = client_socket.recv(1024).decode()
    print(f"Received client address: {client_address_port}")

    # Store the client information
    clients.append(client_address_port)

    # If two clients have connected, send the list of clients to both
    if len(clients) == 2:
        for c in clients:
            other_client_address = next(x for x in clients if x != c)
            other_client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            other_client_socket.connect(tuple(other_client_address.split(':')))
            other_client_socket.sendall(str(clients).encode())
            other_client_socket.close()

    # Close the connection with the current client
    client_socket.close()

if __name__ == "__main__":
    server_host = "127.0.0.1"  # Listen on all available interfaces
    server_port = 80       # Replace with your desired server port

    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((server_host, server_port))
    server_socket.listen(2)  # Expecting connections from two clients
    print(f"Server listening on {server_host}:{server_port}")

    clients = []

    try:
        while len(clients) < 2:
            # Accept incoming client connections
            client_socket, client_address = server_socket.accept()
            print(f"Accepted connection from {client_address}")

            # Handle the client in a separate thread
            threading.Thread(target=handle_client, args=(client_socket, clients)).start()

    except KeyboardInterrupt:
        print("Server shutting down")
    finally:
        # Close the server socket
        server_socket.close()
