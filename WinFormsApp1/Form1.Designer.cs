namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnClickThis = new Button();
            lblHelloWorld = new Label();
            label1 = new Label();
            textName = new TextBox();
            label2 = new Label();
            textPassword = new TextBox();
            label3 = new Label();
            label4 = new Label();
            button1 = new Button();
            SuspendLayout();
            // 
            // btnClickThis
            // 
            btnClickThis.BackColor = SystemColors.MenuHighlight;
            btnClickThis.Location = new Point(86, 321);
            btnClickThis.Name = "btnClickThis";
            btnClickThis.Size = new Size(149, 50);
            btnClickThis.TabIndex = 0;
            btnClickThis.Text = "Ok";
            btnClickThis.UseVisualStyleBackColor = false;
            btnClickThis.Click += Button1_Click;
            // 
            // lblHelloWorld
            // 
            lblHelloWorld.AutoSize = true;
            lblHelloWorld.Location = new Point(321, 167);
            lblHelloWorld.Name = "lblHelloWorld";
            lblHelloWorld.Size = new Size(0, 20);
            lblHelloWorld.TabIndex = 1;
            lblHelloWorld.Click += Label1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(35, 137);
            label1.Name = "label1";
            label1.Size = new Size(50, 20);
            label1.TabIndex = 2;
            label1.Text = "Nome";
            // 
            // textName
            // 
            textName.Location = new Point(133, 137);
            textName.Name = "textName";
            textName.Size = new Size(125, 27);
            textName.TabIndex = 3;
            textName.TextChanged += textName_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(35, 202);
            label2.Name = "label2";
            label2.Size = new Size(70, 20);
            label2.TabIndex = 4;
            label2.Text = "Password";
            label2.Click += Label1_Click;
            // 
            // textPassword
            // 
            textPassword.Location = new Point(133, 202);
            textPassword.Name = "textPassword";
            textPassword.Size = new Size(125, 27);
            textPassword.TabIndex = 5;
            textPassword.TextChanged += TextBox1_TextChanged_1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(135, 40);
            label3.Name = "label3";
            label3.Size = new Size(60, 20);
            label3.TabIndex = 6;
            label3.Text = "ACCEDI";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(526, 137);
            label4.Name = "label4";
            label4.Size = new Size(197, 20);
            label4.TabIndex = 7;
            label4.Text = "se vuoi registrati clicca sotto";
            // 
            // button1
            // 
            button1.Location = new Point(608, 210);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 8;
            button1.Text = "REGISTRATI";
            button1.UseVisualStyleBackColor = true;
            button1.Click += OnClickRegister;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button1);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(textPassword);
            Controls.Add(label2);
            Controls.Add(textName);
            Controls.Add(label1);
            Controls.Add(lblHelloWorld);
            Controls.Add(btnClickThis);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnClickThis;
        private Label lblHelloWorld;
        private Label label1;
        private TextBox textName;
        private Label label2;
        private TextBox textPassword;
        private Label label3;
        private Label label4;
        private Button button1;
    }
}
