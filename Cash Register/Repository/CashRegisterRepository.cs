using System;
using System.Linq;
using System.Data.SQLite;
using System.Configuration;
using Cash_Register.Contracts;
using Cash_Register.ViewModels;
using System.Collections.Generic;

namespace Cash_Register.Repository
{
    public class CashRegisterRepository : ICashRegisterRepository
    {
        readonly string cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

        public bool DeleteCashRegRecord(int oid)
        {
            try
            {
                var con = new SQLiteConnection(cs);
                con.Open();

                var cmd = new SQLiteCommand(con)
                {
                    CommandText = $"Update [CashRegister] SET [IsDeleted] = 1, [IsSynced] = 0 where Oid = {oid};"
                    // TransDate = {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                };

                cmd.ExecuteReader();
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

        public CRegister GetCashRegister(int oid)
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                CommandText = $"select * from CashRegister where (IsDeleted IS NULL OR IsDeleted = 0) AND Oid = {oid} limit 1"
            };

            var rdr = cmd.ExecuteReader();
            var cRegister = new CRegister();

            while (rdr.Read())
            {
                cRegister.Oid = rdr.GetInt32(0);
                cRegister.TransDate = DateTime.Parse(rdr.GetString(1));
                cRegister.StaffDate = DateTime.Parse(rdr.GetString(2));
                cRegister.LocationId = rdr.GetInt16(3);
                cRegister.Salesman = rdr.GetInt16(4);
                cRegister.ShiftType = rdr.GetString(5);
                cRegister.ShiftCount = rdr.GetInt32(6);
                cRegister.Cheques = rdr.GetDecimal(7);
                cRegister.Talabat = rdr.GetDecimal(8);
                cRegister.Online = rdr.GetDecimal(9);
                cRegister.Knet = rdr.GetDecimal(10);
                cRegister.Visa = rdr.GetDecimal(11);
                cRegister.Expense = rdr.GetDecimal(12);
                cRegister.Reserve = rdr.GetDecimal(13);
                cRegister.TwentyKd = rdr.GetInt32(14);
                cRegister.TenKd = rdr.GetInt32(15);
                cRegister.FiveKd = rdr.GetInt32(16);
                cRegister.OneKd = rdr.GetInt32(17);
                cRegister.HalfKd = rdr.GetInt32(18);
                cRegister.QuarterKd = rdr.GetInt32(19);
                cRegister.HundFils = rdr.GetInt32(20);
                cRegister.FiftyFils = rdr.GetInt32(21);
                cRegister.TwentyFils = rdr.GetInt32(22);
                cRegister.TenFils = rdr.GetInt32(23);
                cRegister.FiveFils = rdr.GetInt32(24);
                cRegister.NetBalance = rdr.GetDecimal(25);
                cRegister.SerialNo = rdr.GetInt64(28);
                cRegister.Deliveroo = rdr.GetDecimal(29);
            }

            cmd.Dispose();
            con.Close();
            con.Dispose();

            return cRegister;
        }

        public int GetOid()
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                CommandText = $"select * from CashRegister"
            };

            var rdr = cmd.ExecuteReader();
            var cRegisters = new List<CRegister>();

            while (rdr.Read())
            {
                var cRegister = new CRegister()
                {
                    Oid = rdr.GetInt32(0),
                };

                cRegisters.Add(cRegister);
            }

            cmd.Dispose();
            con.Close();
            con.Dispose();

            if (cRegisters.Any())
                return cRegisters.OrderByDescending(x => x.Oid).FirstOrDefault().Oid + 1;

            return 1;
        }

