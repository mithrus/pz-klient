using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ClientWindowsFormsApplication1.Rozgrywki;

namespace ClientWindowsFormsApplication1
{
    public partial class NowyStolForm : Form
    {
        public Form1 forma;
        public NowyStolForm()
        {
            //forma = Form1.ActiveForm as Form1;
            InitializeComponent();
            label3.Text = trackBar1.Value.ToString();
            trackBar1.ValueChanged += new System.EventHandler(trackBar1_ValueChanged);
        }

        private void trackBar1_ValueChanged(object sender, System.EventArgs e)//max liczba graczy 2-8
        {
            label3.Text = trackBar1.Value.ToString();
        }

        private void button2_Click(object sender, EventArgs e)//ANULUJ 
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)//TWORZENIE STOŁU 
        {
            //Serwisy.pokoj.nazwaPokoju = new string(textBox1.Text.ToCharArray());
            //Serwisy.pokoj.iloscGraczyMax = trackBar1.Value;
            //Serwisy.pokoj.stawkaWejsciowa = (int)numericUpDown1.Value;
            //Serwisy.pokoj.duzyBlind = (int)numericUpDown2.Value;

            Serwisy.str = textBox1.Text;

            try
            {
                Serwisy.komR = Serwisy.serwerRozgrywki.UtworzStol(Serwisy.token, textBox1.Text, trackBar1.Value, (int)numericUpDown1.Value, (int)numericUpDown2.Value);
                
                if (Serwisy.komR.kodKomunikatu == 200)
                {//utworzenie stolu
                    MessageBox.Show(Serwisy.komR.trescKomunikatu, "Nowy stół", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    Serwisy.pokoj = new Pokoj();
                    Serwisy.pokoj = Serwisy.serwerRozgrywki.PobierzPokoje(Serwisy.token).Single(delegate(Pokoj c){return c.nazwaPokoju==Serwisy.str;});

                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                }
                else
                {// błąd: zły token, nazwa stołu już istnieje...
                    MessageBox.Show(Serwisy.komR.trescKomunikatu, "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception exc)
            {//błąd połączenia
                MessageBox.Show(exc.Message, exc.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = System.Windows.Forms.DialogResult.Abort;
                this.Close();
            } 

        }






    }
}
