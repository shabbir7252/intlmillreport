using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.ViewModels
{
    public class VoucherTypeViewModel
    {
        public List<VoucherTypeItem> VoucherTypeItems { get; set; }
    }

    public class VoucherTypeItem
    {
        public short Voucher_Type { get; set; }
        public string L_Voucher_Name { get; set; }
        public string A_Voucher_Name { get; set; }
        public bool IsSelected { get; internal set; }
    }
}