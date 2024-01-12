using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void OnClickBottonRegister(object sender, EventArgs e)
        {
            bool flag=false;
            lblError.Text = string.Empty;
            lblErrorName.Text = string.Empty;
            lblErrorPassword.Text = string.Empty;



            if (textName.Text.Equals(""))
            {
                lblErrorName.Text = "inserisci il nome";
                flag = true;
            }

            if (textPassword.Text.Equals(""))
            { 
                lblErrorPassword.Text = "inserisci la password";
                flag = true;
            }

            if (textName.Text.Equals("") || textPassword.Text.Equals(""))
                lblError.Text = "VOLEVI, GUARDA che FASCIA NON SE LO ASPETTAVA";

            if (flag==false)
                // se va tutto bene si inviano al programma in python nome password
                // per eseguire la ergistrazione nel database e si chiude la scheda 
                // tornando alla scheda del login
                this.Close();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
