using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillPda.ViewModels
{
    public class TransactionDetailVm
    {
        public string Comments { get; set; }
        public IOrderedEnumerable<RequestedItemVM> RequestedItems { get; set; }
        public string LocationNameAr { get; set; }
        public DateTime VoucherDate { get; set; }
        public long VoucherNo { get; set; }
    }

    public class ConsolidatedItems
    {
        public bool IsVerified { get; set; }
        public long Locat_Cd { get; set; }
        public string LocationNameEn { get; set; }
        public string LocationNameAr { get; set; }
        public long Prod_Cd { get; set; }
        public string ProductNameEn { get; set; }
        public string ProductNameAr { get; set; }
        public decimal TotalQuantity { get; set; }
    }

    public class ConsolidatedReportType
    {
        public int Id { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
    }
}