using ImillReports.Contracts;
using ImillReports.Models;
using ImillReports.ViewModels;
using Newtonsoft.Json;
using Syncfusion.EJ2.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ImillReports.Controllers
{
    [Authorize]
    public class SalesReportController : Controller
    {
        private readonly ISalesReportRepository _salesReportRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IBaseUnitRepository _baseUnitRepository;
        private readonly IVoucherTypesRepository _voucherTypesRepository;
        private readonly IProductRepository _productRepository;
        private readonly IBaseRepository _baseRepository;
        public SalesReportController(
            ISalesReportRepository salesReportRepository,
            ILocationRepository locationRepository,
            IBaseUnitRepository baseUnitRepository,
            IProductRepository productRepository,
            IBaseRepository baseRepository,
            IVoucherTypesRepository voucherTypesRepository)
        {
            _salesReportRepository = salesReportRepository;
            _locationRepository = locationRepository;
            _baseUnitRepository = baseUnitRepository;
            _voucherTypesRepository = voucherTypesRepository;
            _productRepository = productRepository;
            _baseRepository = baseRepository;
        }

        public SalesReportController() { }

        [HttpGet]
        public ActionResult Index(DateTime? fromDate, DateTime? toDate, string[] locationStringArray, string[] voucherTypeStringArray)
        {

            if (fromDate == null)
            {
                var fDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 03, 00, 00);
                fromDate = fDate;
                ViewBag.startDate = fDate;
            }

            if (toDate == null)
            {
                var tDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 02, 59, 00).AddDays(1);
                toDate = tDate;
                ViewBag.endDate = tDate;
            }

            ViewBag.startDate = fromDate;
            ViewBag.endDate = toDate;
            ViewBag.validation = "false";

            if (toDate < fromDate)
            {
                ViewBag.validation = "true";
            }

            var locations = _locationRepository.GetLocations().LocationItems;

            var locationArray = new List<int>();
            if (locationStringArray != null)
            {
                locationArray.AddRange(from item in locationStringArray select int.Parse(item));
                foreach (var currentLocation in
                    from location in locations
                    from id in locationArray
                    where location.LocationId == id
                    select location)
                {
                    currentLocation.IsSelected = true;
                }
            }

            ViewBag.locations = locations;
            ViewBag.locationVal = new string[] { "" };


            var voucherTypes = _voucherTypesRepository.GetSalesVoucherTypes().VoucherTypeItems;
            var voucherTypesArray = new List<int>();
            if (voucherTypeStringArray != null)
            {
                voucherTypesArray.AddRange(from item in voucherTypeStringArray select int.Parse(item));
                foreach (var currentVoucherType in
                    from types in voucherTypes
                    from id in voucherTypesArray
                    where types.Voucher_Type == id
                    select types)
                {
                    currentVoucherType.IsSelected = true;
                }
            }

            ViewBag.voucherTypes = voucherTypes;
            ViewBag.voucherTypesVal = new string[] { "" };


            if (fromDate.Value.Date == DateTime.Now.Date || fromDate == null)
            {
                var srs = new List<SalesReportItem>();
                var sr = new SalesReportItem
                {
                    Amount = 0,
                    AmountRecieved = 0,
                    BaseQuantity = 0,
                    BaseUnit = "",
                    Cash = 0,
                    CreditCard = 0,
                    CreditCardType = "",
                    CustomerName = "",
                    CustomerNameAr = "",
                    Date = DateTime.Now,
                    Discount = 0,
                    GroupCD = 0,
                    InvDateTime = DateTime.Now,
                    InvoiceNumber = 0,
                    Knet = 0,
                    Location = "",
                    LocationId = 0,
                    NetAmount = 0,
                    ProductNameAr = "",
                    ProductNameEn = "",
                    Salesman = "",
                    SellQuantity = 0,
                    SellUnit = "",
                    UnitPrice = 0,
                    Voucher = "",
                    VoucherId = 0
                };

                srs.Add(sr);

                ViewBag.DataSource = srs;
                return View();
            }

            var locationsString = locationStringArray != null && locationStringArray.Length > 0 ? string.Join(",", locationStringArray) : "";
            var voucherTypeString = voucherTypeStringArray != null && voucherTypeStringArray.Length > 0 ? string.Join(",", voucherTypeStringArray) : "";

            var salesReportViewModel = _salesReportRepository.GetSalesReport(fromDate, toDate, locationsString, voucherTypeString);
            ViewBag.DataSource = salesReportViewModel.SalesReportItems;
            return View();
        }

        [HttpGet]
        public ActionResult SalesDetailReport(DateTime? fromDate, DateTime? toDate, string[] locationStringArray,
            string[] voucherTypeStringArray, string[] productStringArray, JsonResult querybuilder)
        {

            if (fromDate == null)
            {
                var fDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 03, 00, 00);
                fromDate = fDate;
                ViewBag.startDate = fDate;
            }

            if (toDate == null)
            {
                var tDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 02, 59, 00).AddDays(1);
                toDate = tDate;
                ViewBag.endDate = tDate;
            }

            ViewBag.startDate = fromDate;
            ViewBag.endDate = toDate;
            ViewBag.validation = "false";

            if (toDate < fromDate)
            {
                ViewBag.validation = "true";
            }

            var locations = _locationRepository.GetLocations().LocationItems;

            var locationArray = new List<int>();
            if (locationStringArray != null)
            {
                locationArray.AddRange(from item in locationStringArray select int.Parse(item));
                foreach (var currentLocation in
                    from location in locations
                    from id in locationArray
                    where location.LocationId == id
                    select location)
                {
                    currentLocation.IsSelected = true;
                }
            }

            ViewBag.locations = locations;
            ViewBag.locationVal = new string[] { "" };


            var voucherTypes = _voucherTypesRepository.GetSalesVoucherTypes().VoucherTypeItems;
            var voucherTypesArray = new List<int>();
            if (voucherTypeStringArray != null)
            {
                voucherTypesArray.AddRange(from item in voucherTypeStringArray select int.Parse(item));
                foreach (var currentVoucherType in
                    from types in voucherTypes
                    from id in voucherTypesArray
                    where types.Voucher_Type == id
                    select types)
                {
                    currentVoucherType.IsSelected = true;
                }
            }

            ViewBag.voucherTypes = voucherTypes;
            ViewBag.voucherTypesVal = new string[] { "" };

            var products = _productRepository.GetAllProducts().Items;
            var productArray = new List<int>();
            if (productStringArray != null)
            {
                productArray.AddRange(from item in productStringArray select int.Parse(item));
                foreach (var currentProduct in
                    from types in products
                    from id in productArray
                    where types.ProductId == id
                    select types)
                {
                    currentProduct.IsSelected = true;
                }
            }

            ViewBag.products = products;
            ViewBag.productsVal = new string[] { "" };

            QueryBuilderRule rule = new QueryBuilderRule()
            {
                Condition = "and",
                Rules = new List<QueryBuilderRule>()
                {
                    new QueryBuilderRule { Label="Title", Field="Title", Type="string", Operator="equal", Value = "Sales Manager" }
                }
            };

            ViewBag.rule = rule;

            if (fromDate.Value.Date == DateTime.Now.Date || fromDate == null)
            {
                var srs = new List<SalesReportItem>();
                var sr = new SalesReportItem
                {
                    Amount = 0,
                    AmountRecieved = 0,
                    BaseQuantity = 0,
                    BaseUnit = "",
                    Cash = 0,
                    CreditCard = 0,
                    CreditCardType = "",
                    CustomerName = "",
                    CustomerNameAr = "",
                    Date = DateTime.Now,
                    Discount = 0,
                    GroupCD = 0,
                    InvDateTime = DateTime.Now,
                    InvoiceNumber = 0,
                    Knet = 0,
                    Location = "",
                    LocationId = 0,
                    NetAmount = 0,
                    ProductNameAr = "",
                    ProductNameEn = "",
                    Salesman = "",
                    SellQuantity = 0,
                    SellUnit = "",
                    UnitPrice = 0,
                    Voucher = "",
                    VoucherId = 0
                };

                srs.Add(sr);

                ViewBag.DataSource = srs;
                return View();
            }

            var locationsString = locationStringArray != null && locationStringArray.Length > 0 ? string.Join(",", locationStringArray) : "";
            var voucherTypeString = voucherTypeStringArray != null && voucherTypeStringArray.Length > 0 ? string.Join(",", voucherTypeStringArray) : "";
            var productString = productStringArray != null && productStringArray.Length > 0 ? string.Join(",", productStringArray) : "";

            var salesReportViewModel = _salesReportRepository.GetSalesDetailReport(fromDate, toDate, locationsString, voucherTypeString, productString);
            ViewBag.DataSource = salesReportViewModel.SalesReportItems;
            return View();
        }

        public ContentResult GetView(string from, string to, string location, string voucher, string product)
        {
            var fromDate = DateTime.Now;
            var toDate = DateTime.Now;

            if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
            {
                fromDate = DateTime.Parse(from);
                toDate = DateTime.Parse(to);
            }

            ViewBag.startDate = fromDate;
            ViewBag.endDate = toDate;
            ViewBag.validation = "false";

            if (toDate < fromDate)
            {
                ViewBag.validation = "true";
            }

            var salesReportViewModel = _salesReportRepository.GetSalesDetailReport(fromDate, toDate, location, voucher, product);
            var source = salesReportViewModel.SalesReportItems;

            var serializer = new JavaScriptSerializer();

            // For simplicity just use Int32's max value.
            // You could always read the value from the config section mentioned above.
            serializer.MaxJsonLength = Int32.MaxValue;

            var result = new ContentResult
            {
                // Content = serializer.Serialize(salesReportViewModel.SalesReportItems),
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        public ContentResult GetSalesReport(DateTime from, DateTime to, string locations, string voucherType)
        {
            var salesReportViewModel = _salesReportRepository.GetSalesReport(from, to, locations, voucherType);
            var source = salesReportViewModel.SalesReportItems;

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        public ActionResult SalesHourlyReport(DateTime? fromDate, string[] locationStringArray, int? reportType)
        {
            if (fromDate == null)
            {
                var fDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 06, 00, 00);
                fromDate = fDate;
                ViewBag.startDate = fDate;
            }
            else
            {
                fromDate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 06, 00, 00);
            }

            var toDate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 05, 59, 00).AddDays(1);

            ViewBag.startDate = fromDate;
            ViewBag.validation = "false";

            var locations = _locationRepository.GetLocations().LocationItems;

            var locationArray = new List<int>();
            if (locationStringArray != null)
            {
                locationArray.AddRange(from item in locationStringArray select int.Parse(item));
                foreach (var currentLocation in
                    from location in locations
                    from id in locationArray
                    where location.LocationId == id
                    select location)
                {
                    currentLocation.IsSelected = true;
                }
            }

            ViewBag.locations = locations;
            ViewBag.locationVal = new string[] { "" };

            ViewBag.ReportType = _baseRepository.GetSalesReportType();

            var locationsString = locationStringArray != null && locationStringArray.Length > 0 ? string.Join(",", locationStringArray) : "";
            var voucherTypeString = "201,2021,2022,2025,2026";

            var salesPeakHour = _salesReportRepository.GetSalesHourlyReport(fromDate, toDate, locationsString, voucherTypeString);

            List<ColumnChartData> chartAmountData = (from items in salesPeakHour.SalesPeakHourItems
                                                     select new ColumnChartData { x = items.Hour, yValue = items.Amount }).ToList();
            ViewBag.dataSourceAmount = chartAmountData;

            List<ColumnChartData> chartCountData = (from items in salesPeakHour.SalesPeakHourItems
                                                    select new ColumnChartData { x = items.Hour, yValue = items.TransCount }).ToList();
            ViewBag.dataSourceCount = chartCountData;

            ViewBag.BarChartAmountTitle = fromDate.Value.ToString("dd/MMM/yyyy HH:mm tt") + " to " + toDate.ToString("dd/MMM/yyyy HH:mm tt") + " by Amount";
            ViewBag.BarChartCountTitle = fromDate.Value.ToString("dd/MMM/yyyy HH:mm tt") + " to " + toDate.ToString("dd/MMM/yyyy HH:mm tt") + " by Transaction Count";

            //string[] color = new string[] { "#4286f4", "#f4b642", "#f441a9" };
            //ViewBag.seriesColors = color;

            return View();
        }
    }
}