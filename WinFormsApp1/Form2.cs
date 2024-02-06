using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using MySql.Data.MySqlClient;
using System.Data.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Net.Sockets;

namespace WinFormsApp1
{
    public partial class Form2 : Form
    {
        MySqlConnection Conn;
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {


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

        private void OnClickBottonRegister(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            MySqlDataReader dr;
  
            string sqlQuerry;
            bool flag = false;
            lblError.Text = string.Empty;
            lblErrorName.Text = string.Empty;
            lblErrorPassword.Text = string.Empty;

            sqlQuerry = "SELECT `username` FROM utenti";
            MySqlCommand cmd = new MySqlCommand(sqlQuerry, Conn);
            dr=cmd.ExecuteReader();
            dt.Load(dr);
            
            //devo costruire la querry che mi ritorno
            //la lista di nomi presenti nel database e verificare se il nome inserito è già presente

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

            // dovrei ottenere dal database la lista dei nomi gia presenti
            // e confrontarli col nome inserito, se è già presente notifcare
            // errrore nome già in uso
            foreach (DataRow dataRow in dt.Rows) {
                if (textName.Text.Equals(dataRow["username"].ToString()) && !flag) {
                    lblErrorName.Text = "nome già in uso, provare con un altro";
                    flag = true;
                }
            }
            
            // fare login e registrazione in go
            // richiesta a go per avere assegna un id random  da aggiungere contestualmente agli altri dati

            if (flag == false)
            {
                //---------------
                //invio al database nome, cognome e per proseguire la registrazione
                //se tutto va bene chiudo la scheda, se dovessero esserci problemi,
                //ad esempio utente gia registrato, password che non rispetta delle regole specifiche
                //notificare l'errore
                sqlQuerry = "INSERT INTO `utenti` (`username`, `password`) VALUES ('" + textName.Text + "','" + textPassword.Text + "')";
                try
                {
                    if (Conn != null)
                    {
                        cmd = new MySqlCommand(sqlQuerry, Conn);
                        cmd.ExecuteNonQuery();

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                this.Close();
            }
        }
    }
}
