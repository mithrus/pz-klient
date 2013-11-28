using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MainServer
{
    public class sortujKick : IComparer<Gracz>
    {
        public int Compare(Gracz a, Gracz b)
        {
            if (a.kicker < b.kicker) return 1;
            else if (a.kicker > b.kicker) return -1;
            else return 0;
        }
    }
}