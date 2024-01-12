package main

import (
	"encoding/binary"
	"fmt"
	"log"
	"net"
)

const (
	SO_REUSEADDR = 0x0004
	SO_REUSEPORT = 0x000F
)

type Client struct {
	conn     net.Conn
	addr     net.Addr
	privAddr net.Addr
}

func recvMsg(conn net.Conn) []byte {
	var length uint32
	binary.Read(conn, binary.BigEndian, &length)
	data := make([]byte, length)
	conn.Read(data)
	return data
}

func sendMsg(conn net.Conn, data []byte) {
	length := uint32(len(data))
	binary.Write(conn, binary.BigEndian, length)
	conn.Write(data)
}

func main() {
	fmt.Println("starting server")

	listener, err := net.Listen("tcp", ":8080")

	if err != nil {
		log.Fatal(err)
	}
	defer listener.Close()
	for {
		fmt.Println("a")
		conn, err := listener.Accept()
		fmt.Println("b")

		if err != nil {
			log.Println(err)
			continue
		}

		fmt.Printf(conn.LocalAddr().Network())
		go handleConnection(conn)
	}
}

func handleConnection(conn net.Conn) {
	defer conn.Close()

	var data [4]byte
	_, err := conn.Read(data[:])
	if err != nil {
		log.Println("Error reading data:", err)
		return
	}

	privAddr := string(data[:])
	sendMsg(conn, []byte(privAddr))

	// Continue with the rest of your logic...

}
