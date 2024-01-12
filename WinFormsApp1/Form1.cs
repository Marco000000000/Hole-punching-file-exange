namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void Button1_Click(object sender, EventArgs e)
        {
            bool flag=false;

            lblHelloWorld.Text = string.Empty;
            if (textName.Text.Equals("") || textPassword.Text.Equals(""))
            {
                lblHelloWorld.Text = "VOLEVI, GUARDA che FASCIA NON SE LO ASPETTAVA";
                flag = true;
            }

            if(!flag)
            {
                textName.Text = string.Empty;
                textPassword.Text = string.Empty;

               Form3 form3 = new Form3();
                form3.ShowDialog();
                //bisognerebbe togliere il form di accesso ma se lo chiudo viene chiuso tutto
                //e se lo nascondo rimane aperto come processo in backgrounf
                
                

                
            }
            //se tutto va bene invio le credeziali per poter eseguire l'accesso
            //se esistono chiudo la schede e ne apro una relativa a trasmissione 


        }

        private void TextBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void textName_TextChanged(object sender, EventArgs e)
        {

        }

        private void OnClickRegister(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();
           // form2.Activate();

        }
     }
}
