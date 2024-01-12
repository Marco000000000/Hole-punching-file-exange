using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();

        }



        private void onClickAggiungi(object sender, EventArgs e)
        {

            using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "all files|*.*", ValidateNames = true })
            {


                //puo selezionare un singolo file non una cartella intera
                // bisogna trovare un modo per caricare tutta la cartella.
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    /*
                      foreach (string file in ofd.FileNames)
                      {
                          FileInfo fi = new FileInfo(file);
                          ListViewItem item = new ListViewItem(fi.Name);
                          item.SubItems.Add(fi.FullName);
                     //   item.ImageIndex = 0;
                          listView1.Items.Add(item);
                    */
                    FileInfo f = new FileInfo(ofd.FileName);
                    ListViewItem item = new ListViewItem(f.Name);
                    item.SubItems.Add(f.FullName);

                    // qui bisognerebbe fare tutti gli if in base all'estensione,  fare pdf, mp4, jpg, zip, rar e boh 
                    if (f.Extension == ".pdf")
                        item.ImageIndex = 0;
                    else
                        item.ImageIndex = 1;

                    listView1.Items.Add(item);

                    // in teoria  ora devo salvar l'elemento in un determinato file 
                    // in modo tale che ad un successuvi accesso l'utente  ritrova i file gia caricati
                    //  salvare il nome del file e il path, quando si caricheranno i dati bisognerà fare il confronto
                    //  usando endWith("") poiche non si avrà piu un tipo FileInfo oppure avere un tipo fileinfo usando il path



                    using (StreamWriter writetext = new StreamWriter("prova.txt", true))
                    {
                        writetext.WriteLine(f.FullName);
                    }


                }



                //------------------------------



                //FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                // if(folderBrowserDialog.ShowDialog()==DialogResult.OK){ }

                //qui dato che apriamo una cartella si usa l'icona di una cartella
                //questo permette di selezionare solo le cartelle
            }
        }



        private void onClickRimuovi(object sender, EventArgs e)
        {
            // if(listView1.Items.Count > 0)
            if (listView1.SelectedItems.Count > 0)
            {
                listView1.Items.Remove(listView1.SelectedItems[0]);
                //  FileStream fs = File.Open("prova.txt", FileMode.Open, FileAccess.ReadWrite);

            }
            // qui bisogna cercare l'elemento che si sta eliminando nel file e toglierlo pure da li ,
            // devo aprirlo sia in lettura che scrittura  e poi leggere tutte le righe del file
            // confrontandi con il path selezionato per poi eliminare la riga dove si trova.

        }

        private void Form3_Load(object sender, EventArgs e)
        {
            labelCodice.Text = "10"; // dovrei mettere ciò che mi restituisce il database


            // dovrei caricare  i precedenti file che l'utente rende disponivili
            // quindi apro questo file dove ho salvato le informazioni sul path
            // riempio la lista con le informazioni 
            // chiudo il file
            StreamReader fi = new StreamReader("prova.txt");
            while (!fi.EndOfStream)
            {
                FileInfo f = new FileInfo(fi.ReadLine());

                ListViewItem item = new ListViewItem(f.Name);
                item.SubItems.Add(f.FullName);

                if (f.Extension == ".pdf")
                    item.ImageIndex = 0;
                else
                    item.ImageIndex = 1;

                listView1.Items.Add(item);

            }
            fi.Close();


        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        

        private void onClickRicevitore(object sender, EventArgs e)
        {
            if (textCodice.Text.Length > 0)
            {
                if (textCodice.Text.Equals("10")) //devo mettere una lista dei codici restituita
                                                  //dal database scorrere e  vedere se c'è un match,
                                                  //se c'è si apre il form ricezione relativo all'utente con quel codice
                                                  // se non c'è corrispondenza notificare l'errore
                                                  // textCodice.Text = "ole ole"; // qui dovre fare aprire un nuovo form relativo alla ricezione dei file
                {
                    Form4 form4 = new Form4();
                    form4.Show();
                }
                    
                else
                    textCodice.Text = string.Empty;
            }

            //elsea   deve esserci scritto qualcosa
        }

       
    }
}
