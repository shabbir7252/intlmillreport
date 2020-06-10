﻿using System;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using ImillReports.Models;
using ImillReports.Contracts;
using ImillReports.ViewModels;
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

            ViewBag.locations = locations;
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

        public ContentResult GetSalesDetailReport(string from, string to, string locations, string voucher, string product)
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

            var salesReportViewModel = _salesReportRepository.GetSalesDetailReport(fromDate, toDate, locations, voucher, product);
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
            //var locationList = new List<int>();
            //var locationStringArray = locations.Split(',');

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

            foreach (var items in salesPeakHourItems)
            {
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
                    decimal? totalAmount = 0;
                    var hour = item.Hour;
                    var amount = item.Amount;

                    foreach (var data in salesPeakHourItems)
                    {
                        totalAmount += data.Where(x => x.Hour == hour).Sum(x => x.Amount);
                    }

                    var percentage = totalAmount == 0 ? 0 : 100 / totalAmount * amount;
                    listData.Add(new ColumnChartData { x = hour.ToString("hh:mm tt"), y = amount, text = $"{hour:hh:mm tt} : {amount} ({percentage.Value:0.0}%) <br> Total {totalAmount}" });
                }

                count += 1;
            }

            ViewBag.dataSource1 = chartAmountData1;
            ViewBag.dataSource2 = chartAmountData2;
            ViewBag.dataSource3 = chartAmountData3;
            ViewBag.dataSource4 = chartAmountData4;
            ViewBag.dataSource5 = chartAmountData5;
            ViewBag.dataSource6 = chartAmountData6;
            ViewBag.dataSource7 = chartAmountData7;
            ViewBag.dataSource8 = chartAmountData8;
            ViewBag.dataSource9 = chartAmountData9;
            ViewBag.dataSource10 = chartAmountData10;
            ViewBag.dataSource11 = chartAmountData11;
            ViewBag.dataSource12 = chartAmountData12;
            ViewBag.dataSource13 = chartAmountData13;
            ViewBag.dataSource14 = chartAmountData14;
            ViewBag.dataSource15 = chartAmountData15;
            ViewBag.dataSource16 = chartAmountData16;
            ViewBag.dataSource17 = chartAmountData17;
            ViewBag.dataSource18 = chartAmountData18;
            ViewBag.dataSource19 = chartAmountData19;
            ViewBag.dataSource20 = chartAmountData20;
            ViewBag.dataSource21 = chartAmountData21;
            ViewBag.dataSource22 = chartAmountData22;
            ViewBag.dataSource23 = chartAmountData23;
            ViewBag.dataSource24 = chartAmountData24;
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

            foreach (var items in salesPeakHourCountItems)
            {
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
                    listData.Add(new ColumnChartData { x = item.Hour.ToString("hh:mm tt"), y = item.TransCount });

                transCount += 1;
            }

            ViewBag.dataCountSource1 = chartCountData1;
            ViewBag.dataCountSource2 = chartCountData2;
            ViewBag.dataCountSource3 = chartCountData3;
            ViewBag.dataCountSource4 = chartCountData4;
            ViewBag.dataCountSource5 = chartCountData5;
            ViewBag.dataCountSource6 = chartCountData6;
            ViewBag.dataCountSource7 = chartCountData7;
            ViewBag.dataCountSource8 = chartCountData8;
            ViewBag.dataCountSource9 = chartCountData9;
            ViewBag.dataCountSource10 = chartCountData10;
            ViewBag.dataCountSource11 = chartCountData11;
            ViewBag.dataCountSource12 = chartCountData12;
            ViewBag.dataCountSource13 = chartCountData13;
            ViewBag.dataCountSource14 = chartCountData14;
            ViewBag.dataCountSource15 = chartCountData15;
            ViewBag.dataCountSource16 = chartCountData16;
            ViewBag.dataCountSource17 = chartCountData17;
            ViewBag.dataCountSource18 = chartCountData18;
            ViewBag.dataCountSource19 = chartCountData19;
            ViewBag.dataCountSource20 = chartCountData20;
            ViewBag.dataCountSource21 = chartCountData21;
            ViewBag.dataCountSource22 = chartCountData22;
            ViewBag.dataCountSource23 = chartCountData23;
            ViewBag.dataCountSource24 = chartCountData24;

            #endregion


            ViewBag.BarChartAmountTitle = fromDate.Value.ToString("dd/MMM/yyyy HH:mm tt") + " to " + toDate.ToString("dd/MMM/yyyy HH:mm tt") + " by Amount";
            ViewBag.BarChartCountTitle = fromDate.Value.ToString("dd/MMM/yyyy HH:mm tt") + " to " + toDate.ToString("dd/MMM/yyyy HH:mm tt") + " by Transaction Count";

            //string[] color = new string[] { "#4286f4", "#f4b642", "#f441a9" };
            //ViewBag.seriesColors = color;

            return View();
        }
    }
}