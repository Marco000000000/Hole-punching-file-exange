import socket
import threading

def handle_client(client_socket):
    while True:
        data = client_socket.recv(1024)
        if not data:
            break
        print(f"Received data: {data.decode('utf-8')}")
        # Add your processing logic here
        client_socket.send("Data received.".encode('utf-8'))
    client_socket.close()

def start_server():
    server = socket.socket(socket.AF_INET6, socket.SOCK_STREAM)
    server.bind(("::", 9999))
    server.listen(5)

    print("Server listening on [::]:9999")

    while True:
        client, addr = server.accept()
        print(f"Accepted connection from {addr}")
        client_handler = threading.Thread(target=handle_client, args=(client,))
        client_handler.start()

def start_client():
    client = socket.socket(socket.AF_INET6, socket.SOCK_STREAM)
    server_address = ("[fe80::fa30:82a1:fa5f:6ac0%5]", 9999)  # Replace with the server's IPv6 address
    
    client.connect(server_address)

    while True:
        message = input("Enter message to send: ")
        client.send(message.encode('utf-8'))
        response = client.recv(1024)
        print(f"Server response: {response.decode('utf-8')}")

if __name__ == "__main__":
    mode = input("Enter 'server' or 'client': ").lower()

    if mode == "server":
        start_server()
    elif mode == "client":
        start_client()
    else:
        print("Invalid mode. Please enter 'server' or 'client'.")
