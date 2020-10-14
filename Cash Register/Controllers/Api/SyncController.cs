using System;
using System.Data;
using System.Linq;
using System.Web.Http;
using System.Data.SQLite;
using System.Configuration;
using System.Data.SqlClient;
using Cash_Register.ViewModels;
using System.Collections.Generic;

namespace Cash_Register.Controllers.Api
{
    public class SyncController : ApiController
    {

        [HttpGet]
        [Route("api/GetLocations")]
        public bool GetLocations()
        {
            try
            {
                var crLocations = GetImillLocations();
                var locations = GetLocalLocations();

                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);

                //var lastNumber = locations.OrderByDescending(x => x.Pin).FirstOrDefault()?.Pin;
                //var startCount = lastNumber ?? 1220;

                foreach (var location in crLocations)
                {
                    if (!locations.Any(x => x.LocatCd == location.LocatCd))
                    {
                        cmd.CommandText = "INSERT INTO CR_Location(Locat_Cd, NameEn, NameAr, ShortNameEn, ShortNameAr, Pin) VALUES(" +
                            "'" + location.LocatCd + "'," +
                            "'" + location.NameEn + "'," +
                            "'" + location.NameAr + "'," +
                            "'" + location.ShortNameEn + "'," +
                            "'" + location.ShortNameAr + "'," +
                            "'" + location.Pin + "'" +
                            ")";

                        cmd.ExecuteNonQuery();
                    }
                }

                cmd.Dispose();
                con.Close();
                con.Dispose();
            }
            catch (Exception ex)
            {
                return false;
                throw;
            }
            return true;
        }

        [HttpGet]
        [Route("api/GetPin/{shortName}")]
        public int GetPin(string shortName)
        {
            var locations = GetLocalLocations();
            var pin = locations.Where(x => x.ShortNameEn == shortName).FirstOrDefault()?.Pin;

            return pin ?? 0;
        }

