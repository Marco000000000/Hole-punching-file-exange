namespace WinFormsApp1
{
    partial class Form3
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form3));
            label1 = new Label();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            imageList1 = new ImageList(components);
            AggiungiButton = new Button();
            rimuoviButton = new Button();
            label2 = new Label();
            labelCodice = new Label();
            codiceRicevitore = new Label();
            textCodice = new TextBox();
            buttonRicevitore = new Button();
            buttonVisualizzazione = new Button();
            buttonFolder = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 53);
            label1.Name = "label1";
            label1.Size = new Size(226, 20);
            label1.TabIndex = 0;
            label1.Text = "seleziona i file da rendere visibili";
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listView1.LargeImageList = imageList1;
            listView1.Location = new Point(145, 107);
            listView1.Name = "listView1";
            listView1.Size = new Size(734, 334);
            listView1.SmallImageList = imageList1;
            listView1.TabIndex = 1;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Name";
            columnHeader1.Width = 320;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Path";
            columnHeader2.Width = 360;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "file_pdf1.png");
            imageList1.Images.SetKeyName(1, "file_generic.png");
            imageList1.Images.SetKeyName(2, "file_txt.png");
            imageList1.Images.SetKeyName(3, "file_images.png");
            imageList1.Images.SetKeyName(4, "file_video.png");
            imageList1.Images.SetKeyName(5, "file_zip.png");
            imageList1.Images.SetKeyName(6, "file_exe.png");
            imageList1.Images.SetKeyName(7, "file_xls.png");
            imageList1.Images.SetKeyName(8, "file_doc.png");
            imageList1.Images.SetKeyName(9, "folder.png");
            // 
            // AggiungiButton
            // 
            AggiungiButton.Location = new Point(26, 146);
            AggiungiButton.Name = "AggiungiButton";
            AggiungiButton.Size = new Size(94, 29);
            AggiungiButton.TabIndex = 2;
            AggiungiButton.Text = "Aggiungi";
            AggiungiButton.UseVisualStyleBackColor = true;
            AggiungiButton.Click += onClickAggiungi;
            // 
            // rimuoviButton
            // 
            rimuoviButton.Location = new Point(26, 121);
            rimuoviButton.Name = "rimuoviButton";
            rimuoviButton.Size = new Size(94, 29);
            rimuoviButton.TabIndex = 5;
            rimuoviButton.Text = "rimuovi";
            rimuoviButton.UseVisualStyleBackColor = true;
            rimuoviButton.Click += onClickRimuovi;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(745, 53);
            label2.Name = "label2";
            label2.Size = new Size(105, 20);
            label2.TabIndex = 6;
            label2.Text = "il tuo codice =";
            // 
            // labelCodice
            // 
            labelCodice.AutoSize = true;
            labelCodice.Location = new Point(856, 53);
            labelCodice.Margin = new Padding(0);
            labelCodice.Name = "labelCodice";
            labelCodice.Size = new Size(0, 20);
            labelCodice.TabIndex = 7;
            // 
            // codiceRicevitore
            // 
            codiceRicevitore.Location = new Point(12, 501);
            codiceRicevitore.Name = "codiceRicevitore";
            codiceRicevitore.Size = new Size(121, 27);
            codiceRicevitore.TabIndex = 8;
            codiceRicevitore.Text = "Inserire il codice ";
            codiceRicevitore.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // textCodice
            // 
            textCodice.Location = new Point(145, 501);
            textCodice.Name = "textCodice";
            textCodice.Size = new Size(125, 27);
            textCodice.TabIndex = 9;
            // 
            // buttonRicevitore
            // 
            buttonRicevitore.Location = new Point(276, 501);
            buttonRicevitore.Name = "buttonRicevitore";
            buttonRicevitore.Size = new Size(94, 27);
            buttonRicevitore.TabIndex = 10;
            buttonRicevitore.Text = "Invio";
            buttonRicevitore.UseVisualStyleBackColor = true;
            buttonRicevitore.Click += onClickRicevitore;
            // 
            // buttonVisualizzazione
            // 
            buttonVisualizzazione.Location = new Point(418, 35);
            buttonVisualizzazione.Name = "buttonVisualizzazione";
            buttonVisualizzazione.Size = new Size(119, 57);
            buttonVisualizzazione.TabIndex = 11;
            buttonVisualizzazione.Text = "Cambia Visualizzazione";
            buttonVisualizzazione.UseVisualStyleBackColor = true;
            buttonVisualizzazione.Click += onClickVisualizza;
            // 
            // buttonFolder
            // 
            buttonFolder.Location = new Point(12, 371);
            buttonFolder.Name = "buttonFolder";
            buttonFolder.Size = new Size(108, 49);
            buttonFolder.TabIndex = 12;
            buttonFolder.Text = "Aggiungi Cartella";
            buttonFolder.UseVisualStyleBackColor = true;
            buttonFolder.Click += onClickFolder;
            // 
            // Form3
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(978, 561);
            Controls.Add(buttonFolder);
            Controls.Add(buttonVisualizzazione);
            Controls.Add(buttonRicevitore);
            Controls.Add(textCodice);
            Controls.Add(codiceRicevitore);
            Controls.Add(labelCodice);
            Controls.Add(label2);
            Controls.Add(rimuoviButton);
            Controls.Add(AggiungiButton);
            Controls.Add(listView1);
            Controls.Add(label1);
            Name = "Form3";
            Text = "Form3";
            FormClosing += Form3_FormClosing;
            Load += Form3_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ListView listView1;
        private Button AggiungiButton;
        private Button rimuoviButton;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ImageList imageList1;
        private Label label2;
        private Label labelCodice;
        private Label codiceRicevitore;
        private TextBox textCodice;
        private Button buttonRicevitore;
        private Button buttonVisualizzazione;
        private Button buttonFolder;
    }
}