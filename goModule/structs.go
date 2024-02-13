package main

import (
	"net"
	"net/http"
	"time"
)

// struttura per l'operazione di hole punch

type Client struct {
	conn  net.Conn
	pub   *net.TCPAddr
	priv  *net.TCPAddr
	timer time.Time
}

// struttura dati per ricevere l'operazione di login e registrazione
type UserLogin struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

//struttura dati per ricevere l'operazione di request

type User struct {
	Username string `json:"username"`
	Code     string `json:"code"`
}

// struttura dati per ricevere l'operazione di request
type Request struct {
	Username      string `json:"username"`
	Code          string `json:"code"`
	Peer_username string `json:"peer_username"`
	Peer_code     string `json:"peer_code"`
	Operation     int    `json:"operation"`
	Path          string `json:"path"`
}

// struttura contenente l'insieme delle richieste e dati utili per la gestione della stessa
type ForResponseData struct {
	Channels   []http.ResponseWriter
	Username   []string
	Path       []string
	Operations []int
	link       []chan struct{}
	timer      []time.Time
}

// struttura per la ricezione del messaggio di hearthbit
type HearthBitMessage struct {
	Code      string `json:"code"`
	Operation int    `json:"operation"`
	Path      string `json:"path"`
}

// struttura finale contenente solo il canale http ed il canae di stato di risposta
type ConnectHandler struct {
	channel http.ResponseWriter
	closer  chan struct{}
	timer   time.Time
}

// struttura per la gestione dei permessi usati per l'hole punch
type TimeAndString struct {
	text  string
	timer time.Time
}