        [HttpGet]
        [Route("api/GetPin")]
        public bool GetPin()
        {
            try
            {
                var crLocations = GetImillLocationsPin();
                var locations = GetLocalLocations();

                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);

                foreach (var location in crLocations)
                {
                    foreach (var localLocat in locations)
                    {
                        if (location.LocatCd == localLocat.LocatCd && location.Pin != localLocat.Pin)
                        {
                            cmd.CommandText = $"Update CR_Location SET Pin = {location.Pin} Where Locat_Cd = {localLocat.LocatCd}";
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                cmd.Dispose();
                con.Close();
                con.Dispose();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        [HttpGet]
        [Route("api/GetSalesmans")]
        public bool GetSalesmans()
        {
            try
            {
                var imillSalesmans = GetImillSalesman();
                var salesmans = GetLocalSalesman();

                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);

                foreach (var imillSalesman in imillSalesmans)
                {
                    if (!salesmans.Any(x => x.Sman_Cd == imillSalesman.Sman_Cd))
                    {
                        cmd.CommandText = "INSERT INTO CR_Salesman(Sman_Cd, L_Sman_Name, A_Sman_Name, Default_locat_cd) VALUES(" +
                            "'" + imillSalesman.Sman_Cd + "'," +
                            "'" + imillSalesman.NameEn + "'," +
                            "'" + imillSalesman.NameAr + "'," +
                            "'" + imillSalesman.Locat_Cd + "'" +
                            ")";

                        cmd.ExecuteNonQuery();
                    }

                    var localSalesman = salesmans.FirstOrDefault(x => x.Sman_Cd == imillSalesman.Sman_Cd);
                    if (localSalesman.Locat_Cd != imillSalesman.Locat_Cd)
                    {
                        cmd.CommandText = $"Update CR_Salesman Set Default_locat_cd = {imillSalesman.Locat_Cd} Where Sman_Cd = {imillSalesman.Sman_Cd}";
                        cmd.ExecuteNonQuery();
                    }

                }

                //foreach (var localSalesman in salesmans)
                //{
                //    var imillSalesMan = imillSalesmans.FirstOrDefault(x => x.Sman_Cd == localSalesman.Sman_Cd);
                //    if (imillSalesMan.Locat_Cd != localSalesman.Locat_Cd)
                //    {

                //    }

                //}

                cmd.Dispose();
                con.Close();
                con.Dispose();
            }
            catch (Exception ex)
            {
                return false;
                throw;
            }
            return true;
        }

        [HttpGet]
        [Route("api/GetReserveAmount")]
        public bool GetReserveAmount()
        {
            try
            {
                var imillReserveAmounts = GetImillReserveAmounts();
                var reserveAmount = GetLocalReserveAmounts();

                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);

                foreach (var rec in imillReserveAmounts)
                {
                    if (!reserveAmount.Any(x => x.Locat_Cd == rec.Locat_Cd))
                    {
                        cmd.CommandText = "INSERT INTO CR_ReserveAmount(Locat_Cd, Reserve_Amt, LocationShortcode) VALUES(" +
                            "'" + rec.Locat_Cd + "'," +
                            "'" + rec.Reserve_Amt + "'," +
                            "'" + rec.LocationShortcode + "'" +
                            ")";

                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        cmd.CommandText = $"update CR_ReserveAmount Set Reserve_Amt = {rec.Reserve_Amt} where Locat_Cd = {rec.Locat_Cd}";
                        cmd.ExecuteNonQuery();
                    }
                }

                cmd.Dispose();
                con.Close();
                con.Dispose();
            }
            catch (Exception ex)
            {
                return false;
                throw;
            }
            return true;
        }

        [HttpGet]
        [Route("api/SyncCashRegister")]
        public string SyncCashRegister()
        {
            var cashRegisters = GetLocalCashRegister();
            var problemIn = "";
            var updateQuery = "";
            try
            {
                foreach (var rec in cashRegisters)
                {
                    if (rec.IsDeleted)
                    {
                        string connectionString = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
                        SqlConnection sqlConnection = new SqlConnection(connectionString);
                        // var query = $"Delete From intlmill_cash_register Where CrOid = {rec.Oid} and Locat_Cd = {rec.LocationId}";
                        var query = $"Update intlmill_cash_register Set UpdatedOn = '{DateTime.Now:MM-dd-yyyy HH:mm:ss}', IsDeleted = 1 Where CrOid = {rec.Oid} and Locat_Cd = {rec.LocationId}";
                        SqlCommand cmd = new SqlCommand(query, sqlConnection);
                        sqlConnection.Open();
                        cmd.ExecuteNonQuery();
                        sqlConnection.Close();
                    }
                    else
                    {
                        var result = CheckExistingRecord(rec.Oid, rec.LocationId);
                        if (result)
                        {
                            problemIn = "Update";
                            updateQuery = $"UPDATE intlmill_cash_register SET " +
                                $"UpdatedOn = '{rec.TransDate:MM-dd-yyyy HH:mm:ss}'," +
                                $"shift_type = '{rec.ShiftType}'," +
                                $"shift_count = {rec.ShiftCount}," +
                                $"cheque = {rec.Cheques}," +
                                $"carriage = {rec.Talabat}," +
                                $"deliveroo = {rec.Deliveroo}," +
                                $"online = {rec.Online}," +
                                $"knet = {rec.Knet}," +
                                $"visa = {rec.Visa}," +
                                $"reserve = {rec.Reserve}," +
                                $"expense = {rec.Expense}," +
                                $"total_cash = {rec.NetBalance + rec.Reserve}," +
                                $"net_cash = {rec.NetBalance}," +
                                $"d_20000 = {rec.TwentyKd}," +
                                $"d_10000 = {rec.TenKd}," +
                                $"d_5000 = {rec.FiveKd}," +
                                $"d_1000 = {rec.OneKd}," +
                                $"d_0500 = {rec.FiftyFils}," +
                                $"d_0250 = {rec.QuarterKd}," +
                                $"d_0100 = {rec.HundFils}," +
                                $"d_0050 = {rec.FiftyFils}," +
                                $"d_0020 = {rec.TwentyFils}," +
                                $"d_0010 = {rec.TenFils}," +
                                $"d_0005 = {rec.FiveFils}," +
                                $"total_sales = {rec.Cheques + rec.Talabat + rec.Online + rec.Knet + rec.Visa + rec.Reserve + rec.Expense + rec.NetBalance}," +
                                $"net_sales = {(rec.Cheques + rec.Talabat + rec.Online + rec.Knet + rec.Visa + rec.Reserve + rec.Expense + rec.NetBalance) - rec.Reserve}," +
                                $"staff_date = '{rec.StaffDate:dd-MM-yyyy}'," +
                                $"Sman_Cd = {rec.Salesman}," +
                                $"IsDeleted = 0," +
                                $"SerialNo = {rec.SerialNo} " +
                                $"Where CrOid = {rec.Oid} and Locat_Cd = {rec.LocationId}";

                            string connectionString = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
                            SqlConnection sqlConnection = new SqlConnection(connectionString);
                            SqlCommand cmd = new SqlCommand(updateQuery, sqlConnection);
                            sqlConnection.Open();
                            cmd.ExecuteNonQuery();
                            sqlConnection.Close();
                        }
                        else
                        {
                            problemIn = "Insert " + rec.TransDate.ToString();
                            var insertQuery = "INSERT INTO intlmill_cash_register " +
                                            "(trans_date_time, location, salesman, shift_type, shift_count, cheque, carriage, deliveroo, " +
                                            "online, knet, visa, reserve, expense, total_cash, net_cash, d_20000, d_10000, d_5000, " +
                                            "d_1000, d_0500, d_0250, d_0100, d_0050, d_0020, d_0010, d_0005, total_sales, net_sales, " +
                                            "staff_date, Sman_Cd, Locat_Cd, CrOid, IsDeleted, SerialNo) " +
                                            "VALUES (@trans_date_time, @location, @salesman, @shift_type, @shift_count, @cheque, @carriage, @deliveroo, " +
                                            "@online, @knet, @visa, @reserve, @expense, @total_cash, @net_cash, @d_20000, @d_10000, @d_5000, " +
                                            "@d_1000, @d_0500, @d_0250, @d_0100, @d_0050, @d_0020, @d_0010, @d_0005, @total_sales, @net_sales, " +
                                            "@staff_date, @Sman_Cd, @Locat_Cd, @CrOid, @IsDeleted, @SerialNo) ";

                            string connectionString = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
                            SqlConnection sqlConnection = new SqlConnection(connectionString);
                            SqlCommand cmd = new SqlCommand(insertQuery, sqlConnection);
                            cmd.Parameters.Add("@trans_date_time", SqlDbType.SmallDateTime).Value = rec.TransDate;
                            cmd.Parameters.Add("@location", SqlDbType.NVarChar, 50).Value = GetLocation(rec.LocationId).NameAr;
                            cmd.Parameters.Add("@salesman", SqlDbType.NVarChar, 100).Value = GetSalesman(rec.Salesman).NameAr;
                            cmd.Parameters.Add("@shift_type", SqlDbType.VarChar, 5).Value = rec.ShiftType;
                            cmd.Parameters.Add("@shift_count", SqlDbType.Int).Value = rec.ShiftCount;
                            cmd.Parameters.Add("@cheque", SqlDbType.Money).Value = rec.Cheques;
                            cmd.Parameters.Add("@carriage", SqlDbType.Money).Value = rec.Talabat;
                            cmd.Parameters.Add("@deliveroo", SqlDbType.Money).Value = rec.Deliveroo;
                            cmd.Parameters.Add("@online", SqlDbType.Money).Value = rec.Online;
                            cmd.Parameters.Add("@knet", SqlDbType.Money).Value = rec.Knet;
                            cmd.Parameters.Add("@visa", SqlDbType.Money).Value = rec.Visa;
                            cmd.Parameters.Add("@reserve", SqlDbType.Money).Value = rec.Reserve;
                            cmd.Parameters.Add("@expense", SqlDbType.Money).Value = rec.Expense;
                            cmd.Parameters.Add("@total_cash", SqlDbType.Money).Value = rec.NetBalance + rec.Reserve;
                            cmd.Parameters.Add("@net_cash", SqlDbType.Money).Value = rec.NetBalance;
                            cmd.Parameters.Add("@d_20000", SqlDbType.Int).Value = rec.TwentyKd;
                            cmd.Parameters.Add("@d_10000", SqlDbType.Int).Value = rec.TenKd;
                            cmd.Parameters.Add("@d_5000", SqlDbType.Int).Value = rec.FiveKd;
                            cmd.Parameters.Add("@d_1000", SqlDbType.Int).Value = rec.OneKd;
                            cmd.Parameters.Add("@d_0500", SqlDbType.Int).Value = rec.HalfKd;
                            cmd.Parameters.Add("@d_0250", SqlDbType.Int).Value = rec.QuarterKd;
                            cmd.Parameters.Add("@d_0100", SqlDbType.Int).Value = rec.HundFils;
                            cmd.Parameters.Add("@d_0050", SqlDbType.Int).Value = rec.FiftyFils;
                            cmd.Parameters.Add("@d_0020", SqlDbType.Int).Value = rec.TwentyFils;
                            cmd.Parameters.Add("@d_0010", SqlDbType.Int).Value = rec.TenFils;
                            cmd.Parameters.Add("@d_0005", SqlDbType.Int).Value = rec.FiveFils;
                            cmd.Parameters.Add("@total_sales", SqlDbType.Money).Value = rec.Cheques + rec.Talabat + rec.Online + rec.Knet + rec.Visa + rec.Reserve + rec.Expense + rec.NetBalance;
                            cmd.Parameters.Add("@net_sales", SqlDbType.Money).Value = (rec.Cheques + rec.Talabat + rec.Online + rec.Knet + rec.Visa + rec.Reserve + rec.Expense + rec.NetBalance) - rec.Reserve;
                            cmd.Parameters.Add("@staff_date", SqlDbType.VarChar, 10).Value = rec.StaffDate.ToString("dd-MM-yyyy");
                            cmd.Parameters.Add("@Sman_Cd", SqlDbType.SmallInt).Value = rec.Salesman;
                            cmd.Parameters.Add("@Locat_Cd", SqlDbType.SmallInt).Value = rec.LocationId;
                            cmd.Parameters.Add("@CrOid", SqlDbType.Int).Value = rec.Oid;
                            cmd.Parameters.Add("@IsDeleted", SqlDbType.Bit).Value = false;
                            cmd.Parameters.Add("@SerialNo", SqlDbType.BigInt).Value = rec.SerialNo;

                            sqlConnection.Open();
                            cmd.ExecuteNonQuery();
                            sqlConnection.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message + "-Problem In : " + problemIn + "--Query: " + updateQuery;
                throw;
            }

            UpdateCashRegister(cashRegisters);

            return "True";
        }

        [HttpGet]
        [Route("api/GetSerialSetting")]
        public string GetSerialSetting()
        {
            var command = "";
            try
            {
                var count = CheckBackDaysCol();
                if (count < 1)
                {
                    AddBackDaysCol();
                }
                var imillSerial = GetSerial();

                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);

                //Delete from CR_Serial;
                //INSERT INTO CR_Serial VALUES('2020-09-14', 258);
                // command = $"update CR_Serial Set Date = '{imillSerial.Date:yyyy-MM-dd}', SerialNumber = {imillSerial.SerialNumber}";

                command = $"Delete from CR_Serial;INSERT INTO CR_Serial VALUES('{imillSerial.Date:yyyy-MM-dd}', {imillSerial.SerialNumber}, {imillSerial.BackDays});";
                cmd.CommandText = command;
                cmd.ExecuteNonQuery();

                cmd.Dispose();
                con.Close();
                con.Dispose();
            }
            catch (Exception ex)
            {
                return $"false : {ex.Message}";
                throw;
            }
            return $"true : {command}";
        }

        [HttpGet]
        [Route("api/AlterDb")]
        public string AlterDb()
        {
            var command = "";
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);

                command = "ALTER TABLE CashRegister ADD COLUMN Deliveroo DECIMAL(2000) DEFAULT 0 NOT NULL ON CONFLICT FAIL";

                cmd.CommandText = command;
                cmd.ExecuteNonQuery();

                cmd.Dispose();
                con.Close();
                con.Dispose();
            }
            catch (Exception ex)
            {
                return $"false : {ex.Message}";
                throw;
            }

            return $"true : {command}";
        }

        #region Local Sql Lite Functions

        private List<CRLocation> GetLocalLocations()
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
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
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<CRSalesman> GetLocalSalesman()
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

                var con = new SQLiteConnection(cs);
                con.Open();

                var cmd = new SQLiteCommand(con)
                {
                    CommandText = "select * from CR_Salesman"
                };

                var rdr = cmd.ExecuteReader();
                var crSalesmans = new List<CRSalesman>();
                while (rdr.Read())
                {
                    crSalesmans.Add(
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

                return crSalesmans;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<CRReserveAmount> GetLocalReserveAmounts()
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

                var con = new SQLiteConnection(cs);
                con.Open();

                var cmd = new SQLiteCommand(con)
                {
                    CommandText = "select * from CR_ReserveAmount"
                };

                var rdr = cmd.ExecuteReader();
                var cRLocations = new List<CRReserveAmount>();
                while (rdr.Read())
                {
                    cRLocations.Add(
                        new CRReserveAmount
                        {
                            Locat_Cd = rdr.GetInt16(0),
                            Reserve_Amt = rdr.GetDecimal(1),
                            LocationShortcode = rdr.GetString(2)
                        });
                }

                cmd.Dispose();
                con.Close();
                con.Dispose();

                return cRLocations;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<CRegister> GetLocalCashRegister()
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
                // var cs = "URI=file:F:\\Projects\\TheCode53\\Projects\\InternationalMill\\_root\\imillcr";
                var con = new SQLiteConnection(cs);
                con.Open();

                var cmd = new SQLiteCommand(con)
                {
                    CommandText = "select * from CashRegister where IsSynced IS NULL OR IsSynced = 0"
                };

                var rdr = cmd.ExecuteReader();
                var cRegister = new List<CRegister>();
                while (rdr.Read())
                {
                    cRegister.Add(
                        new CRegister
                        {
                            Oid = rdr.GetInt32(0),
                            TransDate = DateTime.Parse(rdr.GetString(1)),
                            StaffDate = DateTime.Parse(rdr.GetString(2)),
                            LocationId = rdr.GetInt16(3),
                            Salesman = rdr.GetInt16(4),
                            ShiftType = rdr.GetString(5),
                            ShiftCount = rdr.GetInt32(6),
                            Cheques = rdr.GetDecimal(7),
                            Talabat = rdr.GetDecimal(8),
                            Online = rdr.GetDecimal(9),
                            Knet = rdr.GetDecimal(10),
                            Visa = rdr.GetDecimal(11),
                            Expense = rdr.GetDecimal(12),
                            Reserve = rdr.GetDecimal(13),
                            TwentyKd = rdr.GetInt32(14),
                            TenKd = rdr.GetInt32(15),
                            FiveKd = rdr.GetInt32(16),
                            OneKd = rdr.GetInt32(17),
                            HalfKd = rdr.GetInt32(18),
                            QuarterKd = rdr.GetInt32(19),
                            HundFils = rdr.GetInt32(20),
                            FiftyFils = rdr.GetInt32(21),
                            TwentyFils = rdr.GetInt32(22),
                            TenFils = rdr.GetInt32(23),
                            FiveFils = rdr.GetInt32(24),
                            NetBalance = rdr.GetDecimal(25),
                            IsSynced = rdr.GetBoolean(26),
                            IsDeleted = rdr.GetBoolean(27),
                            SerialNo = rdr.GetInt64(28),
                            Deliveroo = rdr.GetDecimal(29)
                        });
                }

                cmd.Dispose();
                con.Close();
                con.Dispose();

                return cRegister;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private CRLocation GetLocation(short oid)
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

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
            catch (Exception ex)
            {
                throw;
            }
        }

        private CRSalesman GetSalesman(short oid)
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

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
            catch (Exception ex)
            {
                throw;
            }
        }

        private bool UpdateCashRegister(List<CRegister> cRegister)
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

                var con = new SQLiteConnection(cs);
                con.Open();
                foreach (var rec in cRegister)
                {

                    var cmd = new SQLiteCommand(con)
                    {
                        CommandText = $"UPDATE [CashRegister] SET [IsSynced] = 1 WHERE [Oid] = {rec.Oid}"
                    };

                    cmd.ExecuteReader();
                    cmd.Dispose();
                }

                con.Close();
                con.Dispose();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private int CheckBackDaysCol()
        {
            var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                CommandText = $"SELECT COUNT(*) AS CNTREC FROM pragma_table_info('CR_Serial') WHERE name='BackDays'"
            };

            var rdr = cmd.ExecuteReader();
            var count = 0;

            while (rdr.Read())
            {
                count = rdr.GetInt32(0);
            }

            cmd.Dispose();
            con.Close();
            con.Dispose();

            return count;
        }

        private void AddBackDaysCol()
        {
            var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                CommandText = $"ALTER TABLE CR_Serial ADD COLUMN BackDays INT"
            };

            cmd.ExecuteReader();
            cmd.Dispose();
            con.Close();
            con.Dispose();
        }

