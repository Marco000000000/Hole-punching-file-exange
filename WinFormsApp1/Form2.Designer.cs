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
            labelRegistrazione = new Label();
            textPassword = new TextBox();
            labelPassword = new Label();
            textName = new TextBox();
            labelNome = new Label();
            buttonRegister = new Button();
            lblError = new Label();
            lblErrorName = new Label();
            lblErrorPassword = new Label();
            SuspendLayout();
            // 
            // labelRegistrazione
            // 
            labelRegistrazione.AutoSize = true;
            labelRegistrazione.Location = new Point(318, 51);
            labelRegistrazione.Name = "labelRegistrazione";
            labelRegistrazione.Size = new Size(99, 20);
            labelRegistrazione.TabIndex = 0;
            labelRegistrazione.Text = "Registrazione";
            // 
            // textPassword
            // 
            textPassword.Location = new Point(360, 218);
            textPassword.Name = "textPassword";
            textPassword.PasswordChar = '*';
            textPassword.Size = new Size(125, 27);
            textPassword.TabIndex = 9;
            // 
            // labelPassword
            // 
            labelPassword.AutoSize = true;
            labelPassword.Location = new Point(262, 218);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(70, 20);
            labelPassword.TabIndex = 8;
            labelPassword.Text = "Password";
            // 
            // textName
            // 
            textName.Location = new Point(360, 128);
            textName.Name = "textName";
            textName.Size = new Size(125, 27);
            textName.TabIndex = 7;
            // 
            // labelNome
            // 
            labelNome.AutoSize = true;
            labelNome.Location = new Point(262, 128);
            labelNome.Name = "labelNome";
            labelNome.Size = new Size(50, 20);
            labelNome.TabIndex = 6;
            labelNome.Text = "Nome";
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
            lblErrorPassword.Size = new Size(0, 20);
            lblErrorPassword.TabIndex = 13;
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
            Controls.Add(labelPassword);
            Controls.Add(textName);
            Controls.Add(labelNome);
            Controls.Add(labelRegistrazione);
            MaximizeBox = false;
            Name = "Form2";
            Text = "Form2";
            Load += Form2_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelRegistrazione;
        private TextBox textPassword;
        private Label labelPassword;
        private TextBox textName;
        private Label labelNome;
        private Button buttonRegister;
        private Label lblError;
        private Label lblErrorName;
        private Label lblErrorPassword;
    }
}