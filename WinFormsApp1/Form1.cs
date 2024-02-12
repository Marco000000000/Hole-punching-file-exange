
using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Diagnostics;


namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        string serverURL = "http://151.74.146.179:80";

        Process process;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "python"; // Aggiorna con il percorso del tuo interprete Python
            start.Arguments = string.Format("{0} {1}", "../pythonModule/tcp_client.py", ""); // Aggiorna con il percorso del tuo script Python e gli argomenti necessari
            start.UseShellExecute = false; 
            start.RedirectStandardOutput = true;  //per motivi di debug
            process = Process.Start(start);
            if (process == null)
                Environment.Exit(1);

        }


        private async void onClickLogin(object sender, EventArgs e)
        {
            // string id="default";
            bool flag = false;
            string id_random;
            var dati = new Dictionary<string, object>();
            lblError.Text = string.Empty;

            if (textName.Text.Equals("") || textPassword.Text.Equals(""))
            {
                lblError.Text = "Inserire le credenziali ";
                flag = true;
            }

            if (!flag)
            {
                //  Form3 form3 = new Form3(id_random, textName.Text); 
                // form3.Show();
                dati["username"] = textName.Text;
                dati["password"] = textPassword.Text;
                textName.Text = string.Empty;
                textPassword.Text = string.Empty;
                lblError.Text = string.Empty;

                var json = JsonConvert.SerializeObject(dati);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                try
                {

                    using HttpClient client = new HttpClient();
                    var response = await client.PostAsync(serverURL + "/login", data);
                    //var response = await client.PostAsync("http://127.0.0.1:80/login", data);
                    string result = response.Content.ReadAsStringAsync().Result;
                    //  MessageBox.Show(result);
                    try
                    {
                        dynamic obj = JsonConvert.DeserializeObject(result) ?? "nullo";
                        if (obj.error != null)
                            MessageBox.Show("si è verificato un errore " + obj.error);
                        else if (obj.code != null)
                        {
                            MessageBox.Show("il codice di risposta: " + obj.code);
                            id_random = obj.code;
                            this.Visible = false;
                            Form3 form3 = new Form3(id_random, (string)dati["username"]);
                            form3.ShowDialog();
                            this.Visible = true;
                            //se tutto va bene invio le credeziali per poter eseguire l'accesso
                            //se esistono chiudo la schede e ne apro una relativa a trasmissione 
                        }
                        else
                            MessageBox.Show("risposta inaspettata dal server");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Si è verificato un errore con la deserializzazione: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Si è verificato un errore: {ex.Message}");
                }



            }


        }


        private void OnClickRegister(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (process != null)
            {
                process.Kill();
                process = null;
            }
        }
    }
}