        #endregion

        #region IMill db Function

        private List<CRLocation> GetImillLocations()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
                var connection = new SqlConnection(connectionString);
                connection.Open();
                var sqlQuery = "select * from SM_Location";
                var sqlCommand = new SqlCommand(sqlQuery, connection);
                var dataReader = sqlCommand.ExecuteReader();
                var cRLocations = new List<CRLocation>();

                while (dataReader.Read())
                {
                    cRLocations.Add(
                   new CRLocation
                   {
                       LocatCd = short.Parse(dataReader.GetValue(0).ToString()),
                       NameEn = dataReader.GetValue(2).ToString(),
                       NameAr = dataReader.GetValue(3).ToString(),
                       ShortNameEn = dataReader.GetValue(4).ToString(),
                       ShortNameAr = dataReader.GetValue(5).ToString()
                   });
                }

                dataReader.Close();
                sqlCommand.Dispose();
                connection.Close();
                return cRLocations;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<CRSalesman> GetImillSalesman()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
                var connection = new SqlConnection(connectionString);
                connection.Open();
                var sqlQuery = "select * from SM_SALESMAN";
                var sqlCommand = new SqlCommand(sqlQuery, connection);
                var dataReader = sqlCommand.ExecuteReader();

                var crSalesmans = new List<CRSalesman>();

