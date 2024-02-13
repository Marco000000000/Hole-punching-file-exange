package main

import (
	"database/sql"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net"
	"net/http"
	"strings"
	"sync"
	"time"

	_ "github.com/go-sql-driver/mysql"
)

// struttura e mutex associato per l'operazione di hole punch
var clientsMutex sync.Mutex
var clients = make(map[string]*Client)

// collegamento al db
var db *sql.DB

//timer globali per la gestione della memoria

var waitingRequestsTimer time.Time = time.Now()
var sendedRequestsTimer time.Time = time.Now()

// struttura e mutex associato per la gestione della request
var waitingRequests map[string]ForResponseData
var waitingRequestsMutex sync.Mutex

// struttura e mutex associato per la gestione delle richieste già restituite
var sendedRequests map[string]connectHandler
var sendedRequestsMutex sync.Mutex

// struttura e mutex associato per la gestione dei permessi usati per l'hole punch

var holeData map[string]timeAndString
var holeMutex sync.Mutex

// funzione handler che implementa lo scambio degli indirizzi ip tra client se passano i controlli di sicurezza
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
	fmt.Print("clients:")

	fmt.Println(clients)
	log.Printf("server - received data: %s", data)

	clientsMutex.Lock()
	log.Printf("number of clients: %d", len(clients))
	fmt.Println(request)
	var connTime *Client
	var ok bool

	if request.Peer_username != "" {
		connTime, ok = clients[request.Peer_username+request.Peer_code]
	} else {
		connTime, ok = clients[request.Username+request.Code]

	}

	if ok {
		delete(clients, request.Peer_username+request.Peer_code)
		clientsMutex.Unlock()
		value, ok1 := holeData[request.Username+request.Code]

		if ok1 || value.text == request.Peer_username+request.Peer_code {
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

// funzione per la gestione della memoria che pulisce le strutture dati se restano richieste pendenti senza risposta
func garbageCollector() {
	for {
		for key, value := range clients {
			if int(time.Since(value.timer).Seconds()) > 5 {

				clientsMutex.Lock()
				delete(clients, key)
				clientsMutex.Unlock()

			}

		}
		if time.Now().Second()-waitingRequestsTimer.Second() > 10 {
			for forDeleteKey, requestForUser := range waitingRequests {
				for i := 0; i < len(requestForUser.link); i++ {
					if time.Now().Second()-requestForUser.timer[i].Second() > 10 {
						removeKeyFromWaitingRequests(forDeleteKey)
					}
				}
			}
			waitingRequestsTimer = time.Now()
		}
		time.Sleep(2 * time.Second)

	}
}

// thread principale che accetta le richieste di hole punching
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

func init() { //iniziaizzazione del logger
	log.SetFlags(log.LstdFlags | log.Lmicroseconds)
}
func GetLocalIP() net.IP { //ottenimento dell'indirizzo ip
	conn, err := net.Dial("udp", "8.8.8.8:80")
	if err != nil {
		log.Fatal(err)
	}
	defer conn.Close()

	localAddress := conn.LocalAddr().(*net.UDPAddr)

	return localAddress.IP
}

// funzione di handle delle goroutine di login
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

	rows, err := stmt.Query(user.Username, user.Password)
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error executing query:", err)
		return
	}
	defer rows.Close()

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

//funzione di handle delle goroutine di Signin

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
	fmt.Println(user)

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
	fmt.Println("containsDangerousCharacters")
	fmt.Println(db)
	stmt, err := db.Prepare("SELECT * FROM users WHERE username = ?")
	fmt.Println(stmt)

	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error preparing statement:", err)
		return
	}
	defer stmt.Close()

	rows, err := stmt.Query(user.Username)
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error executing query:", err)
		return
	}
	defer rows.Close()
	fmt.Println("query")

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
	response := map[string]string{"error": "Username already in use"}
	jsonResponse, err := json.Marshal(response)
	if err != nil {
		http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusBadRequest)
	w.Write(jsonResponse)

}

//handler della goroutine di gestione degli heathbit http

