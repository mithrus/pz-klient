using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Security.Cryptography;



namespace MainServer
{
    static public class Baza
    {
        //początkowa konfiguracja użytkownika
        static private double KasaStandard = 1500.0;
        static private double CzasAFK = 15;// liczba minut po których nastąpi wylogowanie!

        static private string CiagPolaczenia = "Data Source=(local);"
               + "Initial Catalog=PokerDB;"
               + "Persist Security Info=False;"
               + "User ID=PokerDBUser;Password=zaq478mlp";

        static public bool DodajZwyciezce(string nazwa, Int64 numerStolu, int wygrana)
        {
            using (SqlConnection Polaczenie = new SqlConnection(CiagPolaczenia))
            {
                var sqlQuery = "select * from Gra";
                var sqlQuery2 = "select IdUzytkownika from Uzytkownik where Nazwa = @Nazwa";// +wiad.nazwaUzytkownika;
                SqlDataAdapter dataAdapter2 = new SqlDataAdapter(sqlQuery2, Polaczenie);
                DataSet dataSet2 = new DataSet();
                DataSet dataSet = new DataSet();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, Polaczenie);

                dataAdapter2.SelectCommand.Parameters.Add("@Nazwa", SqlDbType.NVarChar).Value = nazwa;
                dataAdapter2.Fill(dataSet2, "Uzytkownik");
                int uzytID = (int)dataSet2.Tables["Uzytkownik"].Rows[0].ItemArray.GetValue(0);

                dataAdapter.Fill(dataSet, "Gra");
                DataRow newRow = dataSet.Tables["Gra"].NewRow();
                newRow["IdZwyciezcy"] = uzytID;
                newRow["IdPokoju"] = numerStolu;
                newRow["Wygrana"] = wygrana;

                dataSet.Tables["Gra"].Rows.Add(newRow);

                new SqlCommandBuilder(dataAdapter);
                dataAdapter.Update(dataSet.Tables["Gra"]);

                return true;
            }
        }
        static public bool CzyIstniejeUzytkownik(string nazwa)
        {
            bool istnieje = false;
            using (SqlConnection connection = new SqlConnection(CiagPolaczenia))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("select count(*) from Uzytkownik where Nazwa = @Nazwa", connection);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@Nazwa";
                param.Value = nazwa;

                cmd.Parameters.Add(param);

                int count = (int)cmd.ExecuteScalar();
                if (count > 0)
                {
                    istnieje = true;
                }
                else
                {
                    istnieje = false;
                }
                connection.Close();

            }
            return istnieje;
        }
        static public List<Uzytkownik> ZwrocUzytkownikowZalogowanych() //trzeba to przemyśleć, żeby z automatu nie można było tworzyć userów
        {
            List<Uzytkownik> listaUzytkownikow = new List<Uzytkownik>();
            using (SqlConnection Polaczenie = new SqlConnection(CiagPolaczenia))
            {
                var sqlQuery = "select * from Uzytkownik u join Sesja s on s.IdUzytkownika=u.IdUzytkownika where s.DoKiedyWazny > @Wazny";

                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, Polaczenie);
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand.Parameters.Add("@Wazny", SqlDbType.Int).Value = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                dataAdapter.Fill(dataSet, "Uzytkownik");
                if (dataSet.Tables["Uzytkownik"].Rows.Count > 0)
                {
                    foreach (DataRow wiersz in dataSet.Tables["Uzytkownik"].Rows)
                    {
                        listaUzytkownikow.Add(new Uzytkownik{nazwaUzytkownika=wiersz["Nazwa"].ToString(),identyfikatorUzytkownika=(int)wiersz["IdUzytkownika"]});
                    }
                }
            }
            return listaUzytkownikow;
        }

        static public bool CzyIstniejEmail(string email)
        {
            bool istnieje = false;
            using (SqlConnection connection = new SqlConnection(CiagPolaczenia))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("select count(*) from Uzytkownik where Email = @Email", connection);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@Email";
                param.Value = email.ToLower();

                cmd.Parameters.Add(param);

                int count = (int)cmd.ExecuteScalar();
                if (count > 0)
                {
                    istnieje = true;
                }
                else
                {
                    istnieje = false;
                }
                connection.Close();
            }
            return istnieje;
        }

        static public bool CzyPoprawny(byte[] token)
        {
            bool zalogowany = false;

            using (SqlConnection connection = new SqlConnection(CiagPolaczenia))
            {
                connection.Open();
                var sqlQuery = "select DoKiedyWazny from Sesja where Token = @Token";

                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, connection);
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand.Parameters.Add("@Token", SqlDbType.VarBinary).Value = token;
                dataAdapter.Fill(dataSet,"Sesja");
                if (dataSet.Tables["Sesja"].Rows.Count > 0)
                {
                    int ok = (Int32)dataSet.Tables["Sesja"].Rows[0].ItemArray.GetValue(0);
                    if (ok > (Int32)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds)
                    {
                        zalogowany = true;
                    }
                    else
                    {
                        zalogowany = false;
                    }
                }
            }
            return zalogowany;
        }
        static public byte[] CzyZalogowany(string nazwa)
        {
            byte[] token = null;
            using (SqlConnection connection = new SqlConnection(CiagPolaczenia))
            {
                
                connection.Open();
                var sqlQuery = "select s.DoKiedyWazny,s.Token from Sesja s join Uzytkownik u on s.IdUzytkownika=u.IdUzytkownika where u.Nazwa like @Nazwa order by s.DoKiedyWazny desc";

                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, connection);

                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand.Parameters.Add("@Nazwa", SqlDbType.NVarChar).Value = nazwa;
                dataAdapter.Fill(dataSet, "Sesja");
                if (dataSet.Tables["Sesja"].Rows.Count > 0)
                {
                    int ok = (int)dataSet.Tables["Sesja"].Rows[0].ItemArray.GetValue(0);
                    //int nic = (int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    if (ok > (int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds)
                    {
                        token = (byte[])dataSet.Tables["Sesja"].Rows[0]["Token"];//.ItemArray.GetValue(1);
                    }
                } 
            }
            return token;
        }
        static public bool CzyIstniejPokoj(string nazwa)
        {
            bool istnieje = false;
            using (SqlConnection connection = new SqlConnection(CiagPolaczenia))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("select count(*) from Pokoj where NazwaPokoju = @Nazwa", connection);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@Nazwa";
                param.Value = nazwa;

                cmd.Parameters.Add(param);

                int count = (int)cmd.ExecuteScalar();
                if (count > 0)
                {
                    istnieje = true;
                }
                else
                {
                    istnieje = false;
                }
                connection.Close();
            }
            return istnieje;
        }

        static public bool CzyIstniejPokoj(int id)
        {
            bool istnieje = false;
            using (SqlConnection connection = new SqlConnection(CiagPolaczenia))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("select count(*) from Pokoj where IdPokoju = @Id", connection);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@Id";
                param.Value = id;

                cmd.Parameters.Add(param);

                int count = (int)cmd.ExecuteScalar();
                if (count > 0)
                {
                    istnieje = true;
                }
                else
                {
                    istnieje = false;
                }
                connection.Close();
            }
            return istnieje;
        }

        static public long DodajPokoj(string nazwa, int stawkaWejsciowa, int bigBlind, int iloscGraczy)
        {
            
            if (CzyIstniejPokoj(nazwa))
            {
                return -1; // jeśli taki pokój istnieje!!!!
            }
            //int id = 0;
            using (SqlConnection Polaczenie = new SqlConnection(CiagPolaczenia))
            {
                var sqlQuery = "select * from Pokoj";
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, Polaczenie);
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet, "Pokoj");

                DataRow newRow = dataSet.Tables["Pokoj"].NewRow();
                newRow["NazwaPokoju"] = nazwa;
                newRow["StawkaWejsciowa"] = stawkaWejsciowa;
                newRow["BigBlind"] = bigBlind;
                newRow["IloscGraczy"] = iloscGraczy;

                dataSet.Tables["Pokoj"].Rows.Add(newRow);

                new SqlCommandBuilder(dataAdapter);
                dataAdapter.Update(dataSet.Tables["Pokoj"]);
            }
            return ZwrocPokoj(nazwa).numerPokoju;
        }
        static public Pokoj ZwrocPokoj(string nazwa)
        {
            Pokoj pokoj = null;
            using (SqlConnection connection = new SqlConnection(CiagPolaczenia))
            {
                connection.Open();
                var sqlQuery = "select * from Pokoj where NazwaPokoju = @Nazwa";

                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, connection);
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand.Parameters.Add("@Nazwa", SqlDbType.VarChar).Value = nazwa;
                dataAdapter.Fill(dataSet, "Pokoj");
                if (dataSet.Tables["Pokoj"].Rows.Count > 0)
                {
                    DataRow wiersz = dataSet.Tables["Pokoj"].Rows[0];
                    pokoj = new Pokoj { numerPokoju = (int)wiersz["IdPokoju"], duzyBlind = (int)wiersz["BigBlind"], stawkaWejsciowa = (int)wiersz["StawkaWejsciowa"], iloscGraczyMax = (int)wiersz["IloscGraczy"] };
                }
            }
            if (pokoj != null)
            {
                List<Uzytkownik> uzytkownicy = ZwrocUzytkownikowZalogowanych();
                foreach (Uzytkownik u in uzytkownicy)
                {
                    if (u.numerPokoju == pokoj.numerPokoju)
                    {
                        pokoj.user.Add(u);
                    }
                }
            }
            return pokoj;
        }
        static public Pokoj ZwrocPokoj(int id)
        {
            Pokoj pokoj = null;
            using (SqlConnection connection = new SqlConnection(CiagPolaczenia))
            {
                connection.Open();
                var sqlQuery = "select * from Pokoj where IdPokoju = @Id";

                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, connection);
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                dataAdapter.Fill(dataSet, "Pokoj");
                if (dataSet.Tables["Pokoj"].Rows.Count > 0)
                {
                    DataRow wiersz = dataSet.Tables["Pokoj"].Rows[0];
                    pokoj = new Pokoj { numerPokoju = (int)wiersz["IdPokoju"], duzyBlind = (int)wiersz["BigBlind"], stawkaWejsciowa = (int)wiersz["StawkaWejsciowa"], iloscGraczyMax = (int)wiersz["IloscGraczy"] };
                }
            }
            if (pokoj != null)
            {
                List<Uzytkownik> uzytkownicy = ZwrocUzytkownikowZalogowanych();
                foreach (Uzytkownik u in uzytkownicy)
                {
                    if (u.numerPokoju == pokoj.numerPokoju)
                    {
                        pokoj.user.Add(u);
                    }
                }
            }
            return pokoj;
        }

        static public void ZmienPokoj(byte[] token, long nowyPokoj)
        {
            using (SqlConnection connection = new SqlConnection(CiagPolaczenia))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Uzytkownik SET IdPokoju = @IdPokoju where IdUzytkownika = (Select IdUzytkownika from Sesja where Token=@Token)", connection);

                SqlParameter id = new SqlParameter();
                SqlParameter tok = new SqlParameter();
                id.ParameterName = "@IdPokoju";
                tok.ParameterName = "@Token";
                id.Value = nowyPokoj;
                tok.Value = token;

                cmd.Parameters.Add(id);
                cmd.Parameters.Add(tok);
                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        static public List<Pokoj> ZwrocPokoje() //trzeba to przemyśleć, żeby z automatu nie można było tworzyć userów
        {
            List<Pokoj> listaPokoi = new List<Pokoj>();
            using (SqlConnection Polaczenie = new SqlConnection(CiagPolaczenia))
            {
                var sqlQuery = "select * from Pokoj";

                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, Polaczenie);
                DataSet dataSet = new DataSet();
                //dataAdapter.SelectCommand.Parameters.Add("@Wazny", SqlDbType.Int).Value = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                dataAdapter.Fill(dataSet, "Pokoj");
                if (dataSet.Tables["Pokoj"].Rows.Count > 0)
                {
                    foreach (DataRow wiersz in dataSet.Tables["Pokoj"].Rows)
                    {
                        listaPokoi.Add(new Pokoj { numerPokoju=(int)wiersz["IdPokoju"],duzyBlind=(int)wiersz["BigBlind"],stawkaWejsciowa=(int)wiersz["StawkaWejsciowa"],iloscGraczyMax=(int)wiersz["IloscGraczy"]});
                    }
                }
            }
            List<Uzytkownik> uzytkownicy = ZwrocUzytkownikowZalogowanych();
            foreach (Pokoj pokoj in listaPokoi)
            {
                foreach (Uzytkownik u in uzytkownicy)
                {
                    if (u.numerPokoju == pokoj.numerPokoju)
                    {
                        pokoj.user.Add(u);
                    }
                }
            }
            return listaPokoi;
        }
        static public int ZwrocIdUzytkownika(byte[] token)
        {
            int id = 0;
            using (SqlConnection connection = new SqlConnection(CiagPolaczenia))
            {
                connection.Open();
                var sqlQuery = "select IdUzytkownika from Sesja where Token = @Token";

                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, connection);
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand.Parameters.Add("@Token", SqlDbType.VarBinary).Value = token;
                dataAdapter.Fill(dataSet, "Sesja");
                if (dataSet.Tables["Sesja"].Rows.Count > 0)
                {
                    id = (int)dataSet.Tables["Sesja"].Rows[0]["IdUzytkownika"];
                }
                return id;
            }
        }
        static public bool DodajUzytkownika(string nazwa, string email, string haslo)
        {
            if (!CzyPoprawnyEmail(email))
            {
                return false;
            }
            using (SqlConnection Polaczenie = new SqlConnection(CiagPolaczenia))
            {
                var sqlQuery = "select * from Uzytkownik";
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, Polaczenie);
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet, "Uzytkownik");
         
                DataRow newRow = dataSet.Tables["Uzytkownik"].NewRow();
                newRow["Nazwa"] = nazwa;
                newRow["Email"] = email.ToLower();
                using (var deriveBytes = new Rfc2898DeriveBytes(haslo, 32)) //PBKDF2 Password-Based Key Derivation Function 2
                {
                    byte[] salt = deriveBytes.Salt;
                    byte[] key = deriveBytes.GetBytes(32);  // derive a 32-byte key(256bit)

                    newRow["Haslo"] = key;
                    newRow["Salt"] = salt;
                }
                newRow["Kasa"] = KasaStandard;

                dataSet.Tables["Uzytkownik"].Rows.Add(newRow);

                new SqlCommandBuilder(dataAdapter);
                dataAdapter.Update(dataSet.Tables["Uzytkownik"]);

                return true;
            }
        }
        static public bool CzyPoprawneHaslo(string haslo, string nazwa)
        {
            byte[] salt, key;

            using (SqlConnection Polaczenie = new SqlConnection(CiagPolaczenia))
            {
                var sqlQuery = "select Haslo, Salt from Uzytkownik where Nazwa = @Nazwa";
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, Polaczenie);
                DataSet dataSet = new DataSet();

                dataAdapter.SelectCommand.Parameters.Add("@Nazwa", SqlDbType.NVarChar).Value = nazwa;
                dataAdapter.Fill(dataSet, "Uzytkownik");
                key = (byte[])dataSet.Tables["Uzytkownik"].Rows[0]["Haslo"];
                salt = (byte[])dataSet.Tables["Uzytkownik"].Rows[0]["Salt"];
            }

            using (var deriveBytes = new Rfc2898DeriveBytes(haslo, salt))
            {
                byte[] newKey = deriveBytes.GetBytes(32);

                if (!newKey.SequenceEqual(key))
                    return false;
                else
                    return true;
            }
        }
        static public byte[] Zaloguj(string nazwa)
        {
            Komunikat kom = new Komunikat();
            using (SqlConnection Polaczenie = new SqlConnection(CiagPolaczenia))
            {
                var sqlQuery = "select * from Sesja";
                var sqlQuery2 = "select IdUzytkownika from Uzytkownik where Nazwa = @Nazwa";// +wiad.nazwaUzytkownika;
                SqlDataAdapter dataAdapter2 = new SqlDataAdapter(sqlQuery2, Polaczenie);
                DataSet dataSet2 = new DataSet();

                dataAdapter2.SelectCommand.Parameters.Add("@Nazwa", SqlDbType.NVarChar).Value = nazwa;
                dataAdapter2.Fill(dataSet2, "Uzytkownik");
                int uzytID = (int)dataSet2.Tables["Uzytkownik"].Rows[0].ItemArray.GetValue(0);

                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, Polaczenie);
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet, "Sesja");

                //string token = Key.Generuj();
                byte[] token = new byte[256] ;
                Random rand = new Random();
                rand.NextBytes(token);

                DataRow newRow = dataSet.Tables["Sesja"].NewRow();
                newRow["IdUzytkownika"] = uzytID;
                newRow["Token"] = token;
                newRow["DoKiedyWazny"] = (Int32)(DateTime.Now.AddMinutes(CzasAFK).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
               
                dataSet.Tables["Sesja"].Rows.Add(newRow);

                new SqlCommandBuilder(dataAdapter);
                dataAdapter.Update(dataSet.Tables["Sesja"]);

                return token;
            }
        }

        static public Komunikat Wyloguj(byte[] token)
        {
            Komunikat kom = new Komunikat();
            using (SqlConnection Polaczenie = new SqlConnection(CiagPolaczenia))
            {
                var sqlQuery = "select * from Sesja where Token = @Token and DoKiedyWazny >= @Wazny";
              
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlQuery, Polaczenie);
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand.Parameters.Add("@Token", SqlDbType.VarBinary).Value = token;
                dataAdapter.SelectCommand.Parameters.Add("@Wazny", SqlDbType.Int).Value = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                dataAdapter.Fill(dataSet, "Sesja");
                if (dataSet.Tables["Sesja"].Rows.Count > 0)
                {

                    foreach (DataRow row in dataSet.Tables["Sesja"].Rows)
                    {
                        row["DoKiedyWazny"] = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    }

                    new SqlCommandBuilder(dataAdapter);
                    dataAdapter.Update(dataSet.Tables["Sesja"]);
                }
                    kom.trescKomunikatu = "OK";
                    kom.kodKomunikatu = 100;

            }
            return kom;
        }

        static public bool CzyPoprawnyEmail(string email)
        {
            if (!Regex.IsMatch(email, @"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[A-Za-z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+(?:[a-zA-Z]{2}||com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b"))
                return false;
            else
                return true;
        }
        static public bool CzyPoprawnyToken(string token)
        {
            return Regex.IsMatch(token, @"[A-Za-z0-9;:=?@\[\\\]^_`]{256}");
        }
    }
}
