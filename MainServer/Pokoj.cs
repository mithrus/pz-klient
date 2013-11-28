using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MainServer
{
    public class Pokoj
    {
        //pola POKOJU
        public string nazwaPokoju;
        public Int64 numerPokoju;
        public int iloscGraczyMax;
        public int iloscGraczyObecna;
        public bool graRozpoczeta;
        public Int64 stawkaWejsciowa;
        public Int64 duzyBlind; // około 1/100 stawki wejściowej
        public List<Uzytkownik> user = new List<Uzytkownik>();

        private Gra gra;//= new Gra(duzyBlind);

        public bool jestWpokoju(Int64 idUsera)//sprawdza czy uzytkownik znajduje sie wpokoju, zwraca nr pokoju
        {
            if (user.FindIndex(delegate(Uzytkownik c) { return c.identyfikatorUzytkownika == idUsera; }) > 0)
                return true;
            else
                return false;
        }
        public Gra zwrocGre()
        {
            return gra;
        }

        public int DodajUzytkownika(Uzytkownik u)
        {
            if (iloscGraczyObecna < iloscGraczyMax)
            {
                if (user.Exists(delegate(Uzytkownik a) { return u.identyfikatorUzytkownika == a.identyfikatorUzytkownika; }))
                {
                    return 0;
                }
                else
                {
                    user.Add(u);                    
                }
            }
            return -1;
        }

        public void utworz()
        {
            gra = new Gra(duzyBlind, user, stawkaWejsciowa);
        }

        /*     public int UsunUzytkownika(Uzytkownik u)
             {
                 if (user.Exists(delegate(Uzytkownik a) { return u.identyfikatorUzytkownika == a.identyfikatorUzytkownika; }))
                 {
                     if (iloscGraczyObecna == 1)
                         ktoBlind = 0;
                     else
                     {
                         int i = user.FindIndex(delegate(Uzytkownik a) { return u.identyfikatorUzytkownika == a.identyfikatorUzytkownika; });
                         if (i == user.Count - 1)
                             ktoBlind = user[1].identyfikatorUzytkownika;
                         else
                             ktoBlind = user[i + 1].identyfikatorUzytkownika;
                     }

                     user.Remove(u);

                     return 1;
                 }
                 else
                 {
                     return 0;
                 }

             }*/


    }
}