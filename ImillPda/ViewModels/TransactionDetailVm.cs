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
}