func handleHearthBit(w http.ResponseWriter, r *http.Request) {
	body, err := io.ReadAll(r.Body)
	if err != nil {
		http.Error(w, "Failed to read request body", http.StatusInternalServerError)
		return
	}

	defer r.Body.Close()

	var user User
	err = json.Unmarshal(body, &user)
	if err != nil {
		http.Error(w, "Failed to parse JSON", http.StatusBadRequest)
		return
	}
	// fmt.Println(user)
	if containsDangerousCharacters(user.Username + user.Code) {
		response := map[string]string{"/error": "not allowed character"}
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
		// fmt.Println("Error preparing statement:", err)
		return
	}
	defer stmt.Close()

	rows, err := stmt.Query(user.Username, user.Code)
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		// fmt.Println("Error executing query:", err)
		return
	}
	defer rows.Close()
	// fmt.Println("query")

	if rows.Next() {
		waitingRequestsMutex.Lock()
		// fmt.Println("waitingRequestsMutex.Lock()")
		value, ok := waitingRequests[user.Username]
		defer waitingRequestsMutex.Unlock()
		stmt, err := db.Prepare("UPDATE users SET last_hearthbit = NOW() WHERE username = ?")
		if err != nil {
			log.Fatal("Error preparing update statement:", err)
		}
		defer stmt.Close()
		// fmt.Println("update query")
		// Execute the update statement
		_, err = stmt.Exec(user.Username)
		if err != nil {
			log.Fatal("Error updating record:", err)
		}

		if ok {
			tempmap := map[string]connectHandler{}
			response := []hearthBitMessage{}
			for i := range value.Operations {
				code := encodeSequence(generateRandomSequence(20))
				response = append(response, hearthBitMessage{Code: code, Path: value.Path[i], Operation: value.Operations[i]})
				tempmap[code] = connectHandler{channel: value.Channels[i], closer: value.link[i], timer: time.Now()}
			}
			// fmt.Println("create tempmap")

			delete(waitingRequests, user.Username)
			go updateSendedRequests(tempmap)
			// fmt.Println("after updateSendedRequests")
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
	response := map[string]string{"/error": "bad code"}
	jsonResponse, err := json.Marshal(response)
	if err != nil {
		http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusBadRequest)
	w.Write(jsonResponse)
}

// handler http della funzione di request
func handleRequest(w http.ResponseWriter, r *http.Request) {
	body, err := io.ReadAll(r.Body)
	if err != nil {
		http.Error(w, "Failed to read request body", http.StatusInternalServerError)
		return
	}

	defer r.Body.Close()

	var request Request
	err = json.Unmarshal(body, &request)
	if err != nil {
		http.Error(w, "Failed to parse JSON", http.StatusBadRequest)
		return
	}
	fmt.Println(request)
	if containsDangerousCharacters(request.Username+request.Code+request.Peer_code+request.Peer_username) || request.Operation < 0 || request.Operation > 4 || request.Operation == 2 {
		response := map[string]string{"/error": "not allowed character or operation"}
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
	fmt.Println("containsDangerousCharacters")

	stmt, err := db.Prepare("SELECT * FROM users WHERE username = ? and code= ? and TIMESTAMPDIFF(SECOND, last_hearthbit, NOW()) < 10")
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error preparing statement:", err)
		return
	}
	defer stmt.Close()

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

	rows1, err := stmt.Query(request.Peer_username, request.Peer_code)
	if err != nil {
		http.Error(w, "Database error", http.StatusInternalServerError)
		fmt.Println("Error executing query:", err)
		return
	}
	defer rows1.Close()
	fmt.Println("Database")

	if rows.Next() && rows1.Next() {
		holeMutex.Lock()
		fmt.Println("holeMutex")

		if request.Operation == 4 {
			_, ok := holeData[request.Username+request.Code]
			if ok {
				response := map[string]string{"/error": "server already full of requests"}
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
				holeData[request.Peer_username+request.Peer_code] = timeAndString{text: request.Username + request.Code, timer: time.Now()}

			}
		}
		holeMutex.Unlock()
		done := make(chan struct{})
		waitingRequestsMutex.Lock()
		fmt.Println("waitingRequests")

		_, ok := waitingRequests[request.Peer_username]
		if ok {
			tempData := waitingRequests[request.Username]
			tempData.Operations = append(tempData.Operations, request.Operation)
			tempData.Channels = append(tempData.Channels, w)
			tempData.Path = append(tempData.Path, request.Path)
			tempData.Username = append(tempData.Username, request.Username)
			tempData.link = append(tempData.link, done)
			tempData.timer = append(tempData.timer, time.Now())
			waitingRequests[request.Username] = tempData
		} else {

			tempData := ForResponseData{Channels: []http.ResponseWriter{w}, link: []chan struct{}{done}, Username: []string{request.Username}, Path: []string{request.Path}, Operations: []int{request.Operation}, timer: []time.Time{time.Now()}}
			waitingRequests[request.Peer_username] = tempData

		}
		waitingRequestsMutex.Unlock()
		fmt.Println(waitingRequests)

		fmt.Printf("lissening")
		select {
		case <-done:

		}
		return
	}
	response := map[string]string{"/error": "bad access data"}
	jsonResponse, err := json.Marshal(response)
	if err != nil {
		http.Error(w, "Failed to create JSON response", http.StatusInternalServerError)
		return
	}
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	w.Write(jsonResponse)
}

// funzione di handle per la richiesta http /response (implementa lo scambio di dati tra le due connessioni)
func handleResponse(w http.ResponseWriter, r *http.Request) {
	body, err := io.ReadAll(r.Body)
	if err != nil {
		http.Error(w, "Failed to read request body", http.StatusInternalServerError)
		return
	}
	path := r.URL.Path

	segments := strings.Split(path, "/")

	lastSegment := segments[len(segments)-1]
	defer r.Body.Close()
	sendedRequestsMutex.Lock()
	w1, ok := sendedRequests[lastSegment]
	fmt.Print("sendedRequests ")
	fmt.Println(sendedRequests)

	if ok {
		w1.channel.WriteHeader(http.StatusOK)
		w1.channel.Write(body)
		close(w1.closer)
		delete(sendedRequests, lastSegment)
		sendedRequestsMutex.Unlock()

		fmt.Println(sendedRequests)
		return
	} else {
		response := map[string]string{"/error": "no sequest sended"}
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

// funzione main che crea i varie handler e goroutine di servizio
func main() {
	ipAddress := GetLocalIP()
	fmt.Println(ipAddress)
	go garbageCollector()
	go handlerHolePunching(ipAddress.String(), 5000)
	waitingRequests = make(map[string]ForResponseData)
	sendedRequests = make(map[string]connectHandler)
	holeData = make(map[string]timeAndString)
	var err error

	db, err = sql.Open("mysql", "root:password@tcp(localhost:3306)/db_holepunch")
	if err != nil {
		fmt.Println("Error connecting to database:", err)
		return
	}
	fmt.Println("connected")
	http.HandleFunc("/login", handleLogin)
	http.HandleFunc("/signin", handleSignin)
	http.HandleFunc("/hearthBit", handleHearthBit)
	http.HandleFunc("/request", handleRequest)
	http.HandleFunc("/response/", handleResponse)
	fmt.Println("Server in ascolto su " + ipAddress.String())
	http.ListenAndServe(":80", nil)
}
