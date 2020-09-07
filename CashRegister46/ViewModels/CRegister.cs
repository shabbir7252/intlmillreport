using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cash_Register.ViewModels
{
    public class CRegister
    {
        public int Oid { get; set; }
        public DateTime TransDate { get; set; }
        public DateTime StaffDate { get; set; }
        public short LocationId { get; set; }
        public short Salesman { get; set; }
        public string ShiftType { get; set; }
        public decimal Cheques { get; set; }
        public decimal Talabat { get; set; }
        public decimal Online { get; set; }
        public decimal Knet { get; set; }
        public decimal Visa { get; set; }
        public decimal Expense { get; set; }
        public decimal Reserve { get; set; }
        public int TwentyKd { get; set; }
        public int TenKd { get; set; }
        public int FiveKd { get; set; }
        public int OneKd { get; set; }
        public int HalfKd { get; set; }
        public int QuarterKd { get; set; }
        public int HundFils { get; set; }
        public int FiftyFils { get; set; }
        public int TwentyFils { get; set; }
        public int TenFils { get; set; }
        public int FiveFils { get; set; }
        public decimal NetBalance { get; set; }
        public int ShiftCount { get; set; }
        public bool IsSynced { get; internal set; }
        public bool IsDeleted { get; internal set; }
    }

    public class ShiftCount
    {
        public short LocationId { get; set; }
        public short Salesman { get; set; }
        public string ShiftType { get; set; }
        public DateTime StaffDate { get; set; }
    }
}