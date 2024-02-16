# Hole punching file exange
## Istruzioni per il deploy:

1.Avviare un server MySQL sulla porta 3306 ed inizializzarlo con il file di inizializzatione contenuto nel modulo Go ,sottocartella MySQL . Nel modulo go,la sottocartella MySQL fornisce una possibile implementazione creata per Docker.
Scaricabile su [Docker Desktop](https://www.docker.com/products/docker-desktop) dal sito ufficiale ed installare l'ambiente Kubernetes associato.

2.Una volta scaricato il progetto su un computer avente 80 e 5000 come porte preimpostate per il NAT traversal(da impostazioni router) ,spostarsi  da shell nella cartella contenente il modulo in go del progetto e utilizzare il comando:
   ```bash
   go build
   ```
in seguito lanciare il seguente comando:
      ```bash
   ./server
   ```

3.Aprire il file .env del modulo Python e modificare il campo TURNSERVER con l'ip pubblico o il dns del server precedente.

4.Entrare nella cartella Windowsform1 tramite shell e lanciare il comando:

   ```bash
   dotnet run program.cs
```
## Descrizione del sistema
   Il nostro progetto ha come obiettivo la realizzazione di un sistema per lo scambio di file tra host posti in una rete che utilizza la tecnica NAT.
Per la nostra condivisione abbiamo posto come prima opzione di collegamento una tecnica sperimentale per il NAT traversal detta Hole Punching.
Purtroppo, essendo una tecnica dipendente dalle singole implementazioni del NAT è soggetta a molti fallimenti, quindi abbiamo implementato allo stesso tempo un metodo alternativo di comunicazione tramite un Server Mediator in Go.                                           L'utente della nostra applicazione dovrà registrarsi, accedere e in seguito avrà la possibilità di stabilire quali e quanti file trasmettere ed attraverso un codice univoco associato ad ogni sessione utente si potrà tentare di accedere in ricezione ai file condivisi da un altro utente che potranno essere visualizzati e scaricati.	
