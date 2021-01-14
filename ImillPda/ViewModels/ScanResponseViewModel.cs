using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillPda.ViewModels
{
    public class ScanResponseViewModel
    {
        public string Identifier { get; set; }
        public string PartNumber { get; set; }
        public decimal Weight { get; set; }
        public decimal SalesRate { get; set; }
        public string CheckDigit { get; set; }
        public string ItemNameEn { get; set; }
        public string ItemNameAr { get; set; }
        public long Oid { get; set; }
        public string Message { get; internal set; }
        public int ResponseId { get; internal set; }
        public DateTime RequestedDate { get; internal set; }
        public long LineNo { get; internal set; }
        public short Locat_Cd { get; internal set; }
        public long Prod_Cd { get; internal set; }
    }
}