package main

import (
	"database/sql"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"math/rand"
	"net/http"
	"strings"
	"sync"

	_ "github.com/go-sql-driver/mysql"
)

var db *sql.DB

func main() {
	waitingRequests = make(map[string]ForResponseData)
	db, err := sql.Open("mysql", "root:password@tcp(localhost:3306)/db_holepunch")
	if err != nil {
		fmt.Println("Error connecting to database:", err)
		return
	}
	defer db.Close()
	http.HandleFunc("/login", handleLogin)
	http.HandleFunc("/signin", handleSignin)
	http.HandleFunc("/hearthBit", handleRequest)
	http.HandleFunc("/request", handleRequest)
	http.HandleFunc("/response", handleRequest)
	fmt.Println("Server in ascolto su http://localhost:8000")
	http.ListenAndServe(":80", nil)
}

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
			for i, _ := range value.Operations {
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
				holeData[request.Username+request.Code] = request.Peer_username + request.Peer_code

			}
		}
		holeMutex.Unlock()
		waitingRequestsMutex.Lock()

		_, ok := waitingRequests[request.Peer_username]
		if ok == true {
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
	response := map[string]string{"error": "no sequest sended"}
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
	}
	return randomSequence
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
