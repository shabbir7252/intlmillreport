using System;
using ImillReports.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ImillReports.ViewModels
{
    public class SalesReportViewModel
    {
        public List<SalesReportItem> SalesReportItems { get; set; }
    }

    public class SalesReportDashboard
    {
        public List<SalesReportItem> SRItemsTrans { get; set; }
        public List<TransDetailsViewModel> SRItemsTransDetails { get; set; }
    }

    public class ChartData
    {
        public JsonResult Chart1 { get; set; }
        public JsonResult Chart2 { get; set; }
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
        public long EntryId { get; internal set; }
        public int CustomerId { get; internal set; }
        public int Year { get; internal set; }
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

    public class SalesPeakHourViewModel
    {
        public List<SalesPeakHourItem> SalesPeakHourItems { get; set; }
    }

    public class SalesPeakHourItem
    {
        public string Location { get; set; }
        public DateTime Hour { get; set; }
        public decimal? Amount { get; set; }
        public int TransCount { get; internal set; }
        public int LocationId { get; internal set; }
    }

    public class SalesReportType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
    }

    public class HourlySalesBranchTotal
    {
        public string LocationName { get; set; }
        public string AmountInfo { get; internal set; }
    }

    public class HourlySalesBranchCountTotal
    {
        public string LocationName { get; set; }
        public string CountInfo { get; internal set; }
    }

    public class TransDetailsViewModel
    {
        public int Oid { get; set; }
        public string Location { get; set; }
        public Nullable<short> LocationId { get; set; }
        public Nullable<System.DateTime> InvDateTime { get; set; }
        public string Salesman { get; set; }
        public string Voucher { get; set; }
        public Nullable<short> VoucherId { get; set; }
        public Nullable<long> InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNameAr { get; set; }
        public Nullable<decimal> AmountRecieved { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public Nullable<decimal> SalesReturn { get; set; }
        public Nullable<decimal> NetAmount { get; set; }
        public string CreditCardType { get; set; }
        public Nullable<decimal> Cash { get; set; }
        public Nullable<decimal> Knet { get; set; }
        public Nullable<decimal> CreditCard { get; set; }
        public Nullable<int> GroupCD { get; set; }
        public string ProductNameEn { get; set; }
        public string ProductNameAr { get; set; }
        public Nullable<decimal> BaseQuantity { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public string BaseUnit { get; set; }
        public Nullable<decimal> SellQuantity { get; set; }
        public string SellUnit { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<short> BaseUnitId { get; set; }
        public Nullable<short> SellUnitId { get; set; }
        public Nullable<long> ProdId { get; set; }
        public Nullable<long> EntryId { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public Nullable<int> Year { get; set; }
    }

    public class DailyConsumptionVM
    {
        public int Oid { get; set; }
        public long? ItemOid { get; set; }
        public string ItemNameEn { get; set; }
        public string ItemNameAr { get; set; }
        public decimal? TotalKgQty { get; set; }
        public decimal? TotalQty { get; set; }
        public decimal? TotalBranchKgQty { get; set; }
        public decimal? TotalBranchQty { get; set; }
        public decimal? CreditQty { get; set; }
        public decimal? CashQty { get; set; }
        public string BaseUnit { get; set; }
    }


    public class SalesReportItemFromXcel
    {
        public string Location { get; set; }

        // InvDateTime
        public DateTime Date { get; set; }
        public string Salesman { get; set; }

        // InvoiceNumber
        public long Invoice { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNameAr { get; set; }
        public string Voucher { get; set; }

        // AmountRecieved
        public decimal? Amount { get; set; }
        public decimal? Discount { get; set; }
        
        // NetAmount
        public decimal? TotalAmount  { get; set; }
        public decimal? Cash { get; set; }
        public decimal? Knet { get; set; }

        // CreditCard
        public decimal? Visa { get; set; }
    }

    public class SalesItemGroup
    {
        public long ProdId { get; set; }
        public string ProdNameEn { get; set; }
        public string ProdNameAr { get; set; }
        public int GroupCd { get; set; }
        public string GroupNameEn { get; set; }
        public string GroupNameAr { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? Cash { get; set; }
        public decimal? Credit { get; set; }
        public int ParentGroupCd { get; set; }
    }

    public class SalesByItemGroupResponse
    {
        public int GroupCd { get; set; }
        public decimal? Amount { get; set; }
        public List<int> GroupCdToIgnore { get; set; }
    }
}