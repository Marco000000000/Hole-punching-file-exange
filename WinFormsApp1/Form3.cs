﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.LinkLabel;
using System.Net.Sockets;
using Newtonsoft.Json;
using Microsoft.VisualBasic;

namespace WinFormsApp1
{
    public partial class Form3 : Form
    {
        //string numero;
        string id_random;
        string username;
        string serverURL = "http://localhost:81";
        List<Form4> list = new List<Form4>();

        public Form3(string codice, string nome)
        {
            InitializeComponent();
            id_random=codice;
            username=nome;
           
        }


        private void onClickAggiungi(object sender, EventArgs e)
        {

            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "all files|*.*", ValidateNames = true })
            {
            
                //puo selezionare un singolo file non una cartella intera
                // bisogna trovare un modo per caricare tutta la cartella.
                if (ofd.ShowDialog() == DialogResult.OK)
                {

                    FileInfo f = new FileInfo(ofd.FileName);
                    // MessageBox.Show(ofd.FileName);
                    ListViewItem item = new ListViewItem(f.Name);
                    item.SubItems.Add(f.FullName);
                    // qui bisognerebbe fare tutti gli if in base all'estensione,  fare pdf, mp4, jpg, zip, rar e boh 
                    Estensione(item, f);
                    listView1.Items.Add(item);
                    // devo aggiungere l'elemento inserito nel file di testo che contine tutti i file condivisi
                    using (StreamWriter outputFile = new StreamWriter("MyFile.txt",true))
                    {
                        outputFile.WriteLine(f.FullName);
                    }
                 //mandare al server python una nuova richiesta di aggiornamento con i nuobi dati
                 InvioDati(); 
                }
            
            }
        }



        private void onClickRimuovi(object sender, EventArgs e)          
        {
            // if(listView1.Items.Count > 0)
            if (listView1.SelectedItems.Count > 0)
            {

                 string[] readText = File.ReadAllLines("MyFile.txt");
                // 2. pulisco il file
                File.WriteAllText("MyFile.txt", String.Empty);

                // 3. riccarico il file senza il path cancellato
                using (StreamWriter writer = new StreamWriter("MyFile.txt"))
                {
                    foreach (string s in readText)
                    {
                        
                        if (!s.Equals(listView1.SelectedItems[0].SubItems[1].Text))
                        {
                            writer.WriteLine(s);
                        }

                    }
                }
                //mandare al server python una nuova richiesta di aggiornamento con i nuobi dati
                listView1.Items.Remove(listView1.SelectedItems[0]);
                InvioDati(); 
            }

        }

        private static void Estensione(ListViewItem it, FileInfo file)
        {
            switch (file.Extension)
            {
                case ".pdf":
                    it.ImageIndex = 0;
                    break;

                case ".txt":
                    it.ImageIndex = 2;
                    break;

                case ".doc" or ".docx":
                    it.ImageIndex = 8;
                    break;

                case ".jpeg" or ".jpg" or ".png" or ".gif":
                    it.ImageIndex = 3;
                    break;

                case ".mp4" or ".mpeg" or ".wmv" or ".avi":
                    it.ImageIndex = 4;
                    break;

                case ".zip" or ".rar":
                    it.ImageIndex = 5;
                    break;

                case ".exe" or ".cmd" or ".bat":
                    it.ImageIndex = 6;
                    break;

                case ".xls" or ".xlsx":
                    it.ImageIndex = 7;
                    break;

                case "":
                    it.ImageIndex = 9;
                    break;
                default:
                    it.ImageIndex = 1;
                    break;
            }

        }
