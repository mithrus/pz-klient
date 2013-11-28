using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MainServer
{
    public class Gracz : Uzytkownik
    {
        private List<Karta> hand = new List<Karta>();
        private List<Karta> najUklad = new List<Karta>();
        public int kicker;
        //dodajemy w przypadku kiedy user wygrał
        public List<Karta> handWin = new List<Karta>();
        public List<Karta> najUkladWin = new List<Karta>();

        public enum StanGracza : int { Fold, Call, Rise, AllIn, BigBlind, SmallBlind, Dealer, Ready, NotReady, Winner };

        public StanGracza stan;
        public int wart;//wartość układu kart
        public string nazwaUkladu;

        //Nowe
        public Int64 kasa;  //ile pieniędzy pozostało w obecnej grze
        public Int64 stawia;    //ile pieniędzi stawia w obecnym rozdaniu
        //public bool start = false;
         public Gracz(Uzytkownik u, Int64 stawka)
         {
             identyfikatorUzytkownika = u.identyfikatorUzytkownika;
             nazwaUzytkownika = u.nazwaUzytkownika;
             u.kasiora -= stawka;
             kasiora = u.kasiora;
             kasa = stawka;
             stawia = 0;
             stan = StanGracza.Ready;
             wart = 0;
             nazwaUkladu = "";
         }

         public List<Karta> zwroc_hand()
         {
             return hand;
         }
         public List<Karta> zwroc_najUklad()
         {
             return najUklad;
         }

    }
}