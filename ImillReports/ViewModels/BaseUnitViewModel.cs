using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.ViewModels
{
    public class BaseUnitViewModel
    {
        public List<BaseUnitItem> BaseUnitItems { get; set; }
    }

    public class BaseUnitItem
    {
        public short Unit_Cd { get; set; }
        public string L_Unit_Name { get; set; }
        public string A_Unit_Name { get; set; }
        public bool IsSelected { get; internal set; }
    }
}