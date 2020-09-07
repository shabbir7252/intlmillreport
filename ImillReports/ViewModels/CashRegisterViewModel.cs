using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.ViewModels
{
    public class CashRegisterViewModel
    {
        public List<CashRegisterItem> CashRegisterItems { get; set; }
    }

    public class CashRegisterItem
    {
        public int Oid { get; set; }
        public DateTime TransDateTime { get; set; }
        public short? LocationId { get; set; }
        public string Location { get; set; }
        public string LocationShortName { get; set; }
        public string Salesman { get; set; }
        public decimal? Cheque { get; set; }
        public decimal? Carriage { get; set; }
        public decimal? Online { get; set; }
        public decimal? Knet { get; set; }
        public decimal? Visa { get; set; }
        public decimal? Reserve { get; set; }
        public decimal Cash { get; set; }
        public decimal? Expense { get; set; }
        public decimal? TotalSales { get; set; }
        public decimal? NetSales { get; set; }
        public decimal? NetAmount { get; set; }
        public int ShiftCount { get; set; }
        public string ShiftType { get; set; }
        public DateTime TransDate { get; internal set; }
        public DateTime StaffDate { get; internal set; }
        public string SalesmanType { get; internal set; }
        public bool? IsVerified { get; internal set; }
    }

    public class CashRegVsSalesViewModel
    {
        public List<CashRegVsSalesItem> CashRegVsSalesItems { get; set; }
    }

    public class CashRegVsSalesItem
    {
        public DateTime TransDate { get; set; }
        public string Location { get; set; }
        public string LocationShortName { get; set; }
        public decimal? CRCash { get; set; }
        public decimal? CRKnet { get; set; }
        public decimal? CRVisa { get; set; }
        public decimal? Cash { get; set; }
        public decimal? Knet { get; set; }
        public decimal? CreditCard { get; set; }
        public int ShiftCount { get; internal set; }
        public string ShiftType { get; internal set; }
        public decimal? Reserve { get; internal set; }
        public decimal? CRKnetVisa { get; internal set; }
        public decimal? KnetVisa { get; internal set; }
        public decimal? Total { get; set; }
        public decimal? TotalCRSales { get; internal set; }
        public decimal? TotalInnovaSales { get; internal set; }
        public decimal? CashDiff { get; internal set; }
        public decimal? KnetDiff { get; internal set; }
        public decimal? CcDiff { get; internal set; }
        public decimal? KnetVisaDiff { get; internal set; }
        public decimal? TotalDiff { get; internal set; }
        public decimal? CROnline { get; internal set; }
        public decimal? Online { get; internal set; }
        public decimal? OnlineDiff { get; internal set; }
        public decimal? CRReserve { get; internal set; }
        public decimal SalesReturn { get; internal set; }
        public decimal? CRTalabat { get; internal set; }
        public decimal Talabat { get; internal set; }
        public decimal? TalabatDiff { get; internal set; }
    }
}