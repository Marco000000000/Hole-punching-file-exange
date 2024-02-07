package main

import (
	"database/sql"
	"encoding/json"
	"fmt"
	"io"
	"math/rand"
	"net/http"
	"strings"

	_ "github.com/go-sql-driver/mysql"
)

var db *sql.DB

func main() {

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
func handleHearthBit(w http.ResponseWriter, r *http.Request) {

}
func handleRequest(w http.ResponseWriter, r *http.Request) {

}

func handleResponse(w http.ResponseWriter, r *http.Request) {

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
