using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.ViewModels
{
    public class ProductViewModel
    {
        public List<Item> Items { get; set; }
    }

    public class Item
    {
        public long ProductId { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public bool IsSelected { get; internal set; }
    }
}