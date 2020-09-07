using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cash_Register.ViewModels
{
    public class CRLocation
    {
        public short LocatCd { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string ShortNameEn { get; set; }
        public string ShortNameAr { get; set; }
        public int Pin { get; set; }
    }
}