using ImillReports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.ViewModels
{
    public class SalesReportViewModel
    {
        public List<SalesReportItem> SalesReportItems { get; set; }
    }

    public class SalesReportItem
    {
        public string Location { get; set; }
        public short LocationId { get; set; }
        public DateTime InvDateTime { get; set; }
        public string Salesman { get; set; }
        public string Voucher { get; set; }
        public short VoucherId { get; set; }
        public long InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNameAr { get; set; }
        public decimal? AmountRecieved { get; set; }
        public decimal? Discount { get; set; }
        public decimal? SalesReturn { get; internal set; }
        public decimal? NetAmount { get; set; }
        public string CreditCardType { get; set; }
        public decimal? Cash { get; set; }
        public decimal? Knet { get; set; }
        public decimal? CreditCard { get; set; }
        public int GroupCD { get; set; }


        // Line Sales Report Items
        public string ProductNameEn { get; set; }
        public decimal BaseQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public string BaseUnit { get; internal set; }
        public decimal SellQuantity { get; internal set; }
        public string SellUnit { get; internal set; }
        public DateTime Date { get; internal set; }
        public string ProductNameAr { get; internal set; }
        public short BaseUnitId { get; internal set; }
        public short SellUnitId { get; internal set; }
        public long ProdId { get; internal set; }
    }

    public class TransactionViewModel
    {
        public long Entry_Id { get; set; }
        public SM_Location SM_Location { get; set; }
        public ICS_Transaction_Types ICS_Transaction_Types { get; set; }
        public DateTime Voucher_Date { get; set; }
        public string Salesman { get; internal set; }
        public long Voucher_No { get; internal set; }
        public int GroupCD { get; internal set; }
        public string CustomerName { get; internal set; }
        public string CustomerNameAr { get; internal set; }
    }
}