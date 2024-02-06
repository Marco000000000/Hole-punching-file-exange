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

namespace WinFormsApp1
{
    public partial class Form3 : Form
    {
        string numero;
        string id_random;
        MySqlConnection Conn;
        string sqlQuerry;
       

        List<Form4> list = new List<Form4>();

        public Form3(string id,string codice)
        {
            InitializeComponent();
            numero = id;
            id_random=codice;
            string server = "localhost";
            string username = "root";
            string database = "hole_punching";
         

            try
            {
                Conn = new MySqlConnection();
                Conn.ConnectionString = "server=" + server + ";" + "user id=" + username + ";" + "database=" + database;
                Conn.Open();
                // MessageBox.Show("connessione eseguita");

            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
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

   private void Form3_Load(object sender, EventArgs e)
        {
           labelCodice.Text = id_random; // dovrei mettere ciò che mi restituisce il database

            //fare magari un if se il file MyFile.txt non esiste allora crealo
            if (!File.Exists("MyFile.txt"))
            {
                using (StreamWriter sw = File.CreateText("MyFile.txt")) { }
            }
                    
                string serverAddress = "localhost";
                int serverPort = 12345;

                TcpClient client = new TcpClient(serverAddress, serverPort);

                // Invia l'identificativo seguito dai dati
                string clientId = id_random;
                
                string data = File.ReadAllText("MyFile.txt");
                string message = $"{"0"};{clientId};{data}";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
                NetworkStream stream = client.GetStream();
                stream.Write(buffer, 0, buffer.Length);
                client.Close();
             //fare un metodo per l'invio dati che poi userò pure quando rimuovo/aggiungo un elemento   


            
           // ---------------
              // dovrei caricare  i precedenti file che l'utente rende disponivili
              // quindi apro questo file dove ho salvato le informazioni sul path
              // riempio la lista con le informazioni 
              // chiudo il file

      
            //_------------------------------------------------------
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



        private void onClickRicevitore(object sender, EventArgs e)
        {

            if (textCodice.Text.Length > 0 && textCodice.Text != id_random)
                 {
                    
                      DataTable dt = new DataTable();
                      MySqlDataReader dr;
                      sqlQuerry = "SELECT `id_random`,`status` FROM utenti WHERE id_random='" + textCodice.Text+"'";
        
                     // MessageBox.Show(sqlQuerry);
                     MySqlCommand cmd = new MySqlCommand(sqlQuerry, Conn);
                     dr = cmd.ExecuteReader();
                     dt.Load(dr);
                     //in teoria dato che id_random dovrebbe essere univoco 
                     //il risultato dcella querry dovrebbe essere un'unica riga 
                     
                     try
                     {
                         foreach (DataRow row in dt.Rows)
                         {
                              // MessageBox.Show(row["status"].ToString());
                             if ((bool)row["status"])
                             {
                                 Form4 form4 = new Form4(textCodice.Text);
                                 list.Add(form4);
                                 form4.Show();
                                 //form4.ShowDialog();
                                // break;
                             }else{
                                MessageBox.Show("utente non più connesso, farlo ricoleggare e inserire il nuovo codice");
                             }
                         }
                         textCodice.Text = string.Empty;
                     }
                     catch (MySqlException ex) 
                     {
                        MessageBox.Show(ex.Message); 
                     }
                    dr.Close();
                    dt.Clear();
                }

             else if(textCodice.Text.Length > 0)
             { 
                textCodice.Text = string.Empty;  
                MessageBox.Show("inserisci un codice che non sia il tuo");
             }
        
         }        
                   // MessageBox.Show("hai inserito un codice accettabile");
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

            sqlQuerry = "UPDATE `utenti` SET `status`= 0 WHERE id='" + numero + "'";
            MySqlCommand cmd1 = new MySqlCommand(sqlQuerry, Conn);
            cmd1.ExecuteNonQuery();
          
        }
    }
}
