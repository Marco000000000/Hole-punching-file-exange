using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net.Sockets;

namespace WinFormsApp1
{
    public partial class Form4 : Form
    {
        string utente;
        MySqlConnection Conn;
        
      
        
        public Form4(string id)
        {
            InitializeComponent();
            utente = id;

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

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

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
        private void Form4_Load(object sender, EventArgs e)
        {
            labelCodiceR.Text = utente;
                
           
                string serverAddress = "localhost";
                int serverPort = 12345;

                TcpClient client = new TcpClient(serverAddress, serverPort);

                // Invia l'identificativo del client di tipo 1
                string clientId = utente;
                string message = $"{"1"};{clientId};{"none"}"; 
                // effettivamente per dividere le tipologie di interfacciamento per il server go, 
                //si potresti stabile un ulteriore divisioni 
                //ad esempio se iniza con 0 abbiamo un aggiunta/ aggiornamento dei dati 
                //mentre se inizia con 1 si ha una richiesta dei dati  
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
                // byte[] buffer = System.Text.Encoding.UTF8.GetBytes(clientId);
                NetworkStream stream = client.GetStream();
                stream.Write(buffer, 0, buffer.Length);

                // Ricevi i dati dal server
                buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                // string receivedData = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string[] lines = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead).Split(new string[] { "\r\n", "\r", "\n" },StringSplitOptions.None);
                foreach (string line in lines)
                {
                    // MessageBox.Show(line);
                    if(!line.Equals("")){
                    FileInfo f = new FileInfo(line);
                    ListViewItem item = new ListViewItem(f.Name);
                    item.SubItems.Add(f.FullName);
                    Estensione(item, f);
                    listView2.Items.Add(item);
                    }
                
                    }
                              
                //  Console.WriteLine($"Dati ricevuti dal server: {receivedData}");

                client.Close();
                    

        }

        private void onClickVisualizzazione(object sender, EventArgs e)
        {
            if (listView2.View == View.Details)
                listView2.View = View.SmallIcon;
            else if (listView2.View == View.SmallIcon)
                listView2.View = View.LargeIcon;
            else
            {
                listView2.View = View.Details;
            }
        }

        private void buttonSeleziona_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                MessageBox.Show(listView2.SelectedItems[0].SubItems[1].Text.ToString());
               
            }
            else
            {
                MessageBox.Show("devi selezionare un file da scaricare ");
            }
        }
    }
}
