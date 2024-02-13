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
            lblUserRicevitore = new Label();
            textUserRicevitore = new TextBox();
            labelUsername = new Label();
            labelUsername1 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listView1.LargeImageList = imageList1;
            resources.ApplyResources(listView1, "listView1");
            listView1.Name = "listView1";
            listView1.SmallImageList = imageList1;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // columnHeader1
            // 
            resources.ApplyResources(columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(columnHeader2, "columnHeader2");
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
            resources.ApplyResources(AggiungiButton, "AggiungiButton");
            AggiungiButton.Name = "AggiungiButton";
            AggiungiButton.UseVisualStyleBackColor = true;
            AggiungiButton.Click += onClickAggiungi;
            // 
            // rimuoviButton
            // 
            resources.ApplyResources(rimuoviButton, "rimuoviButton");
            rimuoviButton.Name = "rimuoviButton";
            rimuoviButton.UseVisualStyleBackColor = true;
            rimuoviButton.Click += onClickRimuovi;
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // labelCodice
            // 
            resources.ApplyResources(labelCodice, "labelCodice");
            labelCodice.Name = "labelCodice";
            // 
            // codiceRicevitore
            // 
            resources.ApplyResources(codiceRicevitore, "codiceRicevitore");
            codiceRicevitore.Name = "codiceRicevitore";
            // 
            // textCodice
            // 
            resources.ApplyResources(textCodice, "textCodice");
            textCodice.Name = "textCodice";
            // 
            // buttonRicevitore
            // 
            resources.ApplyResources(buttonRicevitore, "buttonRicevitore");
            buttonRicevitore.Name = "buttonRicevitore";
            buttonRicevitore.UseVisualStyleBackColor = true;
            buttonRicevitore.Click += onClickRicevitore;
            // 
            // buttonVisualizzazione
            // 
            resources.ApplyResources(buttonVisualizzazione, "buttonVisualizzazione");
            buttonVisualizzazione.Name = "buttonVisualizzazione";
            buttonVisualizzazione.UseVisualStyleBackColor = true;
            buttonVisualizzazione.Click += onClickVisualizza;
            // 
            // buttonFolder
            // 
            resources.ApplyResources(buttonFolder, "buttonFolder");
            buttonFolder.Name = "buttonFolder";
            buttonFolder.UseVisualStyleBackColor = true;
            buttonFolder.Click += onClickFolder;
            // 
            // lblUserRicevitore
            // 
            resources.ApplyResources(lblUserRicevitore, "lblUserRicevitore");
            lblUserRicevitore.Name = "lblUserRicevitore";
            // 
            // textUserRicevitore
            // 
            resources.ApplyResources(textUserRicevitore, "textUserRicevitore");
            textUserRicevitore.Name = "textUserRicevitore";
            // 
            // labelUsername
            // 
            resources.ApplyResources(labelUsername, "labelUsername");
            labelUsername.Name = "labelUsername";
            // 
            // labelUsername1
            // 
            resources.ApplyResources(labelUsername1, "labelUsername1");
            labelUsername1.Name = "labelUsername1";
            // 
            // Form3
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(labelUsername1);
            Controls.Add(labelUsername);
            Controls.Add(textUserRicevitore);
            Controls.Add(lblUserRicevitore);
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
            MaximizeBox = false;
            Name = "Form3";
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
        private Label lblUserRicevitore;
        private TextBox textUserRicevitore;
        private Label labelUsername;
        private Label labelUsername1;
    }
}