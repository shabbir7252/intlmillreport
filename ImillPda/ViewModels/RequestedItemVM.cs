using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillPda.ViewModels
{
    public class RequestedItemVM
    {
        public long Oid { get; set; }
        public long Prod_Cd { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public decimal Qty { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal ActQty { get; set; }
        public long LineNo { get; set; }
        public string PartNumber { get; set; }
        public bool IsVerified { get; set; }
        public bool IsDelivered { get; internal set; }
        public long Locat_Cd { get; set; }
        public bool IsNewlyAdded { get; set; }
        public bool IsReqQtyChanged { get; set; }
        public decimal OrgQty { get; set; }
    }

    public class TransactionVM {
        public long EntryId { get; set; }
        public DateTime TransDate { get; set; }
        public short Locat_Cd { get; internal set; }
        public string LocationNameEn { get; set; }
        public string LocationNameAr { get; set; }
        public int Oid { get; internal set; }
        public bool IsCompleted { get; internal set; }
        public int ItemCount { get; set; }
        public DateTime UserDateTime { get; internal set; }
        public bool IsHidden { get; internal set; }
        public long RequestNumber { get; internal set; }
        public string CustomDate { get; internal set; }
        public string CustomTime { get; internal set; }
    }
}