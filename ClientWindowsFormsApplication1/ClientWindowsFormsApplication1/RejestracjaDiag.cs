using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ClientWindowsFormsApplication1.Glowny;

namespace ClientWindowsFormsApplication1
{
    public partial class RejestracjaDiag : Form
    {
        public RejestracjaDiag()
        {
            InitializeComponent();
        }

        public void button1_Click(object sender, EventArgs e)//ZAREJESTRUJ
        {
            try
            {                
                Serwisy.kom = Serwisy.serwerGlowny.Zarejestruj(textBox1.Text, textBox2.Text, textBox3.Text);
                string messageBoxText = Serwisy.kom.trescKomunikatu;
                string caption = "Rejestracja";

                if (Serwisy.kom.kodKomunikatu == 201)
                {//komunikat OK
                    MessageBox.Show(messageBoxText, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {//jakiś błąd nazwy/adresu email
                    MessageBox.Show(messageBoxText, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Text = "";
                    textBox2.Text = "";
                    textBox3.Text = "";
                }
            }
            catch (Exception exc)
            {//błąd np. usługa niedostępna
                MessageBox.Show(exc.Message, exc.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }                   
        }
    }
}
