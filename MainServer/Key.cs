using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MainServer
{
    static public class Key
    {
        //public Int64 identyfikatorUzytkownika;
        //public string token;

        static public string Generuj()
        {
            string token = "";
            Random rand = new Random();
            byte[] temp = new byte[32];
            rand.NextBytes(temp);
            //this.token = "";
            for (int i = 0; i < 32; i++)
            {// ascii od 48 do 122 /{60,62} //Marcin mial racje ;p
                int num = temp[i] % 74 + 48;//bylo95
                if (num == 60 || num == 62)
                    num++;
                token += (char)num;
            }
            return token;
        }

        //public Key(Int64 idUzytkownika)
        //{
        //    this.identyfikatorUzytkownika = idUzytkownika;
        //}
    }
}
