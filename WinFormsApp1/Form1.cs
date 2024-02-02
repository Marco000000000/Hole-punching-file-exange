

using System.Data;
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        string sqlQuerry;
        MySqlConnection Conn;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            

            string server = "localhost";
            string username = "root";
            string database = "hole_punching";

            try { 
            Conn = new MySqlConnection();
            Conn.ConnectionString = "server=" + server + ";" + "user id=" + username + ";" + "database="+database;
            Conn.Open();
            MessageBox.Show("connessione eseguita");
             
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }


        private void onClickLogin(object sender, EventArgs e)
        {
            string id="default";
            bool flag = false;
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            MySqlDataReader dr;

            lblError.Text = string.Empty;
            sqlQuerry = "SELECT `id`,`username`,`password` FROM utenti";
            MySqlCommand cmd = new MySqlCommand(sqlQuerry, Conn);
            dr = cmd.ExecuteReader();
            dt.Load(dr);


            foreach (DataRow dr2 in dt.Rows)
            {
                if (textName.Text.Equals(dr2["username"].ToString()) && textPassword.Text.Equals(dr2["password"].ToString()))
                //dovrei confrontare i valori inseriti
                //con valori presenti nel database se c'è match accedere       
                //senno mostrare un messaggio di errore
                {
                    id=dr2["id"].ToString();
                    // attraverso il collegamento al serve go id non sarà quello autoincrement
                    // ma sarà una stringa di 4 caratteri alfanumerici
                    sqlQuerry = "UPDATE `utenti` SET `status`= 1 WHERE username='"+dr2["username"].ToString()+"'";
                    MySqlCommand cmd1=new MySqlCommand(sqlQuerry, Conn);
                    cmd1.ExecuteNonQuery();
                    flag = false; break;
                }
                else
                {
                    lblError.Text = "credenziali non valide, inserire le credenziali corrette";
                    flag = true;
                    
                }
            }

           if (textName.Text.Equals("") && textPassword.Text.Equals(""))
            {
                lblError.Text = "Inserire le credenziali ";
                flag = true;
            }

            if (!flag)
            {
                textName.Text = string.Empty;
                textPassword.Text = string.Empty;
                lblError.Text= string.Empty;

                this.Visible = false;
                Form3 form3 = new Form3(id);
                form3.ShowDialog();
               
                this.Visible = true;

                //bisognerebbe togliere il form di accesso ma se lo chiudo viene chiuso tutto
                //e se lo nascondo rimane aperto come processo in backgrounf

            }
            //se tutto va bene invio le credeziali per poter eseguire l'accesso
            //se esistono chiudo la schede e ne apro una relativa a trasmissione 


        }
       

        private void OnClickRegister(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();
            // form2.Activate();

        }

      
    }
}
