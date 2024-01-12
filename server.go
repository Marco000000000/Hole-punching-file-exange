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
	host := "127.0.0.1"
	port := "5005"
	fmt.Print("starting server")

	addr, err := net.ResolveTCPAddr("tcp", host+":"+port)
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println(addr)
	listener, err := net.ListenTCP("tcp", addr)
	fmt.Println(listener)

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
