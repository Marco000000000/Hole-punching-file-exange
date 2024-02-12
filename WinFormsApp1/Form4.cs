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
using Newtonsoft.Json;
namespace WinFormsApp1
{
    public partial class Form4 : Form
    {
        string utente;
        string codice;
        string codice_peer;
        string utente_peer;
        List<string> lista;
        List<Form4> listaForm = new List<Form4>();
        string serverURL = "http://127.0.0.1:80";


        public Form4(string username, string id, string id_peer, string username_peer, List<string> elenco)
        {
            InitializeComponent();
            utente = username;
            codice = id;
            codice_peer = id_peer;
            utente_peer = username_peer;
            lista = elenco;
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
        private async void Form4_Load(object sender, EventArgs e)
        {
            labelCodiceR.Text = utente_peer;
            if (listaForm.Count == 0)
            {
                buttonIndietro.Enabled = false;
                buttonIndietro.Visible = false;
            }
            else
            {
                buttonIndietro.Enabled = true;
                buttonIndietro.Visible = true;
            }
            // se passo la lista dei path come paramtro questo load si deve cancellare tutto. 
            //e semplicememnte riempire la view facendo scorrere la lista di path ogni stringa
            foreach (string line in lista)
            {
                FileInfo f = new FileInfo(line);
                ListViewItem item = new ListViewItem(f.Name);
                item.SubItems.Add(f.FullName);
                Estensione(item, f);
                listView2.Items.Add(item);
             
            }

            // //commento tutto per testare il riempimento con lista passata dal form precedente.
            //     var data = new Dictionary<string, object>();
            //         data["username"]=utente;
            //         data["code"]=codice;
            //         data["peer_username"]=utente_peer;
            //         data["peer_codice"]=codice_peer;
            //         data["query"]=""; // non è ne download ne un click sulla cartella  --> devo richiedere i file condivisi 
            //         //dalla persona con username "utente" e con codice "codice".
            //         //data["paths"]= File.ReadAllText("MyFile.txt");

            //         var json = JsonConvert.SerializeObject(data);
            //         var content = new StringContent(json, Encoding.UTF8, "application/json");

            //         using var client = new HttpClient();
            //         var response = await client.PostAsync(serverURL, content);
            //         // devo avere tornata la lista dei path  in modo tale da riempire la listview2

            //         if (response.IsSuccessStatusCode)
            //         {
            //             Console.WriteLine("Dati inviati con successo.");
            //         }
            //         else
            //         {
            //             Console.WriteLine($"Errore nell'invio dei dati: {response.StatusCode}");
            //         }



            /*  StreamReader sr = new StreamReader("MyFile.txt");

              while (!sr.EndOfStream)
              {
                 FileInfo f= new FileInfo(sr.ReadLine());
                  ListViewItem item = new ListViewItem(f.Name);
                  item.SubItems.Add(f.FullName);
                  Estensione(item, f);
                  listView2.Items.Add(item);
              }
              sr.Close(); */

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

        private async void buttonSeleziona_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                MessageBox.Show(listView2.SelectedItems[0].SubItems[1].Text.ToString());
                // richiesta post con operazione settata per eseguire il downloaf

                var data = new Dictionary<string, object>();
                data["username"] = utente;
                data["code"] = codice;
                data["peer_username"] = utente_peer;
                data["peer_code"] = codice_peer;
                data["query"] = "download";
                data["path"] = listView2.SelectedItems[0].SubItems[1].Text.ToString();

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                var response = await client.PostAsync(serverURL, content);
                // vedere cosa risponde  e notificare se ci sono errori
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Dati inviati con successo.");
                }
                else
                {
                    MessageBox.Show($"Errore nell'invio dei dati: {response.StatusCode}");
                }

            }
            else
            {
                MessageBox.Show("devi selezionare un file da scaricare ");
            }
        }

        private async void listView2_DoubleClick(object sender, EventArgs e)
        {
            // MessageBox.Show(listView2.SelectedItems[0].SubItems[1].Text.ToString());
            FileInfo f = new FileInfo(listView2.SelectedItems[0].SubItems[1].Text.ToString());
            if (f.Extension == "")
            {
                MessageBox.Show("selezionata una cartella");
                //quindi dovrebbe  partire una richiesta post per avere 
                //i nomi dei file contenuti in questa cartella
                //una volta ottenuti aggiornare la schermata solo con quei file , 
                //trovare modo per tornare indietro

                var data = new Dictionary<string, object>();
                data["username"] = utente;
                data["code"] = codice;
                data["peer_username"] = utente_peer;
                data["peer_code"] = codice_peer;
                data["query"] = "names";
                data["path"] = listView2.SelectedItems[0].SubItems[1].Text.ToString();

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                var response = await client.PostAsync(serverURL, content);
                // questa richiesta mi deve tornare i percorsi di tutti i file contenuti 
                //nella cartella a cui si è fatto doppio click

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(response.ToString());
                    string result = response.Content.ReadAsStringAsync().Result;
                    MessageBox.Show(result);
                    dynamic obj = JsonConvert.DeserializeObject(result) ?? "nullo";
                    // if (obj.error != null)
                    //     MessageBox.Show("si è verificato un errore " + obj.error);
                    // if (obj.ok != null)
                    //     MessageBox.Show("questo :" + obj.ok); //capire se ha ok come chiave

            List<string> lista1 = obj.ToObject<List<string>>();  
                //devo vedere quale è la chiave perchè non è ok
            this.Visible = false;
            Form4 form4 = new Form4(utente, codice, codice_peer, utente_peer, lista1);
            listaForm.Add(form4);
            //aggiungere come parametro la lista dei path se gia restituita dalla richiesta ?
            form4.Show();
            this.Visible = true;

                    // --> svuotare la listview corrente e riempirla con i nuovi file? 
                    //pero problema nel tornare indietro
                    //------
                    //altra idea sarebbe modificare il form4 e fare in modo  che abbi nel cotruttore una lista di stringhe, cioè i path
                    //quindi il form3 quando lo invoca la prima volta fornira pure questa lista , che dovrebbe essere risposta 
                    //alla richiesta post che comunque bisogna fare per vedere se esiste l'utente 
                    //con determinato username e determinato codice random. quindi qui si creerebbe un nuovo form4 con stabolta passata 
                    //questa nuova lista di stringhe, il vecchio verebbe reso invisbile e reso dinuovo 
                    //visbile solo dopo la chiusura dell'altro
                }
                else
                {
                    MessageBox.Show($"Errore nell'invio dei dati: {response.StatusCode}");
                }

            }
        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(listaForm.Count>0)
             foreach (Form4 f in listaForm)
             {
                 //forse piu coretto fare un form 5 e scorrere un lista di essi
                 f.Close();
             }
            
        }

        private void buttonIndietro_Click(object sender, EventArgs e)
        {
            listaForm.Remove(this);
            this.Close();
        }
    }
}
