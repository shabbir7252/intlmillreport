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
        public int GroupCd { get; set; }
    }

    public class ItemGroup
    {
        public long ItemGroupId { get; set; }
        public long ParentGroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupNameAr { get; set; }
        public long GroupNumber { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public bool IsSelected { get; set; }
    }
}