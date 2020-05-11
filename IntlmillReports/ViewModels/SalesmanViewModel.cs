using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IntlmillReports.ViewModels
{
    public class SalesmanViewModel
    {
        public List<SalesmanItem> SalesmanItems { get; set; }
    }

    public class SalesmanItem
    {
        public int SalesmanId { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
    }
}