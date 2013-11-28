using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MainServer
{
    public class UkladyKart
    {
        static Karta.figuraKarty[] figury = { Karta.figuraKarty.K2, Karta.figuraKarty.K3, Karta.figuraKarty.K4, Karta.figuraKarty.K5, Karta.figuraKarty.K6, Karta.figuraKarty.K7, Karta.figuraKarty.K8, Karta.figuraKarty.K9, Karta.figuraKarty.K10, Karta.figuraKarty.KJ, Karta.figuraKarty.KD, Karta.figuraKarty.KK, Karta.figuraKarty.KA, };
        static Karta.kolorKarty[] kolory = { Karta.kolorKarty.pik, Karta.kolorKarty.kier, Karta.kolorKarty.karo, Karta.kolorKarty.trefl };
        // public List<Karta> najlepszyUklad = new List<Karta>();
        List<Karta> temp = new List<Karta>();
        List<Karta> rekaIstol = new List<Karta>();
        int p = -1;
        static bool sf = false;//czy w tym momencie szukamy fulla

        public string co_mamy(List<Karta> stol, List<Karta> hand, List<Karta> najlepszyUklad)
        {
            generujKarty(stol, hand);
            if (czyPokerKrolewski(najlepszyUklad) == 1)
                return "Poker krolewski!";
            else if (czyPoker(najlepszyUklad) == 1)
                return "Poker!";
            else if (czyKareta(najlepszyUklad) == 1)
                return "Kareta!";
            else if (czyFull(najlepszyUklad) == 1)
                return "Full!";
            else if (czyKolor(najlepszyUklad) == 1)
                return "Kolor!";
            else if (czyStrit(najlepszyUklad) == 1)
                return "Strit!";
            else if (czyTrojka(najlepszyUklad) == 1)
                return "Trojka!";
            else if (czyDwiePary(najlepszyUklad) == 1)
                return "Dwie pary!";
            else if (czyPara(najlepszyUklad) == 1)
                return "Para!";

            return czyWysokaKarta(najlepszyUklad);
        }

        public List<Karta> reka(List<Karta> najlepszyUklad)
        {
            return najlepszyUklad;
        }
        public void generujKarty(List<Karta> stol, List<Karta> hand)
        {
            rekaIstol.Clear();
            for (int i = 0; i < hand.Count; i++)
            {
                rekaIstol.Add(new Karta { figura = hand[i].figura, kolor = hand[i].kolor });
            }
            for (int i = 0; i < stol.Count; i++)
            {
                rekaIstol.Add(new Karta { figura = stol[i].figura, kolor = stol[i].kolor });
            }
            /* rekaIstol.Add(new Karta { figura = Karta.figuraKarty.K9, kolor = Karta.kolorKarty.trefl });
             rekaIstol.Add(new Karta { figura = Karta.figuraKarty.KK, kolor = Karta.kolorKarty.trefl });
             rekaIstol.Add(new Karta { figura = Karta.figuraKarty.KD, kolor = Karta.kolorKarty.trefl });
             rekaIstol.Add(new Karta { figura = Karta.figuraKarty.KJ, kolor = Karta.kolorKarty.trefl });
             rekaIstol.Add(new Karta { figura = Karta.figuraKarty.K10, kolor = Karta.kolorKarty.trefl });
             rekaIstol.Add(new Karta { figura = Karta.figuraKarty.KK, kolor = Karta.kolorKarty.pik });
             rekaIstol.Add(new Karta { figura = Karta.figuraKarty.KK, kolor = Karta.kolorKarty.karo });*/
        }


        private bool szukaj(int n, int kol)
        {
            for (int i = 0; i < rekaIstol.Count; i++)
            {
                if (((int)rekaIstol[i].figura == n) && ((int)rekaIstol[i].kolor == kol))
                    return true;
            }
            return false;
        }

        private bool szukaj(int n)
        {
            for (int i = 0; i < rekaIstol.Count; i++)
            {
                if ((int)rekaIstol[i].figura == n)
                    return true;
            }
            return false;
        }


        private int czyPokerKrolewski(List<Karta> najlepszyUklad)
        {
            najlepszyUklad.Clear();
            int x = 0;
            for (int k = 0; k < 4; k++)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (szukaj((int)Karta.figuraKarty.K10 + i, (int)Karta.kolorKarty.pik + k) == true)
                    {
                        najlepszyUklad.Add(new Karta { figura = figury[(int)Karta.figuraKarty.K10 + i], kolor = Karta.kolorKarty.pik + k });
                        x++;
                    }
                }
                if (x == 5)
                    return 1;
                x = 0;
                najlepszyUklad.Clear();
            }
            return 0;
        }

        private int czyPoker(List<Karta> najlepszyUklad)
        {
            najlepszyUklad.Clear();
            int x = 0;

            for (int k = 0; k < 4; k++)
            {
                for (int j = 0; j < 8; j++)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (szukaj((int)Karta.figuraKarty.K9 - j + i, (int)Karta.kolorKarty.pik + k) == true)
                        {
                            najlepszyUklad.Add(new Karta { figura = figury[(int)Karta.figuraKarty.K9 - j + i], kolor = Karta.kolorKarty.pik + k });
                            x++;
                        }
                    }
                    if (x == 5)
                        return 1;
                    x = 0;
                    najlepszyUklad.Clear();
                }
            }
            return 0;
        }

        private int szukajKolorDlaFigury(int fig)
        {
            for (int i = 0; i < rekaIstol.Count; i++)
            {
                if ((int)rekaIstol[i].figura == fig)
                {
                    return (int)rekaIstol[i].kolor;
                }
            }
            return -1;
        }

        private int czyStrit(List<Karta> najlepszyUklad)//dokończyć AS jako karta niska
        {
            najlepszyUklad.Clear();
            int x = 0;
            for (int j = 0; j < 9; j++)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (szukaj((int)Karta.figuraKarty.K10 - j + i) == true)
                    {
                        najlepszyUklad.Add(new Karta { figura = figury[(int)Karta.figuraKarty.K10 - j + i], kolor = kolory[szukajKolorDlaFigury((int)Karta.figuraKarty.K10 - j + i)] });
                        x++;
                    }
                }
                if (x == 5)
                    return 1;
                x = 0;
                najlepszyUklad.Clear();
            }
            return 0;
        }

        private bool szukajKolor(int kol, List<Karta> najlepszyUklad)
        {
            int z = 0;
            najlepszyUklad.Clear();

            for (int j = figury.Length - 1; j >= 0; j--)
            {

                for (int i = 0; i < rekaIstol.Count; i++)
                {
                    if (figury[j] == rekaIstol[i].figura)
                    {
                        if ((int)rekaIstol[i].kolor == kol)
                        {
                            z++;
                            najlepszyUklad.Add(new Karta { figura = figury[j], kolor = rekaIstol[i].kolor });

                            if (z == 5)
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        private int czyKolor(List<Karta> najlepszyUklad)
        {
            for (int k = 0; k < 4; k++)
            {
                if (szukajKolor((int)Karta.kolorKarty.pik + k, najlepszyUklad) == true)
                    return 1;
                najlepszyUklad.Clear();
            }
            return 0;
        }

        private void szukajNajwyzsze(int ile, List<Karta> najlepszyUklad)
        {
            temp = new List<Karta>(rekaIstol);
            int x = 0;
            int z = -1;
            int max = -1;
            int index = -1;
            int wyjatek;
            while (x < ile)
            {
                if (najlepszyUklad.Count != 0)
                    wyjatek = (int)najlepszyUklad[0].figura;
                else wyjatek = -1;
                for (int i = 0; i < temp.Count; i++)
                {
                    if ((int)temp[i].figura != wyjatek)
                    {
                        if ((int)temp[i].figura > max)
                        {
                            max = (int)temp[i].figura;
                            z = (int)temp[i].kolor;
                            index = i;
                        }
                    }
                }
                najlepszyUklad.Add(new Karta { figura = figury[max], kolor = kolory[z] });
                temp.Remove(temp[index]);
                z = -1;
                max = -1;
                x++;
            }
        }

        private int czyKareta(List<Karta> najlepszyUklad)
        {
            int x = 0;
            najlepszyUklad.Clear();
            for (int j = 0; j < rekaIstol.Count; j++)
            {
                for (int i = 0; i < rekaIstol.Count; i++)
                {
                    if (((int)rekaIstol[j].figura == (int)rekaIstol[i].figura) && ((int)rekaIstol[j].kolor != (int)rekaIstol[i].kolor))
                    {
                        if (x == 0)
                            najlepszyUklad.Add(new Karta { figura = rekaIstol[j].figura, kolor = rekaIstol[j].kolor });
                        x++;
                        najlepszyUklad.Add(new Karta { figura = rekaIstol[i].figura, kolor = rekaIstol[i].kolor });
                    }
                }
                if (x == 3)
                {
                    szukajNajwyzsze(1, najlepszyUklad);
                    return 1;
                }
                x = 0;
                najlepszyUklad.Clear();

            }
            return 0;
        }

        private int czyTrojka(List<Karta> najlepszyUklad)
        {
            int x = 0;
            najlepszyUklad.Clear();
            for (int j = 0; j < rekaIstol.Count; j++)
            {
                for (int i = 0; i < rekaIstol.Count; i++)
                {
                    if (((int)rekaIstol[j].figura == (int)rekaIstol[i].figura) && ((int)rekaIstol[j].kolor != (int)rekaIstol[i].kolor))
                    {
                        if (x == 0)
                            najlepszyUklad.Add(new Karta { figura = rekaIstol[j].figura, kolor = rekaIstol[j].kolor });
                        x++;
                        najlepszyUklad.Add(new Karta { figura = rekaIstol[i].figura, kolor = rekaIstol[i].kolor });
                        p = (int)rekaIstol[j].figura;
                    }
                }
                if (x == 2)
                {
                    if (sf == false)
                        szukajNajwyzsze(2, najlepszyUklad);
                    return 1;
                }
                x = 0;
                najlepszyUklad.Clear();
                p = -1;

            }
            return 0;
        }

        private int czyPara(List<Karta> najlepszyUklad)
        {
            for (int j = 0; j < rekaIstol.Count; j++)
            {
                for (int i = 0; i < rekaIstol.Count; i++)
                {
                    if ((int)rekaIstol[j].figura != p)
                        if (((int)rekaIstol[j].figura == (int)rekaIstol[i].figura) && ((int)rekaIstol[j].kolor != (int)rekaIstol[i].kolor))
                        {
                            najlepszyUklad.Add(new Karta { figura = rekaIstol[j].figura, kolor = rekaIstol[j].kolor });
                            najlepszyUklad.Add(new Karta { figura = rekaIstol[i].figura, kolor = rekaIstol[i].kolor });
                            if (sf == false)
                                szukajNajwyzsze(3, najlepszyUklad);
                            return 1;
                        }
                }
            }
            return 0;
        }

        private void szukajKickera(int p1, int p2, List<Karta> najlepszyUklad)
        {
            temp = new List<Karta>(rekaIstol);
            int z = -1;
            int max = -1;
            int index = -1;

            for (int i = 0; i < temp.Count; i++)
            {
                if (((int)temp[i].figura != p1) && ((int)temp[i].figura != p2))
                {
                    if ((int)temp[i].figura > max)
                    {
                        max = (int)temp[i].figura;
                        z = (int)temp[i].kolor;
                        index = i;
                    }
                }
            }
            najlepszyUklad.Add(new Karta { figura = figury[max], kolor = kolory[z] });
            z = -1;
            max = -1;
        }
        private int czyDwiePary(List<Karta> najlepszyUklad)
        {
            int pierwsza = -1;
            int druga = -1;
            int trzecia = -1;
            int q = -1, q2 = -1, d = -1, d2 = -1, t = -1, t2 = -1;
            int n = 0;
            for (int j = 0; j < rekaIstol.Count; j++)
            {
                if (pierwsza == -1)
                {
                    for (int i = 0; i < rekaIstol.Count; i++)
                    {
                        if (((int)rekaIstol[j].figura == (int)rekaIstol[i].figura) && ((int)rekaIstol[j].kolor != (int)rekaIstol[i].kolor))
                        {
                            pierwsza = (int)rekaIstol[j].figura;
                            q = j;
                            q2 = i;
                            n++;
                            break;
                        }
                    }
                }
                else
                {
                    if (n < 3)
                    {
                        if (druga == -1)
                            if ((int)rekaIstol[j].figura != pierwsza)
                                for (int k = 0; k < rekaIstol.Count; k++)
                                {
                                    if (((int)rekaIstol[j].figura == (int)rekaIstol[k].figura) && ((int)rekaIstol[j].kolor != (int)rekaIstol[k].kolor))
                                    {
                                        d = j;
                                        d2 = k;
                                        druga = (int)rekaIstol[j].figura;
                                        n++;
                                        break;
                                        //return 1;
                                    }
                                }
                        if (trzecia == -1 && druga != -1)
                            if ((int)rekaIstol[j].figura != pierwsza && (int)rekaIstol[j].figura != druga)
                                for (int k = 0; k < rekaIstol.Count; k++)
                                {
                                    if (((int)rekaIstol[j].figura == (int)rekaIstol[k].figura) && ((int)rekaIstol[j].kolor != (int)rekaIstol[k].kolor))
                                    {
                                        t = j;
                                        t2 = k;
                                        trzecia = (int)rekaIstol[j].figura;
                                        n++;
                                        break;
                                    }
                                }
                    }
                }
            }
            if (((pierwsza > druga) && (druga > trzecia) && (pierwsza > trzecia)) || ((pierwsza < druga) && (druga > trzecia) && (pierwsza > trzecia)))
            {
                najlepszyUklad.Add(new Karta { figura = rekaIstol[q].figura, kolor = rekaIstol[q].kolor });
                najlepszyUklad.Add(new Karta { figura = rekaIstol[q2].figura, kolor = rekaIstol[q2].kolor });
                najlepszyUklad.Add(new Karta { figura = rekaIstol[d].figura, kolor = rekaIstol[d].kolor });
                najlepszyUklad.Add(new Karta { figura = rekaIstol[d2].figura, kolor = rekaIstol[d2].kolor });
                szukajKickera(pierwsza, druga, najlepszyUklad);
                return 1;
            }
            else if (((trzecia > druga) && (druga > pierwsza) && (trzecia > pierwsza)) || ((trzecia < druga) && (druga > pierwsza) && (trzecia > pierwsza)))
            {
                najlepszyUklad.Add(new Karta { figura = rekaIstol[t].figura, kolor = rekaIstol[t].kolor });
                najlepszyUklad.Add(new Karta { figura = rekaIstol[t2].figura, kolor = rekaIstol[t2].kolor });
                najlepszyUklad.Add(new Karta { figura = rekaIstol[d].figura, kolor = rekaIstol[d].kolor });
                najlepszyUklad.Add(new Karta { figura = rekaIstol[d2].figura, kolor = rekaIstol[d2].kolor });
                szukajKickera(trzecia, druga, najlepszyUklad);
                return 1;
            }
            else if (((pierwsza > trzecia) && (trzecia > druga) && (pierwsza > druga)) || ((pierwsza < trzecia) && (trzecia > druga) && (pierwsza > druga)))
            {
                najlepszyUklad.Add(new Karta { figura = rekaIstol[t].figura, kolor = rekaIstol[t].kolor });
                najlepszyUklad.Add(new Karta { figura = rekaIstol[t2].figura, kolor = rekaIstol[t2].kolor });
                najlepszyUklad.Add(new Karta { figura = rekaIstol[d].figura, kolor = rekaIstol[d].kolor });
                najlepszyUklad.Add(new Karta { figura = rekaIstol[d2].figura, kolor = rekaIstol[d2].kolor });
                szukajKickera(pierwsza, trzecia, najlepszyUklad);
                return 1;
            }
            return 0;
        }

        private int czyFull(List<Karta> najlepszyUklad)
        {
            sf = true;
            if ((czyTrojka(najlepszyUklad) == 1) && (czyPara(najlepszyUklad) == 1))
            {
                sf = false;
                return 1;
            }
            sf = false;
            return 0;
        }

        private String czyWysokaKarta(List<Karta> najlepszyUklad)
        {
            szukajNajwyzsze(5, najlepszyUklad);
            return "Wysoka karta!";
        }

    }
}