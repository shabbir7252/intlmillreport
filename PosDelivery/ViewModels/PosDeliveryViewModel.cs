using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PosDelivery.ViewModels
{
    public class PosDeliveryViewModel
    {
        public DateTime DateTime { get; set; }
        public string OrderId { get; set; }
        public short LocatCd { get; set; }
        public bool IsPrinted { get; set; }
        public bool AllowSync { get; set; }
    }
}