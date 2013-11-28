using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MainServer
{
    public class Karta
    {
        public enum figuraKarty:int { K2, K3, K4, K5, K6, K7, K8, K9, K10, KJ, KD, KK, KA };
        public enum kolorKarty :int { pik, kier, karo, trefl };
        public figuraKarty figura;
        public kolorKarty kolor;

    }
}