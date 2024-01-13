package main

import (
	"encoding/binary"
	"fmt"
	"io"
	"log"
	"net"
	"net/http"
	"strconv"
	"strings"
	"sync"
	"unicode"
)

var addressConnectedMutex sync.Mutex
var addressConnected []string

var clientsMutex sync.Mutex
var clients = make(map[string]*Client)

type Client struct {
	conn net.Conn
	pub  *net.TCPAddr
	priv *net.TCPAddr
}

func removeFromAddressConnected(ip string) {
	for i, v := range addressConnected {
		if v == ip {
			addressConnected = append(addressConnected[:i], addressConnected[i+1:]...)
			break
		}
	}
}

func main1(host string, port int) {

	listenAddr, err := net.ResolveTCPAddr("tcp", fmt.Sprintf("%s:%d", "127.0.0.1", port))
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println(listenAddr)
	listener, err := net.ListenTCP("tcp", listenAddr)
	if err != nil {
		log.Fatal(err)
	}
	defer listener.Close()

	for {
		conn, err := listener.Accept()

		if err != nil {
			netErr, ok := err.(net.Error)
			if ok && netErr.Timeout() {
				continue
			} else {
				log.Fatal(err)
			}
		}
		fmt.Println("listenAddr")

		addr := conn.RemoteAddr().(*net.TCPAddr)
		log.Printf("indirizzo addr[0]: %s", addr.IP.String())
		fmt.Println(addr)

		log.Printf("connection address: %s", addr)

		data := recvMsg1(conn)

		privAddr, err := msgToAddr(data)
		if err != nil {
			log.Println("Error decoding privAddr:", err)
			conn.Close()
			continue
		}

		sendMsg(conn, addrToMsg(addr))
		data = recvMsg1(conn)
		fmt.Println("string data/", string(data), "/")
		dataAddr, err := msgToAddr(data)
		if err != nil {
			log.Println("Error decoding dataAddr:", err)
			conn.Close()
			continue
		}
		fmt.Println("string data/", dataAddr.String(), "/")

		if dataAddr.IP.String() == addr.IP.String() && dataAddr.Port == addr.Port {
			log.Println("client reply matches")
			clientsMutex.Lock()
			clients[keepNumbersAndDot(addr.IP.String()+":"+strconv.Itoa(addr.Port))] = &Client{conn, addr, privAddr}
			clientsMutex.Unlock()
		} else {
			log.Println("client reply did not match")
			conn.Close()
		}
		fmt.Println(clients)
		log.Printf("server - received data: %s", data)

		clientsMutex.Lock()
		log.Printf("number of clients: %d", len(clients))

		if len(clients) == 2 {
			var c1, c2 *Client
			for _, client := range clients {
				if c1 == nil {
					c1 = client
				} else {
					c2 = client
				}
			}
			c1msg := addrToMsg(c1.pub)
			c1msg = append(c1msg, byte('|'))
			temp1 := append(c1msg, addrToMsg(c1.priv)...)
			c2msg := addrToMsg(c2.pub)
			c2msg = append(c2msg, byte('|'))
			temp2 := append(c2msg, addrToMsg(c2.priv)...)
			log.Printf("server - send client info to: %s", c1.pub)
			fmt.Println(string(temp1))
			sendMsg(c1.conn, temp2)
			log.Printf("server - send client info to: %s", c2.pub)
			fmt.Println(string(temp2))

			sendMsg(c2.conn, temp1)

			clientsMutex.Unlock()
			clientsMutex.Lock()
			delete(clients, c1.pub.IP.String())
			delete(clients, c2.pub.IP.String())
			for k := range clients {
				delete(clients, k)
			}

			addressConnectedMutex.Lock()

			removeFromAddressConnected(c1.pub.IP.String())
			removeFromAddressConnected(c2.pub.IP.String())
			addressConnectedMutex.Unlock()
			clientsMutex.Unlock()
		} else {
			clientsMutex.Unlock()
		}
	}
}

