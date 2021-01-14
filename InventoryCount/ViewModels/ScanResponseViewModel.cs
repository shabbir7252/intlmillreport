using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryCount.ViewModels
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
    }
}