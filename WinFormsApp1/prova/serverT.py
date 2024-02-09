# Server (server.py)
import socket
import threading

class ClientThread(threading.Thread):
    def __init__(self, client_address, client_socket, server):
        threading.Thread.__init__(self)
        self.client_socket = client_socket
        self.server = server
        print("New connection added: ", client_address)

    def run(self):
        #quello commentato è per testare uno scampio usando un dizionario, caso in cui ci siano n client 1 e n client2
        message=self.client_socket.recv(1024).decode()
        tipo,client_id,data = message.split(";") 
        #tipo,client_id,data = message.split(";",1) 

        #aggiungendo un tipo, "0"--> aggiunta / aggiornamento del dizionario 
        #"1"--> richiesta dei dati legati alla chiave client_id passata, se non è oresente nel dizionario errore
        # effettivamente per dividere le tipologie di interfacciamento per il server python, 
        #si potresti stabile un ulteriore divisione
        # ad esempio se iniza con 0 abbiamo un aggiunta/ aggiornamento dei dati 
        #mentre se inizia con 1 si ha una richiesta dei dati  
       
       #si usa il ; come separatore tra identificativo e dati.
        if tipo=="0":
            if client_id in self.server.data:
                #se identificativo è nel dizionario allora invia i dati al client
                #self.server.data[client_id]=data
                print("data aggiornati")
            else:
                print("Received data", data)
            self.server.data[client_id]=data
        else:
            self.client_socket.send(self.server.data[client_id].encode())
            print("data sent to client")
        self.client_socket.close()    
         
       #in questo modo è esattamnte per due client. 
       #io devo fare in modo che un client x richiedente  
       #si colleghi al server per chiedere i dati di un client y,
       #ma il server potrebbe avere salvati i dati di altri n client.
       # quindi dopo il login  si invia al server i propri dati con il proprio identificativo unico,
       #il server  conservera la struttura fatta da id e dati
       #quando un client si collega in ricezione dovra pure specificare id a cui è interessato 
       #in modo tale che, se sono presenti il server invia esattamente i dati relativi a quel client
       # ad esempio data deve essere una mappa in uni la chiave corrisponde al id mentre il valore ai dati

class Server:
    def __init__(self):
        #self.data = None  # Dati da inviare al secondo client
        self.data={}

    def start_server(self):
        host = 'localhost'
        port = 12345

        server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server_socket.bind((host, port))
        server_socket.listen(1)

        print("Server is listening on port", port)

        while True:
            client_sock, client_address = server_socket.accept()
            new_thread = ClientThread(client_address, client_sock, self)
            new_thread.start()

if __name__ == '__main__':
    Server().start_server()
