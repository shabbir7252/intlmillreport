using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryCount.ViewModels
{
    public class ItemViewModel
    {
        public long ProdCd { get; set; }
        public string PartNo { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public decimal SalesRate { get; set; }
    }

    public class ItemGridRes
    {
        public int SerialNo { get; set; }
        public string ItemNameEn { get; set; }
        public string ItemNameAr { get; set; }
        public string PartNumber { get; set; }
        public decimal Weight { get; set; }
        public decimal SalesRate { get; set; }
        public decimal Total { get; set; }
    }

    public class ItemResponseViewmodel
    {
        public int ReponseId { get; set; }
        public string Message { get; set; }
    }
}