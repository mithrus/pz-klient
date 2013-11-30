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

                        tabPage2.Enabled = false;   //karta rozgrywki
                        Serwisy.pokoj = new Pokoj();
                    }
                    else
                    {
                        MessageBox.Show(Serwisy.komR.trescKomunikatu, "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //MessageBox.Show(Serwisy.komR.trescKomunikatu, Serwisy.komR.kodKomunikatu.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        timer2.Start();
                    }
                    else
                    {
                        MessageBox.Show(Serwisy.komR.trescKomunikatu, "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //MessageBox.Show(Serwisy.komR.trescKomunikatu, Serwisy.komR.kodKomunikatu.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                

                UtworzEtykietyKart();
                start = true;
                timer2.Start();
            }
            //this.Enabled = false;
            //timer2.Start();
            
        }

        private void timer2_Tick(object sender, EventArgs e)//pobieranie akcji
        {
            gra =Serwisy.serwerRozgrywki.ZwrocGre(Serwisy.token);

            if (gra != null)
            {
                label11.Text = gra.pula.ToString(); // na stole
                List<Gracz> aa = new List<Gracz>(gra.aktywni);
                Rozgrywki.Gracz t = gra.aktywni.Single<Gracz>(delegate (Gracz c){ return c.identyfikatorUzytkownika == Serwisy.mojID;});
                if (ja != t)
                {//aktualizacja wszystkiego co związane z naszym graczem
                    ja = t;
                    if (gra.stan == Stan.PREFLOP)
                    {//pobranie kart i wyświetlenie ich
                        List<Karta> k = new List<Karta>(Serwisy.serwerRozgrywki.PobierzKarty(Serwisy.token));
                        label5.Text = k[0].figura + " " + k[0].kolor + " || " + k[1].figura + " " + k[1].kolor;
                       
                    }
                    label8.Text = ja.kasa.ToString(); // moja kasa
                    label9.Text = ja.stawia.ToString(); // ile stawiam
                    if (gra.stan == Stan.FLOP)
                    {
                        List<Karta> stol=new List<Karta>(Serwisy.serwerRozgrywki.zwrocStol(Serwisy.token));
                        textBox6.Clear();
                        for (int i = 0; i < stol.Count; i++)
                        {
                            textBox6.AppendText(stol[i].figura+" "+ stol[i].kolor+ " || ");
                        }

                    }
                }
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
            }
            
        }

        void RozdajGraczom()
        {
            if (!start)
                UtworzEtykietyKart();
            int i = 0;

            //foreach (Akcja a in Serwisy.akcje)
            //{
            //    etykietyKart[i].Text = a.kartyGracza[0].figura + " " + a.kartyGracza[0].kolor;
            //    etykietyKart[i+1].Text = a.kartyGracza[1].figura + " " + a.kartyGracza[1].kolor;
            //    i += 2;
            //}
            if(i>0)
                rozdanie = false;

        }

        void UtworzEtykietyKart()
        {
            Int64 nr = Serwisy.pokoj.numerPokoju;
            Rozgrywki.Pokoj[] pok = Serwisy.serwerRozgrywki.PobierzPokoje(Serwisy.token);

            Serwisy.pokoje.Clear();
            ////comboBox1.Items.Clear();
            for (int i = 0; i < pok.Length; i++)
            {
                //{//przepisanie ściągniętej tablicy stołów
                Serwisy.pokoje.Add(pok[i]);
            }

            Serwisy.pokoj = Serwisy.pokoje.Find(delegate(Pokoj c) { return c.numerPokoju == nr; });
            for (int i = 0; i < (2 * Serwisy.pokoj.user.Length); i += 2)
            {

                //etykietyKart.Add(new Label());
                if (i == 0)
                {
                    //etykietyKart[i].Location = new Point(260, 280);
                    //etykietyKart[i + 1].Location = new Point(260, 295);
                    etykietyKart.Add(new Label { Text = "1-1", Location = new Point(260, 280) });
                    etykietyKart.Add(new Label { Text = "1-2", Location = new Point(260, 300) });
                }
                else
                    if (i == 2)
                    {
                        //etykietyKart[i].Location = new Point(100, 170);
                        //etykietyKart[i + 1].Location = new Point(85, 170);
                        etykietyKart.Add(new Label { Text = "2-1", Location = new Point(85, 170) });
                        etykietyKart.Add(new Label { Text = "2-2", Location = new Point(85, 185) });
                    }
                    else
                        if (i == 4)
                        {
                            //etykietyKart[i].Location = new Point(260, 75);
                            //etykietyKart[i + 1].Location = new Point(260, 60);
                            etykietyKart.Add(new Label { Text = "3-1", Location = new Point(260, 75) });
                            etykietyKart.Add(new Label { Text = "3-2", Location = new Point(260, 50) });
                        }
                //RozdajGraczom();
                tabPage2.Controls.Add(etykietyKart[i]);
                tabPage2.Controls.Add(etykietyKart[i + 1]);
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
            Serwisy.komR = Serwisy.serwerRozgrywki.CallRiseAllIn(Serwisy.token, Convert.ToInt64(textBox5.Text)); //(Int64)numericUpDown1.Value);
            if (Serwisy.komR.kodKomunikatu == 200)
                timer2.Start();
            //else alarm
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
        public static Int64 mojID;

        public static byte[] token;
        public static string str;
 

    }




}
