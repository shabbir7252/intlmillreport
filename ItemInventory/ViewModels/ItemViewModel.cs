using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ItemInventory.ViewModels
{
    public class ItemViewModel
    {
        public string TransDate { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public int Quantity { get; set; }
    }


    public class ItemResponseViewmodel
    {
        public int ReponseId { get; set; }
        public string Message { get; set; }
    }
}