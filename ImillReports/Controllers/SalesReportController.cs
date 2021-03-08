using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using Syncfusion.XlsIO;
using ImillReports.Models;
using System.Configuration;
using ImillReports.Contracts;
using ImillReports.ViewModels;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using Syncfusion.EJ2.QueryBuilder;
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

        private readonly string PageName = "SalesDetailReport";
        // jsuts che
        public SalesReportController() { }

        #region Sales Report

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

            if (toDate < fromDate) ViewBag.validation = "true";

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

            ViewBag.locations = locations.OrderBy(x => x.Name);
            ViewBag.locationVal = new string[] { "" };

            //var enumData = from LocationType e in Enum.GetValues(typeof(LocationType))
            //               select new
            //               {
            //                   Id = (int)e,
            //                   Name = e.ToString()
            //               };

            //var selectEnumData = new SelectList(enumData, "Id", "Name");

            //var locationGroupItems = new List<LocationGroupItem>();

            //foreach (var item in selectEnumData)
            //{
            //    var locationGroupItem = new LocationGroupItem
            //    {
            //        LocationId = int.Parse(item.Value),
            //        Name = item.Text
            //    };

            //    locationGroupItems.Add(locationGroupItem);
            //}


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

            ViewBag.voucherTypes = voucherTypes.OrderBy(x => x.L_Voucher_Name);
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
            string[] voucherTypeStringArray, bool? _, string[] productStringArray, string[] _1, JsonResult _2)
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

            ViewBag.locations = locations.OrderBy(x => x.Name);
            ViewBag.locationVal = new string[] { "" };

            //var enumData = from LocationType e in Enum.GetValues(typeof(LocationType))
            //               select new
            //               {
            //                   Id = (int)e,
            //                   Name = e.ToString()
            //               };

            //var selectEnumData = new SelectList(enumData, "Id", "Name");

            //var locationGroupItems = new List<LocationGroupItem>();

            //foreach (var item in selectEnumData)
            //{
            //    var locationGroupItem = new LocationGroupItem
            //    {
            //        LocationId = int.Parse(item.Value),
            //        Name = item.Text
            //    };

            //    locationGroupItems.Add(locationGroupItem);
            //}

            //ViewBag.locations = locationGroupItems;
            //ViewBag.locationVal = new string[] { "" };


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

            ViewBag.voucherTypes = voucherTypes.OrderBy(x => x.L_Voucher_Name);
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

            ViewBag.products = products.OrderBy(x => x.Name);
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

            SetSalesDetailsColChooserVal();

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

            var salesReportViewModel = _salesReportRepository.GetSalesDetailTransaction(fromDate, toDate, locationsString, voucherTypeString, productString);
            ViewBag.DataSource = salesReportViewModel;

            return View();
        }

        [HttpPost]
        public ActionResult GetSalesDetailReport(string from, string to, string locations, string voucher, string product, string productAr, bool isChecked)
        {
            try
            {
                var fromDate = DateTime.Now;
                var toDate = DateTime.Now;
                var productIds = isChecked ? product : productAr;

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

                //var locationList = new List<int>();
                //var locationStringArray = location.Split(',');

                //foreach (var item in locationStringArray)
                //{
                //    var id = int.Parse(item);
                //    if ((LocationType)id == LocationType.Coops)
                //    {
                //        var locationIds = _baseRepository.GetLocationIds(LocationType.Coops);
                //        foreach (var locationId in locationIds)
                //            locationList.Add(locationId);
                //    }
                //    else if ((LocationType)id == LocationType.Mall)
                //    {
                //        var locationIds = _baseRepository.GetLocationIds(LocationType.Mall);
                //        foreach (var locationId in locationIds)
                //            locationList.Add(locationId);
                //    }
                //}

                //var locationString = string.Join(",", locationList.Select(n => n.ToString()).ToArray());

                //var salesReportViewModel = _salesReportRepository.GetSalesDetailReport(fromDate, toDate, locations, voucher, productIds);
                //var source = salesReportViewModel.SalesReportItems;
                var salesReportViewModel = _salesReportRepository.GetSalesDetailTransaction(fromDate, toDate, locations, voucher, productIds);
                var source = salesReportViewModel;
                //ViewBag.DataSource = source;
                //return View();

                var serializer = new JavaScriptSerializer
                {

                    // For simplicity just use Int32's max value.
                    // You could always read the value from the config section mentioned above.
                    MaxJsonLength = Int32.MaxValue
                };

                var result = new ContentResult
                {
                    // Content = serializer.Serialize(salesReportViewModel.SalesReportItems),
                    Content = JsonConvert.SerializeObject(source),
                    ContentType = "application/json"
                };

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public ContentResult GetSalesReport(DateTime from, DateTime to, string locations, string voucherType)
        {
            var salesReportViewModel = _salesReportRepository.GetSalesTransaction(from, to, locations, voucherType);
            var source = salesReportViewModel.SalesReportItems.Where(x => x.GroupCD != 329);

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            var exportData = source.ToList().Select(x => new SalesReportItemFromXcel
            {
                Amount = x.AmountRecieved,
                Cash = x.Cash,
                CustomerName = x.CustomerName,
                CustomerNameAr = x.CustomerNameAr,
                Date = x.InvDateTime,
                Discount = x.Discount,
                Invoice = x.InvoiceNumber,
                Knet = x.Knet,
                Location = x.Location,
                Salesman = x.Salesman,
                TotalAmount = x.NetAmount,
                Visa = x.CreditCard,
                Voucher = x.Voucher
            }).ToList();

            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];

                //Import data to worksheet
                worksheet.ImportData(exportData, 1, 1, true);

                var cs = @ConfigurationManager.ConnectionStrings["ExcelConnection"].ConnectionString;
                var reportNamePath = $"{cs}SalesReport.xlsx";

                //Save the file in the given path
                Stream excelStream = System.IO.File.Create(Path.GetFullPath(@reportNamePath));
                workbook.SaveAs(excelStream);
                excelStream.Dispose();
            }

            return result;
        }

        public string GetSalesSync(int days)
        {
            return _salesReportRepository.GetSales(days);
        }

        public string GetSalesDetailSync(int days)
        {
            return _salesReportRepository.GetSalesDetail(days);
        }

        public string GetSalesMonthSync(int year, int month, int from, int to)
        {
            return $"{_salesReportRepository.GetSalesMonth(year, month, from, to)} Date : ({from}/{to})-{month}-{year}";
        }

        public string GetSalesDetailMonthSync(int year, int month, int from, int to)
        {
            return $"{_salesReportRepository.GetSalesDetailMonth(year, month, from, to)} Date : ({from}/{to})-{month}-{year}";
        }

        #endregion

        #region Sales Report By Item group

        [HttpGet]
        public ActionResult ItemGroupSales(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate == null)
            {
                var fDate = new DateTime(DateTime.Now.Year, 10, 1, 10, 00, 00);
                fromDate = fDate;
                ViewBag.startDate = fDate;
            }

            if (toDate == null)
            {
                var tDate = new DateTime(DateTime.Now.Year, 10, 1, 11, 00, 00);
                toDate = tDate;
                ViewBag.endDate = tDate;
            }

            ViewBag.startDate = fromDate;
            ViewBag.endDate = toDate;
            ViewBag.validation = "false";

            if (toDate < fromDate) ViewBag.validation = "true";

            _salesReportRepository.GetReportByItemGroup(fromDate, toDate);

            return View();
        }

        #endregion

        #region Sales Hourly Report

        [Authorize(Roles = "Admin,Sales, StaffAdmin")]
        public ActionResult SalesHourlyReport(DateTime? fromDate, string[] locationStringArray)
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

            var locations = _locationRepository.GetLocations().LocationItems.Where(x => x.Type != Repository.LocationRepository.LocationType.HO);

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

            ViewBag.locations = locations.OrderBy(x => x.Name);
            ViewBag.locationVal = new string[] { "" };

            var reportType = _baseRepository.GetSalesReportType();

            if (User.IsInRole("Sales"))
            {
                var type = reportType.FirstOrDefault(x => x.Id == 1);
                reportType.Remove(type);
            }

            ViewBag.ReportType = reportType;

            var locationsString = locationStringArray != null && locationStringArray.Length > 0 ? string.Join(",", locationStringArray) : "";

            var branchWiseTotals = new List<HourlySalesBranchTotal>();
            var branchWiseCountTotals = new List<HourlySalesBranchCountTotal>();

            #region Chart Amount

            List<ColumnChartData> chartAmountData1 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData2 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData3 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData4 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData5 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData6 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData7 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData8 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData9 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData10 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData11 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData12 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData13 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData14 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData15 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData16 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData17 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData18 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData19 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData20 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData21 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData22 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData23 = new List<ColumnChartData>();
            List<ColumnChartData> chartAmountData24 = new List<ColumnChartData>();

            // var salesHourlyReport = _salesReportRepository.GetSalesHourlyReport(fromDate, toDate, locationsString, "");
            var salesPeakHourItems = _salesReportRepository.GetSalesHourlyReport(fromDate, toDate, locationsString, "").SalesPeakHourItems.GroupBy(x => x.LocationId);
            var count = 1;

            var totalChartAmount = salesPeakHourItems.Sum(x => x.Sum(y => y.Amount)).Value;
            ViewBag.TotalChartAmount = totalChartAmount.ToString("0.000");

            foreach (var items in salesPeakHourItems.OrderByDescending(x => x.Sum(y => y.Amount)))
            {
                var branchTotal = items.Sum(x => x.Amount).Value;
                var branchPercentage = totalChartAmount == 0 ? 0 : 100 / totalChartAmount * branchTotal;
                var branchWiseTotal = new HourlySalesBranchTotal
                {
                    LocationName = locations.FirstOrDefault(x => x.LocationId == items.Key).Name,
                    AmountInfo = $"{branchTotal:0.000} ({branchPercentage:0.0}%)"
                };

                branchWiseTotals.Add(branchWiseTotal);

                var listData = chartAmountData1;

                switch (count)
                {
                    case 1:
                        listData = chartAmountData1;
                        ViewBag.locationName1 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 2:
                        listData = chartAmountData2;
                        ViewBag.locationName2 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 3:
                        listData = chartAmountData3;
                        ViewBag.locationName3 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 4:
                        listData = chartAmountData4;
                        ViewBag.locationName4 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 5:
                        listData = chartAmountData5;
                        ViewBag.locationName5 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 6:
                        listData = chartAmountData6;
                        ViewBag.locationName6 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 7:
                        listData = chartAmountData7;
                        ViewBag.locationName7 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 8:
                        listData = chartAmountData8;
                        ViewBag.locationName8 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 9:
                        listData = chartAmountData9;
                        ViewBag.locationName9 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 10:
                        listData = chartAmountData10;
                        ViewBag.locationName10 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 11:
                        listData = chartAmountData11;
                        ViewBag.locationName11 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 12:
                        listData = chartAmountData12;
                        ViewBag.locationName12 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 13:
                        listData = chartAmountData13;
                        ViewBag.locationName13 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 14:
                        listData = chartAmountData14;
                        ViewBag.locationName14 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 15:
                        listData = chartAmountData15;
                        ViewBag.locationName15 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 16:
                        listData = chartAmountData16;
                        ViewBag.locationName16 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 17:
                        listData = chartAmountData17;
                        ViewBag.locationName17 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 18:
                        listData = chartAmountData18;
                        ViewBag.locationName18 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 19:
                        listData = chartAmountData19;
                        ViewBag.locationName19 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 20:
                        listData = chartAmountData20;
                        ViewBag.locationName20 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 21:
                        listData = chartAmountData21;
                        ViewBag.locationName21 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 22:
                        listData = chartAmountData22;
                        ViewBag.locationName22 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 23:
                        listData = chartAmountData23;
                        ViewBag.locationName23 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    default:
                        listData = chartAmountData24;
                        ViewBag.locationName24 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;
                }

                foreach (var item in items)
                {
                    decimal? totalHourAmount = 0;
                    var hour = item.Hour;
                    var amount = item.Amount;

                    foreach (var data in salesPeakHourItems)
                    {
                        totalHourAmount += data.Where(x => x.Hour == hour).Sum(x => x.Amount);
                    }

                    decimal? totalAmount = items.Sum(x => x.Amount);

                    var totalbranchPercent = totalAmount == 0 ? 0 : 100 / totalAmount * amount;
                    var totalHourPercent = totalHourAmount == 0 ? 0 : 100 / totalHourAmount * amount;

                    listData.Add(new ColumnChartData
                    {
                        hour = hour,
                        x = hour.ToString("hh:mm tt"),
                        y = amount,
                        text = $"{hour:hh:mm tt} : {amount.Value:0.000} ({totalHourPercent.Value:0.0}%) <br> Branch Total : {totalAmount.Value:0.000} ({totalbranchPercent.Value:0.0}%) <br> Hour Total : {totalHourAmount.Value:0.000}"
                    });
                }

                var fromHour = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 6, 0, 0);
                var toHour = fromHour.AddHours(23);
                for (var i = fromHour; i <= toHour;)
                {
                    if (!listData.Any(x => x.hour == i))
                    {
                        listData.Add(new ColumnChartData
                        {
                            hour = i,
                            x = i.ToString("hh:mm tt"),
                            y = 0,
                            text = $"{i:hh:mm tt} : {0:0.000} ({0:0.0}%) <br> Branch Total : {0:0.000} ({0:0.0}%) <br> Hour Total : {0:0.000}"
                        });
                    }

                    i = i.AddHours(1);
                }

                count += 1;
            }

            var fromHour2 = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 6, 0, 0);
            var toHour2 = fromHour2.AddHours(23);

            for (var i = fromHour2; i <= toHour2;)
            {
                var skip = false;

                if (chartAmountData1.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData2.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData3.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData4.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData5.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData6.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData7.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData8.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData9.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData10.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData11.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData12.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData12.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData14.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData15.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData16.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData17.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData18.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData19.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData20.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData21.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData22.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData23.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartAmountData24.Any(a => a.hour == i && a.y != 0)) skip = true;

                if (!skip)
                {
                    chartAmountData1.Remove(chartAmountData1.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData2.Remove(chartAmountData2.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData3.Remove(chartAmountData3.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData4.Remove(chartAmountData4.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData5.Remove(chartAmountData5.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData6.Remove(chartAmountData6.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData7.Remove(chartAmountData7.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData8.Remove(chartAmountData8.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData9.Remove(chartAmountData9.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData10.Remove(chartAmountData10.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData11.Remove(chartAmountData11.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData12.Remove(chartAmountData12.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData13.Remove(chartAmountData13.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData14.Remove(chartAmountData14.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData15.Remove(chartAmountData15.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData16.Remove(chartAmountData16.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData17.Remove(chartAmountData17.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData18.Remove(chartAmountData18.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData19.Remove(chartAmountData19.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData20.Remove(chartAmountData20.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData21.Remove(chartAmountData21.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData22.Remove(chartAmountData22.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData23.Remove(chartAmountData23.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartAmountData24.Remove(chartAmountData24.FirstOrDefault(a => a.hour == i && a.y == 0));
                }

                i = i.AddHours(1);
            }



            ViewBag.dataSource1 = chartAmountData1.OrderBy(x => x.hour);
            ViewBag.dataSource2 = chartAmountData2.OrderBy(x => x.hour);
            ViewBag.dataSource3 = chartAmountData3.OrderBy(x => x.hour);
            ViewBag.dataSource4 = chartAmountData4.OrderBy(x => x.hour);
            ViewBag.dataSource5 = chartAmountData5.OrderBy(x => x.hour);
            ViewBag.dataSource6 = chartAmountData6.OrderBy(x => x.hour);
            ViewBag.dataSource7 = chartAmountData7.OrderBy(x => x.hour);
            ViewBag.dataSource8 = chartAmountData8.OrderBy(x => x.hour);
            ViewBag.dataSource9 = chartAmountData9.OrderBy(x => x.hour);
            ViewBag.dataSource10 = chartAmountData10.OrderBy(x => x.hour);
            ViewBag.dataSource11 = chartAmountData11.OrderBy(x => x.hour);
            ViewBag.dataSource12 = chartAmountData12.OrderBy(x => x.hour);
            ViewBag.dataSource13 = chartAmountData13.OrderBy(x => x.hour);
            ViewBag.dataSource14 = chartAmountData14.OrderBy(x => x.hour);
            ViewBag.dataSource15 = chartAmountData15.OrderBy(x => x.hour);
            ViewBag.dataSource16 = chartAmountData16.OrderBy(x => x.hour);
            ViewBag.dataSource17 = chartAmountData17.OrderBy(x => x.hour);
            ViewBag.dataSource18 = chartAmountData18.OrderBy(x => x.hour);
            ViewBag.dataSource19 = chartAmountData19.OrderBy(x => x.hour);
            ViewBag.dataSource20 = chartAmountData20.OrderBy(x => x.hour);
            ViewBag.dataSource21 = chartAmountData21.OrderBy(x => x.hour);
            ViewBag.dataSource22 = chartAmountData22.OrderBy(x => x.hour);
            ViewBag.dataSource23 = chartAmountData23.OrderBy(x => x.hour);
            ViewBag.dataSource24 = chartAmountData24.OrderBy(x => x.hour);

            #endregion

            #region Chart Trans Count

            List<ColumnChartData> chartCountData1 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData2 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData3 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData4 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData5 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData6 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData7 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData8 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData9 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData10 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData11 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData12 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData13 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData14 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData15 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData16 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData17 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData18 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData19 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData20 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData21 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData22 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData23 = new List<ColumnChartData>();
            List<ColumnChartData> chartCountData24 = new List<ColumnChartData>();

            var voucherTypeString = "201,2021,2022,2025,2026";
            var salesPeakHourCountItems = _salesReportRepository.GetSalesHourlyReport(fromDate, toDate, locationsString, voucherTypeString).SalesPeakHourItems.GroupBy(x => x.LocationId);
            var transCount = 1;
            decimal totalChartCount = salesPeakHourCountItems.Sum(x => x.Sum(y => y.TransCount));
            ViewBag.TotalChartCount = totalChartCount;

            foreach (var items in salesPeakHourCountItems.OrderByDescending(x => x.Sum(y => y.TransCount)))
            {
                decimal branchCountTotal = items.Sum(x => x.TransCount);
                decimal branchCountPercentage = totalChartCount == 0 ? 0 : 100 / totalChartCount * branchCountTotal;
                var branchWiseCountTotal = new HourlySalesBranchCountTotal
                {
                    LocationName = locations.FirstOrDefault(x => x.LocationId == items.Key).Name,
                    CountInfo = $"{branchCountTotal:0.000} ({branchCountPercentage:0.0}%)"
                };

                branchWiseCountTotals.Add(branchWiseCountTotal);


                var listData = chartCountData1;

                switch (transCount)
                {
                    case 1:
                        listData = chartCountData1;
                        ViewBag.locationCountName1 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 2:
                        listData = chartCountData2;
                        ViewBag.locationCountName2 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 3:
                        listData = chartCountData3;
                        ViewBag.locationCountName3 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 4:
                        listData = chartCountData4;
                        ViewBag.locationCountName4 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 5:
                        listData = chartCountData5;
                        ViewBag.locationCountName5 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 6:
                        listData = chartCountData6;
                        ViewBag.locationCountName6 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 7:
                        listData = chartCountData7;
                        ViewBag.locationCountName7 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 8:
                        listData = chartCountData8;
                        ViewBag.locationCountName8 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 9:
                        listData = chartCountData9;
                        ViewBag.locationCountName9 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 10:
                        listData = chartCountData10;
                        ViewBag.locationCountName10 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 11:
                        listData = chartCountData11;
                        ViewBag.locationCountName11 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 12:
                        listData = chartCountData12;
                        ViewBag.locationCountName12 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 13:
                        listData = chartCountData13;
                        ViewBag.locationCountName13 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 14:
                        listData = chartCountData14;
                        ViewBag.locationCountName14 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 15:
                        listData = chartCountData15;
                        ViewBag.locationCountName15 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 16:
                        listData = chartCountData16;
                        ViewBag.locationCountName16 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 17:
                        listData = chartCountData17;
                        ViewBag.locationCountName17 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 18:
                        listData = chartCountData18;
                        ViewBag.locationCountName18 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 19:
                        listData = chartCountData19;
                        ViewBag.locationCountName19 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 20:
                        listData = chartCountData20;
                        ViewBag.locationCountName20 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 21:
                        listData = chartCountData21;
                        ViewBag.locationCountName21 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 22:
                        listData = chartCountData22;
                        ViewBag.locationCountName22 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 23:
                        listData = chartCountData23;
                        ViewBag.locationCountName23 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    default:
                        listData = chartCountData24;
                        ViewBag.locationCountName24 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;
                }

                foreach (var item in items)
                {
                    decimal totalHourTransCount = 0;
                    var hour = item.Hour;
                    decimal itemTransCount = item.TransCount;

                    foreach (var data in salesPeakHourItems)
                    {
                        totalHourTransCount += data.Where(x => x.Hour == hour).Sum(x => x.TransCount);
                    }

                    decimal totalTransCount = items.Sum(x => x.TransCount);

                    decimal totalbranchPercent = totalTransCount == 0 ? 0 : 100 / totalTransCount * totalHourTransCount;
                    decimal totalHourPercent = totalHourTransCount == 0 ? 0 : 100 / totalHourTransCount * itemTransCount;

                    listData.Add(new ColumnChartData { date = item.TransDate, hour = hour, x = item.Hour.ToString("hh:mm tt"), y = item.TransCount, text = $"{hour:hh:mm tt} : {itemTransCount} ({totalHourPercent:0.0}%) <br> Branch Total : {totalTransCount} ({totalbranchPercent:0.0}%) <br> Hour Total : {totalHourTransCount}" });
                }

                var fromHour = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 6, 0, 0);
                var toHour = fromHour.AddHours(23);
                for (var i = fromHour; i <= toHour;)
                {
                    if (!listData.Any(x => x.hour == i))
                    {
                        listData.Add(new ColumnChartData
                        {
                            hour = i,
                            x = i.ToString("hh:mm tt"),
                            y = 0,
                            text = $"{i:hh:mm tt} : {0:0.000} ({0:0.0}%) <br> Branch Total : {0:0.000} ({0:0.0}%) <br> Hour Total : {0:0.000}"
                        });
                    }

                    i = i.AddHours(1);
                }

                transCount += 1;
            }

            var fromHour3 = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 6, 0, 0);
            var toHour3 = fromHour3.AddHours(23);

            for (var i = fromHour3; i <= toHour3;)
            {
                var skip = false;

                if (chartCountData1.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData2.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData3.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData4.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData5.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData6.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData7.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData8.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData9.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData10.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData11.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData12.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData12.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData14.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData15.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData16.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData17.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData18.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData19.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData20.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData21.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData22.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData23.Any(a => a.hour == i && a.y != 0)) skip = true;
                if (chartCountData24.Any(a => a.hour == i && a.y != 0)) skip = true;

                if (!skip)
                {
                    chartCountData1.Remove(chartCountData1.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData2.Remove(chartCountData2.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData3.Remove(chartCountData3.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData4.Remove(chartCountData4.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData5.Remove(chartCountData5.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData6.Remove(chartCountData6.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData7.Remove(chartCountData7.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData8.Remove(chartCountData8.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData9.Remove(chartCountData9.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData10.Remove(chartCountData10.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData11.Remove(chartCountData11.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData12.Remove(chartCountData12.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData13.Remove(chartCountData13.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData14.Remove(chartCountData14.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData15.Remove(chartCountData15.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData16.Remove(chartCountData16.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData17.Remove(chartCountData17.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData18.Remove(chartCountData18.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData19.Remove(chartCountData19.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData20.Remove(chartCountData20.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData21.Remove(chartCountData21.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData22.Remove(chartCountData22.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData23.Remove(chartCountData23.FirstOrDefault(a => a.hour == i && a.y == 0));
                    chartCountData24.Remove(chartCountData24.FirstOrDefault(a => a.hour == i && a.y == 0));
                }

                i = i.AddHours(1);
            }

            ViewBag.dataCountSource1 = chartCountData1.OrderBy(x => x.hour);
            ViewBag.dataCountSource2 = chartCountData2.OrderBy(x => x.hour);
            ViewBag.dataCountSource3 = chartCountData3.OrderBy(x => x.hour);
            ViewBag.dataCountSource4 = chartCountData4.OrderBy(x => x.hour);
            ViewBag.dataCountSource5 = chartCountData5.OrderBy(x => x.hour);
            ViewBag.dataCountSource6 = chartCountData6.OrderBy(x => x.hour);
            ViewBag.dataCountSource7 = chartCountData7.OrderBy(x => x.hour);
            ViewBag.dataCountSource8 = chartCountData8.OrderBy(x => x.hour);
            ViewBag.dataCountSource9 = chartCountData9.OrderBy(x => x.hour);
            ViewBag.dataCountSource10 = chartCountData10.OrderBy(x => x.hour);
            ViewBag.dataCountSource11 = chartCountData11.OrderBy(x => x.hour);
            ViewBag.dataCountSource12 = chartCountData12.OrderBy(x => x.hour);
            ViewBag.dataCountSource13 = chartCountData13.OrderBy(x => x.hour);
            ViewBag.dataCountSource14 = chartCountData14.OrderBy(x => x.hour);
            ViewBag.dataCountSource15 = chartCountData15.OrderBy(x => x.hour);
            ViewBag.dataCountSource16 = chartCountData16.OrderBy(x => x.hour);
            ViewBag.dataCountSource17 = chartCountData17.OrderBy(x => x.hour);
            ViewBag.dataCountSource18 = chartCountData18.OrderBy(x => x.hour);
            ViewBag.dataCountSource19 = chartCountData19.OrderBy(x => x.hour);
            ViewBag.dataCountSource20 = chartCountData20.OrderBy(x => x.hour);
            ViewBag.dataCountSource21 = chartCountData21.OrderBy(x => x.hour);
            ViewBag.dataCountSource22 = chartCountData22.OrderBy(x => x.hour);
            ViewBag.dataCountSource23 = chartCountData23.OrderBy(x => x.hour);
            ViewBag.dataCountSource24 = chartCountData24.OrderBy(x => x.hour);

            #endregion


            ViewBag.BarChartAmountTitle = fromDate.Value.ToString("dd/MMM/yyyy HH:mm tt") + " to " + toDate.ToString("dd/MMM/yyyy HH:mm tt") + " by Amount";
            ViewBag.BarChartCountTitle = fromDate.Value.ToString("dd/MMM/yyyy HH:mm tt") + " to " + toDate.ToString("dd/MMM/yyyy HH:mm tt") + " by Transaction Count";

            ViewBag.BranchWiseTotals = branchWiseTotals;
            ViewBag.BranchWiseCountTotals = branchWiseCountTotals;

            //string[] color = new string[] { "#4286f4", "#f4b642", "#f441a9" };
            //ViewBag.seriesColors = color;

            return View();
        }

        #endregion

        #region Daily Consumption Report

        public ActionResult DailyConsumption(DateTime? fromDate, DateTime? toDate)
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

            if (toDate < fromDate) ViewBag.validation = "true";

            var result = _salesReportRepository.GetDailyConsumptionTrans(fromDate, toDate);
            ViewBag.DataSource = result.OrderByDescending(x => x.TotalQty);

            return View();
        }

        #endregion

        #region Column Chooser
        public bool SaveColumnChooser(List<ColumnChooserItem> columnChooserItems) =>
          _baseRepository.SaveColumnChooser(columnChooserItems, PageName, User.Identity.GetUserId());

        private void SetSalesDetailsColChooserVal()
        {
            ViewBag.Location = true;
            ViewBag.InvDateTime = true;
            ViewBag.Salesman = true;
            ViewBag.InvoiceNumber = true;
            ViewBag.CustomerName = true;
            ViewBag.CustomerNameAr = true;
            ViewBag.Voucher = true;
            ViewBag.ProductNameEn = true;
            ViewBag.ProductNameAr = true;
            ViewBag.BaseQuantity = true;
            ViewBag.BaseUnit = true;
            ViewBag.SellQuantity = true;
            ViewBag.SellUnit = true;
            ViewBag.Discount = true;
            ViewBag.Amount = true;

            var jsonPath = Server.MapPath("~/App_Data/column_chooser.json");

            using (var fileStream = new FileStream(jsonPath, FileMode.Open))
            {
                var streamReader = new StreamReader(fileStream, Encoding.UTF8);

                var columnChooserFile = streamReader.ReadToEnd();
                var list = JsonConvert.DeserializeObject<List<ColumnChooserItem>>(columnChooserFile);
                if (list != null)
                {
                    var userId = User.Identity.GetUserId();
                    foreach (var listItem in list)
                    {
                        if (listItem.PageName == PageName && listItem.UserId == userId)
                        {
                            switch (listItem.FieldName)
                            {
                                case "Location":
                                    ViewBag.Location = bool.Parse(listItem.FieldValue);
                                    break;
                                case "InvDateTime":
                                    ViewBag.InvDateTime = bool.Parse(listItem.FieldValue);
                                    break;
                                case "Salesman":
                                    ViewBag.Salesman = bool.Parse(listItem.FieldValue);
                                    break;
                                case "InvoiceNumber":
                                    ViewBag.InvoiceNumber = bool.Parse(listItem.FieldValue);
                                    break;
                                case "CustomerName":
                                    ViewBag.CustomerName = bool.Parse(listItem.FieldValue);
                                    break;
                                case "CustomerNameAr":
                                    ViewBag.CustomerNameAr = bool.Parse(listItem.FieldValue);
                                    break;
                                case "Voucher":
                                    ViewBag.Voucher = bool.Parse(listItem.FieldValue);
                                    break;
                                case "ProductNameEn":
                                    ViewBag.ProductNameEn = bool.Parse(listItem.FieldValue);
                                    break;
                                case "ProductNameAr":
                                    ViewBag.ProductNameAr = bool.Parse(listItem.FieldValue);
                                    break;
                                case "BaseQuantity":
                                    ViewBag.BaseQuantity = bool.Parse(listItem.FieldValue);
                                    break;
                                case "BaseUnit":
                                    ViewBag.BaseUnit = bool.Parse(listItem.FieldValue);
                                    break;
                                case "SellQuantity":
                                    ViewBag.SellQuantity = bool.Parse(listItem.FieldValue);
                                    break;
                                case "SellUnit":
                                    ViewBag.SellUnit = bool.Parse(listItem.FieldValue);
                                    break;
                                case "Discount":
                                    ViewBag.Discount = bool.Parse(listItem.FieldValue);
                                    break;
                                case "Amount":
                                    ViewBag.Amount = bool.Parse(listItem.FieldValue);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

                streamReader.Close();
                fileStream.Close();
            }
        }

        #endregion

        #region Sales Trend

        // int trendType, int months, int reportType, int groupWise
        [Authorize(Roles = "Admin,Sales, StaffAdmin")]
        public ActionResult SalesTrend(int? trendType, int? trendYear, int? trendMonth, int? reportType, string[] locations,
            DateTime? fromDate, DateTime? toDate, string[] products, string[] productsAr, string[] groups)
        {
            ViewBag.From = fromDate.HasValue ? fromDate.Value.ToString("dd/MMM/yyyy hh:mm tt") : "";
            ViewBag.To = toDate.HasValue ? toDate.Value.ToString("dd/MMM/yyyy hh:mm tt") : "";
            ViewBag.TrendType = trendType;
            ViewBag.Trends = new List<Trends>
            {
                new Trends
                {
                    Id= 1,
                    NameEn = "Yearly",
                    NameAr = "Yearly"
                },
                new Trends
                {
                    Id= 2,
                    NameEn = "Month & Year",
                    NameAr = "Month & Year"
                },
                new Trends
                {
                    Id= 3,
                    NameEn = "Monthly",
                    NameAr = "Monthly"
                },
                new Trends
                {
                    Id= 4,
                    NameEn = "Weekly",
                    NameAr = "Weekly"
                }
            };

            ViewBag.ReportType = new List<SalesReportType>
            {
                new SalesReportType
                {
                    Id= 1,
                    Name = "Location",
                    NameAr = "Location"
                },
                new SalesReportType
                {
                    Id= 2,
                    Name = "Item",
                    NameAr = "Item"
                },
                new SalesReportType
                {
                    Id= 3,
                    Name = "Group",
                    NameAr = "Group"
                }
            };
            ViewBag.ReportTypeVal = reportType;
            var dbLocations = _locationRepository.GetLocations().LocationItems.Where(x => x.Type != Repository.LocationRepository.LocationType.HO);
            

            var locationArray = new List<int>();
            if (locations != null)
            {
                locationArray.AddRange(from item in locations select int.Parse(item));
                foreach (var currentLocation in
                    from location in dbLocations
                    from id in locationArray
                    where location.LocationId == id
                    select location)
                {
                    currentLocation.IsSelected = true;
                }
            }

            ViewBag.Locations = dbLocations;
            ViewBag.locationVal = new string[] { "" };

            var dbProducts = _productRepository.GetAllProducts().Items.OrderBy(x => x.Name);
            ViewBag.products = dbProducts;

            var dbGroups = _productRepository.GetItemGroups().OrderBy(x => x.ParentGroupId);
            ViewBag.ItemGroups = dbGroups;

            ViewBag.Years = new List<SalesMonthsYears>
            {
                new SalesMonthsYears
                {
                    Name = "2015",
                    Year = 2015
                },
                new SalesMonthsYears
                {
                    Name = "2016",
                    Year = 2016
                },
                new SalesMonthsYears
                {
                    Name = "2017",
                    Year = 2017
                },
                new SalesMonthsYears
                {
                    Name = "2018",
                    Year = 2018
                },
                new SalesMonthsYears
                {
                    Name = "2019",
                    Year = 2019
                },
                new SalesMonthsYears
                {
                    Name = "2020",
                    Year = 2020
                },
                new SalesMonthsYears
                {
                    Name = "2021",
                    Year = 2021
                }
            };
            ViewBag.TrendYear = trendYear;

            ViewBag.Months = new List<SalesMonthsYears>
            {
                new SalesMonthsYears
                {
                    Name = "January",
                    MonthNumber = 1
                },
                new SalesMonthsYears
                {
                    Name = "February",
                    MonthNumber = 2
                },
                new SalesMonthsYears
                {
                    Name = "March",
                    MonthNumber = 3
                },
                new SalesMonthsYears
                {
                    Name = "April",
                    MonthNumber = 4
                },
                new SalesMonthsYears
                {
                    Name = "May",
                    MonthNumber = 5
                },
                new SalesMonthsYears
                {
                    Name = "June",
                    MonthNumber = 6
                },
                new SalesMonthsYears
                {
                    Name = "July",
                    MonthNumber = 7
                },
                new SalesMonthsYears
                {
                    Name = "August",
                    MonthNumber = 8
                },
                new SalesMonthsYears
                {
                    Name = "September",
                    MonthNumber = 9
                },
                new SalesMonthsYears
                {
                    Name = "October",
                    MonthNumber = 10
                },
                new SalesMonthsYears
                {
                    Name = "November",
                    MonthNumber = 11
                },
                new SalesMonthsYears
                {
                    Name = "December",
                    MonthNumber = 12
                }
            };
            ViewBag.TrendMonth = trendMonth;

            if (trendType != null)
            {
                switch (trendType)
                {
                    // Yearly
                    case 1:
                        {
                            if (reportType != null)
                            {
                                // Location
                                if (reportType == 1)
                                {
                                    var locationsString = locations != null && locations.Length > 0
                                        ? string.Join(",", locations)
                                        : "";

                                    GetYearlyReportLocationWise(locationsString, dbLocations);

                                }
                                // Item
                                else if (reportType == 2)
                                {
                                    if (products != null && products.Length > 0)
                                    {
                                        var productString = string.Join(",", products);
                                        GetYearlyReportItemWise(productString, dbProducts);
                                    }
                                    else if (productsAr != null && productsAr.Length > 0)
                                    {
                                        var productString = string.Join(",", productsAr);
                                        GetYearlyReportItemWise(productString, dbProducts);
                                    }
                                }
                                // Group
                                else if (reportType == 3)
                                {
                                    if (groups != null && groups.Length > 0)
                                    {
                                        GetYearlyReportGroupWise(groups, dbGroups, dbProducts);
                                    }
                                }
                            }
                            break;
                        }

                    // Month & Year
                    case 2:
                        {
                            if (reportType != null)
                            {
                                // Location
                                if (reportType == 1)
                                {
                                    var locationsString = locations != null && locations.Length > 0
                                        ? string.Join(",", locations)
                                        : "";

                                    if (trendMonth.HasValue)
                                    {
                                        GetMonthYearReportLocationWise(locationsString, trendMonth.Value, dbLocations);
                                    }
                                    else
                                    {
                                        ViewBag.Validation = "Invalid Year!";
                                        return View();
                                    }
                                }
                                // Item
                                else if (reportType == 2)
                                {
                                    if (trendMonth.HasValue)
                                    {
                                        if (products != null && products.Length > 0)
                                        {
                                            var productString = string.Join(",", products);
                                            GetMonthYearReportItemWise(productString, trendMonth.Value, dbProducts);
                                        }
                                        else if (productsAr != null && productsAr.Length > 0)
                                        {
                                            var productString = string.Join(",", productsAr);
                                            GetMonthYearReportItemWise(productString, trendMonth.Value, dbProducts);
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.Validation = "Invalid Year!";
                                        return View();
                                    }
                                    
                                }
                                // Group
                                else if (reportType == 3)
                                {

                                }
                            }
                            break;
                        }

                    // Monthly
                    case 3:
                        {
                            if (reportType != null)
                            {
                                // Location
                                if (reportType == 1)
                                {
                                    var locationsString = locations != null && locations.Length > 0
                                        ? string.Join(",", locations)
                                        : "";

                                    if (trendYear.HasValue)
                                    {
                                        GetMonthlyReportLocationWise(locationsString, trendYear.Value, dbLocations);
                                    }
                                    else
                                    {
                                        ViewBag.Validation = "Invalid Year!";
                                        return View();
                                    }
                                }

                                // Item
                                else if (reportType == 2)
                                {
                                    if (products != null && products.Length > 0)
                                    {
                                        var productString = string.Join(",", products);
                                        GetMonthlyReportItemWise(productString, trendYear.Value, dbProducts);
                                    }
                                    else if (productsAr != null && productsAr.Length > 0)
                                    {
                                        var productString = string.Join(",", productsAr);
                                        GetMonthlyReportItemWise(productString, trendYear.Value, dbProducts);
                                    }
                                }

                                // Group
                                else if (reportType == 3)
                                {
                                    if (groups != null && groups.Length > 0)
                                    {
                                        GetMonthlyReportGroupWise(groups, trendYear.Value, dbGroups, dbProducts);
                                    }
                                }
                            }
                            break;
                        }

                    // Weekly
                    case 4:
                        {
                            if (reportType != null)
                            {
                                // Location
                                if (reportType == 1)
                                {
                                    var locationsString = locations != null && locations.Length > 0
                                        ? string.Join(",", locations)
                                        : "";
                                    GetWeeklyReportLocationWise(locationsString, fromDate, toDate, dbLocations);
                                }
                                // Item
                                else if (reportType == 2)
                                {
                                    if (products != null && products.Length > 0)
                                    {
                                        var productString = string.Join(",", products);
                                        GetWeeklyReportItemWise(productString, fromDate, toDate, dbProducts);
                                    }
                                    else if (productsAr != null && productsAr.Length > 0)
                                    {
                                        var productString = string.Join(",", productsAr);
                                        GetWeeklyReportItemWise(productString, fromDate, toDate, dbProducts);
                                    }
                                }
                                // Group
                                else if (reportType == 3)
                                {
                                    if (groups != null && groups.Length > 0)
                                    {
                                        GetWeeklyReportGroupWise(groups, fromDate, toDate, dbGroups, dbProducts);
                                    }
                                }
                            }
                            break;
                        }
                }
            }

            return View();
        }

        private void GetMonthlyReportGroupWise(string[] groupsArray, int year, IOrderedEnumerable<ItemGroup> dbGroups, IOrderedEnumerable<Item> products)
        {
            ViewBag.MonthlyReportGroupWise = true;
            var productArray = new List<string>();
            foreach (var groupString in groupsArray)
            {
                var groupId = int.Parse(groupString);
                var prodIds = products.Where(x => x.GroupCd == groupId).Select(x => x.ProductId);
                productArray.AddRange(from id in prodIds
                                      select id.ToString());
            }

            var productString = string.Join(",", productArray.ToArray());


            ViewBag.MonthlyReportItemWise = true;

            var report = _salesReportRepository.GetMonthlyReportItemWise(productString, year).SalesLocationTrendsItems.GroupBy(x => x.GroupCd);

            var count = 1;
            List<ColumnChartData> chartData1 = new List<ColumnChartData>();
            List<ColumnChartData> chartData2 = new List<ColumnChartData>();
            List<ColumnChartData> chartData3 = new List<ColumnChartData>();
            List<ColumnChartData> chartData4 = new List<ColumnChartData>();
            List<ColumnChartData> chartData5 = new List<ColumnChartData>();
            List<ColumnChartData> chartData6 = new List<ColumnChartData>();
            List<ColumnChartData> chartData7 = new List<ColumnChartData>();
            List<ColumnChartData> chartData8 = new List<ColumnChartData>();
            List<ColumnChartData> chartData9 = new List<ColumnChartData>();
            List<ColumnChartData> chartData10 = new List<ColumnChartData>();
            List<ColumnChartData> chartData11 = new List<ColumnChartData>();
            List<ColumnChartData> chartData12 = new List<ColumnChartData>();
            List<ColumnChartData> chartData13 = new List<ColumnChartData>();
            List<ColumnChartData> chartData14 = new List<ColumnChartData>();
            List<ColumnChartData> chartData15 = new List<ColumnChartData>();
            List<ColumnChartData> chartData16 = new List<ColumnChartData>();
            List<ColumnChartData> chartData17 = new List<ColumnChartData>();
            List<ColumnChartData> chartData18 = new List<ColumnChartData>();
            List<ColumnChartData> chartData19 = new List<ColumnChartData>();
            List<ColumnChartData> chartData20 = new List<ColumnChartData>();
            List<ColumnChartData> chartData21 = new List<ColumnChartData>();
            List<ColumnChartData> chartData22 = new List<ColumnChartData>();
            List<ColumnChartData> chartData23 = new List<ColumnChartData>();
            List<ColumnChartData> chartData24 = new List<ColumnChartData>();
            List<ColumnChartData> chartData25 = new List<ColumnChartData>();

            foreach (var items in report)
            {

                var listData = chartData1;
                switch (count)
                {
                    case 1:
                        listData = chartData1;
                        ViewBag.locationName1 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 2:
                        listData = chartData2;
                        ViewBag.locationName2 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 3:
                        listData = chartData3;
                        ViewBag.locationName3 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 4:
                        listData = chartData4;
                        ViewBag.locationName4 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 5:
                        listData = chartData5;
                        ViewBag.locationName5 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 6:
                        listData = chartData6;
                        ViewBag.locationName6 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 7:
                        listData = chartData7;
                        ViewBag.locationName7 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 8:
                        listData = chartData8;
                        ViewBag.locationName8 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 9:
                        listData = chartData9;
                        ViewBag.locationName9 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 10:
                        listData = chartData10;
                        ViewBag.locationName10 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 11:
                        listData = chartData11;
                        ViewBag.locationName11 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    default:
                        listData = chartData12;
                        ViewBag.locationName12 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;
                }

                foreach (var item in items)
                {
                    decimal? totalMonthAmount = 0;
                    var month = item.Month;
                    var amount = item.Amount;

                    foreach (var data in report)
                    {
                        totalMonthAmount += data.Where(x => x.Month == month).Sum(x => x.Amount);
                    }

                    decimal? totalAmount = items.Sum(x => x.Amount);

                    var totalbranchPercent = totalAmount == 0 ? 0 : 100 / totalAmount * amount;
                    var totalMonthPercent = totalMonthAmount == 0 ? 0 : 100 / totalMonthAmount * amount;

                    listData.Add(new ColumnChartData
                    {
                        MonthNumber = item.MonthNumber,
                        x = month,
                        y = amount,
                        text = $"{month} : {amount.Value:0.000} ({totalMonthPercent.Value:0.0}%) <br> Group Total : {totalAmount.Value:0.000} ({totalbranchPercent.Value:0.0}%) <br> Month Total : {totalMonthAmount.Value:0.000}"
                    });
                }


                count += 1;

            }

            ViewBag.dataSource1 = chartData1;
            ViewBag.dataSource2 = chartData2;
            ViewBag.dataSource3 = chartData3;
            ViewBag.dataSource4 = chartData4;
            ViewBag.dataSource5 = chartData5;
            ViewBag.dataSource6 = chartData6;
            ViewBag.dataSource7 = chartData7;
            ViewBag.dataSource8 = chartData8;
            ViewBag.dataSource9 = chartData9;
            ViewBag.dataSource10 = chartData10;
            ViewBag.dataSource11 = chartData11;
            ViewBag.dataSource12 = chartData12;
            ViewBag.dataSource13 = chartData13;
            ViewBag.dataSource14 = chartData14;
            ViewBag.dataSource15 = chartData15;
            ViewBag.dataSource16 = chartData16;
            ViewBag.dataSource17 = chartData17;
            ViewBag.dataSource18 = chartData18;
            ViewBag.dataSource19 = chartData19;
            ViewBag.dataSource20 = chartData20;
            ViewBag.dataSource21 = chartData21;
            ViewBag.dataSource22 = chartData22;
            ViewBag.dataSource23 = chartData23;
            ViewBag.dataSource24 = chartData24;
            ViewBag.dataSource25 = chartData25;
        }

        private void GetWeeklyReportGroupWise(string[] groupsArray, DateTime? fromDate, DateTime? toDate, IOrderedEnumerable<ItemGroup> dbGroups, IOrderedEnumerable<Item> products)
        {
            ViewBag.WeeklyReportGroupWise = true;
            var productArray = new List<string>();
            foreach (var groupString in groupsArray)
            {
                var groupId = int.Parse(groupString);
                var prodIds = products.Where(x => x.GroupCd == groupId).Select(x => x.ProductId);
                productArray.AddRange(from id in prodIds
                                      select id.ToString());
            }

            var productString = string.Join(",", productArray.ToArray());


            var report = _salesReportRepository.GetWeeklyReportItemWise(productString, fromDate, toDate).SalesLocationTrendsItems.GroupBy(x => x.GroupCd);

            var count = 1;
            List<ColumnChartData> chartData1 = new List<ColumnChartData>();
            List<ColumnChartData> chartData2 = new List<ColumnChartData>();
            List<ColumnChartData> chartData3 = new List<ColumnChartData>();
            List<ColumnChartData> chartData4 = new List<ColumnChartData>();
            List<ColumnChartData> chartData5 = new List<ColumnChartData>();
            List<ColumnChartData> chartData6 = new List<ColumnChartData>();
            List<ColumnChartData> chartData7 = new List<ColumnChartData>();
            List<ColumnChartData> chartData8 = new List<ColumnChartData>();
            List<ColumnChartData> chartData9 = new List<ColumnChartData>();
            List<ColumnChartData> chartData10 = new List<ColumnChartData>();
            List<ColumnChartData> chartData11 = new List<ColumnChartData>();
            List<ColumnChartData> chartData12 = new List<ColumnChartData>();
            List<ColumnChartData> chartData13 = new List<ColumnChartData>();
            List<ColumnChartData> chartData14 = new List<ColumnChartData>();
            List<ColumnChartData> chartData15 = new List<ColumnChartData>();
            List<ColumnChartData> chartData16 = new List<ColumnChartData>();
            List<ColumnChartData> chartData17 = new List<ColumnChartData>();
            List<ColumnChartData> chartData18 = new List<ColumnChartData>();
            List<ColumnChartData> chartData19 = new List<ColumnChartData>();
            List<ColumnChartData> chartData20 = new List<ColumnChartData>();
            List<ColumnChartData> chartData21 = new List<ColumnChartData>();
            List<ColumnChartData> chartData22 = new List<ColumnChartData>();
            List<ColumnChartData> chartData23 = new List<ColumnChartData>();
            List<ColumnChartData> chartData24 = new List<ColumnChartData>();

            foreach (var items in report)
            {

                var listData = chartData1;
                switch (count)
                {
                    case 1:
                        listData = chartData1;
                        ViewBag.locationName1 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 2:
                        listData = chartData2;
                        ViewBag.locationName2 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 3:
                        listData = chartData3;
                        ViewBag.locationName3 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 4:
                        listData = chartData4;
                        ViewBag.locationName4 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 5:
                        listData = chartData5;
                        ViewBag.locationName5 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 6:
                        listData = chartData6;
                        ViewBag.locationName6 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 7:
                        listData = chartData7;
                        ViewBag.locationName7 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 8:
                        listData = chartData8;
                        ViewBag.locationName8 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 9:
                        listData = chartData9;
                        ViewBag.locationName9 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 10:
                        listData = chartData10;
                        ViewBag.locationName10 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 11:
                        listData = chartData11;
                        ViewBag.locationName11 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    default:
                        listData = chartData12;
                        ViewBag.locationName12 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;
                }

                foreach (var item in items)
                {
                    decimal? totalWeekAmount = 0;
                    var week = item.Week;
                    var amount = item.Amount;

                    foreach (var data in report)
                    {
                        totalWeekAmount += data.Where(x => x.WeekStartDate >= item.WeekStartDate && x.WeekEndDate <= item.WeekEndDate).Sum(x => x.Amount);
                    }

                    decimal? totalAmount = items.Sum(x => x.Amount);

                    var totalbranchPercent = totalAmount == 0 ? 0 : 100 / totalAmount * amount;
                    var totalWeekPercent = totalWeekAmount == 0 ? 0 : 100 / totalWeekAmount * amount;

                    listData.Add(new ColumnChartData
                    {
                        x = $"W-{week}",
                        y = amount,
                        text = $"W-{week} {item.WeekText}: {amount.Value:0.000} ({totalWeekPercent.Value:0.0}%) <br> Group Total : {totalAmount.Value:0.000} ({totalbranchPercent.Value:0.0}%) <br> Week Total : {totalWeekAmount.Value:0.000}"
                    });
                }

                count += 1;

            }

            ViewBag.dataSource1 = chartData1;
            ViewBag.dataSource2 = chartData2;
            ViewBag.dataSource3 = chartData3;
            ViewBag.dataSource4 = chartData4;
            ViewBag.dataSource5 = chartData5;
            ViewBag.dataSource6 = chartData6;
            ViewBag.dataSource7 = chartData7;
            ViewBag.dataSource8 = chartData8;
            ViewBag.dataSource9 = chartData9;
            ViewBag.dataSource10 = chartData10;
            ViewBag.dataSource11 = chartData11;
            ViewBag.dataSource12 = chartData12;
            ViewBag.dataSource13 = chartData13;
            ViewBag.dataSource14 = chartData14;
            ViewBag.dataSource15 = chartData15;
            ViewBag.dataSource16 = chartData16;
            ViewBag.dataSource17 = chartData17;
            ViewBag.dataSource18 = chartData18;
            ViewBag.dataSource19 = chartData19;
            ViewBag.dataSource20 = chartData20;
            ViewBag.dataSource21 = chartData21;
            ViewBag.dataSource22 = chartData22;
            ViewBag.dataSource23 = chartData23;
            ViewBag.dataSource24 = chartData24;
        }

        private void GetYearlyReportGroupWise(string[] groupsArray, IOrderedEnumerable<ItemGroup> dbGroups, IOrderedEnumerable<Item> products)
        {
            ViewBag.YearlyReportGroupWise = true;

            var productArray = new List<string>();
            foreach (var groupString in groupsArray)
            {
                var groupId = int.Parse(groupString);
                var prodIds = products.Where(x => x.GroupCd == groupId).Select(x => x.ProductId);
                productArray.AddRange(from id in prodIds
                                      select id.ToString());
            }

            var productString = string.Join(",", productArray.ToArray());


            var report = _salesReportRepository.GetYearlyReportItemWise(productString).SalesLocationTrendsItems.GroupBy(x => x.GroupCd);

            var count = 1;
            List<ColumnChartData> chartData1 = new List<ColumnChartData>();
            List<ColumnChartData> chartData2 = new List<ColumnChartData>();
            List<ColumnChartData> chartData3 = new List<ColumnChartData>();
            List<ColumnChartData> chartData4 = new List<ColumnChartData>();
            List<ColumnChartData> chartData5 = new List<ColumnChartData>();
            List<ColumnChartData> chartData6 = new List<ColumnChartData>();
            List<ColumnChartData> chartData7 = new List<ColumnChartData>();
            List<ColumnChartData> chartData8 = new List<ColumnChartData>();
            List<ColumnChartData> chartData9 = new List<ColumnChartData>();
            List<ColumnChartData> chartData10 = new List<ColumnChartData>();
            List<ColumnChartData> chartData11 = new List<ColumnChartData>();
            List<ColumnChartData> chartData12 = new List<ColumnChartData>();
            List<ColumnChartData> chartData13 = new List<ColumnChartData>();
            List<ColumnChartData> chartData14 = new List<ColumnChartData>();
            List<ColumnChartData> chartData15 = new List<ColumnChartData>();
            List<ColumnChartData> chartData16 = new List<ColumnChartData>();
            List<ColumnChartData> chartData17 = new List<ColumnChartData>();
            List<ColumnChartData> chartData18 = new List<ColumnChartData>();
            List<ColumnChartData> chartData19 = new List<ColumnChartData>();
            List<ColumnChartData> chartData20 = new List<ColumnChartData>();
            List<ColumnChartData> chartData21 = new List<ColumnChartData>();
            List<ColumnChartData> chartData22 = new List<ColumnChartData>();
            List<ColumnChartData> chartData23 = new List<ColumnChartData>();
            List<ColumnChartData> chartData24 = new List<ColumnChartData>();

            foreach (var items in report)
            {

                var listData = chartData1;
                switch (count)
                {
                    case 1:
                        listData = chartData1;
                        ViewBag.locationName1 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 2:
                        listData = chartData2;
                        ViewBag.locationName2 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 3:
                        listData = chartData3;
                        ViewBag.locationName3 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 4:
                        listData = chartData4;
                        ViewBag.locationName4 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 5:
                        listData = chartData5;
                        ViewBag.locationName5 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 6:
                        listData = chartData6;
                        ViewBag.locationName6 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 7:
                        listData = chartData7;
                        ViewBag.locationName7 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 8:
                        listData = chartData8;
                        ViewBag.locationName8 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 9:
                        listData = chartData9;
                        ViewBag.locationName9 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 10:
                        listData = chartData10;
                        ViewBag.locationName10 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    case 11:
                        listData = chartData11;
                        ViewBag.locationName11 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;

                    default:
                        listData = chartData12;
                        ViewBag.locationName12 = dbGroups.FirstOrDefault(x => x.ItemGroupId == items.Key).Name;
                        break;
                }

                foreach (var item in items)
                {
                    decimal? totalYearAmount = 0;
                    var year = item.Year;
                    var amount = item.Amount;

                    foreach (var data in report)
                    {
                        totalYearAmount += data.Where(x => x.Year == year).Sum(x => x.Amount);
                    }

                    decimal? totalAmount = items.Sum(x => x.Amount);

                    var totalbranchPercent = totalAmount == 0 ? 0 : 100 / totalAmount * amount;
                    var totalYearPercent = totalYearAmount == 0 ? 0 : 100 / totalYearAmount * amount;

                    listData.Add(new ColumnChartData
                    {
                        MonthNumber = item.MonthNumber,
                        x = year.ToString(),
                        y = amount,
                        text = $"{year} : {amount.Value:0.000} ({totalYearPercent.Value:0.0}%) <br> Group Total : {totalAmount.Value:0.000} ({totalbranchPercent.Value:0.0}%) <br> Year Total : {totalYearAmount.Value:0.000}"
                    });
                }

                count += 1;

            }

            ViewBag.dataSource1 = chartData1;
            ViewBag.dataSource2 = chartData2;
            ViewBag.dataSource3 = chartData3;
            ViewBag.dataSource4 = chartData4;
            ViewBag.dataSource5 = chartData5;
            ViewBag.dataSource6 = chartData6;
            ViewBag.dataSource7 = chartData7;
            ViewBag.dataSource8 = chartData8;
            ViewBag.dataSource9 = chartData9;
            ViewBag.dataSource10 = chartData10;
            ViewBag.dataSource11 = chartData11;
            ViewBag.dataSource12 = chartData12;
            ViewBag.dataSource13 = chartData13;
            ViewBag.dataSource14 = chartData14;
            ViewBag.dataSource15 = chartData15;
            ViewBag.dataSource16 = chartData16;
            ViewBag.dataSource17 = chartData17;
            ViewBag.dataSource18 = chartData18;
            ViewBag.dataSource19 = chartData19;
            ViewBag.dataSource20 = chartData20;
            ViewBag.dataSource21 = chartData21;
            ViewBag.dataSource22 = chartData22;
            ViewBag.dataSource23 = chartData23;
            ViewBag.dataSource24 = chartData24;
        }

        private void GetMonthYearReportItemWise(string productString, int month, IOrderedEnumerable<Item> products)
        {
            ViewBag.MonthYearReportLocationWise = true;
            var report = _salesReportRepository.GetMonthYearReportItemWise(productString, month).SalesLocationTrendsItems.GroupBy(x => x.ProductId);
            var monthName = new DateTime(2020, month, 1).ToString("MMM");

            var count = 1;
            List<ColumnChartData> chartData1 = new List<ColumnChartData>();
            List<ColumnChartData> chartData2 = new List<ColumnChartData>();
            List<ColumnChartData> chartData3 = new List<ColumnChartData>();
            List<ColumnChartData> chartData4 = new List<ColumnChartData>();
            List<ColumnChartData> chartData5 = new List<ColumnChartData>();
            List<ColumnChartData> chartData6 = new List<ColumnChartData>();
            List<ColumnChartData> chartData7 = new List<ColumnChartData>();
            List<ColumnChartData> chartData8 = new List<ColumnChartData>();
            List<ColumnChartData> chartData9 = new List<ColumnChartData>();
            List<ColumnChartData> chartData10 = new List<ColumnChartData>();
            List<ColumnChartData> chartData11 = new List<ColumnChartData>();
            List<ColumnChartData> chartData12 = new List<ColumnChartData>();
            List<ColumnChartData> chartData13 = new List<ColumnChartData>();
            List<ColumnChartData> chartData14 = new List<ColumnChartData>();
            List<ColumnChartData> chartData15 = new List<ColumnChartData>();
            List<ColumnChartData> chartData16 = new List<ColumnChartData>();
            List<ColumnChartData> chartData17 = new List<ColumnChartData>();
            List<ColumnChartData> chartData18 = new List<ColumnChartData>();
            List<ColumnChartData> chartData19 = new List<ColumnChartData>();
            List<ColumnChartData> chartData20 = new List<ColumnChartData>();
            List<ColumnChartData> chartData21 = new List<ColumnChartData>();
            List<ColumnChartData> chartData22 = new List<ColumnChartData>();
            List<ColumnChartData> chartData23 = new List<ColumnChartData>();
            List<ColumnChartData> chartData24 = new List<ColumnChartData>();

            foreach (var items in report)
            {

                var listData = chartData1;
                switch (count)
                {
                    case 1:
                        listData = chartData1;
                        ViewBag.locationName1 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 2:
                        listData = chartData2;
                        ViewBag.locationName2 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 3:
                        listData = chartData3;
                        ViewBag.locationName3 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 4:
                        listData = chartData4;
                        ViewBag.locationName4 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 5:
                        listData = chartData5;
                        ViewBag.locationName5 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 6:
                        listData = chartData6;
                        ViewBag.locationName6 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 7:
                        listData = chartData7;
                        ViewBag.locationName7 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 8:
                        listData = chartData8;
                        ViewBag.locationName8 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 9:
                        listData = chartData9;
                        ViewBag.locationName9 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 10:
                        listData = chartData10;
                        ViewBag.locationName10 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 11:
                        listData = chartData11;
                        ViewBag.locationName11 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    default:
                        listData = chartData12;
                        ViewBag.locationName12 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;
                }

                foreach (var item in items)
                {
                    decimal? totalYearAmount = 0;
                    var year = item.Year;
                    var amount = item.Amount;

                    foreach (var data in report)
                    {
                        totalYearAmount += data.Where(x => x.Year == year).Sum(x => x.Amount);
                    }

                    decimal? totalAmount = items.Sum(x => x.Amount);

                    var totalbranchPercent = totalAmount == 0 ? 0 : 100 / totalAmount * amount;
                    var totalYearPercent = totalYearAmount == 0 ? 0 : 100 / totalYearAmount * amount;

                    listData.Add(new ColumnChartData
                    {
                        x = $"{monthName}-{year}",
                        y = amount,
                        text = $"{year} : {amount.Value:0.000} ({totalYearPercent.Value:0.0}%) <br> Item Total : {totalAmount.Value:0.000} ({totalbranchPercent.Value:0.0}%) <br> Year Total : {totalYearAmount.Value:0.000}"
                    });
                }

                count += 1;

            }

            ViewBag.dataSource1 = chartData1;
            ViewBag.dataSource2 = chartData2;
            ViewBag.dataSource3 = chartData3;
            ViewBag.dataSource4 = chartData4;
            ViewBag.dataSource5 = chartData5;
            ViewBag.dataSource6 = chartData6;
            ViewBag.dataSource7 = chartData7;
            ViewBag.dataSource8 = chartData8;
            ViewBag.dataSource9 = chartData9;
            ViewBag.dataSource10 = chartData10;
            ViewBag.dataSource11 = chartData11;
            ViewBag.dataSource12 = chartData12;
            ViewBag.dataSource13 = chartData13;
            ViewBag.dataSource14 = chartData14;
            ViewBag.dataSource15 = chartData15;
            ViewBag.dataSource16 = chartData16;
            ViewBag.dataSource17 = chartData17;
            ViewBag.dataSource18 = chartData18;
            ViewBag.dataSource19 = chartData19;
            ViewBag.dataSource20 = chartData20;
            ViewBag.dataSource21 = chartData21;
            ViewBag.dataSource22 = chartData22;
            ViewBag.dataSource23 = chartData23;
            ViewBag.dataSource24 = chartData24;
        }

        private void GetYearlyReportItemWise(string productString, IOrderedEnumerable<Item> products)
        {
            ViewBag.YearlyReportLocationWise = true;

            var report = _salesReportRepository.GetYearlyReportItemWise(productString).SalesLocationTrendsItems.GroupBy(x => x.ProductId);

            var count = 1;
            List<ColumnChartData> chartData1 = new List<ColumnChartData>();
            List<ColumnChartData> chartData2 = new List<ColumnChartData>();
            List<ColumnChartData> chartData3 = new List<ColumnChartData>();
            List<ColumnChartData> chartData4 = new List<ColumnChartData>();
            List<ColumnChartData> chartData5 = new List<ColumnChartData>();
            List<ColumnChartData> chartData6 = new List<ColumnChartData>();
            List<ColumnChartData> chartData7 = new List<ColumnChartData>();
            List<ColumnChartData> chartData8 = new List<ColumnChartData>();
            List<ColumnChartData> chartData9 = new List<ColumnChartData>();
            List<ColumnChartData> chartData10 = new List<ColumnChartData>();
            List<ColumnChartData> chartData11 = new List<ColumnChartData>();
            List<ColumnChartData> chartData12 = new List<ColumnChartData>();
            List<ColumnChartData> chartData13 = new List<ColumnChartData>();
            List<ColumnChartData> chartData14 = new List<ColumnChartData>();
            List<ColumnChartData> chartData15 = new List<ColumnChartData>();
            List<ColumnChartData> chartData16 = new List<ColumnChartData>();
            List<ColumnChartData> chartData17 = new List<ColumnChartData>();
            List<ColumnChartData> chartData18 = new List<ColumnChartData>();
            List<ColumnChartData> chartData19 = new List<ColumnChartData>();
            List<ColumnChartData> chartData20 = new List<ColumnChartData>();
            List<ColumnChartData> chartData21 = new List<ColumnChartData>();
            List<ColumnChartData> chartData22 = new List<ColumnChartData>();
            List<ColumnChartData> chartData23 = new List<ColumnChartData>();
            List<ColumnChartData> chartData24 = new List<ColumnChartData>();

            foreach (var items in report)
            {

                var listData = chartData1;
                switch (count)
                {
                    case 1:
                        listData = chartData1;
                        ViewBag.locationName1 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 2:
                        listData = chartData2;
                        ViewBag.locationName2 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 3:
                        listData = chartData3;
                        ViewBag.locationName3 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 4:
                        listData = chartData4;
                        ViewBag.locationName4 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 5:
                        listData = chartData5;
                        ViewBag.locationName5 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 6:
                        listData = chartData6;
                        ViewBag.locationName6 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 7:
                        listData = chartData7;
                        ViewBag.locationName7 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 8:
                        listData = chartData8;
                        ViewBag.locationName8 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 9:
                        listData = chartData9;
                        ViewBag.locationName9 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 10:
                        listData = chartData10;
                        ViewBag.locationName10 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 11:
                        listData = chartData11;
                        ViewBag.locationName11 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    default:
                        listData = chartData12;
                        ViewBag.locationName12 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;
                }

                foreach (var item in items)
                {
                    decimal? totalYearAmount = 0;
                    var year = item.Year;
                    var amount = item.Amount;

                    foreach (var data in report)
                    {
                        totalYearAmount += data.Where(x => x.Year == year).Sum(x => x.Amount);
                    }

                    decimal? totalAmount = items.Sum(x => x.Amount);

                    var totalbranchPercent = totalAmount == 0 ? 0 : 100 / totalAmount * amount;
                    var totalYearPercent = totalYearAmount == 0 ? 0 : 100 / totalYearAmount * amount;

                    listData.Add(new ColumnChartData
                    {
                        MonthNumber = item.MonthNumber,
                        x = year.ToString(),
                        y = amount,
                        text = $"{year} : {amount.Value:0.000} ({totalYearPercent.Value:0.0}%) <br> Item Total : {totalAmount.Value:0.000} ({totalbranchPercent.Value:0.0}%) <br> Year Total : {totalYearAmount.Value:0.000}"
                    });
                }

                count += 1;

            }

            ViewBag.dataSource1 = chartData1;
            ViewBag.dataSource2 = chartData2;
            ViewBag.dataSource3 = chartData3;
            ViewBag.dataSource4 = chartData4;
            ViewBag.dataSource5 = chartData5;
            ViewBag.dataSource6 = chartData6;
            ViewBag.dataSource7 = chartData7;
            ViewBag.dataSource8 = chartData8;
            ViewBag.dataSource9 = chartData9;
            ViewBag.dataSource10 = chartData10;
            ViewBag.dataSource11 = chartData11;
            ViewBag.dataSource12 = chartData12;
            ViewBag.dataSource13 = chartData13;
            ViewBag.dataSource14 = chartData14;
            ViewBag.dataSource15 = chartData15;
            ViewBag.dataSource16 = chartData16;
            ViewBag.dataSource17 = chartData17;
            ViewBag.dataSource18 = chartData18;
            ViewBag.dataSource19 = chartData19;
            ViewBag.dataSource20 = chartData20;
            ViewBag.dataSource21 = chartData21;
            ViewBag.dataSource22 = chartData22;
            ViewBag.dataSource23 = chartData23;
            ViewBag.dataSource24 = chartData24;
        }

        private void GetWeeklyReportItemWise(string productString, DateTime? fromDate, DateTime? toDate, IOrderedEnumerable<Item> products)
        {
            ViewBag.WeeklyReportItemWise = true;
            var report = _salesReportRepository.GetWeeklyReportItemWise(productString, fromDate, toDate).SalesLocationTrendsItems.GroupBy(x => x.ProductId);

            var count = 1;
            List<ColumnChartData> chartData1 = new List<ColumnChartData>();
            List<ColumnChartData> chartData2 = new List<ColumnChartData>();
            List<ColumnChartData> chartData3 = new List<ColumnChartData>();
            List<ColumnChartData> chartData4 = new List<ColumnChartData>();
            List<ColumnChartData> chartData5 = new List<ColumnChartData>();
            List<ColumnChartData> chartData6 = new List<ColumnChartData>();
            List<ColumnChartData> chartData7 = new List<ColumnChartData>();
            List<ColumnChartData> chartData8 = new List<ColumnChartData>();
            List<ColumnChartData> chartData9 = new List<ColumnChartData>();
            List<ColumnChartData> chartData10 = new List<ColumnChartData>();
            List<ColumnChartData> chartData11 = new List<ColumnChartData>();
            List<ColumnChartData> chartData12 = new List<ColumnChartData>();
            List<ColumnChartData> chartData13 = new List<ColumnChartData>();
            List<ColumnChartData> chartData14 = new List<ColumnChartData>();
            List<ColumnChartData> chartData15 = new List<ColumnChartData>();
            List<ColumnChartData> chartData16 = new List<ColumnChartData>();
            List<ColumnChartData> chartData17 = new List<ColumnChartData>();
            List<ColumnChartData> chartData18 = new List<ColumnChartData>();
            List<ColumnChartData> chartData19 = new List<ColumnChartData>();
            List<ColumnChartData> chartData20 = new List<ColumnChartData>();
            List<ColumnChartData> chartData21 = new List<ColumnChartData>();
            List<ColumnChartData> chartData22 = new List<ColumnChartData>();
            List<ColumnChartData> chartData23 = new List<ColumnChartData>();
            List<ColumnChartData> chartData24 = new List<ColumnChartData>();

            foreach (var items in report)
            {

                var listData = chartData1;
                switch (count)
                {
                    case 1:
                        listData = chartData1;
                        ViewBag.locationName1 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 2:
                        listData = chartData2;
                        ViewBag.locationName2 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 3:
                        listData = chartData3;
                        ViewBag.locationName3 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 4:
                        listData = chartData4;
                        ViewBag.locationName4 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 5:
                        listData = chartData5;
                        ViewBag.locationName5 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 6:
                        listData = chartData6;
                        ViewBag.locationName6 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 7:
                        listData = chartData7;
                        ViewBag.locationName7 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 8:
                        listData = chartData8;
                        ViewBag.locationName8 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 9:
                        listData = chartData9;
                        ViewBag.locationName9 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 10:
                        listData = chartData10;
                        ViewBag.locationName10 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 11:
                        listData = chartData11;
                        ViewBag.locationName11 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    default:
                        listData = chartData12;
                        ViewBag.locationName12 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;
                }

                foreach (var item in items)
                {
                    decimal? totalWeekAmount = 0;
                    var week = item.Week;
                    var amount = item.Amount;

                    foreach (var data in report)
                    {
                        totalWeekAmount += data.Where(x => x.WeekStartDate >= item.WeekStartDate && x.WeekEndDate <= item.WeekEndDate).Sum(x => x.Amount);
                    }

                    decimal? totalAmount = items.Sum(x => x.Amount);

                    var totalbranchPercent = totalAmount == 0 ? 0 : 100 / totalAmount * amount;
                    var totalWeekPercent = totalWeekAmount == 0 ? 0 : 100 / totalWeekAmount * amount;

                    listData.Add(new ColumnChartData
                    {
                        x = $"W-{week}",
                        y = amount,
                        text = $"W-{week} {item.WeekText}: {amount.Value:0.000} ({totalWeekPercent.Value:0.0}%) <br> Item Total : {totalAmount.Value:0.000} ({totalbranchPercent.Value:0.0}%) <br> Week Total : {totalWeekAmount.Value:0.000}"
                    });
                }

                count += 1;

            }

            ViewBag.dataSource1 = chartData1;
            ViewBag.dataSource2 = chartData2;
            ViewBag.dataSource3 = chartData3;
            ViewBag.dataSource4 = chartData4;
            ViewBag.dataSource5 = chartData5;
            ViewBag.dataSource6 = chartData6;
            ViewBag.dataSource7 = chartData7;
            ViewBag.dataSource8 = chartData8;
            ViewBag.dataSource9 = chartData9;
            ViewBag.dataSource10 = chartData10;
            ViewBag.dataSource11 = chartData11;
            ViewBag.dataSource12 = chartData12;
            ViewBag.dataSource13 = chartData13;
            ViewBag.dataSource14 = chartData14;
            ViewBag.dataSource15 = chartData15;
            ViewBag.dataSource16 = chartData16;
            ViewBag.dataSource17 = chartData17;
            ViewBag.dataSource18 = chartData18;
            ViewBag.dataSource19 = chartData19;
            ViewBag.dataSource20 = chartData20;
            ViewBag.dataSource21 = chartData21;
            ViewBag.dataSource22 = chartData22;
            ViewBag.dataSource23 = chartData23;
            ViewBag.dataSource24 = chartData24;
        }

        private void GetMonthlyReportItemWise(string productString, int year, IOrderedEnumerable<Item> products)
        {
            ViewBag.MonthlyReportItemWise = true;

            var report = _salesReportRepository.GetMonthlyReportItemWise(productString, year).SalesLocationTrendsItems.GroupBy(x => x.ProductId);

            var count = 1;
            List<ColumnChartData> chartData1 = new List<ColumnChartData>();
            List<ColumnChartData> chartData2 = new List<ColumnChartData>();
            List<ColumnChartData> chartData3 = new List<ColumnChartData>();
            List<ColumnChartData> chartData4 = new List<ColumnChartData>();
            List<ColumnChartData> chartData5 = new List<ColumnChartData>();
            List<ColumnChartData> chartData6 = new List<ColumnChartData>();
            List<ColumnChartData> chartData7 = new List<ColumnChartData>();
            List<ColumnChartData> chartData8 = new List<ColumnChartData>();
            List<ColumnChartData> chartData9 = new List<ColumnChartData>();
            List<ColumnChartData> chartData10 = new List<ColumnChartData>();
            List<ColumnChartData> chartData11 = new List<ColumnChartData>();
            List<ColumnChartData> chartData12 = new List<ColumnChartData>();
            List<ColumnChartData> chartData13 = new List<ColumnChartData>();
            List<ColumnChartData> chartData14 = new List<ColumnChartData>();
            List<ColumnChartData> chartData15 = new List<ColumnChartData>();
            List<ColumnChartData> chartData16 = new List<ColumnChartData>();
            List<ColumnChartData> chartData17 = new List<ColumnChartData>();
            List<ColumnChartData> chartData18 = new List<ColumnChartData>();
            List<ColumnChartData> chartData19 = new List<ColumnChartData>();
            List<ColumnChartData> chartData20 = new List<ColumnChartData>();
            List<ColumnChartData> chartData21 = new List<ColumnChartData>();
            List<ColumnChartData> chartData22 = new List<ColumnChartData>();
            List<ColumnChartData> chartData23 = new List<ColumnChartData>();
            List<ColumnChartData> chartData24 = new List<ColumnChartData>();
            List<ColumnChartData> chartData25 = new List<ColumnChartData>();

            foreach (var items in report)
            {

                var listData = chartData1;
                switch (count)
                {
                    case 1:
                        listData = chartData1;
                        ViewBag.locationName1 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 2:
                        listData = chartData2;
                        ViewBag.locationName2 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 3:
                        listData = chartData3;
                        ViewBag.locationName3 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 4:
                        listData = chartData4;
                        ViewBag.locationName4 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 5:
                        listData = chartData5;
                        ViewBag.locationName5 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 6:
                        listData = chartData6;
                        ViewBag.locationName6 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 7:
                        listData = chartData7;
                        ViewBag.locationName7 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 8:
                        listData = chartData8;
                        ViewBag.locationName8 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 9:
                        listData = chartData9;
                        ViewBag.locationName9 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 10:
                        listData = chartData10;
                        ViewBag.locationName10 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    case 11:
                        listData = chartData11;
                        ViewBag.locationName11 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;

                    default:
                        listData = chartData12;
                        ViewBag.locationName12 = products.FirstOrDefault(x => x.ProductId == items.Key).Name;
                        break;
                }

                foreach (var item in items)
                {
                    decimal? totalMonthAmount = 0;
                    var month = item.Month;
                    var amount = item.Amount;

                    foreach (var data in report)
                    {
                        totalMonthAmount += data.Where(x => x.Month == month).Sum(x => x.Amount);
                    }

                    decimal? totalAmount = items.Sum(x => x.Amount);

                    var totalbranchPercent = totalAmount == 0 ? 0 : 100 / totalAmount * amount;
                    var totalMonthPercent = totalMonthAmount == 0 ? 0 : 100 / totalMonthAmount * amount;

                    listData.Add(new ColumnChartData
                    {
                        MonthNumber = item.MonthNumber,
                        x = month,
                        y = amount,
                        text = $"{month} : {amount.Value:0.000} ({totalMonthPercent.Value:0.0}%) <br> Branch Total : {totalAmount.Value:0.000} ({totalbranchPercent.Value:0.0}%) <br> Month Total : {totalMonthAmount.Value:0.000}"
                    });
                }


                count += 1;

            }

            ViewBag.dataSource1 = chartData1;
            ViewBag.dataSource2 = chartData2;
            ViewBag.dataSource3 = chartData3;
            ViewBag.dataSource4 = chartData4;
            ViewBag.dataSource5 = chartData5;
            ViewBag.dataSource6 = chartData6;
            ViewBag.dataSource7 = chartData7;
            ViewBag.dataSource8 = chartData8;
            ViewBag.dataSource9 = chartData9;
            ViewBag.dataSource10 = chartData10;
            ViewBag.dataSource11 = chartData11;
            ViewBag.dataSource12 = chartData12;
            ViewBag.dataSource13 = chartData13;
            ViewBag.dataSource14 = chartData14;
            ViewBag.dataSource15 = chartData15;
            ViewBag.dataSource16 = chartData16;
            ViewBag.dataSource17 = chartData17;
            ViewBag.dataSource18 = chartData18;
            ViewBag.dataSource19 = chartData19;
            ViewBag.dataSource20 = chartData20;
            ViewBag.dataSource21 = chartData21;
            ViewBag.dataSource22 = chartData22;
            ViewBag.dataSource23 = chartData23;
            ViewBag.dataSource24 = chartData24;
            ViewBag.dataSource25 = chartData25;
        }

        private void GetWeeklyReportLocationWise(string locationsString, DateTime? fromDate, DateTime? toDate, IEnumerable<LocationItem> locations)
        {
            ViewBag.WeeklyReportLocationWise = true;
            var report = _salesReportRepository.GetWeeklyReportLocationWise(locationsString, fromDate, toDate).SalesLocationTrendsItems.GroupBy(x => x.LocationId);

            var count = 1;
            List<ColumnChartData> chartData1 = new List<ColumnChartData>();
            List<ColumnChartData> chartData2 = new List<ColumnChartData>();
            List<ColumnChartData> chartData3 = new List<ColumnChartData>();
            List<ColumnChartData> chartData4 = new List<ColumnChartData>();
            List<ColumnChartData> chartData5 = new List<ColumnChartData>();
            List<ColumnChartData> chartData6 = new List<ColumnChartData>();
            List<ColumnChartData> chartData7 = new List<ColumnChartData>();
            List<ColumnChartData> chartData8 = new List<ColumnChartData>();
            List<ColumnChartData> chartData9 = new List<ColumnChartData>();
            List<ColumnChartData> chartData10 = new List<ColumnChartData>();
            List<ColumnChartData> chartData11 = new List<ColumnChartData>();
            List<ColumnChartData> chartData12 = new List<ColumnChartData>();
            List<ColumnChartData> chartData13 = new List<ColumnChartData>();
            List<ColumnChartData> chartData14 = new List<ColumnChartData>();
            List<ColumnChartData> chartData15 = new List<ColumnChartData>();
            List<ColumnChartData> chartData16 = new List<ColumnChartData>();
            List<ColumnChartData> chartData17 = new List<ColumnChartData>();
            List<ColumnChartData> chartData18 = new List<ColumnChartData>();
            List<ColumnChartData> chartData19 = new List<ColumnChartData>();
            List<ColumnChartData> chartData20 = new List<ColumnChartData>();
            List<ColumnChartData> chartData21 = new List<ColumnChartData>();
            List<ColumnChartData> chartData22 = new List<ColumnChartData>();
            List<ColumnChartData> chartData23 = new List<ColumnChartData>();
            List<ColumnChartData> chartData24 = new List<ColumnChartData>();

            foreach (var items in report)
            {

                var listData = chartData1;
                switch (count)
                {
                    case 1:
                        listData = chartData1;
                        ViewBag.locationName1 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 2:
                        listData = chartData2;
                        ViewBag.locationName2 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 3:
                        listData = chartData3;
                        ViewBag.locationName3 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 4:
                        listData = chartData4;
                        ViewBag.locationName4 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 5:
                        listData = chartData5;
                        ViewBag.locationName5 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 6:
                        listData = chartData6;
                        ViewBag.locationName6 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 7:
                        listData = chartData7;
                        ViewBag.locationName7 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 8:
                        listData = chartData8;
                        ViewBag.locationName8 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 9:
                        listData = chartData9;
                        ViewBag.locationName9 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 10:
                        listData = chartData10;
                        ViewBag.locationName10 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 11:
                        listData = chartData11;
                        ViewBag.locationName11 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    default:
                        listData = chartData12;
                        ViewBag.locationName12 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;
                }

                foreach (var item in items)
                {
                    decimal? totalWeekAmount = 0;
                    var week = item.Week;
                    var amount = item.Amount;

                    foreach (var data in report)
                    {
                        totalWeekAmount += data.Where(x => x.WeekStartDate >= item.WeekStartDate && x.WeekEndDate <= item.WeekEndDate).Sum(x => x.Amount);
                    }

                    decimal? totalAmount = items.Sum(x => x.Amount);

                    var totalbranchPercent = totalAmount == 0 ? 0 : 100 / totalAmount * amount;
                    var totalWeekPercent = totalWeekAmount == 0 ? 0 : 100 / totalWeekAmount * amount;

                    listData.Add(new ColumnChartData
                    {
                        x = $"W-{week}",
                        y = amount,
                        text = $"W-{week} {item.WeekText}: {amount.Value:0.000} ({totalWeekPercent.Value:0.0}%) <br> Branch Total : {totalAmount.Value:0.000} ({totalbranchPercent.Value:0.0}%) <br> Week Total : {totalWeekAmount.Value:0.000}"
                    });
                }

                count += 1;

            }

            ViewBag.dataSource1 = chartData1;
            ViewBag.dataSource2 = chartData2;
            ViewBag.dataSource3 = chartData3;
            ViewBag.dataSource4 = chartData4;
            ViewBag.dataSource5 = chartData5;
            ViewBag.dataSource6 = chartData6;
            ViewBag.dataSource7 = chartData7;
            ViewBag.dataSource8 = chartData8;
            ViewBag.dataSource9 = chartData9;
            ViewBag.dataSource10 = chartData10;
            ViewBag.dataSource11 = chartData11;
            ViewBag.dataSource12 = chartData12;
            ViewBag.dataSource13 = chartData13;
            ViewBag.dataSource14 = chartData14;
            ViewBag.dataSource15 = chartData15;
            ViewBag.dataSource16 = chartData16;
            ViewBag.dataSource17 = chartData17;
            ViewBag.dataSource18 = chartData18;
            ViewBag.dataSource19 = chartData19;
            ViewBag.dataSource20 = chartData20;
            ViewBag.dataSource21 = chartData21;
            ViewBag.dataSource22 = chartData22;
            ViewBag.dataSource23 = chartData23;
            ViewBag.dataSource24 = chartData24;





        }

        private void GetMonthYearReportLocationWise(string locationsString, int month, IEnumerable<LocationItem> locations)
        {
            ViewBag.MonthYearReportLocationWise = true;
            var report = _salesReportRepository.GetMonthYearReportLocationWise(locationsString, month).SalesLocationTrendsItems.GroupBy(x => x.LocationId);
            var monthName = new DateTime(2020, month, 1).ToString("MMM");

            var count = 1;
            List<ColumnChartData> chartData1 = new List<ColumnChartData>();
            List<ColumnChartData> chartData2 = new List<ColumnChartData>();
            List<ColumnChartData> chartData3 = new List<ColumnChartData>();
            List<ColumnChartData> chartData4 = new List<ColumnChartData>();
            List<ColumnChartData> chartData5 = new List<ColumnChartData>();
            List<ColumnChartData> chartData6 = new List<ColumnChartData>();
            List<ColumnChartData> chartData7 = new List<ColumnChartData>();
            List<ColumnChartData> chartData8 = new List<ColumnChartData>();
            List<ColumnChartData> chartData9 = new List<ColumnChartData>();
            List<ColumnChartData> chartData10 = new List<ColumnChartData>();
            List<ColumnChartData> chartData11 = new List<ColumnChartData>();
            List<ColumnChartData> chartData12 = new List<ColumnChartData>();
            List<ColumnChartData> chartData13 = new List<ColumnChartData>();
            List<ColumnChartData> chartData14 = new List<ColumnChartData>();
            List<ColumnChartData> chartData15 = new List<ColumnChartData>();
            List<ColumnChartData> chartData16 = new List<ColumnChartData>();
            List<ColumnChartData> chartData17 = new List<ColumnChartData>();
            List<ColumnChartData> chartData18 = new List<ColumnChartData>();
            List<ColumnChartData> chartData19 = new List<ColumnChartData>();
            List<ColumnChartData> chartData20 = new List<ColumnChartData>();
            List<ColumnChartData> chartData21 = new List<ColumnChartData>();
            List<ColumnChartData> chartData22 = new List<ColumnChartData>();
            List<ColumnChartData> chartData23 = new List<ColumnChartData>();
            List<ColumnChartData> chartData24 = new List<ColumnChartData>();

            foreach (var items in report)
            {

                var listData = chartData1;
                switch (count)
                {
                    case 1:
                        listData = chartData1;
                        ViewBag.locationName1 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 2:
                        listData = chartData2;
                        ViewBag.locationName2 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 3:
                        listData = chartData3;
                        ViewBag.locationName3 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 4:
                        listData = chartData4;
                        ViewBag.locationName4 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 5:
                        listData = chartData5;
                        ViewBag.locationName5 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 6:
                        listData = chartData6;
                        ViewBag.locationName6 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 7:
                        listData = chartData7;
                        ViewBag.locationName7 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 8:
                        listData = chartData8;
                        ViewBag.locationName8 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 9:
                        listData = chartData9;
                        ViewBag.locationName9 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 10:
                        listData = chartData10;
                        ViewBag.locationName10 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 11:
                        listData = chartData11;
                        ViewBag.locationName11 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    default:
                        listData = chartData12;
                        ViewBag.locationName12 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;
                }

                foreach (var item in items)
                {
                    decimal? totalYearAmount = 0;
                    var year = item.Year;
                    var amount = item.Amount;

                    foreach (var data in report)
                    {
                        totalYearAmount += data.Where(x => x.Year == year).Sum(x => x.Amount);
                    }

                    decimal? totalAmount = items.Sum(x => x.Amount);

                    var totalbranchPercent = totalAmount == 0 ? 0 : 100 / totalAmount * amount;
                    var totalYearPercent = totalYearAmount == 0 ? 0 : 100 / totalYearAmount * amount;

                    listData.Add(new ColumnChartData
                    {
                        x = $"{monthName}-{year}",
                        y = amount,
                        text = $"{year} : {amount.Value:0.000} ({totalYearPercent.Value:0.0}%) <br> Branch Total : {totalAmount.Value:0.000} ({totalbranchPercent.Value:0.0}%) <br> Year Total : {totalYearAmount.Value:0.000}"
                    });
                }

                count += 1;

            }

            ViewBag.dataSource1 = chartData1;
            ViewBag.dataSource2 = chartData2;
            ViewBag.dataSource3 = chartData3;
            ViewBag.dataSource4 = chartData4;
            ViewBag.dataSource5 = chartData5;
            ViewBag.dataSource6 = chartData6;
            ViewBag.dataSource7 = chartData7;
            ViewBag.dataSource8 = chartData8;
            ViewBag.dataSource9 = chartData9;
            ViewBag.dataSource10 = chartData10;
            ViewBag.dataSource11 = chartData11;
            ViewBag.dataSource12 = chartData12;
            ViewBag.dataSource13 = chartData13;
            ViewBag.dataSource14 = chartData14;
            ViewBag.dataSource15 = chartData15;
            ViewBag.dataSource16 = chartData16;
            ViewBag.dataSource17 = chartData17;
            ViewBag.dataSource18 = chartData18;
            ViewBag.dataSource19 = chartData19;
            ViewBag.dataSource20 = chartData20;
            ViewBag.dataSource21 = chartData21;
            ViewBag.dataSource22 = chartData22;
            ViewBag.dataSource23 = chartData23;
            ViewBag.dataSource24 = chartData24;
        }

        private void GetYearlyReportLocationWise(string locationsString, IEnumerable<LocationItem> locations)
        {
            ViewBag.YearlyReportLocationWise = true;

            var report = _salesReportRepository.GetYearlyReportLocationWise(locationsString).SalesLocationTrendsItems.GroupBy(x => x.LocationId);

            var count = 1;
            List<ColumnChartData> chartData1 = new List<ColumnChartData>();
            List<ColumnChartData> chartData2 = new List<ColumnChartData>();
            List<ColumnChartData> chartData3 = new List<ColumnChartData>();
            List<ColumnChartData> chartData4 = new List<ColumnChartData>();
            List<ColumnChartData> chartData5 = new List<ColumnChartData>();
            List<ColumnChartData> chartData6 = new List<ColumnChartData>();
            List<ColumnChartData> chartData7 = new List<ColumnChartData>();
            List<ColumnChartData> chartData8 = new List<ColumnChartData>();
            List<ColumnChartData> chartData9 = new List<ColumnChartData>();
            List<ColumnChartData> chartData10 = new List<ColumnChartData>();
            List<ColumnChartData> chartData11 = new List<ColumnChartData>();
            List<ColumnChartData> chartData12 = new List<ColumnChartData>();
            List<ColumnChartData> chartData13 = new List<ColumnChartData>();
            List<ColumnChartData> chartData14 = new List<ColumnChartData>();
            List<ColumnChartData> chartData15 = new List<ColumnChartData>();
            List<ColumnChartData> chartData16 = new List<ColumnChartData>();
            List<ColumnChartData> chartData17 = new List<ColumnChartData>();
            List<ColumnChartData> chartData18 = new List<ColumnChartData>();
            List<ColumnChartData> chartData19 = new List<ColumnChartData>();
            List<ColumnChartData> chartData20 = new List<ColumnChartData>();
            List<ColumnChartData> chartData21 = new List<ColumnChartData>();
            List<ColumnChartData> chartData22 = new List<ColumnChartData>();
            List<ColumnChartData> chartData23 = new List<ColumnChartData>();
            List<ColumnChartData> chartData24 = new List<ColumnChartData>();

            foreach (var items in report)
            {

                var listData = chartData1;
                switch (count)
                {
                    case 1:
                        listData = chartData1;
                        ViewBag.locationName1 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 2:
                        listData = chartData2;
                        ViewBag.locationName2 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 3:
                        listData = chartData3;
                        ViewBag.locationName3 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 4:
                        listData = chartData4;
                        ViewBag.locationName4 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 5:
                        listData = chartData5;
                        ViewBag.locationName5 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 6:
                        listData = chartData6;
                        ViewBag.locationName6 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 7:
                        listData = chartData7;
                        ViewBag.locationName7 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 8:
                        listData = chartData8;
                        ViewBag.locationName8 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 9:
                        listData = chartData9;
                        ViewBag.locationName9 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 10:
                        listData = chartData10;
                        ViewBag.locationName10 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 11:
                        listData = chartData11;
                        ViewBag.locationName11 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    default:
                        listData = chartData12;
                        ViewBag.locationName12 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;
                }

                foreach (var item in items)
                {
                    decimal? totalYearAmount = 0;
                    var year = item.Year;
                    var amount = item.Amount;

                    foreach (var data in report)
                    {
                        totalYearAmount += data.Where(x => x.Year == year).Sum(x => x.Amount);
                    }

                    decimal? totalAmount = items.Sum(x => x.Amount);

                    var totalbranchPercent = totalAmount == 0 ? 0 : 100 / totalAmount * amount;
                    var totalYearPercent = totalYearAmount == 0 ? 0 : 100 / totalYearAmount * amount;

                    listData.Add(new ColumnChartData
                    {
                        MonthNumber = item.MonthNumber,
                        x = year.ToString(),
                        y = amount,
                        text = $"{year} : {amount.Value:0.000} ({totalYearPercent.Value:0.0}%) <br> Branch Total : {totalAmount.Value:0.000} ({totalbranchPercent.Value:0.0}%) <br> Year Total : {totalYearAmount.Value:0.000}"
                    });
                }

                count += 1;

            }

            ViewBag.dataSource1 = chartData1;
            ViewBag.dataSource2 = chartData2;
            ViewBag.dataSource3 = chartData3;
            ViewBag.dataSource4 = chartData4;
            ViewBag.dataSource5 = chartData5;
            ViewBag.dataSource6 = chartData6;
            ViewBag.dataSource7 = chartData7;
            ViewBag.dataSource8 = chartData8;
            ViewBag.dataSource9 = chartData9;
            ViewBag.dataSource10 = chartData10;
            ViewBag.dataSource11 = chartData11;
            ViewBag.dataSource12 = chartData12;
            ViewBag.dataSource13 = chartData13;
            ViewBag.dataSource14 = chartData14;
            ViewBag.dataSource15 = chartData15;
            ViewBag.dataSource16 = chartData16;
            ViewBag.dataSource17 = chartData17;
            ViewBag.dataSource18 = chartData18;
            ViewBag.dataSource19 = chartData19;
            ViewBag.dataSource20 = chartData20;
            ViewBag.dataSource21 = chartData21;
            ViewBag.dataSource22 = chartData22;
            ViewBag.dataSource23 = chartData23;
            ViewBag.dataSource24 = chartData24;
        }

        private void GetMonthlyReportLocationWise(string locationsString, int year, IEnumerable<LocationItem> locations)
        {
            ViewBag.MonthlyReportLocationWise = true;

            var report = _salesReportRepository.GetMonthlyReportLocationWise(locationsString, year).SalesLocationTrendsItems.GroupBy(x => x.LocationId);

            var count = 1;
            List<ColumnChartData> chartData1 = new List<ColumnChartData>();
            List<ColumnChartData> chartData2 = new List<ColumnChartData>();
            List<ColumnChartData> chartData3 = new List<ColumnChartData>();
            List<ColumnChartData> chartData4 = new List<ColumnChartData>();
            List<ColumnChartData> chartData5 = new List<ColumnChartData>();
            List<ColumnChartData> chartData6 = new List<ColumnChartData>();
            List<ColumnChartData> chartData7 = new List<ColumnChartData>();
            List<ColumnChartData> chartData8 = new List<ColumnChartData>();
            List<ColumnChartData> chartData9 = new List<ColumnChartData>();
            List<ColumnChartData> chartData10 = new List<ColumnChartData>();
            List<ColumnChartData> chartData11 = new List<ColumnChartData>();
            List<ColumnChartData> chartData12 = new List<ColumnChartData>();
            List<ColumnChartData> chartData13 = new List<ColumnChartData>();
            List<ColumnChartData> chartData14 = new List<ColumnChartData>();
            List<ColumnChartData> chartData15 = new List<ColumnChartData>();
            List<ColumnChartData> chartData16 = new List<ColumnChartData>();
            List<ColumnChartData> chartData17 = new List<ColumnChartData>();
            List<ColumnChartData> chartData18 = new List<ColumnChartData>();
            List<ColumnChartData> chartData19 = new List<ColumnChartData>();
            List<ColumnChartData> chartData20 = new List<ColumnChartData>();
            List<ColumnChartData> chartData21 = new List<ColumnChartData>();
            List<ColumnChartData> chartData22 = new List<ColumnChartData>();
            List<ColumnChartData> chartData23 = new List<ColumnChartData>();
            List<ColumnChartData> chartData24 = new List<ColumnChartData>();

            foreach (var items in report)
            {

                var listData = chartData1;
                switch (count)
                {
                    case 1:
                        listData = chartData1;
                        ViewBag.locationName1 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 2:
                        listData = chartData2;
                        ViewBag.locationName2 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 3:
                        listData = chartData3;
                        ViewBag.locationName3 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 4:
                        listData = chartData4;
                        ViewBag.locationName4 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 5:
                        listData = chartData5;
                        ViewBag.locationName5 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 6:
                        listData = chartData6;
                        ViewBag.locationName6 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 7:
                        listData = chartData7;
                        ViewBag.locationName7 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 8:
                        listData = chartData8;
                        ViewBag.locationName8 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 9:
                        listData = chartData9;
                        ViewBag.locationName9 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 10:
                        listData = chartData10;
                        ViewBag.locationName10 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    case 11:
                        listData = chartData11;
                        ViewBag.locationName11 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;

                    default:
                        listData = chartData12;
                        ViewBag.locationName12 = locations.FirstOrDefault(x => x.LocationId == items.Key).Name;
                        break;
                }

                foreach (var item in items)
                {
                    decimal? totalMonthAmount = 0;
                    var month = item.Month;
                    var amount = item.Amount;

                    foreach (var data in report)
                    {
                        totalMonthAmount += data.Where(x => x.Month == month).Sum(x => x.Amount);
                    }

                    decimal? totalAmount = items.Sum(x => x.Amount);

                    var totalbranchPercent = totalAmount == 0 ? 0 : 100 / totalAmount * amount;
                    var totalMonthPercent = totalMonthAmount == 0 ? 0 : 100 / totalMonthAmount * amount;

                    listData.Add(new ColumnChartData
                    {
                        MonthNumber = item.MonthNumber,
                        x = month,
                        y = amount,
                        text = $"{month} : {amount.Value:0.000} ({totalMonthPercent.Value:0.0}%) <br> Branch Total : {totalAmount.Value:0.000} ({totalbranchPercent.Value:0.0}%) <br> Month Total : {totalMonthAmount.Value:0.000}"
                    });
                }


                count += 1;

            }

            ViewBag.dataSource1 = chartData1;
            ViewBag.dataSource2 = chartData2;
            ViewBag.dataSource3 = chartData3;
            ViewBag.dataSource4 = chartData4;
            ViewBag.dataSource5 = chartData5;
            ViewBag.dataSource6 = chartData6;
            ViewBag.dataSource7 = chartData7;
            ViewBag.dataSource8 = chartData8;
            ViewBag.dataSource9 = chartData9;
            ViewBag.dataSource10 = chartData10;
            ViewBag.dataSource11 = chartData11;
            ViewBag.dataSource12 = chartData12;
            ViewBag.dataSource13 = chartData13;
            ViewBag.dataSource14 = chartData14;
            ViewBag.dataSource15 = chartData15;
            ViewBag.dataSource16 = chartData16;
            ViewBag.dataSource17 = chartData17;
            ViewBag.dataSource18 = chartData18;
            ViewBag.dataSource19 = chartData19;
            ViewBag.dataSource20 = chartData20;
            ViewBag.dataSource21 = chartData21;
            ViewBag.dataSource22 = chartData22;
            ViewBag.dataSource23 = chartData23;
            ViewBag.dataSource24 = chartData24;


        }

        #endregion

    }
}