        public List<CRegister> GetSalesmanCashRegister(short oid)
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                // CommandText = $"Select * from CashRegister where (IsDeleted IS NULL OR IsDeleted = 0) AND Salesman = {oid} AND StaffDate >= date('now','-1 day','localtime')"
                CommandText = $"Select * from CashRegister where (IsDeleted IS NULL OR IsDeleted = 0) AND Salesman = {oid}"
            };

            var rdr = cmd.ExecuteReader();
            var cRegisters = new List<CRegister>();

            while (rdr.Read())
            {
                cRegisters.Add(new CRegister
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
                    NetBalance = rdr.GetDecimal(25)
                });
            }

            cmd.Dispose();
            con.Close();
            con.Dispose();

            var backDate = GetBackDays();
            var creg = cRegisters.Where(x => x.StaffDate >= DateTime.Now.AddDays(-backDate).Date).ToList();
            return creg;
        }

        public int GetShiftCount(ShiftCount shiftCount)
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                CommandText = $"select * from CashRegister where (IsDeleted IS NULL OR IsDeleted = 0) AND LocatCd = {shiftCount.LocationId} and Salesman = {shiftCount.Salesman} and ShiftType = '{shiftCount.ShiftType}' and (IsSynced IS NULL OR IsSynced = 0)"
            };

            var rdr = cmd.ExecuteReader();
            var cRegisters = new List<CRegister>();

            while (rdr.Read())
            {
                var cRegister = new CRegister()
                {
                    Oid = rdr.GetInt32(0),
                    StaffDate = DateTime.Parse(rdr.GetString(2)),
                    LocationId = rdr.GetInt16(3),
                    Salesman = rdr.GetInt16(4),
                    ShiftType = rdr.GetString(5),
                    ShiftCount = rdr.GetInt32(6)
                };

                cRegisters.Add(cRegister);
            }

            cmd.Dispose();
            con.Close();
            con.Dispose();

            if (cRegisters.Any())
            {
                var reg = cRegisters.Where(x => x.StaffDate >= shiftCount.StaffDate && x.StaffDate <= shiftCount.StaffDate);
                return reg.Any() ? reg.OrderByDescending(x => x.ShiftCount).FirstOrDefault().ShiftCount + 1 : 1;
            }
            else
                return 1;
        }

        public int SaveCashRegister(CRegister cRegister)
        {
            try
            {
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con)
                {
                    CommandText = $"INSERT INTO CashRegister(Oid, TransDate, StaffDate, LocatCd, Salesman, ShiftType, " +
                    $"ShiftCount, Cheques, Talabat, Deliveroo, Online, Knet, Visa, Expense, 'Reserve', TwentyKd, TenKd, FiveKd, OneKd, HalfKd, " +
                    $"QuarterKd, HundFils, FiftyFils, TwentyFils, TenFils, FiveFils, NetBalance, IsSynced, IsDeleted, SerialNo) " +
                    $"VALUES({cRegister.Oid}, '{cRegister.TransDate:yyyy-MM-dd HH:mm:ss}', '{cRegister.StaffDate: yyyy-MM-dd}', {cRegister.LocationId},{cRegister.Salesman}," +
                    $"'{cRegister.ShiftType}', {cRegister.ShiftCount}, {cRegister.Cheques}, {cRegister.Talabat}, {cRegister.Deliveroo}, {cRegister.Online}," +
                    $"{cRegister.Knet}, {cRegister.Visa}, {cRegister.Expense}, {cRegister.Reserve}, {cRegister.TwentyKd}, {cRegister.TenKd}, " +
                    $"{cRegister.FiveKd}, {cRegister.OneKd}, {cRegister.HalfKd}, {cRegister.QuarterKd}, {cRegister.HundFils}, {cRegister.FiftyFils}, " +
                    $"{cRegister.TwentyFils}, {cRegister.TenFils}, {cRegister.FiveFils}, {cRegister.NetBalance}, {cRegister.IsSynced}, {cRegister.IsDeleted}, {cRegister.SerialNo})"
                };

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                con.Close();
                con.Dispose();

                return cRegister.Oid;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateCashRegister(CRegister cRegister)
        {
            try
            {
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con)
                {
                    CommandText = $"UPDATE CashRegister SET " +
                    $"TransDate = '{cRegister.TransDate:yyyy-MM-dd HH:mm:ss}'," +
                    $"StaffDate = '{cRegister.StaffDate: yyyy-MM-dd}'," +
                    $"LocatCd = {cRegister.LocationId}," +
                    $"Salesman = {cRegister.Salesman}," +
                    $"ShiftType = '{cRegister.ShiftType}'," +
                    $"ShiftCount = {cRegister.ShiftCount}," +
                    $"Cheques = {cRegister.Cheques}," +
                    $"Talabat = {cRegister.Talabat}," +
                    $"Deliveroo = {cRegister.Deliveroo}," +
                    $"Online = {cRegister.Online}," +
                    $"Knet = {cRegister.Knet}," +
                    $"Visa = {cRegister.Visa}," +
                    $"Expense = {cRegister.Expense}," +
                    $"Reserve = {cRegister.Reserve}," +
                    $"TwentyKd = {cRegister.TwentyKd}," +
                    $"TenKd={cRegister.TenKd}," +
                    $"FiveKd={cRegister.FiveKd}," +
                    $"OneKd={cRegister.OneKd}," +
                    $"HalfKd={cRegister.HalfKd}," +
                    $"QuarterKd={cRegister.QuarterKd}," +
                    $"HundFils={cRegister.HundFils}," +
                    $"FiftyFils={cRegister.FiftyFils}," +
                    $"TwentyFils={cRegister.TwentyFils}," +
                    $"TenFils={cRegister.TenFils}," +
                    $"FiveFils={cRegister.FiveFils}," +
                    $"NetBalance={cRegister.NetBalance}," +
                    $"IsSynced={0}," +
                    $"IsDeleted={0}," +
                    $"SerialNo={cRegister.SerialNo} " +
                    $" Where Oid = {cRegister.Oid}"
                };

                cmd.ExecuteNonQuery();

                cmd.Dispose();
                con.Close();
                con.Dispose();

                return cRegister.Oid;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public long GetSerialNo(DateTime dateTime)
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                CommandText = $"select * from CR_Serial"
            };

            var rdr = cmd.ExecuteReader();

            var crSerial = new CRSerial();
            while (rdr.Read())
            {
                crSerial.Date = DateTime.Parse(rdr.GetString(0));
                crSerial.SerialNumber = rdr.GetInt64(1);
                crSerial.BackDays = rdr.GetInt32(2);
            }

            cmd.Dispose();
            con.Close();
            con.Dispose();

            var differenceDays = (DateTime.Now.Date - crSerial.Date.Date).TotalDays;
            
            if(dateTime != null || dateTime != DateTime.MinValue)
                differenceDays = (dateTime.Date - crSerial.Date.Date).TotalDays;

            var serial = crSerial.SerialNumber + differenceDays;

            return (long)serial;
        }

        public int GetBackDays()
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                CommandText = $"select * from CR_Serial"
            };

            var rdr = cmd.ExecuteReader();

            var crSerial = new CRSerial();
            while (rdr.Read())
            {
                crSerial.BackDays = rdr.GetInt32(2);
            }

            cmd.Dispose();
            con.Close();
            con.Dispose();

            return crSerial.BackDays != 0 ? crSerial.BackDays : 1;
        }
    }
}