package main

import (
	"fmt"
	"math/rand"
	"net/http"
)

func main() {
	http.HandleFunc("/", handleRequest)
	fmt.Println("Server in ascolto su http://localhost:8000")
	http.ListenAndServe(":8000", nil)
}

func handleRequest(w http.ResponseWriter, r *http.Request) {
	// Genera una sequenza casuale di numeri tra 0 e 99
	randomSequence := generateRandomSequence()

	// Codifica la sequenza in caratteri ASCII
	encodedSequence := encodeSequence(randomSequence)

	// Invia la sequenza codificata al client
	fmt.Fprintf(w, "%s", encodedSequence)
}

func generateRandomSequence() []int {
	//rand.Seed(time.Now().UnixNano())
	const sequenceLength = 4 //se voglio una sequenza di diversa lunguezza cambaire qui
	randomSequence := make([]int, sequenceLength)
	for i := 0; i < sequenceLength; i++ {
		randomSequence[i] = rand.Intn(58) // Genera numeri casuali tra 0 e 58 in quanto le lettere
		//tra maiuscole e minuscole sono codificate in 57 numeri
		//ci sono sei caratteri non lettere cioè ' [ ] _ \ ^ bisogna trovare un modo per toglierli ? o vanno bene ?
	}
	return randomSequence
}

func encodeSequence(sequence []int) string {
	// Codifica la sequenza in caratteri ASCII
	encoded := ""
	for _, num := range sequence {
		// Aggiungi il carattere corrispondente al numero (ASCII)
		char := rune(num + 65) // 'A' per num=0, 'B' per num=1
		//visto che num ha un valore tra 0 e 57 aggiungendo 65
		//si entrerà nel range in cui sono codificate lettere maiscole e minuscole
		encoded += string(char)
	}
	return encoded
}
