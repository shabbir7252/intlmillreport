using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillPda.ViewModels
{
    public class ItemResponseViewmodel
    {
        public int ReponseId { get; set; }
        public string Message { get; set; }
    }

    public class ItemViewModel
    {
        public long ProdCd { get; set; }
        public string PartNo { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public decimal SalesRate { get; set; }
    }
}