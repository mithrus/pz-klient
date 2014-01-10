using System;
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
using System.Threading;

namespace ClientWindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        List<Label> etykietyKart = new List<Label>();
        bool rozdanie = true;
        bool start = false;


        bool mojRuch = false;
        Rozgrywki.Gra gra = new Gra();
        Rozgrywki.Gracz ja = new Gracz();
        private Thread _xThread;

        List<Button> btn = new List<Button>();
        List<TextBox> txt = new List<TextBox>();
        List<Rozgrywki.Uzytkownik> us;
        List<Karta> stol;

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
            groupBox4.Visible = false;

            comboBox1.MouseClick += comboBox1_MouseClick;

            paneleGraczy();
        }

        public void paneleGraczy()
        {
            int r = 0;
            for (int i = 0; i < 5; i++)
            {
                btn.Add(new Button());
                btn[i].BackColor = Color.DeepSkyBlue;
                btn[i].Location = new Point(20 + r, 30);
                btn[i].Size = new System.Drawing.Size(25, 25);
                btn[i].Visible = false;
                btn[i].Enabled = false;
                tabPage2.Controls.Add(btn[i]);
                txt.Add(new TextBox());
                txt[i].Location = new Point(5 + r, 60);
                txt[i].Multiline = true;
                txt[i].Size = new System.Drawing.Size(100, 120);
                txt[i].ReadOnly = true;
                txt[i].Visible = false;
                tabPage2.Controls.Add(txt[i]);
                r += 130;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)//ZALOGUJ
        {         
            try
            {
                Serwisy.token = Serwisy.serwerGlowny.Zaloguj(textBox1.Text, textBox2.Text);

                Serwisy.kom = Serwisy.serwerGlowny.PobierzSwojeID(Serwisy.token);


                if (Serwisy.token != null)
                {// zalogowanie                    
                    label3.Text = "Nastąpiło poprawne logowanie!";

                    button2.Visible = true;

                    groupBox1.Enabled = false; //logowanie

                    groupBox2.Visible = true;   //obsługa czatu
                    groupBox3.Visible = true;   //obsługa stołów

                    Serwisy.pokoj.nazwaPokoju = "";
                    
                    pobierzStoly();
                    
                    timer1.Enabled = true;  //uruchomienie ściągania nowych wiadomości z czatu głównego

                    Serwisy.kom = Serwisy.serwerGlowny.PobierzSwojeID(Serwisy.token);// pobranie swojego id

                    if (Serwisy.kom.kodKomunikatu == 200)
                        Serwisy.mojID = Int64.Parse(Serwisy.kom.trescKomunikatu);

                }
                else
                {// błąd: złe hasło, brak takiego usera
                    label3.Text = "Błąd logowania";                    
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
            if (rejDiag.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                button4_Click(button4, null);
                comboBox1.Enabled = false;
            }
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
            timer2.Interval = 1000;
        }

        private void timer1_Tick(object sender, EventArgs e)//akcja odczytu wiadomości czatu głównego
        {
            Wiadomosc[] listaWiad = Serwisy.serwerGlowny.PobierzWiadomosci(Serwisy.token, Serwisy.czasOstatniejWiadGlobal, 0);

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
                    Serwisy.komR = Serwisy.serwerRozgrywki.OpuscStol(Serwisy.token);

                    if (Serwisy.komR.kodKomunikatu == 200)
                    {//opuszczenie stołu
                        button4.Text = "Dołącz do stołu";
                        Serwisy.wybranyStol = false;
                        button3.Enabled = true;
                        comboBox1.Enabled = true;

                        tabPage2.Enabled = false;   //karta rozgrywki
                        Serwisy.pokoj = new Pokoj();
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
                        comboBox1.Enabled = false;

                        tabPage2.Enabled = true;   //karta rozgrywki
                        timer2.Start();
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

        private void button6_Click(object sender, EventArgs e)//START 
        {
            Serwisy.komR =  Serwisy.serwerRozgrywki.Start(Serwisy.token);
            //Int64 nr = Serwisy.pokoj.numerPokoju;

            if (Serwisy.komR.kodKomunikatu != 200)
            {
                MessageBox.Show(Serwisy.komR.trescKomunikatu, Serwisy.komR.kodKomunikatu.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //Serwisy.czasOstatniejAkcji = 0;
                //UtworzEtykietyKart();
                start = true;             
                timer2.Start();

            }
            //this.Enabled = false;
            //timer2.Start();
            
        }

        private void timer2_Tick(object sender, EventArgs e)//pobieranie akcji
        {
            try
            {
                gra = Serwisy.serwerRozgrywki.ZwrocGre(Serwisy.token);
                ////odświeżanie innych graczy przed wciśnienciem startu
                if (gra == null)
                {                    

                    /*List<Rozgrywki.Uzytkownik>*/
                    us = new List<Rozgrywki.Uzytkownik>(Serwisy.serwerRozgrywki.ZwrocUserowStart(Serwisy.token));

                    if (us.Count > 0)
                    {
                        int index = us.FindIndex(delegate(Rozgrywki.Uzytkownik a) { return a.identyfikatorUzytkownika == Serwisy.mojID; });
                        for (int i = 0; i < us.Count; i++)
                        {
                            if (i == index)
                            {
                                btn[i].Text = us[i].nazwaUzytkownika;
                                btn[i].BackColor = Color.Red;
                                btn[i].Visible = true;
                                txt[i].Visible = true;

                            }
                            else
                            {
                                btn[i].Text = us[i].nazwaUzytkownika;
                                btn[i].Visible = true;
                                txt[i].Visible = true;
                            }
                        }
                    }
                    else
                    {// i tak tu nie wchodzi!!!
                        //MessageBox.Show("Przegrałeś!", "GRA", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        //button4.Text = "Dołącz do stołu";
                        //Serwisy.wybranyStol = false;
                        //button3.Enabled = true;
                        //comboBox1.Enabled = true;

                        //tabPage2.Enabled = false;   //karta rozgrywki
                        //Serwisy.pokoj = new Pokoj();
                    }
                    //for (int i = 0; i < index; i++)
                    //{
                    //    btn[i].Text = us[i].nazwaUzytkownika;
                    //    btn[i].Visible = true;                    
                    //}

                }
                else // gra!=null
                {
                    Serwisy.gram = true;
                    daneUserow();
                    if (gra.stan == Stan.SHOWDOWN)
                    {
                        label9.Text = "0";
                        label11.Text = "0";
                        textBox7.Clear();
                        textBox7.Text = "Wygrani: \n";
                        List<Gracz> win = new List<Gracz>(gra.listaWin);
                        for (int i = 0; i < win.Count; i++)
                            textBox7.AppendText(win[i].identyfikatorUzytkownika.ToString() + " " + win[i].nazwaUzytkownika + " || ");
                    }


                    label11.Text = gra.pula.ToString(); // na stole
                    List<Gracz> aa = new List<Gracz>(gra.aktywni);
                    //=============
                    if (Serwisy.serwerRozgrywki.PobierzGracza(Serwisy.token, Serwisy.mojID) != null)
                    //if (gra.aktywni.Single<Gracz>(delegate(Gracz c) { return c.identyfikatorUzytkownika == Serwisy.mojID; }) != null)
                    {
                        Rozgrywki.Gracz t = gra.aktywni.Single<Gracz>(delegate(Gracz c) { return c.identyfikatorUzytkownika == Serwisy.mojID; });

                        if (t != null)
                        {
                            if (ja != t)
                            {//aktualizacja wszystkiego co związane z naszym graczem
                                ja = t;
                                if (gra.stan == Stan.PREFLOP)
                                {//pobranie kart i wyświetlenie ich
                                    textBox6.Clear();//czyszczenie wyświetlania stołu
                                    textBox7.Clear();//czyszczenie wyświetlania wygranych
                                    textBox8.Clear();//czyszczenie wyswietlania najlepszego ukladu
                                    label12.Text = "Twoj uklad";
                                    List<Karta> k = new List<Karta>(Serwisy.serwerRozgrywki.PobierzKarty(Serwisy.token));
                                    label5.Text = k[0].figura + " " + k[0].kolor + " || " + k[1].figura + " " + k[1].kolor;

                                }
                                label8.Text = ja.kasa.ToString(); // moja kasa
                                label9.Text = ja.stawia.ToString(); // ile stawiam
                                if (gra.stan == Stan.FLOP || gra.stan == Stan.TURN || gra.stan == Stan.RIVER || gra.stan == Stan.SHOWDOWN)
                                {
                                    /*List<Karta>*/
                                    stol = new List<Karta>(Serwisy.serwerRozgrywki.zwrocStol(Serwisy.token));
                                    textBox6.Clear();
                                    for (int i = 0; i < stol.Count; i++)
                                    {
                                        textBox6.AppendText(stol[i].figura + " " + stol[i].kolor + " || ");
                                    }
                                    label12.Text = Serwisy.serwerRozgrywki.NazwaMojegoUkladu(Serwisy.token);

                                    textBox8.Clear();//wyswietlanie najlepszego ukladu
                                    List<Karta> najUkl = new List<Karta>(Serwisy.serwerRozgrywki.MojNajUkl(Serwisy.token));
                                    for (int i = 0; i < najUkl.Count; i++)
                                    {
                                        textBox8.AppendText(najUkl[i].figura + " " + najUkl[i].kolor + " || ");
                                    }
                                }

                            }
                        }

                    }//==================

                    //===========
                    if (gra.czyjRuch == Serwisy.mojID)
                    {//gdy mój ruch, to można wyłączyć timer                    
                        numericUpDown1.Minimum = 0;
                        numericUpDown1.Maximum = ja.kasa;

                        groupBox4.Visible = true;

                        //timer2.Stop();
                    }
                    else
                    {
                        groupBox4.Visible = false;
                    }
                    if (Serwisy.serwerRozgrywki.czyWyniki(Serwisy.token) == true)
                        button9.Visible = true;
                    else
                        button9.Visible = false;

                    if (gra.stan == Stan.END)
                    {
                        timer2.Stop();
                        MessageBox.Show("Wygrałeś!", "GRA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Serwisy.komR = Serwisy.serwerRozgrywki.PotwierdzZakonczenie(Serwisy.token);
                        MessageBox.Show(Serwisy.komR.trescKomunikatu, Serwisy.komR.kodKomunikatu.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Information);

                        button4.Text = "Dołącz do stołu";
                        Serwisy.wybranyStol = false;
                        button3.Enabled = true;
                        comboBox1.Enabled = true;

                        tabPage2.Enabled = false;   //karta rozgrywki
                        Serwisy.pokoj = new Pokoj();

                    }

                }
            }
            catch (Exception exc)
            {
                if (Serwisy.gram)
                {
                    timer2.Stop();
                    MessageBox.Show("Przegrałeś!", "GRA", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    button4.Text = "Dołącz do stołu";
                    Serwisy.wybranyStol = false;
                    button3.Enabled = true;
                    comboBox1.Enabled = true;

                    tabPage2.Enabled = false;   //karta rozgrywki
                    Serwisy.pokoj = new Pokoj();
                    Serwisy.gram = false;
                }
            }
        }

        public void daneUserow()
        {
            try
            {
                List<Gracz> gr = new List<Gracz>(Serwisy.serwerRozgrywki.ZwrocGraczy(Serwisy.token));
                if (gr != null)
                {
                    int index = gr.FindIndex(delegate(Gracz a) { return a.identyfikatorUzytkownika == Serwisy.mojID; });
                    if (index >= 0)
                    {
                        for (int i = 0; i < gr.Count; i++)
                        {
                            if (gra.czyjRuch == gr[i].identyfikatorUzytkownika)
                                txt[i].BackColor = Color.SkyBlue;
                            else
                                txt[i].BackColor = Color.White;

                            int idx = gr.FindIndex(delegate(Gracz a) { return a.identyfikatorUzytkownika == us[i].identyfikatorUzytkownika; });
                            if (idx >= 0)
                            {
                                if (i == index)
                                {
                                    txt[i].Clear();
                                    txt[i].AppendText("Nazwa: " + gr[idx].nazwaUzytkownika + "||Kasa: " + gr[idx].kasa + "|| stawia: " + gr[idx].stawia + "|| Stan: " + gr[idx].stan);

                                }
                                else
                                {
                                    txt[i].Clear();
                                    txt[i].AppendText("Nazwa: " + gr[idx].nazwaUzytkownika + "||Kasa: " + gr[idx].kasa + "|| stawia: " + gr[idx].stawia + "|| Stan: " + gr[idx].stan);
                                    if (gra.stan == Stan.SHOWDOWN)
                                    {
                                        if (Serwisy.serwerRozgrywki.CzyJestWaktywnych(Serwisy.token, gr[idx].identyfikatorUzytkownika) == true)
                                        {
                                            if (gr[idx].stan != StanGracza.Fold && stol != null)//&& gr[idx].stan!=StanGracza.Winner)
                                            {
                                                txt[i].AppendText("|| Układ: " + gr[idx].nazwaUkladu);
                                                txt[i].AppendText("|| Karty: \n");
                                                List<Karta> cards = new List<Karta>(Serwisy.serwerRozgrywki.ZwrocNajUklGraczy(Serwisy.token, gr[idx].identyfikatorUzytkownika));//idx));
                                                for (int j = 0; j < 5; j++)
                                                {
                                                    txt[i].AppendText(cards[j].figura + " " + cards[j].kolor + " || ");
                                                }
                                            }
                                        }
                                    }
                                }
                            }// end if idx >= 0
                        }// end for i
                    }// end if index>=0
                }// end if gra!=null
            }
            catch (Exception exc)
            {

            }
        }

        private void button7_Click(object sender, EventArgs e) // FOLD 
        {
            Serwisy.komR = Serwisy.serwerRozgrywki.Fold(Serwisy.token);
            if(Serwisy.komR.kodKomunikatu == 200)
                timer2.Start();
            //else alarm
        }

        private void button8_Click(object sender, EventArgs e) // CALL / RISE / ALLIN
        {
            _xThread = new Thread(funkcja);
            _xThread.Start();
           // Serwisy.komR = Serwisy.serwerRozgrywki.CallRiseAllIn(Serwisy.token, Convert.ToInt64(textBox5.Text)); //(Int64)numericUpDown1.Value);
            if (Serwisy.komR.kodKomunikatu == 200)
                timer2.Start();
            //else if (Serwisy.komR.kodKomunikatu == 213)
            //{
            //    button9.Visible = true;

            //}
            //else alarm
        }

        private void funkcja() // funkcja nowego wątku na call/rise/allin
        {
            Serwisy.komR = Serwisy.serwerRozgrywki.CallRiseAllIn(Serwisy.token, Convert.ToInt64(numericUpDown1.Value));//textBox5.Text)); //(Int64)numericUpDown1.Value);
        }

        private void button9_Click(object sender, EventArgs e) // NOWE ROZDANIE
        {
            Serwisy.serwerRozgrywki.ustawNoweRoz(Serwisy.token);
            button9.Visible = false;
            //Serwisy.serwerRozgrywki.NoweRoz(Serwisy.token);
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

        //public static List<Akcja> akcje = new List<Akcja>();

        public static bool wybranyStol = false;
        public static bool gram = false;
        public static Int64 mojID;

        public static byte[] token;
        public static string str;
 

    }




}
