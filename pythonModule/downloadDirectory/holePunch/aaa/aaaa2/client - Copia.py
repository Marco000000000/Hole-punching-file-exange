import socket
import threading

def initiate_connection(server_host, server_port, local_port):
    client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    client_socket.connect((server_host, server_port))
    print("Connected to server")

    # Get the local address assigned by the NAT
    local_address = socket.gethostbyname(socket.gethostname())
    print(f"Local Address: {local_address}, Local Port: {local_port}")

    # Send local address and port to the server
    client_socket.send(f"{local_address}:{local_port}".encode())

    # Receive the list of clients from the server
    clients_info = client_socket.recv(1024).decode()

    try:
        clients = eval(clients_info)
    except SyntaxError:
        print("Error parsing client information.")
        clients = []

    if len(clients) >= 2:
        # Close the connection to the server
        client_socket.close()

        # Start a new thread to establish a direct connection to the other client
        other_client_address = clients[1] if f"{local_address}:{local_port}" == clients[0] else clients[0]
        threading.Thread(target=connect_directly, args=(other_client_address,)).start()
    else:
        print("Error: Not enough clients received from the server.")
        client_socket.close()

def connect_directly(other_client_address):
    other_client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    other_client_socket.connect(tuple(other_client_address.split(':')))
    print(f"Connected directly to {other_client_address}")

    # Now you can use 'other_client_socket' to communicate directly with the other client

if __name__ == "__main__":
    server_host = "127.0.0.1"  # Replace with your server IP address
    server_port = 80              # Replace with your server port
    local_port = 1024               # Replace with your desired local port

    threading.Thread(target=initiate_connection, args=(server_host, server_port, local_port)).start()
