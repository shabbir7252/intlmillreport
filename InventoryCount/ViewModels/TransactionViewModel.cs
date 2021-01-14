using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryCount.ViewModels
{
    public class TransactionViewModel
    {
        public int Oid { get; internal set; }
        public int ItemCount { get; internal set; }
        public long TransactionNumber { get; internal set; }
        public string TransNum { get; internal set; }
        public DateTime TransDate { get; internal set; }
        public bool IsEmailSent { get; internal set; }
        public bool IsPrinted { get; internal set; }
        public string Location { get; internal set; }
    }

    public class TransactionDetailVM
    {
        public int Oid { get; internal set; }
        public int SerialNo { get; set; }
        public string PartNumber { get; set; }
        public long ItemOid { get; set; }
        public string ItemNameEn { get; set; }
        public string ItemNameAr { get; set; }
        public long TransactionId { get; set; }
        public DateTime TransDate { get; set; }
        public decimal Weight { get; set; }
        public decimal SalesRate { get; set; }
        public decimal Total { get; set; }
    }

    public class TransactionJsonVM
    {
        public int TransactionOid { get; set; }
    }
}