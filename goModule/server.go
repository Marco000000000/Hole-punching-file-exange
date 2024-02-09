package main

import (
	"database/sql"
	"encoding/binary"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"math/rand"
	"net"
	"net/http"
	"strconv"
	"strings"
	"sync"
	"time"
	"unicode"

	_ "github.com/go-sql-driver/mysql"
)

var clientsMutex sync.Mutex
var clients = make(map[string]*Client)
var db *sql.DB

type UserLogin struct {
	Username string `json:"username"`
	Password string `json:"password"`
}
type User struct {
	Username string `json:"username"`
	Code     string `json:"code"`
}
type Request struct {
	Username      string `json:"username"`
	Code          string `json:"code"`
	Peer_username string `json:"peer_username"`
	Peer_code     string `json:"peer_code"`
	Operation     int    `json:"operation"`
	Path          string `json:"path"`
}
type ForResponseData struct {
	Channels   []http.ResponseWriter
	Username   []string
	Path       []string
	Operations []int
}
type hearthBitMessage struct {
	Code      string `json:"code"`
	Operation int    `json:"operation"`
	Path      string `json:"path"`
}

var waitingRequests map[string]ForResponseData
var waitingRequestsMutex sync.Mutex
var sendedRequests map[string]http.ResponseWriter
var sendedRequestsMutex sync.Mutex
var holeData map[string]string
var holeMutex sync.Mutex

type Client struct {
	conn  net.Conn
	pub   *net.TCPAddr
	priv  *net.TCPAddr
	timer time.Time
}

func HandleHoleConnect(conn net.Conn) {
	msg := recvMsg(conn)
	var request Request
	err := json.Unmarshal(msg, &request)
	if err != nil {
		conn.Close()
		return
	}

	fmt.Println("listenAddr")

	addr := conn.RemoteAddr().(*net.TCPAddr)
	log.Printf("indirizzo addr[0]: %s", addr.IP.String())
	fmt.Println(addr)

	log.Printf("connection address: %s", addr)

	data := recvMsg(conn)

	privAddr, err := msgToAddr(data)
	if err != nil {
		log.Println("Error decoding privAddr:", err)
		conn.Close()
		return
	}

	sendMsg(conn, addrToMsg(addr))
	data = recvMsg(conn)
	fmt.Println("string data/", string(data), "/")
	dataAddr, err := msgToAddr(data)
	if err != nil {
		log.Println("Error decoding dataAddr:", err)
		conn.Close()
		return
	}
	fmt.Println("string data/", dataAddr.String(), "/")
	var temp *Client
	if dataAddr.IP.String() == addr.IP.String() && dataAddr.Port == addr.Port {
		log.Println("client reply matches")
		temp = &Client{conn, addr, privAddr, time.Now()}
	} else {
		log.Println("client reply did not match")
		conn.Close()
		return
	}
	fmt.Println(clients)
	log.Printf("server - received data: %s", data)

	clientsMutex.Lock()
	log.Printf("number of clients: %d", len(clients))
	connTime, ok := clients[request.Peer_username+request.Peer_code]
	if ok {
		delete(clients, request.Peer_username+request.Peer_code)
		clientsMutex.Unlock()
		value, ok1 := holeData[request.Username+request.Code]

		if ok1 || value == request.Peer_username+request.Peer_code {
			holeMutex.Lock()
			delete(holeData, request.Username+request.Code)
			holeMutex.Unlock()
		} else {
			conn.Close()
			return
		}

		c1 := temp
		c2 := connTime
		c1msg := addrToMsg(c1.pub)
		c1msg = append(c1msg, byte('|'))
		temp1 := append(c1msg, addrToMsg(c1.priv)...)
		c2msg := addrToMsg(c2.pub)
		c2msg = append(c2msg, byte('|'))
		temp2 := append(c2msg, addrToMsg(c2.priv)...)
		log.Printf("server - send client %s info to: %s", c1.pub, temp2)
		sendMsg(c1.conn, temp2)
		log.Printf("server - send client %s info to: %s", c2.pub, temp1)

		sendMsg(c2.conn, temp1)

	} else {
		clients[request.Peer_username+request.Peer_code] = temp
		clientsMutex.Unlock()
	}

}
func garbageCollector() {
	for {
		for key, value := range clients {
			if int(time.Since(value.timer).Seconds()) > 5 {

				clientsMutex.Lock()
				delete(clients, key)
				clientsMutex.Unlock()

			}
		}
		time.Sleep(2 * time.Second)
	}
}
func handlerHolePunching(host string, port int) {

	listenAddr, err := net.ResolveTCPAddr("tcp", fmt.Sprintf("%s:%d", host, port))
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
		go HandleHoleConnect(conn)

	}
}

