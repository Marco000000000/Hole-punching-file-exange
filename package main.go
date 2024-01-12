package main

import (
	"fmt"
	"log"
	"net"
)

func main() {
	host := "127.0.0.1"
	port := 5005

	addr := fmt.Sprintf("%s:%d", host, port)
	listener, err := net.Listen("tcp", addr)
	if err != nil {
		log.Fatal(err)
	}
	defer listener.Close()

	log.Printf("Server listening on %s", addr)

	for {
		conn, err := listener.Accept()
		if err != nil {
			log.Println(err)
			continue
		}

		log.Println("Accepted connection from", conn.RemoteAddr())

		// Handle the connection in a separate goroutine or function.
		go handleConnection(conn)
	}
}

func handleConnection(conn net.Conn) {
	// Implement your logic to handle the connection here.
	// For example, read and write data.
	defer conn.Close()
}
