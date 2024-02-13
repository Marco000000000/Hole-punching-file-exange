using System.Text;
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
        bool primo;
        string serverURL = "http://127.0.0.1:80";

        private Form4 parent;

        public Form4(string username, string id, string id_peer, string username_peer, List<string> elenco, bool first, Form4 padre)
        {
            InitializeComponent();
            utente = username;
            codice = id;
            codice_peer = id_peer;
            utente_peer = username_peer;
            lista = elenco;
            primo = first;
            this.parent = padre;
            this.FormClosed += new FormClosedEventHandler(Form2_FormClosed);

        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.parent != null)
            {
                this.parent.Visible = true;
                this.parent.Close();
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
        private async void Form4_Load(object sender, EventArgs e)
        {
            labelCodiceR.Text = utente_peer;
            //dal booleano capisco se è un 
            if (primo)
            {
                buttonIndietro.Visible = false;
                buttonIndietro.Enabled = false;
            }
            else
            {
                buttonIndietro.Visible = true;
                buttonIndietro.Enabled = true;
            }

            //eriempire la view facendo scorrere la lista di path ogni stringa
            foreach (string line in lista)
            {
                FileInfo f = new FileInfo(line);
                ListViewItem item = new ListViewItem(f.Name);
                item.SubItems.Add(f.FullName);
                Estensione(item, f);
                listView2.Items.Add(item);

            }


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
                    MessageBox.Show("Download eseguito con successo.");
                }
                else
                {
                    MessageBox.Show($"Errore nella connessione {response.StatusCode}");
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
                //MessageBox.Show("selezionata una cartella");
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
                    //MessageBox.Show(response.ToString());
                    string result = response.Content.ReadAsStringAsync().Result;
                    // MessageBox.Show(result);
                    dynamic obj = JsonConvert.DeserializeObject(result) ?? "nullo";
                  

                    List<string> lista1 = obj.ToObject<List<string>>();
                    if(lista1[0].Equals("/error")){
                            MessageBox.Show("errore generico"+obj.error);
                    }
                    else{
                    Form4 form4 = new Form4(utente, codice, codice_peer, utente_peer, lista1, false, this);


                    this.Visible = false;
                    form4.ShowDialog();


                    //altra idea sarebbe modificare il form4 e fare in modo  che abbi nel cotruttore una lista di stringhe, cioè i path
                    //quindi il form3 quando lo invoca la prima volta fornira pure questa lista , che dovrebbe essere risposta 
                    //alla richiesta post che comunque bisogna fare per vedere se esiste l'utente 
                    //con determinato username e determinato codice random. quindi qui si creerebbe un nuovo form4 con stabolta passata 
                    //questa nuova lista di stringhe, il vecchio verebbe reso invisbile e reso dinuovo 
                    //visbile solo dopo la chiusura dell'altro
                    //inoltre aggiunto un booleano nel costruttore per il pulante idetro e  il riferimetno al form4 che sta generando un ulteriore form4
                }
                }
                else
                {
                    MessageBox.Show($"Errore nella comunicazione {response.StatusCode}");
                }

            }
        }

        private void buttonIndietro_Click(object sender, EventArgs e)
        {
            //  listaForm.Remove(this);
            this.parent.Show();
            this.parent = null;
            this.Close();

            //si rende visibile il padre , e per evitare che si chiudano tutti i form 
            //secondo la close imposto this.parent a null e poi chiudo il form corrente
        }
    }
}
