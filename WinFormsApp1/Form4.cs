using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WinFormsApp1
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form4_Load(object sender, EventArgs e)
        {
            labelCodiceR.Text = "10"; // dovrei mettere ciò che mi restituisce il database


            // dovrei caricare  i  file che l'utente con il codice inserito rende disponivili
            // il database deve restituirmi i path di tutti i file che quel utente a reso disponibili
            // riempio la lista con le informazioni 
           


        }
    }
}
