//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ImillReports.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Trans_Detail_2020
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
        public Nullable<long> Line_No { get; set; }
    }
}
