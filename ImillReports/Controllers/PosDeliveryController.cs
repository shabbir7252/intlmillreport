using ImillReports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImillReports.Controllers
{
    public class PosDeliveryController : Controller
    {
        public PosDeliveryEntities db = new PosDeliveryEntities();
        public intlmilldbEntities onlineDb = new intlmilldbEntities();
        // GET: PosDelivery
        public ActionResult Index()
        {
            return View();
        }

        public string PosDeliverySync(int backDays)
        {
            try
            {
                var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 00, 00, 00).AddDays(backDays);
                var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

                // Fecthing orders from Online website (GoDaddy Sql Server) where only order which is
                // (KNET(200) & Paid (30)) and 
                // (CC(300) & Paid (30)) and 
                // (COD(100) & Pending (10))
                // will be considered.
                var orders = onlineDb.Orders
                    .Where(x => x.CreatedOn >= startDate && x.CreatedOn <= endDate &&
                    ((x.PaymentType == 200 && x.PaymentStatus == 30) ||
                    (x.PaymentType == 300 && x.PaymentStatus == 30) ||
                    (x.PaymentType == 100 && x.PaymentStatus == 10))).ToList();

                var orderIds = orders.Select(x => x.OrderId).ToList();
                var orderItems = onlineDb.OrderItems.Where(x => orderIds.Contains(x.Order_OrderId.Value)).ToList();

                var models = new List<PosOrder>();
                var orderDetails = new List<PosOrderDetail>();

                var posOrders = db.PosOrders.ToList();

                foreach (var order in orders)
                {
                    if (!posOrders.Any(x => x.OrderId == order.CustomOrderNumber.ToString()))
                    {
                        if (!models.Any(x => x.OrderId == order.CustomOrderNumber.ToString()))
                        {
                            var posOrder = new PosOrder
                            {
                                Address = $"المحافظة : {order.Address.Governorate.GovernoratesNameAr}, المنطقة : {order.Address.Area.AreaNameAr}, " +
                            $"جادة : {order.Address.Avenue}, القطعه : {order.Address.Block}, الشارع : {order.Address.Street}, " +
                            $"رقم المنزل/البناية : {order.Address.House}, رقم الشقة/المكتب : {order.Address.Flat}, الطابق : {order.Address.Floor}, " +
                            $"الرقم الآلي : {order.Address.PACINumber}",
                                ClientName = order.Customer.FullName,
                                DeliveryOn = order.DeliveryDate,
                                MobileNo = order.Address.PhoneNumber.ToString(),
                                NetAmount = order.TotalOrderAmount,
                                OrderDate = order.CreatedOn.Value,
                                OrderId = order.CustomOrderNumber.ToString(),
                                PaymentMethod = order.PaymentType.ToString(),
                                PaymentStatus = order.PaymentStatus.ToString(),
                                ShippingFees = order.ShippingCharge,
                                TotalOrderValue = order.TotalOrderAmount - order.ShippingCharge,
                                IsPrinted = false,
                                Driver = "",
                                Locat_Cd = 0,
                                AllowSync = false
                            };

                            models.Add(posOrder);

                            var items = orderItems.Where(x => x.Order_OrderId.Value == order.OrderId).ToList();
                            var posOrderDetails = db.PosOrderDetails.Where(x => x.OrderOid == order.CustomOrderNumber.ToString());

                            foreach (var item in items)
                            {
                                var itemNameEn = item.ProductAttributeValue.Product_ProductAttribute_Mapping.Product.ProductName;
                                var itemNameAr = item.ProductAttributeValue.Product_ProductAttribute_Mapping.Product.ProductNameAr;
                                var unit = item.ProductAttributeValue.ProductAttributeValueName;
                                var orderNumber = item.Order.CustomOrderNumber.ToString();

                                if (posOrderDetails == null || !posOrderDetails.Any(x => x.Amount == item.Subtotal &&
                                                                      x.ItemNameEn == itemNameEn &&
                                                                      x.ItemNameAr == itemNameAr &&
                                                                      x.OrderOid == orderNumber &&
                                                                      x.Qty == item.Quantity &&
                                                                      x.Unit == unit))
                                {
                                    var posOrderDetail = new PosOrderDetail
                                    {
                                        Amount = item.Subtotal,
                                        ItemNameEn = itemNameEn,
                                        ItemNameAr = itemNameAr,
                                        OrderOid = orderNumber,
                                        Qty = item.Quantity,
                                        Unit = unit
                                    };

                                    orderDetails.Add(posOrderDetail);
                                }
                            }
                        }
                    }
                }

                if (models.Any())
                {
                    db.PosOrders.AddRange(models);
                    db.PosOrderDetails.AddRange(orderDetails);
                    db.SaveChanges();
                }

                return $"POS Delviery: Updated From : {startDate.ToString("dd-MMM-yyyy hh:mm:ss")} to {endDate.ToString("dd-MMM-yyyy hh:mm:ss")}";
            }
            catch (Exception ex)
            {
                return $"POS Delviery: {ex.Message}";
            }

        }
    }
}