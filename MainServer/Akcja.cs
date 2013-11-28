using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MainServer
{
    public class Akcja
    {
        public Int64 identyfikatorGracza;
        public string nazwaAkcji;
        public DateTime stempelCzasowy;
        public Int64 numerStolu;
        public List<Karta> kartyGracza;
        public List<Karta> kartyNaStole;
        public bool malyBlind;
        public bool duzyBlind;
        public Int64 obecnaStawkaStolu;
        public Int64 obecnaStawkaGracza;
        public Int64 iloscKasyNaStole;
        public Int64 iloscKasyGracza;
        //o czymś jeszcze myślałem, jak mi się przypomni to dopiszę
        // co z użytkownikami którzy przegrają?

    }
}