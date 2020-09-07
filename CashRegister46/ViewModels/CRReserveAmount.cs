using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cash_Register.ViewModels
{
    public class CRReserveAmount
    {
        public short Locat_Cd { get; set; }
        public decimal Reserve_Amt { get; set; }
        public string LocationShortcode { get; set; }
    }
}