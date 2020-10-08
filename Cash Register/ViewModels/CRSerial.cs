using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cash_Register.ViewModels
{
    public class CRSerial
    {
        public DateTime Date { get; set; }
        public long SerialNumber { get; set; }
        public int BackDays { get; set; }
    }
}