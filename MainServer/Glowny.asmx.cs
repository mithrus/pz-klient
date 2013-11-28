using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;


namespace MainServer
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Glowny : System.Web.Services.WebService
    {
        //Tymczasowe deklaracje;
        static private Komunikat temp = new Komunikat();
        static private List<Wiadomosc> wiadomosci = new List<Wiadomosc>();
        static private List<Uzytkownik> uzytkownicy = new List<Uzytkownik>(); //zalogowani użytkownicy

        //static private List<Key> klucze = new List<Key>(); //klucze(id,token) zalogowanych użytkowników

        static private StatycznaBaza baza = new StatycznaBaza();

        //Logowanie
        [WebMethod]
        public List<Uzytkownik> ZwrocZalogowanych() //Funkcja testowa
        {
            return Baza.ZwrocUzytkownikowZalogowanych();
        }

        [WebMethod]
        public bool dodajZw() //Funkcja testowa
        {
            string nazwa = "rafs";
            int wyg = 2000;
            int pok = 1;
            return Baza.DodajZwyciezce(nazwa, pok, wyg);
        }

        [WebMethod]
        public Komunikat Zarejestruj(string nazwa, string haslo, string email) //Funkcja działająca!!!
        {//rejestracja uzytkownika
            Komunikat kom = new Komunikat();
            if (Baza.CzyIstniejeUzytkownik(nazwa))
            {
                kom.kodKomunikatu = 111;
                kom.trescKomunikatu = "ISTNIEJE";
            }
            else
            {
                if(!Baza.CzyPoprawnyEmail(email))
                {
                    kom.trescKomunikatu="NIEPOPRAWNY FORMAT";
                    return kom;
                }

                if (Baza.CzyIstniejEmail(email))
                {
                    kom.kodKomunikatu = 111;
                    kom.trescKomunikatu = "ISTNIEJE";
                }
                else
                {
                    if(Baza.DodajUzytkownika(nazwa, email, haslo))
                        kom.trescKomunikatu = "OK";
                    else
                        kom.trescKomunikatu = "Blad";
                }
            }
            return kom;
        }
        
        [WebMethod]
        public Komunikat SprawdzNazwe(string nazwa)//Funkcja działająca!!!
        {
            Komunikat kom = new Komunikat();
            if (Baza.CzyIstniejeUzytkownik(nazwa))
            {
                kom.trescKomunikatu = "ISTNIEJE";
                kom.kodKomunikatu = 111;
            }
            else
            {
                kom.trescKomunikatu = "OK";
                kom.kodKomunikatu = 100;
            }
            return kom;
        }
        
        [WebMethod]
        public Komunikat SprawdzEmail(string email)//Funkcja działająca!!!
        {
            Komunikat kom = new Komunikat();
            if (!Baza.CzyPoprawnyEmail(email))
            {
                kom.trescKomunikatu = "NIEPOPRAWNY FORMAT";
                kom.kodKomunikatu = 110;
                return kom;
            }
            if (Baza.CzyIstniejEmail(email))
            {
                kom.trescKomunikatu = "ISTNIEJE";
                kom.kodKomunikatu = 111;
            }
            else
            {
                kom.trescKomunikatu = "OK";
                kom.kodKomunikatu = 100;
            }
            return kom;
        }

        [WebMethod]
        public byte[] Zaloguj(string nazwa, string haslo)//Funkcja działająca!!!
        {
            byte[] token = null;
            Komunikat kom = new Komunikat();
            if (!Baza.CzyIstniejeUzytkownik(nazwa))
            {
                kom.trescKomunikatu = "NIE ISTNIEJE";
            }
            else
            {
                if(!Baza.CzyPoprawneHaslo(haslo,nazwa))
                {
                    kom.trescKomunikatu = "NIEPOPRAWNE DANE LOGOWANIA";
                }
                else
                {
                    byte[] temp = Baza.CzyZalogowany(nazwa);
                    if (temp == null)
                    {
                        token = Baza.Zaloguj(nazwa);
                    }
                    else
                    {
                        Baza.Wyloguj(temp);
                        //Baza.PrzedluzToken(temp);
                        token = Baza.Zaloguj(nazwa);
                    }
                    uzytkownicy = Baza.ZwrocUzytkownikowZalogowanych();
                }
            }

            return token;
        }
       
        [WebMethod]
        public Komunikat Wyloguj(byte[] token)
        {
            Komunikat kom = new Komunikat();
            //if(Baza.CzyPoprawnyToken(token))
            if (Baza.CzyPoprawny(token))
            {
                kom = Baza.Wyloguj(token);
                uzytkownicy = Baza.ZwrocUzytkownikowZalogowanych();
            }
            else
            {
                kom.trescKomunikatu = "BLAD";
            }
            return kom;
        }

        //[WebMethod]
        //public Komunikat ZmienHaslo(string token, string haslo)
        //{
        //    try
        //    {
        //        Key klucz = klucze.Find(delegate(Key u) { return u.token == token; });
        //        if (klucz == null)
        //        {//nieprawidlowy token
        //            temp.kodKomunikatu = 666;
        //            temp.trescKomunikatu = "Nie powiodło się!!! Nieprawidłowy token!!!";
        //        }
        //        else
        //        {//prawidlowy token -> user zalogowany                    
        //            baza_user user = baza.zarejestrowani.Find(delegate(baza_user u)
        //            {
        //                return u.idUzytkownika == klucz.identyfikatorUzytkownika;
        //            });

        //            baza.zarejestrowani.Remove(user);
        //            user.haslo = haslo;
        //            baza.zarejestrowani.Add(user);

        //            temp.kodKomunikatu = 200;
        //            temp.trescKomunikatu = "Hasło zostało zmienione!";
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        temp.kodKomunikatu = 666;
        //        temp.trescKomunikatu = "Fatal error!!!\n" + e.ToString();
        //    }
        //    return temp;
        //}

        //[WebMethod]
        //public Komunikat ResetujHaslo(string email)
        //{
        //    try
        //    {
        //        baza_user user = baza.zarejestrowani.Find(delegate(baza_user u) { return u.email == email; });

        //        if (user == null)
        //        {//brak zarejestrowanego uzytkownika na podany email
        //            temp.kodKomunikatu = 404;
        //            temp.trescKomunikatu = "Wykryto konflikt!!! Brak adresu email " + email + " w bazie!!!";
        //        }
        //        else
        //        {//email wystepuje w bazie                    
        //            if (uzytkownicy.FindIndex(delegate(Uzytkownik u) { return u.identyfikatorUzytkownika == user.idUzytkownika; }) < 0)
        //            {//błąd, użytkownik jest zalogowany
        //                temp.kodKomunikatu = 404;
        //                temp.trescKomunikatu = "Wykryto konflikt!!! Użytkownik jest zalogowany.";
        //            }
        //            else
        //            {//użytkownik niezalogowany -> generacja nowego hasła
        //                string noweHaslo = new Key(0).token;
        //                baza.zarejestrowani.Remove(user);
        //                user.haslo = noweHaslo;
        //                baza.zarejestrowani.Add(user);

        //                // WYSŁANIE EMAIL'a Z NOWYM HASŁEM

        //                System.Net.Mail.MailMessage text = new System.Net.Mail.MailMessage(
        //                    //nadawca
        //                    "system@poker.pl",
        //                    //odbiorca
        //                    email,
        //                    //temat
        //                    "Odzyskiwanie hasla",
        //                    //<treść>
        //                    "[wiadomość wygenerowana automatycznie, nie odpowiadaj na nią]\n\n" +
        //                    "Witaj " + user.nazwa + " !!!\n\n" +
        //                    "Twoje nowe hasło wygenerowane automatycznie to:\n" +
        //                    noweHaslo +
        //                    "\n\nMożesz teraz się zalogować do gry Poker!!!"
        //                    //</treść>
        //                    );

        //                System.Net.Mail.SmtpClient SMTPserwer = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587);
        //                SMTPserwer.Credentials = new System.Net.NetworkCredential("pokertxh@gmail.com", "bacillo52");
        //                SMTPserwer.EnableSsl = true;

        //                SMTPserwer.Send(text);

        //                temp.kodKomunikatu = 201;
        //                temp.trescKomunikatu = "Mail wysłany!!!";
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        temp.kodKomunikatu = 666;
        //        temp.trescKomunikatu = "Fatal error!!!\n"+e.ToString();
        //    }

        //    return temp;
        //}

        //[WebMethod]
        //public Komunikat UsunKonto(string token)
        //{//trzeba byc zalogowanym, aby usunac konto
        //    try
        //    {
        //        Key klucz = klucze.Find(delegate(Key u) { return u.token == token; });
        //        if (klucz == null)
        //        {//nieprawidlowy token
        //            temp.kodKomunikatu = 666;
        //            temp.trescKomunikatu = "Nie powiodło się!!! Nieprawidłowy token!!!";
        //        }
        //        else
        //        {//prawidlowy token -> user zalogowany
        //            baza_user user = baza.zarejestrowani.Find(delegate(baza_user u)
        //            {
        //                return u.idUzytkownika == klucz.identyfikatorUzytkownika;
        //            });

        //            baza.zarejestrowani.Remove(user);

        //            temp.kodKomunikatu = 200;
        //            temp.trescKomunikatu = "Konto zostało usuniętę!!!";
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        temp.kodKomunikatu = 666;
        //        temp.trescKomunikatu = "Fatal error!!!\n" + e.ToString();
        //    }
        //    return temp;
        //}

        //Chat
        [WebMethod]
        public List<Uzytkownik> PobierzUzytkownikow(string token)
        {
            return uzytkownicy;
        }

        [WebMethod]
        public List<Wiadomosc> PobierzWiadomosci(string token, Int32 timT, Int64 pokoj)
        {
            List<Wiadomosc> wiad = new List<Wiadomosc>();
            wiad.Clear();
            for (int i = 0; i < wiadomosci.Count; i++)
            {
                if (timT < wiadomosci.ElementAt(i).stempelCzasowy)
                {
                    wiad.Add(wiadomosci.ElementAt(i));                   
                }
            }
            return wiad;
        }

        [WebMethod]
        public Komunikat WyslijWiadomosc(string token, Wiadomosc wiadomosc)
        {
            Int32 timer;
            timer = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            wiadomosc.stempelCzasowy = timer;
            wiadomosci.Add(wiadomosc);
            //Baza.DodajWiadomosc(wiadomosc);
            temp.trescKomunikatu = "wyslano";
            return temp;
        }

        //serwer-serwer
        [WebMethod]
        static public Uzytkownik PobierzUzytkownika(Int64 id)
        {
            return uzytkownicy.Find(delegate(Uzytkownik c) { return c.identyfikatorUzytkownika == id; });
        }
        //co dalej?

    }
}
