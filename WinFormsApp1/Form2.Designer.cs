namespace WinFormsApp1
{
    partial class Form2
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
            label1 = new Label();
            textPassword = new TextBox();
            label2 = new Label();
            textName = new TextBox();
            label3 = new Label();
            buttonRegister = new Button();
            lblError = new Label();
            lblErrorName = new Label();
            lblErrorPassword = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(318, 51);
            label1.Name = "label1";
            label1.Size = new Size(99, 20);
            label1.TabIndex = 0;
            label1.Text = "Registrazione";
            label1.Click += label1_Click;
            // 
            // textPassword
            // 
            textPassword.Location = new Point(360, 218);
            textPassword.Name = "textPassword";
            textPassword.Size = new Size(125, 27);
            textPassword.TabIndex = 9;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(262, 218);
            label2.Name = "label2";
            label2.Size = new Size(70, 20);
            label2.TabIndex = 8;
            label2.Text = "Password";
            // 
            // textName
            // 
            textName.Location = new Point(360, 128);
            textName.Name = "textName";
            textName.Size = new Size(125, 27);
            textName.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(262, 128);
            label3.Name = "label3";
            label3.Size = new Size(50, 20);
            label3.TabIndex = 6;
            label3.Text = "Nome";
            // 
            // buttonRegister
            // 
            buttonRegister.BackColor = SystemColors.ActiveBorder;
            buttonRegister.Location = new Point(591, 305);
            buttonRegister.Name = "buttonRegister";
            buttonRegister.Size = new Size(170, 70);
            buttonRegister.TabIndex = 10;
            buttonRegister.Text = "procedi";
            buttonRegister.UseVisualStyleBackColor = false;
            buttonRegister.Click += OnClickBottonRegister;
            // 
            // lblError
            // 
            lblError.AutoSize = true;
            lblError.ForeColor = Color.Red;
            lblError.Location = new Point(108, 305);
            lblError.Name = "lblError";
            lblError.Size = new Size(0, 20);
            lblError.TabIndex = 11;
            lblError.Click += label4_Click;
            // 
            // lblErrorName
            // 
            lblErrorName.AutoSize = true;
            lblErrorName.ForeColor = Color.Red;
            lblErrorName.Location = new Point(360, 176);
            lblErrorName.Name = "lblErrorName";
            lblErrorName.Size = new Size(0, 20);
            lblErrorName.TabIndex = 12;
            // 
            // lblErrorPassword
            // 
            lblErrorPassword.AutoSize = true;
            lblErrorPassword.ForeColor = Color.Red;
            lblErrorPassword.Location = new Point(360, 268);
            lblErrorPassword.Name = "lblErrorPassword";
            lblErrorPassword.Size = new Size(37, 20);
            lblErrorPassword.TabIndex = 13;
            lblErrorPassword.Text = "ciao";
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblErrorPassword);
            Controls.Add(lblErrorName);
            Controls.Add(lblError);
            Controls.Add(buttonRegister);
            Controls.Add(textPassword);
            Controls.Add(label2);
            Controls.Add(textName);
            Controls.Add(label3);
            Controls.Add(label1);
            Name = "Form2";
            Text = "Form2";
            Load += Form2_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textPassword;
        private Label label2;
        private TextBox textName;
        private Label label3;
        private Button buttonRegister;
        private Label lblError;
        private Label lblErrorName;
        private Label lblErrorPassword;
    }
}