                while (dataReader.Read())
                {
                    crSalesmans.Add(
                   new CRSalesman
                   {
                       Sman_Cd = short.Parse(dataReader.GetValue(0).ToString()),
                       NameEn = dataReader.GetValue(3).ToString(),
                       NameAr = dataReader.GetValue(4).ToString(),
                       Locat_Cd = short.Parse(dataReader.GetValue(24).ToString())
                   });
                }

                dataReader.Close();
                sqlCommand.Dispose();
                connection.Close();

                return crSalesmans;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<CRReserveAmount> GetImillReserveAmounts()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
                var connection = new SqlConnection(connectionString);
                connection.Open();
                var sqlQuery = "select * from intlmill_cash_register_reserve";
                var sqlCommand = new SqlCommand(sqlQuery, connection);
                var dataReader = sqlCommand.ExecuteReader();

                var crReserveAmount = new List<CRReserveAmount>();

                while (dataReader.Read())
                {
                    crReserveAmount.Add(
                   new CRReserveAmount
                   {
                       Locat_Cd = short.Parse(dataReader.GetValue(0).ToString()),
                       Reserve_Amt = decimal.Parse(dataReader.GetValue(1).ToString()),
                       LocationShortcode = dataReader.GetValue(2).ToString()
                   });
                }

