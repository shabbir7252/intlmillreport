using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillPda.ViewModels
{
    public class ItemVm
    {
        public string Part_No { get; set; }
        public long Prod_Cd { get; set; }
        public string L_Prod_Name { get; set; }
        public string A_Prod_Name { get; set; }
        public byte Item_Type_Cd { get; set; }
        public int GroupCd { get; internal set; }
    }
}