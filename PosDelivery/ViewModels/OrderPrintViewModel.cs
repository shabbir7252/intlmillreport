using System;
using System.Collections.Generic;

namespace PosDelivery.ViewModels
{
    public class OrderPrintViewModel
    {
        public DateTime DateTime { get; set; }
        public string OrderId { get; set; }
        public string ClientName { get; set; }
        public string MobileNo { get; set; }
        public DateTime DeliveryOn { get; set; }
        public string Address { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public decimal TotalOrderValue { get; set; }
        public decimal ShippingFees { get; set; }
        public decimal NetAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string Driver { get; internal set; }
        public string PaymentStatus { get; internal set; }
        public short Location { get; internal set; }
    }

    public class OrderItem
    {
        public string ItemNameEn { get; set; }
        public string ItemNameAr { get; set; }
        public string Unit { get; set; }
        public int Qty { get; set; }
        public decimal Amount { get; set; }
    }
}