func recvMsg(conn net.Conn) []byte {
	// Implement recvMsg logic as needed
	data := make([]byte, 4) // Assuming you know the length beforehand
	_, err := conn.Read(data)
	if err != nil {
		log.Println("Error reading data:", err)
	}
	return data
}
func keepNumbersAndDot(input string) string {
	var result string

	for _, char := range input {
		// Keep only numbers and the period
		if unicode.IsDigit(char) || char == '.' {
			result += string(char)
		}
	}

	return result
}
func recvMsg1(conn net.Conn) []byte {
	// Implement recvMsg logic as needed
	data := make([]byte, 4) // Assuming you know the length beforehand
	io.ReadFull(conn, data)
	n := int(binary.BigEndian.Uint32(data))
	fmt.Println("n=", n)
	readBuff := make([]byte, n)
	count := 0
	for count < n {
		readBytes, err := io.ReadFull(conn, readBuff)
		fmt.Println("count:", count)

		if err != nil && err != io.EOF {
			if err != io.ErrUnexpectedEOF {
				count += readBytes
				fmt.Println("error count:", count)

			}
			// Handle the error if it's not EOF or UnexpectedEOF
			break
		}
		count += readBytes
	}

	return readBuff
}
func sendMsg(conn net.Conn, msg []byte) error {
	// Implement sendMsg logic as needed
	lengthBytes := make([]byte, 4)
	binary.BigEndian.PutUint32(lengthBytes, uint32(len(msg)))
	ms := append(lengthBytes, msg...)
	n, err := conn.Write(ms)
	fmt.Println("numero di byte spediti =", n)
	fmt.Println("numero di byte da spedire =", len(msg)+4)

	return err
}

func msgToAddr(data []byte) (*net.TCPAddr, error) {
	// Implement msgToAddr logic as needed
	addrStr := string(data)
	parts := strings.Split(addrStr, ":")
	fmt.Println(parts)
	if len(parts) != 2 {
		return nil, fmt.Errorf("invalid address format")
	}

	ip := parts[0]
	ip = keepNumbersAndDot(ip)
	fmt.Println(len(ip))
	fmt.Println(ip)

	port, err := strconv.Atoi(keepNumbersAndDot(parts[1]))
	fmt.Println("port=", port)

	if err != nil {
		return nil, err
	}
	fmt.Println("net.TCPAddr{IP: net.ParseIP(ip), Port: port}", net.TCPAddr{IP: net.ParseIP(ip), Port: port})

	return &net.TCPAddr{IP: net.ParseIP(ip), Port: port}, nil
}

func addrToMsg(addr *net.TCPAddr) []byte {
	// Implement addrToMsg logic as needed
	addrStr := fmt.Sprintf("%s:%d", addr.IP.String(), addr.Port)
	length := make([]byte, 4)
	binary.BigEndian.PutUint32(length, uint32(len(addrStr)))
	return append(length, []byte(addrStr)...)
}

func holePunchControlHandler(w http.ResponseWriter, r *http.Request) {
	if r.URL.Path != "/holePunch/1" || r.Method != "GET" {
		http.Error(w, "Not Found", http.StatusNotFound)
		return
	}

	addressConnectedMutex.Lock()
	addressConnected = append(addressConnected, r.RemoteAddr)
	fmt.Println(addressConnected)
	addressConnectedMutex.Unlock()

	w.Write([]byte("ok"))
}

func parseInt(s string) int {
	val, err := strconv.Atoi(s)
	if err != nil {
		fmt.Println("Error converting string to int:", err)
		// You might want to handle the error accordingly, e.g., log and provide a default value
	}
	return val
}
func init() {
	log.SetFlags(log.LstdFlags | log.Lmicroseconds)
}
func GetLocalIP() net.IP {
	conn, err := net.Dial("udp", "8.8.8.8:80")
	if err != nil {
		log.Fatal(err)
	}
	defer conn.Close()

	localAddress := conn.LocalAddr().(*net.UDPAddr)

	return localAddress.IP
}

func main() {
	go func() {
		err := http.ListenAndServe(":80", http.HandlerFunc(holePunchControlHandler))
		if err != nil {
			log.Fatal(err)
		}
	}()
	ipAddress := GetLocalIP()
	fmt.Println(ipAddress)

	main1(ipAddress.String(), 5000)
}
