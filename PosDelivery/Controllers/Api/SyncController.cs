using System;
using System.Linq;
using System.Web.Http;
using System.Data.SQLite;
using System.Configuration;
using System.Data.SqlClient;
using PosDelivery.ViewModels;
using System.Collections.Generic;

namespace PosDelivery.Controllers.Api
{
    public class SyncController : ApiController
    {
        public string Datetime = "";
        [HttpGet]
        [Route("api/SyncData")]
        public string SyncData(int locatcd)
        {
            var message = "";
            try
            {
                var imillPosDel = GetImillPosDel(locatcd);
                message = "GetImillPosDel";

                var localPosDel = GetLocalPosDel();
                message = "GetLocalPosDel";

                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);

                foreach (var posDel in imillPosDel)
                {
                    if (posDel.AllowSync)
                    {
                        if (!localPosDel.Any(x => x.LocatCd == posDel.LocatCd && x.OrderId == posDel.OrderId))
                        {
                            cmd.CommandText = $"Insert Into Delivery(OrderId, DateTime, LocatCd, IsPrinted) " +
                                $"VALUES('{posDel.OrderId}','{posDel.DateTime:yyyy-MM-dd HH:mm:ss}', {posDel.LocatCd}, {posDel.IsPrinted})";
                            message = cmd.CommandText;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                cmd.Dispose();
                con.Close();
                con.Dispose();

                return "true";
            }
            catch (Exception ex)
            {
                return ex.Message + " : " + message + " : " + Datetime;
            }
        }


        private List<PosDeliveryViewModel> GetImillPosDel(int locatcd)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PosDelivery"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            connection.Open();
            var sqlQuery = $"select * from PosOrder where Locat_Cd = {locatcd}";
            var sqlCommand = new SqlCommand(sqlQuery, connection);
            var dataReader = sqlCommand.ExecuteReader();

            var crLocations = new List<PosDeliveryViewModel>();

            while (dataReader.Read())
            {
                crLocations.Add(
               new PosDeliveryViewModel
               {
                   DateTime = DateTime.Parse(dataReader.GetValue(1).ToString()),
                   IsPrinted = bool.Parse(dataReader.GetValue(14).ToString()),
                   OrderId = dataReader.GetValue(2).ToString(),
                   LocatCd = short.Parse(dataReader.GetValue(13).ToString()),
                   AllowSync = bool.Parse(dataReader.GetValue(15).ToString())
               });
            }

            dataReader.Close();
            sqlCommand.Dispose();
            connection.Close();

            return crLocations;
        }

        private List<PosDeliveryViewModel> GetLocalPosDel()
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
                var con = new SQLiteConnection(cs);
                con.Open();

                var cmd = new SQLiteCommand(con)
                {
                    CommandText = "select * from Delivery"
                };

                var rdr = cmd.ExecuteReader();
                var cRLocations = new List<PosDeliveryViewModel>();
                while (rdr.Read())
                {
                    cRLocations.Add(
                        new PosDeliveryViewModel
                        {
                            DateTime = rdr.GetDateTime(0),
                            OrderId = rdr.GetString(1),
                            IsPrinted = rdr.GetBoolean(2),
                            LocatCd = rdr.GetInt16(3)
                        });
                }

                cmd.Dispose();
                con.Close();
                con.Dispose();

                return cRLocations;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}