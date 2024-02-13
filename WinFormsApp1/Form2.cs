
using System.Text;
using Newtonsoft.Json;

namespace WinFormsApp1
{
    public partial class Form2 : Form
    {
        //string serverURL = "151.74.146.179:80";
        string serverURL= "http://127.0.0.1:80";
        public Form2()
        {
            InitializeComponent();
        }

        private async void OnClickBottonRegister(object sender, EventArgs e)
        {
            //bisogna togliere la querry al database perchè fatta da go, 
            //devo fare la richiesta a go passando il nome e la password dopo aver fatto i controlli
            //sulla forma lato client, stessa cosa nel form1 
          
            bool flag = false;
            lblError.Text = string.Empty;
            lblErrorName.Text = string.Empty;
            lblErrorPassword.Text = string.Empty;

            
            
            //controllo per vere se i campit sono vuoti

            if (textName.Text.Equals("") && textPassword.Text.Equals("")) {
                lblError.Text = "Inserisci entrambi i campi ";
                flag = true;
                }
                else if (textName.Text.Equals("")) {
                    lblErrorName.Text = "inserisci il nome";
                    flag = true;
                }

                else if (textPassword.Text.Equals("")){
                    lblErrorPassword.Text = "inserisci la password";
                    flag = true;
                }

            if(textPassword.Text.Length<7 && textPassword.Text.Length>0)  {
                    lblErrorPassword.Text = "password troppo corta, minimo inserire 7 caratteri";
                    flag = true;
                }

            
            
            // fare login e registrazione in go
            // richiesta a go per avere assegnato un id random  da aggiungere contestualmente agli altri dati

            if (flag == false)
            {
                //---------------
                //invio al database nome, pass  per proseguire la registrazione
                //se tutto va bene chiudo la scheda, se dovessero esserci problemi,
                //ad esempio utente gia registrato, password che non rispetta delle regole specifiche
                //notificare l'errore
                var dati = new Dictionary<string, object>();
                dati["Username"] = textName.Text;
                dati["Password"] = textPassword.Text;

                //se tutto va bene invio le credeziali per poter eseguire l'accesso
                var json = JsonConvert.SerializeObject(dati);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                try
                {

                    using HttpClient client = new HttpClient();
                    var response = await client.PostAsync(serverURL + "/signin", data);
                    MessageBox.Show(response.ToString());
                    string result = response.Content.ReadAsStringAsync().Result;
                    MessageBox.Show(result.ToString());
                    try
                    {
                        dynamic obj = JsonConvert.DeserializeObject(result) ?? "nullo";
                        if (obj.error != null)
                            MessageBox.Show("si è verificato un errore nella registrazione: " + obj.error);
                        else if (obj.code != null)
                        {
                            MessageBox.Show("La registrazione è  stata eseguita ");
                            //chiudo la scheda
                            this.Close();
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
                textName.Text = "";
                textPassword.Text = "";
            }
        }
    }
}
