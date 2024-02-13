namespace WinFormsApp1
{
    partial class Form4
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form4));
            listView2 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            imageList1 = new ImageList(components);
            buttonSeleziona = new Button();
            label1 = new Label();
            label2 = new Label();
            buttonVisualizzazione1 = new Button();
            labelCodiceR = new Label();
            buttonIndietro = new Button();
            SuspendLayout();
            // 
            // listView2
            // 
            listView2.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listView2.LargeImageList = imageList1;
            listView2.Location = new Point(46, 90);
            listView2.Name = "listView2";
            listView2.Size = new Size(734, 334);
            listView2.SmallImageList = imageList1;
            listView2.TabIndex = 2;
            listView2.UseCompatibleStateImageBehavior = false;
            listView2.View = View.Details;
            listView2.DoubleClick += listView2_DoubleClick;
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
            // buttonSeleziona
            // 
            buttonSeleziona.Location = new Point(800, 378);
            buttonSeleziona.Name = "buttonSeleziona";
            buttonSeleziona.Size = new Size(136, 46);
            buttonSeleziona.TabIndex = 3;
            buttonSeleziona.Text = "Download";
            buttonSeleziona.UseVisualStyleBackColor = true;
            buttonSeleziona.Click += buttonSeleziona_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(180, 31);
            label1.Name = "label1";
            label1.Size = new Size(359, 20);
            label1.TabIndex = 4;
            label1.Text = "seleziona il file da scaricare e poi clicca su download";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(593, 31);
            label2.Name = "label2";
            label2.Size = new Size(187, 20);
            label2.TabIndex = 5;
            label2.Text = "Sei collegato con l'utente=";
            // 
            // buttonVisualizzazione1
            // 
            buttonVisualizzazione1.Location = new Point(284, 430);
            buttonVisualizzazione1.Name = "buttonVisualizzazione1";
            buttonVisualizzazione1.Size = new Size(119, 57);
            buttonVisualizzazione1.TabIndex = 12;
            buttonVisualizzazione1.Text = "Cambia Visualizzazione";
            buttonVisualizzazione1.UseVisualStyleBackColor = true;
            buttonVisualizzazione1.Click += onClickVisualizzazione;
            // 
            // labelCodiceR
            // 
            labelCodiceR.AutoSize = true;
            labelCodiceR.Location = new Point(812, 31);
            labelCodiceR.Margin = new Padding(0);
            labelCodiceR.Name = "labelCodiceR";
            labelCodiceR.Size = new Size(0, 20);
            labelCodiceR.TabIndex = 13;
            // 
            // buttonIndietro
            // 
            buttonIndietro.Location = new Point(823, 90);
            buttonIndietro.Name = "buttonIndietro";
            buttonIndietro.Size = new Size(101, 52);
            buttonIndietro.TabIndex = 14;
            buttonIndietro.Text = "Torna Indietro";
            buttonIndietro.UseVisualStyleBackColor = true;
            buttonIndietro.Click += buttonIndietro_Click;
            // 
            // Form4
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(957, 520);
            Controls.Add(buttonIndietro);
            Controls.Add(labelCodiceR);
            Controls.Add(buttonVisualizzazione1);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(buttonSeleziona);
            Controls.Add(listView2);
            MaximizeBox = false;
            Name = "Form4";
            Text = "Form4";
            Load += Form4_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView listView2;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private Button buttonSeleziona;
        private Label label1;
        private Label label2;
        private Button buttonVisualizzazione1;
        private Label labelCodiceR;
        private ImageList imageList1;
        private Button buttonIndietro;
    }
}