using System.Data.SQLite;
using System.Configuration;
using Cash_Register.Contracts;
using Cash_Register.ViewModels;
using System.Collections.Generic;

namespace Cash_Register.Repository
{
    public class LocationRepository : ILocationRepository
    {
        readonly string cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

        public CRLocation GetLocation(short oid)
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                CommandText = $"select * from CR_Location where [Locat_Cd] = {oid} limit 1"
            };

            var rdr = cmd.ExecuteReader();
            var crLocation = new CRLocation();
            while (rdr.Read())
            {
                crLocation.LocatCd = rdr.GetInt16(0);
                crLocation.NameEn = rdr.GetString(1);
                crLocation.NameAr = rdr.GetString(2);
                crLocation.ShortNameEn = rdr.GetString(3);
                crLocation.ShortNameAr = rdr.GetString(4);
                crLocation.Pin = rdr.GetInt32(5);
            }

            cmd.Dispose();
            con.Close();
            con.Dispose();

            return crLocation;
        }

        public List<CRLocation> GetLocations()
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                CommandText = "select * from CR_Location"
            };

            var rdr = cmd.ExecuteReader();
            var cRLocations = new List<CRLocation>();
            while (rdr.Read())
            {
                cRLocations.Add(
                    new CRLocation
                    {
                        LocatCd = rdr.GetInt16(0),
                        NameEn = rdr.GetString(1),
                        NameAr = rdr.GetString(2),
                        ShortNameEn = rdr.GetString(3),
                        ShortNameAr = rdr.GetString(4),
                        Pin = rdr.GetInt32(5)
                    });
            }

            cmd.Dispose();
            con.Close();
            con.Dispose();

            return cRLocations;
        }

        public decimal GetReserveAmount(short locationId)
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                CommandText = $"select * from CR_ReserveAmount where locat_cd = {locationId}"
            };

            var rdr = cmd.ExecuteReader();
            decimal reserveAmount = 0;
            while (rdr.Read())
            {
                reserveAmount = rdr.GetDecimal(1);
            }

            cmd.Dispose();
            con.Close();
            con.Dispose();

            return reserveAmount;
        }
    }
}