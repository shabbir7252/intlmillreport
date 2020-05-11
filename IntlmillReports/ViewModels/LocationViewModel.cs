using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IntlmillReports.ViewModels
{
    public class LocationViewModel
    {
        public List<LocationItem> LocationItems { get; set; }
    }

    public class LocationItem
    {
        public int LocationId { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string ShortName { get; set; }
    }
}