private async void InvioDati(){
            List<string> lista = File.ReadAllLines("MyFile.txt").ToList();
           // MessageBox.Show(lista);
            var data = new Dictionary<string, object>();
            data["username"]=username;
            data["code"]=id_random;
            data["query"]="start_share";
            data["path"]= lista;
            
            var json = JsonConvert.SerializeObject(data);
            //MessageBox.Show(json);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try{
            using HttpClient client = new HttpClient();
            //var response = await client.PostAsync(serverURL+"/", content);
            var response = await client.PostAsync(serverURL+"/", content);
            MessageBox.Show(response.ToString());
            // vedere se ritorna qualcosa questa richiesta e notificare in caso di errore
              string result = response.Content.ReadAsStringAsync().Result;
              MessageBox.Show(result);
              
               dynamic obj = JsonConvert.DeserializeObject(result)??"nullo";
                    if (obj.error != null)
                    MessageBox.Show("si è verificato un errore " + obj.error);
                    if(obj.ok !=null)
                    MessageBox.Show("questo :"+obj.ok);
                    //start_share risponde con ok: i path dei file
                    
                   
            }catch(Exception e ){
                MessageBox.Show(e.Message);
            }
            
}
   private  void Form3_Load(object sender, EventArgs e)
        {
           labelCodice.Text = id_random; // dovrei mettere ciò che mi restituisce il database

            //fare magari un if se il file MyFile.txt non esiste allora crealo
            if (!File.Exists("MyFile.txt"))
            {
                using (StreamWriter sw = File.CreateText("MyFile.txt")) { }
                return;
            }
            InvioDati(); 
        
           // ---------------
              // dovrei caricare  i precedenti file che l'utente rende disponivili
              // quindi apro questo file dove ho salvato le informazioni sul path
              // riempio la lista con le informazioni 
              // chiudo il file
            // devo aprire il file dove ho conservato tutti i path dei mie elemmenti condivisi
            // leggere ogni riga ed aggiungere gli elementi alla list view

            StreamReader sr = new StreamReader("MyFile.txt");

            while (!sr.EndOfStream)
            {
               FileInfo f= new FileInfo(sr.ReadLine());
                ListViewItem item = new ListViewItem(f.Name);
                item.SubItems.Add(f.FullName);
                Estensione(item, f);
                listView1.Items.Add(item);
            }
            sr.Close();

        }



        private async void onClickRicevitore(object sender, EventArgs e)
        {

            if (textCodice.Text.Length > 0  && textUserRicevitore.Text.Length > 0 && textUserRicevitore.Text != username)
            {

                // devo compilare entrambi i campi e il campo realtivo all' username deve essere diverso dal mio
                //a questo pinto posso creare la richiesta POST con username, codice, username_peer, codice_peer, operation 
                 MessageBox.Show("preparo la richiesta ");
                var data = new Dictionary<string, object>();
                data["username"]=username;
                data["code"]=id_random;
                data["peer_username"]=textUserRicevitore.Text;
                data["peer_code"]=textCodice.Text;
                data["query"]="names"; // non è ne download ne un click sulla cartella 
                data["path"]="/";

     //provo con una richiesta start share per vedere se il form 4 viene costruito bene passando una lista
                // data["query"]="start_share";
                // List<string> lista = File.ReadAllLines("MyFile.txt").ToList();
                // data["path"]=lista;
    //verificato che passando la lista di stringhe il form4 viene costruito bene

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                try{
                using HttpClient client = new HttpClient();
                var response = await client.PostAsync(serverURL+"/", content);
                MessageBox.Show(response.ToString());
                //dovrei avere in risposta una lista  passarla direttamnte al form4 per evitare 
                //di fare un'ulteriore richiesta post li nella sua load.
                string result = response.Content.ReadAsStringAsync().Result;
                 MessageBox.Show(result);
                //  dynamic obj = JsonConvert.DeserializeObject(result)??"nullo";
                //     if (obj.error != null)
                //     MessageBox.Show("si è verificato un errore " + obj.error);
                //     if(obj.ok !=null){
                //     MessageBox.Show("questo :"+obj.ok);
                 //capire se ha ok come chiave

                    // la risposta alla richiesta post deve essere 
                    //l'elenco dei path dell'utente con il codice e username scritti nei campi
        //   List<string> lista1 = obj.ok.ToObject<List<string>>();
                    //funziona si come stringa che come string[]
                    List<string>lista1= File.ReadAllLines("MyFile.txt").ToList();
                     Form4 form4 = new Form4(username,id_random,textCodice.Text,textUserRicevitore.Text,lista1); 
                     //aggiungere come parametro la lista dei path se gia restituita dalla richiesta ?
                     list.Add(form4);
                     form4.Show();
               
                    // MessageBox.Show("errore nel invio dati");
                //}

            }catch(Exception ex){
                MessageBox.Show(ex.Message);
            }
            }

            else if (textCodice.Text.Length > 0 && textUserRicevitore.Text.Length>0)
            {
                textCodice.Text = string.Empty;
                textUserRicevitore.Text = string.Empty;
                MessageBox.Show("Non puoi inserire il tuo username");
            }
        
         }        
    
                    //si dovrebbe fare il controllo sul campo status del database facendo un querry usando 
                    //text.codice com id_random a cui voflio collegarmi
                    //se status è 1 allora si apre il form4, in caso contrario cioè se status è 0 oppure 
                    //se i caratteri inserito non corrispondo a nessun utente allora stampare errore
                    // Form4 form4 = new Form4(textCodice.Text);
                     // list.Add(form4);
                    //  form4.Show();
         
                  // devo fare in modo che il codice inserito sia mandato a go
                    // che dirà se corrisponderà a un id di un determinato utente,
                   // inoltre per poter far partire la richiesta esso deeve avere status uguale ad 1,
                   // se corrisponde si apre la finestra ricevitore di quel utente
                    // dopo che esso ha accettato la richiesta
                    // quindi ricapitolando in input stringa alfanumerica di 4 cifre mandando a go mi ritornerà id dell'utente corrispondente
                    // si fa la querry per vedere se ha status = 1 e se lo ha si avvia la richiesta per accedere alla sua schermata di condivisione
        

        private void onClickVisualizza(object sender, EventArgs e)
        {
            if (listView1.View == View.Details)
                listView1.View = View.SmallIcon;
            else if (listView1.View == View.SmallIcon)
                listView1.View = View.LargeIcon;
            else
            {
                listView1.View = View.Details;
            }
        }

        private void onClickFolder(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                FileInfo f = new FileInfo(folderBrowserDialog.SelectedPath);
                // MessageBox.Show(f.Extension);
                ListViewItem item = new ListViewItem(f.Name);
                item.SubItems.Add(f.FullName);
                item.ImageIndex = 9;
                listView1.Items.Add(item);

                // devo salvare il path della cartella nel file di testo
                using (StreamWriter outputFile = new StreamWriter("MyFile.txt",true))
                {
                   
                        outputFile.WriteLine(f.FullName);
                }
                InvioDati();
            }

            //qui dato che apriamo una cartella si usa l'icona di una cartella
            //questo permette di selezionare solo le cartelle
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(list.Count>0)
            foreach(Form4 f in list)
                {
                    f.Close(); 
                }
          
        }
    }
}