                dataReader.Close();
                sqlCommand.Dispose();
                connection.Close();

                return crReserveAmount;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<CRLocation> GetImillLocationsPin()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            connection.Open();
            var sqlQuery = "select * from SM_USERS";
            var sqlCommand = new SqlCommand(sqlQuery, connection);
            var dataReader = sqlCommand.ExecuteReader();

            var crLocations = new List<CRLocation>();

            while (dataReader.Read())
            {
                crLocations.Add(
               new CRLocation
               {
                   LocatCd = short.Parse(dataReader.GetValue(46).ToString()),
                   Pin = int.Parse(dataReader.GetValue(45).ToString())
               });
            }

            dataReader.Close();
            sqlCommand.Dispose();
            connection.Close();

            return crLocations;
        }

        private bool CheckExistingRecord(int oid, short locationId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            connection.Open();
            var sqlQuery = $"select * from intlmill_cash_register Where CrOid = {oid} and Locat_Cd = {locationId}";
            var sqlCommand = new SqlCommand(sqlQuery, connection);
            // sqlCommand.ExecuteReader();

            var dataReader = sqlCommand.ExecuteReader();
            var cRegisters = new List<CRegister>();

            while (dataReader.Read())
            {
                cRegisters.Add(
                new CRegister
                {
                    Oid = short.Parse(dataReader.GetValue(0).ToString())
                });
            }

            dataReader.Close();
            sqlCommand.Dispose();
            connection.Close();

            return cRegisters.Count() > 0;
        }

        private CRSerial GetSerial()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["reportconnection"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            connection.Open();

            var sqlQuery = "select * from Settings";
            var sqlCommand = new SqlCommand(sqlQuery, connection);
            var dataReader = sqlCommand.ExecuteReader();

            var crSerial = new CRSerial();

            while (dataReader.Read())
            {
                crSerial.Date = DateTime.Parse(dataReader.GetValue(0).ToString());
                crSerial.SerialNumber = long.Parse(dataReader.GetValue(1).ToString());
                crSerial.BackDays = int.Parse(dataReader.GetValue(2).ToString());
            }

            dataReader.Close();
            sqlCommand.Dispose();
            connection.Close();

            return crSerial;
        }

        #endregion

    }
}
