package main

import (
	"fmt"
	"net"
)

func handleConnection(conn net.Conn) {
	defer conn.Close()

	buffer := make([]byte, 1024)

	for {
		// Read data from the connection
		n, err := conn.Read(buffer)
		if err != nil {
			fmt.Println("Error reading:", err)
			data := buffer[:n]
			fmt.Printf("Received: %s", data)
			conn.Write(data)
			return
		}

		// Echo the received data back to the client
		data := buffer[:n]
		fmt.Printf("Received: %s", data)
		conn.Write(data)
	}
}

func main() {
	// Listen on port 8080
	listener, err := net.Listen("tcp", ":8080")
	if err != nil {
		fmt.Println("Error listening:", err)
		return
	}
	defer listener.Close()

	fmt.Println("Mediator server listening on :8080")

	for {
		// Accept a new connection
		conn, err := listener.Accept()
		if err != nil {
			fmt.Println("Error accepting connection:", err)
			continue
		}

		// Handle the connection in a separate goroutine
		go handleConnection(conn)
	}
}
