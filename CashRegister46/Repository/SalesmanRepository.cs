using System.Data.SQLite;
using System.Configuration;
using Cash_Register.Contracts;
using Cash_Register.ViewModels;
using System.Collections.Generic;

namespace Cash_Register.Repository
{
    public class SalesmanRepository : ISalesmanRepository
    {
        readonly string cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
        public CRSalesman GetSalesman(short oid)
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                CommandText = $"select * from CR_Salesman where Sman_Cd = {oid} limit 1"
            };

            var rdr = cmd.ExecuteReader();
            var crSalesman = new CRSalesman();
            
            while (rdr.Read())
            {
                crSalesman.Sman_Cd = rdr.GetInt16(0);
                crSalesman.NameEn = rdr.GetString(1);
                crSalesman.NameAr = rdr.GetString(2);
                crSalesman.Locat_Cd = rdr.GetInt16(3);
            }

            cmd.Dispose();
            con.Close();
            con.Dispose();

            return crSalesman;
        }

        public List<CRSalesman> GetSalesmans(short locationId)
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                CommandText = $"select * from CR_Salesman where Default_locat_cd = {locationId}"
            };

            var rdr = cmd.ExecuteReader();
            var crSalesman = new List<CRSalesman>();
            while (rdr.Read())
            {
                crSalesman.Add(
                    new CRSalesman
                    {
                        Sman_Cd = rdr.GetInt16(0),
                        NameEn = rdr.GetString(1),
                        NameAr = rdr.GetString(2),
                        Locat_Cd = rdr.GetInt16(3)
                    });
            }

            cmd.Dispose();
            con.Close();
            con.Dispose();

            return crSalesman;
        }
    }
}