﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ClientWindowsFormsApplication1.Glowny;
using ClientWindowsFormsApplication1.Rozgrywki;
using System.Net;

namespace ClientWindowsFormsApplication1
{
    public partial class Form1 : Form
    {        
        public Form1()
        {
            InitializeComponent();

            textBox2.PasswordChar = '*';    //zakryte hasło
            label3.ForeColor = Color.Red;   //komunikaty logowania na czerwono
            tabPage2.Enabled = false;       //karta rozgrywki niedostępna
            groupBox2.Visible = false;      //czat niewidoczny bez logowania
            groupBox3.Visible = false;      //menu stołów niewidoczne bez logowania
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            textBox4.KeyPress += new KeyPressEventHandler(button5_EnterPress);
            textBox3.ReadOnly = true;

            comboBox1.MouseClick += comboBox1_MouseClick;
            
        }
       
        private void button1_Click_1(object sender, EventArgs e)//ZALOGUJ
        {
            try
            {
                Serwisy.kom = Serwisy.serwerGlowny.Zaloguj(textBox1.Text, textBox2.Text);
                
                if (Serwisy.kom.kodKomunikatu == 200)
                {// zalogowanie
                    Serwisy.token = Serwisy.kom.trescKomunikatu;
                    label3.Text = "Nastąpiło poprawne logowanie!";

                    button2.Visible = true;

                    groupBox1.Enabled = false; //logowanie

                    groupBox2.Visible = true;   //obsługa czatu
                    groupBox3.Visible = true;   //obsługa stołów

                    Serwisy.pokoj.nazwaPokoju = "";
                    
                    pobierzStoly();

                    

                    timer1.Enabled = true;  //uruchomienie ściągania nowych wiadomości z czatu głównego
                }
                else
                {// błąd: złe hasło, brak takiego usera
                    label3.Text = Serwisy.kom.trescKomunikatu;                    
                }
            }
            catch (Exception exc)
            {// błąd połączenia
                MessageBox.Show(exc.Message, exc.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }                      
        }

        void comboBox1_MouseClick(object sender, MouseEventArgs e)//klikniecie na listę stołów
        {            
            pobierzStoly();     
        }

        private void button2_Click(object sender, EventArgs e)//WYLOGUJ
        {
            try
            {
                Serwisy.kom = Serwisy.serwerGlowny.Wyloguj(Serwisy.token);

                if (Serwisy.kom.kodKomunikatu == 200)
                {//wylogowanie
                    label3.Text = Serwisy.kom.trescKomunikatu;

                    button2.Visible = false;

                    groupBox1.Enabled = true;   //logowanie

                    groupBox2.Visible = false;  //czat
                    groupBox3.Visible = false;  //stół

                    tabPage2.Enabled = false;   //karta rozgrywki
                }
                else
                {// błąd: zły token?
                    label3.Text = Serwisy.kom.trescKomunikatu; 
                }
            }
            catch (Exception exc)
            {//błąd połączenia
                MessageBox.Show(exc.Message, exc.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            } 
        }

        private void button3_Click(object sender, EventArgs e)//NOWY STÓŁ
        {
            NowyStolForm nowyStol = new NowyStolForm();
            nowyStol.FormClosed += nowyStol_FormClosed;
            nowyStol.Show();        
        }

        private void nowyStol_FormClosed(object sender, FormClosedEventArgs e)//akcja zamknięcia okna nowego stołu
        {
            if (((NowyStolForm)sender).DialogResult == System.Windows.Forms.DialogResult.OK)
            {//został utworzony nowy stół
                pobierzStoly();

                button4.Text = "Opuść stół";
                Serwisy.wybranyStol = true;
                comboBox1.SelectedIndex = comboBox1.FindString(Serwisy.pokoj.nazwaPokoju);

                tabPage2.Enabled = true;   //karta rozgrywki
            }
            //throw new NotImplementedException();
        }

        private void rejestracjaToolStripMenuItem_Click(object sender, EventArgs e)//REJESTRACJA USERA
        {
            RejestracjaDiag rejDiag = new RejestracjaDiag();
            rejDiag.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)//WYŚLIJ WIAD. czatu głównego
        {
            wyslijWiad(0,textBox4);
        }

        private void wyslijWiad(int nrPokoju, TextBox obj)//wyślij WIAD. gdzie chcesz
        {
            if (textBox4.TextLength > 0)
            {//coś jest wpisane do wysłania
                try
                {
                    Serwisy.wiad.nazwaUzytkownika = textBox1.Text;
                    Serwisy.wiad.numerPokoju = nrPokoju;
                    Serwisy.wiad.trescWiadomosci = obj.Text;
                    Serwisy.wiad.stempelCzasowy = 0;

                    Serwisy.kom = Serwisy.serwerGlowny.WyslijWiadomosc(Serwisy.token, Serwisy.wiad);

                    if (Serwisy.kom.kodKomunikatu != 200)
                    {
                        MessageBox.Show(Serwisy.kom.trescKomunikatu, "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        obj.Clear();
                    }


                }
                catch (Exception exc)
                {// błąd połączenia
                    MessageBox.Show(exc.Message, exc.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button5_EnterPress(object sender, KeyPressEventArgs e)//akcja dla ENTER'a czatu głównego
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                wyslijWiad(0, textBox4);
            }
        }

        private void Form1_Load(object sender, EventArgs e)//ładowanie formy głównej
        {
            timer1.Interval = 500;  //co 0,5s ściąganie wiadomości czatu głównego z serwera 
            timer2.Interval = 500;
        }

        private void timer1_Tick(object sender, EventArgs e)//akcja odczytu wiadomości czatu głównego
        {
            Wiadomosc[] listaWiad = Serwisy.serwerGlowny.PobierzWiadomosci(Serwisy.token, Serwisy.czasOstatniejWiadGlobal);

            for (int i = 0; i < listaWiad.Length; i++)
            {
                Serwisy.czasOstatniejWiadGlobal = listaWiad[i].stempelCzasowy;
                TimeSpan t = TimeSpan.FromSeconds(Serwisy.czasOstatniejWiadGlobal);
                DateTime d = new DateTime(1970, 1, 1);
                d = d.Add(t);  

                textBox3.AppendText(
                    "[" + d.TimeOfDay.ToString() + "] "
                    + listaWiad[i].nazwaUzytkownika
                    + ": " + listaWiad[i].trescWiadomosci + "\n" );
                
            }

        }

        public void pobierzStoly()//pobranie listy stołów
        {
            try
            {
                Rozgrywki.Pokoj[] pok = Serwisy.serwerRozgrywki.PobierzPokoje(Serwisy.token);

                Serwisy.pokoje.Clear();
                comboBox1.Items.Clear();
                for (int i = 0; i < pok.Length; i++)
                {//przepisanie ściągniętej tablicy stołów
                    Serwisy.pokoje.Add(pok[i]);
                    
                    comboBox1.Items.Add(pok[i].nazwaPokoju);
                }
                

                if (pok.Length == 0)
                {//lista stołów jest pusta
                    MessageBox.Show("Lista stołów jest pusta.\nSpróbuj utworzyć nowy stół.", "Stan wyjątkowy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    button3.Select();
                }
                
            }
            catch (Exception exc)
            {//błąd połączenia
                MessageBox.Show(exc.Message, exc.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)//DOŁĄCZ DO STOŁU/OPUŚĆ STÓŁ
        {
            try
            {
                Serwisy.pokoj = Serwisy.pokoje.Find(delegate(Pokoj c) { return c.nazwaPokoju == comboBox1.SelectedItem.ToString(); });

                if (Serwisy.wybranyStol)
                {//jest wybrany stół -> akcja to opóść stół
                    Serwisy.komR = Serwisy.serwerRozgrywki.OpuscStol(Serwisy.token, Serwisy.pokoj.numerPokoju);

                    if (Serwisy.komR.kodKomunikatu == 200)
                    {//opuszczenie stołu
                        button4.Text = "Dołącz do stołu";
                        Serwisy.wybranyStol = false;
                        button3.Enabled = true;

                        tabPage2.Enabled = false;   //karta rozgrywki
                    }
                    else
                    {
                        //MessageBox.Show(Serwisy.komR.trescKomunikatu, "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        MessageBox.Show(Serwisy.komR.trescKomunikatu, Serwisy.komR.kodKomunikatu.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {//nie wybrano stołu -> akcja dołączania do stołu
                    //Int64 numer = Serwisy.pokoje.Find(delegate(Rozgrywki.Pokoj p){ return p.nazwaPokoju == comboBox1.SelectedText; }).numerPokoju;
                    Serwisy.komR = Serwisy.serwerRozgrywki.DolaczDoStolu(Serwisy.token, Serwisy.pokoj.numerPokoju);

                    if (Serwisy.komR.kodKomunikatu == 200)
                    {//dołączenie do stołu
                        button4.Text = "Opuść stół";
                        Serwisy.wybranyStol = true;
                        button3.Enabled = false;

                        tabPage2.Enabled = true;   //karta rozgrywki
                    }
                    else
                    {
                        //MessageBox.Show(Serwisy.komR.trescKomunikatu, "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        MessageBox.Show(Serwisy.komR.trescKomunikatu, Serwisy.komR.kodKomunikatu.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    
                }



            }
            catch (Exception exc)
            {//błąd połączenia
                MessageBox.Show(exc.Message, exc.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);                
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Serwisy.komR =  Serwisy.serwerRozgrywki.Start(Serwisy.token, Serwisy.pokoj.numerPokoju);

            MessageBox.Show(Serwisy.komR.trescKomunikatu, Serwisy.komR.kodKomunikatu.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                                  
            Serwisy.czasOstatniejAkcji = 0;

            //timer2.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)//pobieranie akcji
        {



        }



    }//koniec klasy Form1


    public static class Serwisy
    {
        public static Glowny.Glowny serwerGlowny = new Glowny.Glowny();
        public static Glowny.Komunikat kom = new Glowny.Komunikat();
        public static Wiadomosc wiad = new Wiadomosc();
        public static Int32 czasOstatniejWiadGlobal = 0;

        public static Rozgrywki.Komunikat komR = new Rozgrywki.Komunikat();
        public static Rozgrywki.Rozgrywki serwerRozgrywki = new Rozgrywki.Rozgrywki();
        public static Pokoj pokoj = new Pokoj();
        public static List<Pokoj> pokoje = new List<Pokoj>();
        public static Int32 czasOstatniejAkcji = 0;

        public static bool wybranyStol = false;

        public static string token;
        public static string str;

    }




}