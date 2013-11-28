using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MainServer
{
    public class baza_user
    {
        public Int64 idUzytkownika;
        public string nazwa;
        public string haslo;
        public string email;

        public baza_user(Int64 id, string n, string h, string e)
        {
            this.idUzytkownika = id;
            this.nazwa = n;
            this.haslo = h;
            this.email = e;
        }
    }
    public class StatycznaBaza
    {
        public List<baza_user> zarejestrowani = new List<baza_user>();
      
        public void AddUser(Int64 id, string name, string pass, string mail)
        {
            baza_user temp = new baza_user(id, name, pass, mail);
            this.zarejestrowani.Add(temp);
        }

        public StatycznaBaza()
        {
            AddUser(1, "user1", "user1", "user1@mail.com");
        }

    }
}