func keepNumbersAndDot(input string) string {
	var result string

	for _, char := range input {
		if unicode.IsDigit(char) || char == '.' {
			result += string(char)
		}
	}

	return result
}
func recvMsg(conn net.Conn) []byte {
	data := make([]byte, 4)
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
			break
		}
		count += readBytes
	}

	return readBuff
}
func sendMsg(conn net.Conn, msg []byte) error {
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

func containsDangerousCharacters(s string) bool {
	dangerousCharacters := []string{"'", ";", `"`, `\`, `%`, "&", "<", ">", "(", ")", "[", "]", "{", "}", "|", "*", "?", ":", "+", "-", "="}
	for _, c := range dangerousCharacters {
		if strings.Contains(s, c) {
			return true
		}
	}
	return false
}

func handleLogin(w http.ResponseWriter, r *http.Request) {
	body, err := io.ReadAll(r.Body)
	if err != nil {
		http.Error(w, "Failed to read request body", http.StatusInternalServerError)
		return
	}

	// Close the request body
	defer r.Body.Close()

	var user UserLogin
	err = json.Unmarshal(body, &user)
	if err != nil {
		http.Error(w, "Failed to parse JSON", http.StatusBadRequest)
		return
	}
	if containsDangerousCharacters(user.Username + user.Password) {
		response := map[string]string{"error": "not allowed character"}
		jsonResponse, err := json.Marshal(response)
		if err != nil {
			http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusBadRequest)
		w.Write(jsonResponse)
		return
	}
	stmt, err := db.Prepare("SELECT * FROM users WHERE username = ? and password= ?")
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error preparing statement:", err)
		return
	}
	defer stmt.Close()

	// Execute the query
	rows, err := stmt.Query(user.Username, user.Password)
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error executing query:", err)
		return
	}
	defer rows.Close()

	// Process query results
	if rows.Next() {
		// Handle each row here
		randomSequence := generateRandomSequence(4)

		// Codifica la sequenza in caratteri ASCII
		encodedSequence := encodeSequence(randomSequence)
		stmt, err := db.Prepare("UPDATE users SET code = ? WHERE username = ?")
		if err != nil {
			fmt.Println("Error preparing update statement:", err)
			return
		}
		defer stmt.Close()

		// Execute the update statement
		_, err = stmt.Exec(encodedSequence, user.Username) // Assuming we want to update the record with ID 1
		if err != nil {
			fmt.Println("Error updating record:", err)
			return
		}
		response := map[string]string{"code": encodedSequence}
		jsonResponse, err := json.Marshal(response)
		if err != nil {
			http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		w.Write(jsonResponse)
		return
	}
	response := map[string]string{"error": "bad parameters"}
	jsonResponse, err := json.Marshal(response)
	if err != nil {
		http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusBadRequest)
	w.Write(jsonResponse)

}

func handleSignin(w http.ResponseWriter, r *http.Request) {
	body, err := io.ReadAll(r.Body)
	if err != nil {
		http.Error(w, "Failed to read request body", http.StatusInternalServerError)
		return
	}

	// Close the request body
	defer r.Body.Close()

	var user UserLogin
	err = json.Unmarshal(body, &user)
	if err != nil {
		http.Error(w, "Failed to parse JSON", http.StatusBadRequest)
		return
	}
	if containsDangerousCharacters(user.Username + user.Password) {
		response := map[string]string{"error": "not allowed character"}
		jsonResponse, err := json.Marshal(response)
		if err != nil {
			http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusBadRequest)
		w.Write(jsonResponse)
		return
	}

	stmt, err := db.Prepare("SELECT * FROM users WHERE username = ?")
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error preparing statement:", err)
		return
	}
	defer stmt.Close()

	// Execute the query
	rows, err := stmt.Query(user.Username, user.Password)
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error executing query:", err)
		return
	}
	defer rows.Close()

	// Process query results
	if !rows.Next() {
		randomSequence := generateRandomSequence(4)

		// Codifica la sequenza in caratteri ASCII
		encodedSequence := encodeSequence(randomSequence)
		insertQuery := "INSERT INTO users (username, password, code) VALUES (?, ?, ?)"
		_, err = db.Exec(insertQuery, user.Username, user.Password, encodedSequence)
		if err != nil {
			http.Error(w, "Failed to insert data into database", http.StatusInternalServerError)
			return
		}
		// Handle each row here

		response := map[string]string{"code": encodedSequence}
		jsonResponse, err := json.Marshal(response)
		if err != nil {
			http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		w.Write(jsonResponse)
		return
	}
	response := map[string]string{"error": "bad parameters"}
	jsonResponse, err := json.Marshal(response)
	if err != nil {
		http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusBadRequest)
	w.Write(jsonResponse)

}
func updateSendedRequests(tempmap map[string]http.ResponseWriter) {
	sendedRequestsMutex.Lock()
	for key, value := range tempmap {
		sendedRequests[key] = value
	}
	sendedRequestsMutex.Unlock()
}
func handleHearthBit(w http.ResponseWriter, r *http.Request) {
	body, err := io.ReadAll(r.Body)
	if err != nil {
		http.Error(w, "Failed to read request body", http.StatusInternalServerError)
		return
	}

	// Close the request body
	defer r.Body.Close()

	var user User
	err = json.Unmarshal(body, &user)
	if err != nil {
		http.Error(w, "Failed to parse JSON", http.StatusBadRequest)
		return
	}
	if containsDangerousCharacters(user.Username + user.Code) {
		response := map[string]string{"error": "not allowed character"}
		jsonResponse, err := json.Marshal(response)
		if err != nil {
			http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusBadRequest)
		w.Write(jsonResponse)
		return
	}

	stmt, err := db.Prepare("SELECT * FROM users WHERE username = ? and code= ?")
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error preparing statement:", err)
		return
	}
	defer stmt.Close()

	// Execute the query
	rows, err := stmt.Query(user.Username, user.Code)
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error executing query:", err)
		return
	}
	defer rows.Close()

	// Process query results
	if rows.Next() {
		waitingRequestsMutex.Lock()

		value, ok := waitingRequests[user.Username]
		defer waitingRequestsMutex.Unlock()
		stmt, err := db.Prepare("UPDATE users SET last_hearthbit = NOW() WHERE username = ?")
		if err != nil {
			log.Fatal("Error preparing update statement:", err)
		}
		defer stmt.Close()

		// Execute the update statement
		_, err = stmt.Exec(user.Username)
		if err != nil {
			log.Fatal("Error updating record:", err)
		}

		if ok {
			tempmap := map[string]http.ResponseWriter{}
			response := []hearthBitMessage{}
			for i := range value.Operations {
				code := encodeSequence(generateRandomSequence(20))
				response = append(response, hearthBitMessage{Code: code, Path: value.Path[i], Operation: value.Operations[i]})
				tempmap[code] = value.Channels[i]
			}
			delete(waitingRequests, user.Username)
			go updateSendedRequests(tempmap)
			jsonResponse, err := json.Marshal(response)
			if err != nil {
				http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
				return
			}

			w.Header().Set("Content-Type", "application/json")
			w.WriteHeader(http.StatusBadRequest)
			w.Write(jsonResponse)

			return
		} else {
			response := map[string]bool{"heathbit": true}
			jsonResponse, err := json.Marshal(response)
			if err != nil {
				http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
				return
			}

			w.Header().Set("Content-Type", "application/json")
			w.WriteHeader(http.StatusBadRequest)
			w.Write(jsonResponse)
			return
		}

	}
	response := map[string]string{"error": "bad code"}
	jsonResponse, err := json.Marshal(response)
	if err != nil {
		http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusBadRequest)
	w.Write(jsonResponse)
}
func handleRequest(w http.ResponseWriter, r *http.Request) {
	body, err := io.ReadAll(r.Body)
	if err != nil {
		http.Error(w, "Failed to read request body", http.StatusInternalServerError)
		return
	}

	// Close the request body
	defer r.Body.Close()

	var request Request
	err = json.Unmarshal(body, &request)
	if err != nil {
		http.Error(w, "Failed to parse JSON", http.StatusBadRequest)
		return
	}
	if containsDangerousCharacters(request.Username+request.Code+request.Peer_code+request.Peer_username) || request.Operation < 0 || request.Operation > 4 || request.Operation == 2 {
		response := map[string]string{"error": "not allowed character or operation"}
		jsonResponse, err := json.Marshal(response)
		if err != nil {
			http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusBadRequest)
		w.Write(jsonResponse)
		return
	}
	//controllare se è collegato
	stmt, err := db.Prepare("SELECT * FROM users WHERE username = ? and code= ?")
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error preparing statement:", err)
		return
	}
	defer stmt.Close()

	// Execute the query
	rows, err := stmt.Query(request.Username, request.Code)
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error executing query:", err)
		return
	}
	stmt, err = db.Prepare("SELECT * FROM users WHERE username = ? and code= ?")
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error preparing statement:", err)
		return
	}
	defer stmt.Close()

	// Execute the query
	rows1, err := stmt.Query(request.Peer_username, request.Peer_code)
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error executing query:", err)
		return
	}
	defer rows1.Close()

	// Process query results
	if rows.Next() && rows1.Next() {
		holeMutex.Lock()

		if request.Operation == 4 {
			_, ok := holeData[request.Username+request.Code]
			if ok {
				response := map[string]string{"error": "server already full of requests"}
				jsonResponse, err := json.Marshal(response)
				if err != nil {
					http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
					return
				}

				w.Header().Set("Content-Type", "application/json")
				w.WriteHeader(http.StatusBadRequest)
				w.Write(jsonResponse)
				return
			} else {
				holeData[request.Peer_username+request.Peer_code] = request.Username + request.Code

			}
		}
		holeMutex.Unlock()
		waitingRequestsMutex.Lock()

		_, ok := waitingRequests[request.Peer_username]
		if ok {
			tempData := waitingRequests[request.Username]
			tempData.Operations = append(tempData.Operations, request.Operation)
			tempData.Channels = append(tempData.Channels, w)
			tempData.Path = append(tempData.Path, request.Path)
			tempData.Username = append(tempData.Username, request.Username)
			waitingRequests[request.Username] = tempData
		} else {

			tempData := ForResponseData{Channels: []http.ResponseWriter{w}, Username: []string{request.Username}, Path: []string{request.Path}, Operations: []int{request.Operation}}
			waitingRequests[request.Peer_username] = tempData

		}
		waitingRequestsMutex.Unlock()

	}
}

func handleResponse(w http.ResponseWriter, r *http.Request) {
	body, err := io.ReadAll(r.Body)
	if err != nil {
		http.Error(w, "Failed to read request body", http.StatusInternalServerError)
		return
	}
	path := r.URL.Path

	// Split the path into segments
	segments := strings.Split(path, "/")

	// Get the last segment
	lastSegment := segments[len(segments)-1]
	// Close the request body
	defer r.Body.Close()
	sendedRequestsMutex.Lock()
	channel, ok := sendedRequests[lastSegment]
	if ok {
		channel.WriteHeader(http.StatusOK)
		channel.Write(body)
		delete(sendedRequests, lastSegment)
		return
	} else {
		response := map[string]string{"error": "no sequest sended"}
		jsonResponse, err := json.Marshal(response)
		if err != nil {
			http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
			return
		}
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusBadRequest)
		w.Write(jsonResponse)
	}
	sendedRequestsMutex.Unlock()
	response := map[string]string{"response": "ok"}
	jsonResponse, err := json.Marshal(response)
	if err != nil {
		http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
		return
	}
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	w.Write(jsonResponse)

}

func generateRandomSequence(sequenceLength int) []int {
	//rand.Seed(time.Now().UnixNano())
	randomSequence := make([]int, sequenceLength)
	for i := 0; i < sequenceLength; i++ {
		randomSequence[i] = rand.Intn(58) // Genera numeri casuali tra 0 e 58 in quanto le lettere 58
		//tra maiuscole e minuscole sono codificate in 57 numeri
		//ci sono sei caratteri non lettere cioè ' [ ] _ \ ^ bisogna trovare un modo per toglierli ? o vanno bene ?
		// sono i caratteri che vanno da 91 a 96
		randomSequence[i] = checkNumber(randomSequence[i])
	}
	return randomSequence
}

func checkNumber(number int) int {
	if number >= 26 && number <= 31 {
		number = rand.Intn(58)
		number = checkNumber(number)
	}
	return number
}

func encodeSequence(sequence []int) string {
	// Codifica la sequenza in caratteri ASCII
	encoded := ""
	for _, num := range sequence {
		// Aggiungi il carattere corrispondente al numero (ASCII) 65
		char := rune(num + 65) // 'A' per num=0, 'B' per num=1
		//visto che num ha un valore tra 0 e 57 aggiungendo 65
		//si entrerà nel range in cui sono codificate lettere maiscole e minuscole
		encoded += string(char)
	}
	return encoded
}

func main() {
	ipAddress := GetLocalIP()
	fmt.Println(ipAddress)
	go garbageCollector()
	go handlerHolePunching(ipAddress.String(), 5000)
	waitingRequests = make(map[string]ForResponseData)
	db, err := sql.Open("mysql", "root:password@tcp(localhost:3306)/db_holepunch")
	if err != nil {
		fmt.Println("Error connecting to database:", err)
		return
	}
	defer db.Close()
	http.HandleFunc("/login", handleLogin)
	http.HandleFunc("/signin", handleSignin)
	http.HandleFunc("/hearthBit", handleHearthBit)
	http.HandleFunc("/request", handleRequest)
	http.HandleFunc("/response", handleResponse)
	fmt.Println("Server in ascolto su http://localhost:80")
	http.ListenAndServe(":80", nil)
}
