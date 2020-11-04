using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ItemInventory.ViewModels
{
    public class TransactionViewModel
    {
        public int Oid { get; set; }
        public DateTime TransDate { get; set; }
        public long TransactionNumber { get; set; }
        public string RequestedBy { get; set; }
        public int ItemCount { get; set; }
        public string TransNum { get; set; }
    }

    public class TransactionDetailVM
    {
        public int Oid { get; set; }
        public long ItemId { get; set; }
        public string ItemNameEn { get; set; }
        public string ItemNameAr { get; set; }
        public long UnitId { get; set; }
        public string UnitNameEn { get; set; }
        public string UnitNameAr { get; set; }
        public int Quantity { get; set; }
        public long TransactionNumber { get; set; }
    }

    public class TransactionJsonVM
    {
        public int TransactionOid { get; set; }
    }
}