package main

import (
	"encoding/binary"
	"fmt"
	"io"
	"math/rand"
	"net"
	"strconv"
	"strings"
	"unicode"
)

// funzione ausiliaria che implementa la chiusura delle richieste pendenti e rimuove il rispettivo campo da waitingRequests
func RemoveKeyFromWaitingRequests(keyToDelete string) {
	waitingRequestsMutex.Lock()
	for i := 0; i < len(waitingRequests[keyToDelete].link); i++ {
		close(waitingRequests[keyToDelete].link[i])
	}
	delete(waitingRequests, keyToDelete)
	waitingRequestsMutex.Unlock()
}

// funzioni ausiliarie per l'implementazione del protocollo di scambio degli indirizzi
func KeepNumbersAndDot(input string) string {
	var result string

	for _, char := range input {
		if unicode.IsDigit(char) || char == '.' {
			result += string(char)
		}
	}

	return result
}
func RecvMsg(conn net.Conn) []byte {
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
func SendMsg(conn net.Conn, msg []byte) error {
	lengthBytes := make([]byte, 4)
	binary.BigEndian.PutUint32(lengthBytes, uint32(len(msg)))
	ms := append(lengthBytes, msg...)
	n, err := conn.Write(ms)
	fmt.Println("numero di byte spediti =", n)
	fmt.Println("numero di byte da spedire =", len(msg)+4)

	return err
}

func MsgToAddr(data []byte) (*net.TCPAddr, error) {
	// Implement msgToAddr logic as needed
	addrStr := string(data)
	parts := strings.Split(addrStr, ":")
	fmt.Println(parts)
	if len(parts) != 2 {
		return nil, fmt.Errorf("invalid address format")
	}

	ip := parts[0]
	ip = KeepNumbersAndDot(ip)
	fmt.Println(len(ip))
	fmt.Println(ip)

	port, err := strconv.Atoi(KeepNumbersAndDot(parts[1]))
	fmt.Println("port=", port)

	if err != nil {
		return nil, err
	}
	fmt.Println("net.TCPAddr{IP: net.ParseIP(ip), Port: port}", net.TCPAddr{IP: net.ParseIP(ip), Port: port})

	return &net.TCPAddr{IP: net.ParseIP(ip), Port: port}, nil
}

func AddrToMsg(addr *net.TCPAddr) []byte {
	// Implement addrToMsg logic as needed
	addrStr := fmt.Sprintf("%s:%d", addr.IP.String(), addr.Port)
	length := make([]byte, 4)
	binary.BigEndian.PutUint32(length, uint32(len(addrStr)))
	return append(length, []byte(addrStr)...)
}

// funzione che ritorna un booleano se ci sono dei caratteri non permessi
func ContainsDangerousCharacters(s string) bool {
	dangerousCharacters := []string{"'", ";", `"`, `\`, `%`, "&", "<", ">", "(", ")", "[", "]", "{", "}", "|", "*", "?", ":", "+", "-", "="}
	for _, c := range dangerousCharacters {
		if strings.Contains(s, c) {
			return true
		}
	}
	return false
}

//funzione ausiliaria per l'aggiornamento parallelo della struttura updateSended

func UpdateSendedRequests(tempmap map[string]ConnectHandler) {
	sendedRequestsMutex.Lock()
	fmt.Println("updateSendedRequests")
	fmt.Println(sendedRequests)
	fmt.Println(tempmap)
	if sendedRequests == nil {
		sendedRequests = make(map[string]ConnectHandler) // Reinitialize the map
	}
	for key, value := range tempmap {
		sendedRequests[key] = value
	}
	sendedRequestsMutex.Unlock()
}

// funzione di generazione dei codici
func GenerateRandomSequence(SequenceLength int) []int {
	randomSequence := make([]int, SequenceLength)
	for i := 0; i < SequenceLength; i++ {
		randomSequence[i] = rand.Intn(58) // Genera numeri casuali tra 0 e 58 in quanto le lettere 58
		//tra maiuscole e minuscole sono codificate in 57 numeri
		// sono i caratteri che vanno da 91 a 96
		randomSequence[i] = CheckNumber(randomSequence[i]) //ci sono sei caratteri non lettere cioè ' [ ] _ \ ^

	}
	return randomSequence
}

// funzione che rimuove caratteri non desiderati
func CheckNumber(number int) int {
	if number >= 26 && number <= 31 {
		number = rand.Intn(58)
		number = CheckNumber(number)
	}
	return number
}

// passa da interi a caratteri
func EncodeSequence(sequence []int) string {
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
