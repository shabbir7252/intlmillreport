using System;
using System.Linq;
using System.Drawing;
using System.Web.Mvc;
using System.Data.SQLite;
using PosDelivery.Models;
using System.Configuration;
using System.Data.SqlClient;
using PosDelivery.ViewModels;
using System.Drawing.Printing;
using System.Collections.Generic;

namespace PosDelivery.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        readonly string cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

        public intlmilldbEntities onlineDb = new intlmilldbEntities();
        public PosDeliveryEntities db = new PosDeliveryEntities();
        public IMILLEntities imillDb = new IMILLEntities();

        // Deprecated Fuction - 05-07-2021 Shabbir Babji
        public ActionResult Index_Deprecated()
        {
            try
            {
                //var lastOrderDate = onlineDb.Orders.OrderByDescending(x => x.CreatedOn).FirstOrDefault().CreatedOn;
                //var startDate = new DateTime(lastOrderDate.Value.Year, lastOrderDate.Value.Month, lastOrderDate.Value.Day);
                var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 00, 00, 00).AddDays(-1);
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

                var pOrders = db.PosOrders.OrderByDescending(x => x.OrderDate).ToList();
                ViewBag.DataSource = pOrders;
                return View(pOrders);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting  
                        // the current instance as InnerException  
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult Index()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 00, 00, 00).AddDays(-10);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

            var pOrders = db.PosOrders.Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate).OrderByDescending(x => x.OrderDate).ToList();
            ViewBag.DataSource = pOrders;
            return View(pOrders);
        }

        // Function Copied to Sync Functionality On Dashboard APP - 05-07-2021 -Shabbir Ismail
        public string PosDeliverySync(int backDays)
        {
            try
            {
                var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 00, 00, 00).AddDays(-1);
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

        [AllowAnonymous]
        public ActionResult SyncPosData()
        {
            return View();
        }

        public ActionResult AddNewOrder()
        {
            var locations = imillDb.SM_Location.ToList();
            ViewBag.Locations = new SelectList(locations, "Locat_Cd", "A_Locat_Name");

            var items = imillDb.ICS_Item.ToList();
            ViewBag.Items = new SelectList(items, "A_Prod_Name", "A_Prod_Name");
            return View();
        }

        public string SaveNewOrder(string orderDate, string clientName, string mobileNumber, string deliveryDate,
            string address, string driver, string branch, string paymentMethod, string shippingCharges, List<string> itemList)
        {
            var message = "Order Not Completed!";

            try
            {
                var _orderDate = DateTime.Parse(orderDate);
                var today = DateTime.Now;
                var newOrderDate = new DateTime(_orderDate.Year, _orderDate.Month, _orderDate.Day, today.Hour, today.Minute, today.Second);
                var _deliveryDate = DateTime.Parse(deliveryDate);
                var orderId = DateTime.Now.ToString("yyMMddHHmmss");
                var order = new PosOrder
                {
                    Address = address,
                    AllowSync = true,
                    ClientName = clientName,
                    DeliveryOn = _deliveryDate,
                    Driver = driver,
                    IsPrinted = false,
                    Locat_Cd = short.Parse(branch),
                    MobileNo = mobileNumber,
                    OrderDate = newOrderDate,
                    PaymentMethod = paymentMethod,
                    PaymentStatus = "10",
                    ShippingFees = decimal.Parse(shippingCharges),
                    OrderId = orderId
                };

                var orderDetails = new List<PosOrderDetail>();
                var items = imillDb.ICS_Item.ToList();

                foreach (var rec in itemList)
                {
                    var itemArray = rec.Split(',');

                    var item = itemArray[1];
                    var unit = itemArray[2];
                    var qty = itemArray[3];
                    var amount = itemArray[4];

                    var dbItem = items.FirstOrDefault(x => x.A_Prod_Name == item);
                    var orderDetail = new PosOrderDetail
                    {
                        Amount = decimal.Parse(amount),
                        OrderOid = orderId,
                        Qty = int.Parse(qty),
                        Unit = unit,
                        ItemNameAr = dbItem.A_Prod_Name,
                        ItemNameEn = dbItem.L_Prod_Name
                    };

                    orderDetails.Add(orderDetail);
                }

                var grossAmount = orderDetails.Sum(x => x.Amount);
                var netAmount = grossAmount + decimal.Parse(shippingCharges);

                order.TotalOrderValue = grossAmount;
                order.NetAmount = netAmount;

                db.PosOrders.Add(order);
                db.PosOrderDetails.AddRange(orderDetails);
                db.SaveChanges();

                message = $"Order Saved SuccessFully. Order Id : {order.OrderId}";
            }
            catch (Exception ex)
            {
                return message = $"Error : { ex.Message }";
            }

            return message;
        }

        public string UpdateOrder(string orderId, string clientName, string mobileNumber, string address, string driver,
            string branch, string shippingCharges, List<string> itemList)
        {

            var message = "Order Not Completed!";

            try
            {

                var order = db.PosOrders.FirstOrDefault(x => x.OrderId == orderId);

                order.Address = address;
                order.AllowSync = true;
                order.ClientName = clientName;
                order.Driver = driver;
                order.IsPrinted = false;
                order.Locat_Cd = short.Parse(branch);
                order.MobileNo = mobileNumber;
                order.ShippingFees = decimal.Parse(shippingCharges);
                order.OrderId = orderId;

                var orderDetails = new List<PosOrderDetail>();
                var dbOrderDetails = db.PosOrderDetails.Where(x => x.OrderOid == orderId).ToList();
                var items = imillDb.ICS_Item.ToList();

                foreach (var rec in itemList)
                {
                    var itemArray = rec.Split(',');

                    var item = itemArray[1];
                    var unit = itemArray[2];
                    var qty = itemArray[3];
                    var amount = itemArray[4];

                    var dbItem = items.FirstOrDefault(x => x.A_Prod_Name == item);

                    if (dbItem != null)
                    {
                        var orderDetail = new PosOrderDetail
                        {
                            Amount = decimal.Parse(amount),
                            OrderOid = orderId,
                            Qty = int.Parse(qty),
                            Unit = unit,
                            ItemNameAr = dbItem.A_Prod_Name,
                            ItemNameEn = dbItem.L_Prod_Name
                        };
                        orderDetails.Add(orderDetail);
                    }
                    else
                    {
                        var orderDetail = new PosOrderDetail
                        {
                            Amount = decimal.Parse(amount),
                            OrderOid = orderId,
                            Qty = int.Parse(qty),
                            Unit = unit,
                            ItemNameAr = item,
                            ItemNameEn = item
                        };
                        orderDetails.Add(orderDetail);
                    }

                }

                var grossAmount = orderDetails.Sum(x => x.Amount);
                var netAmount = grossAmount + decimal.Parse(shippingCharges);

                order.TotalOrderValue = grossAmount;
                order.NetAmount = netAmount;

                // db.PosOrders.Add(order);
                db.PosOrderDetails.RemoveRange(dbOrderDetails);
                db.PosOrderDetails.AddRange(orderDetails);
                db.SaveChanges();

                message = $"Order Updated SuccessFully. Order Id : {order.OrderId}";
            }
            catch (Exception ex)
            {
                return message = $"Error : { ex.Message }";
            }

            return message;
        }

        [AllowAnonymous]
        public string GetPrintId(int locatcd)
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                CommandText = $"select OrderId from Delivery where IsPrinted = 0 and LocatCd = {locatcd} order by DateTime Limit 1"
            };

            var rdr = cmd.ExecuteReader();
            var orderId = "";
            while (rdr.Read())
            {
                orderId = rdr.GetString(0);
            }

            cmd.Dispose();
            con.Close();
            con.Dispose();

            return orderId;
        }

        [AllowAnonymous]
        public ActionResult Print(string orderId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PosDelivery"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            connection.Open();

            var sqlQuery = $"select * from PosOrder where OrderId = '{orderId}'";

            var sqlCommand = new SqlCommand(sqlQuery, connection);
            var dataReader = sqlCommand.ExecuteReader();

            var model = new OrderPrintViewModel();

            try
            {
                while (dataReader.Read())
                {
                    var payMethod = dataReader.GetValue(10).ToString();

                    model.DateTime = DateTime.Parse(dataReader.GetValue(1).ToString());
                    model.OrderId = dataReader.GetValue(2).ToString();
                    model.ClientName = dataReader.GetValue(3).ToString();
                    model.MobileNo = dataReader.GetValue(4).ToString();
                    model.DeliveryOn = DateTime.Parse(dataReader.GetValue(5).ToString());
                    model.Address = dataReader.GetValue(6).ToString();
                    model.TotalOrderValue = decimal.Parse(dataReader.GetValue(7).ToString());
                    model.ShippingFees = decimal.Parse(dataReader.GetValue(8).ToString());
                    model.NetAmount = decimal.Parse(dataReader.GetValue(9).ToString());
                    model.PaymentMethod = payMethod == "100" ? "COD"
                                                             : payMethod == "200" ? "KNET"
                                                                                  : payMethod == "300" ? "CC" : "N/A";
                    model.OrderItems = GetOrderDetails(dataReader.GetValue(2).ToString());

                    SetIsPrinted(model.OrderId, true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            dataReader.Close();
            sqlCommand.Dispose();
            connection.Close();

            return View(model);
        }

        [AllowAnonymous]
        public string SetIsPrinted(string orderId, bool isPrinted)
        {
            var con = new SQLiteConnection(cs);
            con.Open();

            var cmd = new SQLiteCommand(con)
            {
                CommandText = $"Update Delivery Set IsPrinted = {isPrinted} where OrderId = '{orderId}'"
            };
            cmd.ExecuteReader();

            cmd.Dispose();
            con.Close();
            con.Dispose();

            return "true";
        }

        public List<ViewModels.OrderItem> GetOrderDetails(string orderId)
        {
            var models = new List<ViewModels.OrderItem>();
            string connectionString = ConfigurationManager.ConnectionStrings["PosDelivery"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            connection.Open();

            var sqlQuery = $"select * from PosOrderDetails where OrderOid = {orderId}";

            var sqlCommand = new SqlCommand(sqlQuery, connection);
            var dataReader = sqlCommand.ExecuteReader();


            while (dataReader.Read())
            {
                var model = new ViewModels.OrderItem();
                model.ItemNameEn = dataReader.GetValue(1).ToString();
                model.ItemNameAr = dataReader.GetValue(2).ToString();
                model.Unit = dataReader.GetValue(3).ToString();
                model.Qty = int.Parse(dataReader.GetValue(4).ToString());
                model.Amount = decimal.Parse(dataReader.GetValue(5).ToString());
                models.Add(model);
            }

            dataReader.Close();
            sqlCommand.Dispose();
            connection.Close();


            return models;
        }

        public ActionResult OrderDetails(string id)
        {
            var posOrder = db.PosOrders.FirstOrDefault(x => x.OrderId == id.ToString());
            var details = db.PosOrderDetails.Where(x => x.OrderOid == id).ToList();

            var detailModel = new List<ViewModels.OrderItem>();

            foreach (var det in details)
            {
                var orderItem =
                        new ViewModels.OrderItem
                        {
                            ItemNameEn = det.ItemNameEn,
                            ItemNameAr = det.ItemNameAr,
                            Unit = det.Unit,
                            Qty = det.Qty,
                            Amount = det.Amount
                        };

                detailModel.Add(orderItem);
            }

            var orderPrintViewModel = new OrderPrintViewModel
            {
                DateTime = posOrder.OrderDate,
                OrderId = posOrder.OrderId,
                ClientName = posOrder.ClientName,
                MobileNo = posOrder.MobileNo,
                DeliveryOn = posOrder.DeliveryOn,
                Address = posOrder.Address,
                TotalOrderValue = posOrder.TotalOrderValue,
                ShippingFees = posOrder.ShippingFees,
                NetAmount = posOrder.NetAmount,
                PaymentMethod = posOrder.PaymentMethod,
                OrderItems = detailModel,
                Driver = posOrder.Driver,
                PaymentStatus = posOrder.PaymentStatus,
                Location = posOrder.Locat_Cd
            };

            //var Governorates = db.Governorates.ToList();
            //ViewBag.Governorates = new SelectList(Governorates, "GovernorateId", "GovernoratesName");

            var locations = imillDb.SM_Location.ToList();
            ViewBag.Locations = new SelectList(locations, "Locat_Cd", "A_Locat_Name");

            var items = imillDb.ICS_Item.ToList();
            ViewBag.Items = new SelectList(items, "A_Prod_Name", "A_Prod_Name");

            return View(orderPrintViewModel);
        }

        public ActionResult Save(FormCollection formCol)
        {
            return View();
        }

        private string Print()
        {
            var pd = new PrintDocument
            {
                PrinterSettings =
                    {
                        // PrinterName = "OneNote for Windows 10"
                        PrinterName = "Adobe PDF"
                    }
            };
            pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
            if (pd.PrinterSettings.IsValid)
            {
                pd.Print();
                return "Printed";
            }

            try
            {
                pd.Print();
            }
            catch (Exception ex)
            {

                return ex.Message;
            }

            return "Not Printed";
        }

        void pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawString("Test Print",
                new System.Drawing.Font(
                    "Arial",
                    12),
                new SolidBrush(Color.Blue),
                60,
                190);


        }
    }
}