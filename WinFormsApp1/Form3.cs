using System;
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
using MySql.Data.MySqlClient;

namespace WinFormsApp1
{
    public partial class Form3 : Form
    {
        string numero;
        MySqlConnection Conn;
        string sqlQuerry;
        DataTable dt;

        MySqlDataReader dr;

        List<Form4> list = new List<Form4>();

        public Form3(string id)
        {
            InitializeComponent();
            numero = id;

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



                    //bisogna salvare nella lista di path in un database.
                    sqlQuerry = "INSERT INTO `file` (`path`,`filename`, `userID`) VALUES ('" + f.FullName + "','" + f.Name + "','" + numero + "')";

                    MySqlCommand cmd = new MySqlCommand(sqlQuerry, Conn);
                    cmd.ExecuteNonQuery();

                }
            }
        }



        private void onClickRimuovi(object sender, EventArgs e)
        {
            // if(listView1.Items.Count > 0)
            if (listView1.SelectedItems.Count > 0)
            {

                //  FileStream fs = File.Open("prova.txt", FileMode.Open, FileAccess.ReadWrite);
                sqlQuerry = "DELETE  FROM file WHERE filename='" + listView1.SelectedItems[0].Text.ToString() + "'";
                //fare funzionare la delete che per ora non va, dice colonna sconosciuta
                MessageBox.Show(listView1.SelectedItems[0].Text.ToString());
                MySqlCommand cmd = new MySqlCommand(sqlQuerry, Conn);
                try { cmd.ExecuteNonQuery(); }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.Message);

                }

                listView1.Items.Remove(listView1.SelectedItems[0]);
            }
            // qui bisogna cercare l'elemento che si sta eliminando nel file e toglierlo pure da li ,
            // devo aprirlo sia in lettura che scrittura  e poi leggere tutte le righe del file
            // confrontandi con il path selezionato per poi eliminare la riga dove si trova.

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
            labelCodice.Text = numero; // dovrei mettere ciò che mi restituisce il database

            string server = "localhost";
            string username = "root";
            string database = "hole_punching";
            dt = new DataTable();


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
            sqlQuerry = "SELECT `path`,`filename` FROM file WHERE userID=" + numero;

            MySqlCommand cmd = new MySqlCommand(sqlQuerry, Conn);
            dr = cmd.ExecuteReader();
            dt.Load(dr);

            foreach (DataRow dr2 in dt.Rows)
            {
                FileInfo f = new FileInfo(dr2["path"].ToString());
                ListViewItem item = new ListViewItem(dr2["filename"].ToString());
                item.SubItems.Add(f.FullName);

                Estensione(item, f);

                listView1.Items.Add(item);

            }
            // dovrei caricare  i precedenti file che l'utente rende disponivili
            // quindi apro questo file dove ho salvato le informazioni sul path
            // riempio la lista con le informazioni 
            // chiudo il file
            dr.Close();
            dt.Clear();

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }



        private void onClickRicevitore(object sender, EventArgs e)
        {
            dt.Clear();

            if (textCodice.Text.Length > 0 && textCodice.Text != numero)
            {
                //   try {
                // sqlQuerry = "SELECT `filename` FROM file WHERE userID=" + textCodice.Text;
                sqlQuerry = "SELECT`id` FROM utenti";
                // MessageBox.Show(sqlQuerry);
                MySqlCommand cmd = new MySqlCommand(sqlQuerry, Conn);
                dr = cmd.ExecuteReader();
                dt = new DataTable();
                dt.Load(dr);
                try
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        //  MessageBox.Show(row["id"].ToString());
                        if (row["id"].ToString().Equals(textCodice.Text))
                        {

                            Form4 form4 = new Form4(textCodice.Text);
                            list.Add(form4);
                            form4.Show();
                            //form4.ShowDialog();

                            break;
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
            else
            {
                textCodice.Text = string.Empty;
                MessageBox.Show("inserisci un codice che non sia il tuo");
            }

        }

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

                sqlQuerry = "INSERT INTO `file` (`path`,`filename`, `userID`) VALUES ('" + f.FullName + "','" + f.Name + "','" + numero + "')";

                MySqlCommand cmd = new MySqlCommand(sqlQuerry, Conn);
                cmd.ExecuteNonQuery();

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
