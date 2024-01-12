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
            labelCodiceR = new Label();
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
            listView2.SelectedIndexChanged += listView2_SelectedIndexChanged;
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
            imageList1.Images.SetKeyName(0, "pdf.png");
            imageList1.Images.SetKeyName(1, "file_generic.png");
            // 
            // buttonSeleziona
            // 
            buttonSeleziona.Location = new Point(800, 378);
            buttonSeleziona.Name = "buttonSeleziona";
            buttonSeleziona.Size = new Size(136, 46);
            buttonSeleziona.TabIndex = 3;
            buttonSeleziona.Text = "Download";
            buttonSeleziona.UseVisualStyleBackColor = true;
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
            label2.Location = new Point(738, 24);
            label2.Name = "label2";
            label2.Size = new Size(61, 20);
            label2.TabIndex = 5;
            label2.Text = "utente=";
            // 
            // labelCodiceR
            // 
            labelCodiceR.AutoSize = true;
            labelCodiceR.Location = new Point(805, 24);
            labelCodiceR.Name = "labelCodiceR";
            labelCodiceR.Size = new Size(0, 20);
            labelCodiceR.TabIndex = 6;
            // 
            // Form4
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(957, 520);
            Controls.Add(labelCodiceR);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(buttonSeleziona);
            Controls.Add(listView2);
            Name = "Form4";
            Text = "Form4";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView listView2;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ImageList imageList1;
        private Button buttonSeleziona;
        private Label label1;
        private Label label2;
        private Label labelCodiceR;
    }
}