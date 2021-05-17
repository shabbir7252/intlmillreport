using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using ImillReports.Models;
using ImillReports.Contracts;
using ImillReports.ViewModels;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using System.Web;
using System.Collections.Generic;

namespace ImillReports.Controllers
{
    [Authorize(Roles = "Sales, HR, Admin, HO, StaffAdmin, Purchase")]
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardController(IDashboardRepository dashboardRepository) =>
            _dashboardRepository = dashboardRepository;

        // in use
        public ActionResult SyncSalesData()
        {
            return View();
        }

        public DashboardController() { }

        [Authorize(Roles = "Admin")]
        public ActionResult Index(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate == null)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 03, 00, 00);

            if (toDate == null)
                toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 02, 59, 00).AddDays(1);

            ViewBag.fromDate = fromDate.ToString();
            ViewBag.toDate = toDate.ToString();
            ViewBag.fromD = fromDate.Value.ToString("dd/MMM/yyyy hh:mm tt");
            ViewBag.toD = toDate.Value.ToString("dd/MMM/yyyy hh:mm tt");

            if (toDate < fromDate) ViewBag.validation = "true";

            var jsonResult = BranchSalesData(fromDate.ToString(), toDate.ToString()).Chart2;
            ViewBag.jsonResult = jsonResult.Data;
            return View();
        }

        // used
        public JsonResult BarChartData(SalesOfMonthViewModel salesOfMonth)
        {
            //var fromDate = DateTime.Parse(fromD);
            //var toDate = DateTime.Parse(toD);

            // var salesOfMonth = _dashboardRepository.GetSalesOfMonth(fromDate, toDate);

            List<ColumnChartData> chartData = new List<ColumnChartData>();

            var count = 0;
            List<ColumnChartData> chartAmountData = new List<ColumnChartData>();
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

            decimal? totalSalesData = 0;

            foreach (var item in salesOfMonth.SalesMonthItems)
            {
                totalSalesData += item.Data;
            }


            foreach (var item in salesOfMonth.SalesMonthItems)
            {

                var listData = chartAmountData;
                switch (count)
                {
                    case 0:
                        listData = chartAmountData;
                        ViewBag.locationName = item.Label;
                        break;
                    case 1:
                        listData = chartAmountData1;
                        ViewBag.locationName1 = item.Label;
                        break;
                    case 2:
                        listData = chartAmountData2;
                        ViewBag.locationName2 = item.Label;
                        break;
                    case 3:
                        listData = chartAmountData3;
                        ViewBag.locationName3 = item.Label;
                        break;
                    case 4:
                        listData = chartAmountData4;
                        ViewBag.locationName4 = item.Label;
                        break;
                    case 5:
                        listData = chartAmountData5;
                        ViewBag.locationName5 = item.Label;
                        break;
                    case 6:
                        listData = chartAmountData6;
                        ViewBag.locationName6 = item.Label;
                        break;
                    case 7:
                        listData = chartAmountData7;
                        ViewBag.locationName7 = item.Label;
                        break;
                    case 8:
                        listData = chartAmountData8;
                        ViewBag.locationName8 = item.Label;
                        break;
                    case 9:
                        listData = chartAmountData9;
                        ViewBag.locationName9 = item.Label;
                        break;
                    case 10:
                        listData = chartAmountData10;
                        ViewBag.locationName10 = item.Label;
                        break;
                    case 11:
                        listData = chartAmountData11;
                        ViewBag.locationName11 = item.Label;
                        break;
                    case 12:
                        listData = chartAmountData12;
                        ViewBag.locationName12 = item.Label;
                        break;
                    case 13:
                        listData = chartAmountData13;
                        ViewBag.locationName13 = item.Label;
                        break;
                    case 14:
                        listData = chartAmountData14;
                        ViewBag.locationName14 = item.Label;
                        break;
                    case 15:
                        listData = chartAmountData15;
                        ViewBag.locationName15 = item.Label;
                        break;
                    case 16:
                        listData = chartAmountData16;
                        ViewBag.locationName16 = item.Label;
                        break;
                    case 17:
                        listData = chartAmountData17;
                        ViewBag.locationName17 = item.Label;
                        break;
                    case 18:
                        listData = chartAmountData18;
                        ViewBag.locationName18 = item.Label;
                        break;
                    case 19:
                        listData = chartAmountData19;
                        ViewBag.locationName19 = item.Label;
                        break;
                    case 20:
                        listData = chartAmountData20;
                        ViewBag.locationName20 = item.Label;
                        break;
                    case 21:
                        listData = chartAmountData21;
                        ViewBag.locationName21 = item.Label;
                        break;
                    case 22:
                        listData = chartAmountData22;
                        ViewBag.locationName22 = item.Label;
                        break;
                }

                decimal? branchPercent = totalSalesData != 0 ? Math.Round((decimal)(100 / totalSalesData * item.Data), 2) : 0;

                listData.Add(new ColumnChartData { x = item.Label, y = item.Data, text = item.Data.ToString() });

                chartData.Add(new ColumnChartData { x = item.Label, y = item.Data, text = $"{item.Label} : {branchPercent}%", branchPercent = $"{branchPercent}" });

                count += 1;
            };

            ViewBag.dataSource = chartAmountData;
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


            ViewBag.dataSourcePie = chartData;
            //chartData.Sort((x, y) => x.y.Value.CompareTo(y.y.Value));
            //chartData.Reverse();
            ViewBag.Top10Branch = chartData;

            //HttpCookie chartInfo = new HttpCookie("chartInfo");
            //chartInfo.Value = JsonConvert.SerializeObject(chartData);
            //chartInfo.Expires.Add(new TimeSpan(0, 1, 0));
            //Response.Cookies.Add(chartInfo);


            var labelsArray = new string[salesOfMonth.SalesMonthItems.Count];
            var dataArray = new decimal?[salesOfMonth.SalesMonthItems.Count];

            var colorArray = new string[]
            {
                //"#FF6633",
                "#945713", "#7bb4eb", "#ea7a57", "#00bdae",
                "#404041", "#e56590", "#f8b883", "#70ad47", "#dd8abd",
                "#F8BF00", "#E11C34", "#E6B3B3", "#999999", "#7f84e8",
                "#FF99E6", "#CCFF1A", "#555555", "#59A28F", "#33FFCC",
                "#66994D", "#B366CC", "#4D8000", "#B33300", "#CC80CC",
                "#66664D", "#991AFF", "#E666FF", "#4DB3FF", "#1AB399",
                "#E666B3", "#33991A", "#CC9999", "#B3B31A", "#00E680",
                "#4D8066", "#809980", "#E6FF80", "#1AFF33", "#999933",
                "#FF3380", "#CCCC00", "#66E64D", "#4D80CC", "#9900B3",
                "#E64D66", "#4DB380", "#FF4D4D", "#99E6E6"
                //, "#6666FF"
            };

            ViewBag.Palettes = colorArray;

            for (int i = 0; i < salesOfMonth.SalesMonthItems.Count; i++)
            {
                var item = salesOfMonth.SalesMonthItems[i];
                labelsArray[i] = item.Label;
                dataArray[i] = item.Data;
            }

            Chart _chart = new Chart
            {
                labels = labelsArray,
                datasets = new List<Datasets>()
            };

            var _dataSet = new List<Datasets>
            {
                new Datasets()
                {
                    label = "",
                    data = dataArray,
                    backgroundColor = colorArray,
                    borderColor = colorArray,
                    borderWidth = "1"
                }
            };
            _chart.datasets = _dataSet;



            var dashboardJson = new DashboardViewModel
            {
                ChartData = _chart,
                TotalBranchSales = salesOfMonth.TotalBranchSales.HasValue ? salesOfMonth.TotalBranchSales.Value.ToString("0.###") : "0",
                TotalHOSales = salesOfMonth.TotalHOSales.HasValue ? salesOfMonth.TotalHOSales.Value.ToString("0.###") : "0",
                TotalHOSalesCash = salesOfMonth.TotalHOSalesCash.HasValue ? salesOfMonth.TotalHOSalesCash.Value.ToString("0.###") : "0",
                TotalHOSalesCredit = salesOfMonth.TotalHOSalesCredit.HasValue ? salesOfMonth.TotalHOSalesCredit.Value.ToString("0.###") : "0",
                SalesReturnHO = salesOfMonth.SalesReturnHO.HasValue ? salesOfMonth.SalesReturnHO.Value.ToString("0.###") : "0",
                SalesReturnBranches = salesOfMonth.SalesReturnBranches.HasValue ? salesOfMonth.SalesReturnBranches.Value.ToString("0.###") : "0"
            };

            string JSONresult = JsonConvert.SerializeObject(dashboardJson);
            return Json(JSONresult, JsonRequestBehavior.AllowGet);
        }

        // Used
        private ChartData BranchSalesData(string fromD, string toD)
        {
            var fromDate = DateTime.Parse(fromD);
            var toDate = DateTime.Parse(toD);

            var salesOfMonth = _dashboardRepository.GetSalesRecordOfMonth(fromDate, toDate);
            var data2 = BarChartData(salesOfMonth);

            #region HO

            var totalHOSalesCash = salesOfMonth.TotalHOSalesCash ?? 0;
            var totalHOSalesCredit = salesOfMonth.TotalHOSalesCredit ?? 0;
            var salesReturnHO = salesOfMonth.SalesReturnHO ?? 0;
            var totalHoCount = salesOfMonth.TotalHOCount;
            var totalHOSales = totalHOSalesCash + totalHOSalesCredit - Math.Abs(salesReturnHO);

            #endregion

            #region Branch

            var totalCashSales = salesOfMonth.TotalBranchCash ?? 0;
            var totalKnetSales = salesOfMonth.TotalBranchKnet ?? 0;
            var totalCreditCardSales = salesOfMonth.TotalBranchCC ?? 0;
            var salesReturnBranches = salesOfMonth.SalesReturnBranches ?? 0;
            var totalBranchCount = salesOfMonth.TotalBranchCount;
            var totalBranchSales = totalCashSales + totalKnetSales + totalCreditCardSales - Math.Abs(salesReturnBranches);
            // + Math.Abs((salesReturnBranches + Math.Abs(brOnlineReturn)));

            #endregion

            #region Website Orders

            var webOrderCash = salesOfMonth.TotalBranchOnlineCash != null ? (decimal)salesOfMonth.TotalBranchOnlineCash : 0;
            var webOrderKnet = salesOfMonth.TotalBranchOnlineKnet != null ? (decimal)salesOfMonth.TotalBranchOnlineKnet : 0;
            var webOrderCc = salesOfMonth.TotalBranchOnlineCc != null ? (decimal)salesOfMonth.TotalBranchOnlineCc : 0;
            var webOrderReturns = salesOfMonth.TotalBranchOnlineReturn != null ? (decimal)salesOfMonth.TotalBranchOnlineReturn : 0;
            var totalwebOrderSales = webOrderCash + webOrderKnet + webOrderCc - Math.Abs(webOrderReturns);
            var webOrderCount = salesOfMonth.TotalOnlineTransCount;


            var totalTalabatSales = salesOfMonth.TotalTalabat ?? 0;
            var talabatTransCount = salesOfMonth.TalabatTransCount;

            var totalDeliverooSales = salesOfMonth.TotalDeliveroo ?? 0;
            var deliverooTransCount = salesOfMonth.DeliverooTransCount;

            var onlineSales = totalwebOrderSales + totalTalabatSales;
            var onlineTransCount = webOrderCount + talabatTransCount;

            ViewBag.WebOrderPercent = onlineSales != 0 ? Math.Round(100 / onlineSales * totalwebOrderSales, 2) : 0;
            ViewBag.WebOrderCashPercent = (webOrderCash + webOrderKnet + webOrderCc) != 0 ? Math.Round(100 / (webOrderCash + webOrderKnet + webOrderCc) * webOrderCash, 2) : 0;
            ViewBag.WebOrderKnetPercent = (webOrderCash + webOrderKnet + webOrderCc) != 0 ? Math.Round(100 / (webOrderCash + webOrderKnet + webOrderCc) * webOrderKnet, 2) : 0;
            ViewBag.WebOrderCcPercent = (webOrderCash + webOrderKnet + webOrderCc) != 0 ? Math.Round(100 / (webOrderCash + webOrderKnet + webOrderCc) * webOrderCc, 2) : 0;
            ViewBag.WebOrderReturnPercent = totalwebOrderSales != 0 ? Math.Abs(Math.Round(100 / totalwebOrderSales * webOrderReturns, 2)) : 0;
            ViewBag.WebOrderTransPercent = onlineTransCount != 0 ? Math.Round(100 / (decimal)onlineTransCount * webOrderCount, 2) : 0;

            ViewBag.TalabatPercent = onlineSales != 0 ? Math.Round(100 / onlineSales * totalTalabatSales, 2) : 0;
            ViewBag.TalabatTransPercent = onlineTransCount != 0 ? Math.Round(100 / (decimal)onlineTransCount * talabatTransCount, 2) : 0;

            ViewBag.DeliverooPercent = onlineSales != 0 ? Math.Round(100 / onlineSales * totalDeliverooSales, 2) : 0;
            ViewBag.DeliverooTransPercent = onlineTransCount != 0 ? Math.Round(100 / (decimal)onlineTransCount * deliverooTransCount, 2) : 0;

            #endregion


            var totalSales = totalHOSales + totalBranchSales + onlineSales;
            var totalCount = totalHoCount + totalBranchCount + onlineTransCount;


            ViewBag.DSTop5BranchProdByVal = salesOfMonth.Top5ProductsByAmount;
            ViewBag.DSTop5HoProdByVal = salesOfMonth.Top5HoProductsByAmount;

            ViewBag.DSTop5ProdByKg = salesOfMonth.Top5ProductsByKg;
            ViewBag.DSTop5ProdHoByKg = salesOfMonth.Top5ProductsHoByKg;

            ViewBag.DSTop5ProdByQty = salesOfMonth.Top5ProductsByQty;
            ViewBag.DSTop5ProdHoByQty = salesOfMonth.Top5ProductsHoByQty;

            //ViewBag.BRSalesPercent = 0;
            //ViewBag.BRCashPercent = 0;
            //ViewBag.BRKnetPercent = 0;
            //ViewBag.BRCCPercent = 0;
            //ViewBag.BRsalesReturnPercent = 0;



            //ViewBag.HOSalesPercent = 0;
            //ViewBag.HOSalesCashPercent = 0;
            //ViewBag.HOSalesCreditPercent = 0;
            //ViewBag.HOSalesReturnPercent = 0;

            ViewBag.BRSalesPercent = totalSales != 0 ? Math.Round(100 / totalSales * totalBranchSales, 2) : 0;
            ViewBag.HOSalesPercent = totalSales != 0 ? Math.Round(100 / totalSales * totalHOSales, 2) : 0;
            ViewBag.OnlineSalesPercent = totalSales != 0 ? Math.Round(100 / totalSales * onlineSales, 2) : 0;

            ViewBag.BRCashPercent = totalBranchSales != 0 ? Math.Round(100 / totalBranchSales * totalCashSales, 2) : 0;
            ViewBag.BRKnetPercent = totalBranchSales != 0 ? Math.Round(100 / totalBranchSales * totalKnetSales, 2) : 0;
            ViewBag.BRCCPercent = totalBranchSales != 0 ? Math.Round(100 / totalBranchSales * totalCreditCardSales, 2) : 0;
            ViewBag.BRsalesReturnPercent = totalBranchSales != 0 ? Math.Round(100 / totalBranchSales * (salesReturnBranches - Math.Abs(webOrderReturns)), 2) : 0;
            ViewBag.BRCountPercent = totalCount != 0 ? Math.Round(100 / (decimal)totalCount * totalBranchCount, 2) : 0;

            ViewBag.HOSalesCashPercent = (totalHOSalesCash + totalHOSalesCredit) != 0 ? Math.Round(100 / (totalHOSalesCash + totalHOSalesCredit) * totalHOSalesCash, 2) : 0;
            ViewBag.HOSalesCreditPercent = (totalHOSalesCash + totalHOSalesCredit) != 0 ? Math.Round(100 / (totalHOSalesCash + totalHOSalesCredit) * totalHOSalesCredit, 2) : 0;
            ViewBag.HOSalesReturnPercent = totalHOSales != 0 ? Math.Abs(Math.Round(100 / totalHOSales * salesReturnHO, 2)) : 0;
            ViewBag.HOCountPercent = totalCount != 0 ? Math.Round(100 / (decimal)totalCount * totalHoCount, 2) : 0;

            ViewBag.OnlineCountPercent = totalCount != 0 ? Math.Round(100 / (decimal)totalCount * onlineTransCount, 2) : 0;


            var dashboardJson = new BranchSalesDashboardViewModel
            {
                GrandTotal = totalSales.ToString("0.###"),

                TotalBranchSales = totalBranchSales.ToString("0.###"),
                TotalCashSales = totalCashSales.ToString("0.###"),
                TotalKnetSales = totalKnetSales.ToString("0.###"),
                TotalCreditCardSales = totalCreditCardSales.ToString("0.###"),
                SalesReturnBranches = salesReturnBranches.ToString("0.###"),
                TotalBranchCount = totalBranchCount,

                TotalHOSales = totalHOSales.ToString("0.###"),
                TotalHOSalesCash = totalHOSalesCash.ToString("0.###"),
                TotalHOSalesCredit = totalHOSalesCredit.ToString("0.###"),
                SalesReturnHO = salesReturnHO.ToString("0.###"),
                TotalHoCount = totalHoCount,

                TotalWebOrderSales = totalwebOrderSales.ToString("0.###"),
                TotalWebOrderCash = webOrderCash,
                TotalWebOrderKnet = webOrderKnet,
                TotalWebOrderCc = webOrderCc,
                TotalWebOrderReturn = webOrderReturns,
                TotalWebOrderCount = webOrderCount,

                TotalTalabatSales = totalTalabatSales.ToString("0.###"),
                TotalTalabatTransCount = talabatTransCount,

                TotalDeliverooSales = totalDeliverooSales.ToString("0.###"),
                TotalDeliverooTransCount = deliverooTransCount,

                OnlineSales = onlineSales,
                OnlineTransCount = onlineTransCount

            };

            string JSONresult = JsonConvert.SerializeObject(dashboardJson);
            return new ChartData
            {
                Chart1 = Json(JSONresult, JsonRequestBehavior.AllowGet),
                Chart2 = data2
            };
        }

        public ActionResult BranchSales(DateTime? fromDate, DateTime? toDate)
        {
            if (User.IsInRole("Sales"))
                return RedirectToAction("Index", "SalesReport");

            if (User.IsInRole("HR"))
                return RedirectToAction("Index", "TAReport");

            if (fromDate == null)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 03, 00, 00);

            if (toDate == null)
                toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 02, 59, 00).AddDays(1);

            ViewBag.fromDate = fromDate.ToString();
            ViewBag.toDate = toDate.ToString();
            ViewBag.fromD = fromDate.Value.ToString("dd/MMM/yyyy hh:mm tt");
            ViewBag.toD = toDate.Value.ToString("dd/MMM/yyyy hh:mm tt");

            if (toDate < fromDate) ViewBag.validation = "true";
            var data = BranchSalesData(fromDate.ToString(), toDate.ToString());

            ViewBag.jsonResult = data.Chart1;
            ViewBag.jsonResult2 = data.Chart2;

            // GeneratePdf();

            return View();

        }

        //[HttpGet]
        //public string BranchSalesTest()
        //{
        //    string data = "";
        //    try
        //    {
        //        var fromDate = new DateTime(2020, 07, 01, 03, 00, 00);
        //        var toDate = new DateTime(2020, 08, 12, 02, 59, 00);

        //        ViewBag.fromDate = fromDate.ToString();
        //        ViewBag.toDate = toDate.ToString();
        //        ViewBag.fromD = fromDate.ToString("dd/MMM/yyyy hh:mm tt");
        //        ViewBag.toD = toDate.ToString("dd/MMM/yyyy hh:mm tt");

        //        if (toDate < fromDate) ViewBag.validation = "true";
        //        data = BranchSalesDataTest(fromDate.ToString(), toDate.ToString());
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message + " Data : " + data;
        //    }

        //    return "true" + " BranchSalesDataTest : " + data;

        //}


        //private string BranchSalesDataTest(string fromD, string toD)
        //{
        //    ChartData t = null;
        //    string salesOfMonth = "";
        //    try
        //    {
        //        var fromDate = DateTime.Parse(fromD);
        //        var toDate = DateTime.Parse(toD);

        //        salesOfMonth = _dashboardRepository.GetSalesOfMonthTest(fromDate, toDate);
        //       //  var data2 = BarChartData(salesOfMonth);

        //        //#region HO

        //        //var totalHOSalesCash = salesOfMonth.TotalHOSalesCash ?? 0;
        //        //var totalHOSalesCredit = salesOfMonth.TotalHOSalesCredit ?? 0;
        //        //var salesReturnHO = salesOfMonth.SalesReturnHO ?? 0;
        //        //var totalHoCount = salesOfMonth.TotalHOCount;
        //        //var totalHOSales = totalHOSalesCash + totalHOSalesCredit - Math.Abs(salesReturnHO);

        //        //#endregion

        //        //#region Branch

        //        //var totalCashSales = salesOfMonth.TotalBranchCash ?? 0;
        //        //var totalKnetSales = salesOfMonth.TotalBranchKnet ?? 0;
        //        //var totalCreditCardSales = salesOfMonth.TotalBranchCC ?? 0;
        //        //var salesReturnBranches = salesOfMonth.SalesReturnBranches ?? 0;
        //        //var totalBranchCount = salesOfMonth.TotalBranchCount;
        //        //var totalBranchSales = totalCashSales + totalKnetSales + totalCreditCardSales - Math.Abs(salesReturnBranches);
        //        //// + Math.Abs((salesReturnBranches + Math.Abs(brOnlineReturn)));

        //        //#endregion

        //        //#region Website Orders

        //        //var webOrderCash = salesOfMonth.TotalBranchOnlineCash != null ? (decimal)salesOfMonth.TotalBranchOnlineCash : 0;
        //        //var webOrderKnet = salesOfMonth.TotalBranchOnlineKnet != null ? (decimal)salesOfMonth.TotalBranchOnlineKnet : 0;
        //        //var webOrderCc = salesOfMonth.TotalBranchOnlineCc != null ? (decimal)salesOfMonth.TotalBranchOnlineCc : 0;
        //        //var webOrderReturns = salesOfMonth.TotalBranchOnlineReturn != null ? (decimal)salesOfMonth.TotalBranchOnlineReturn : 0;
        //        //var totalwebOrderSales = webOrderCash + webOrderKnet + webOrderCc - Math.Abs(webOrderReturns);
        //        //var webOrderCount = salesOfMonth.TotalOnlineTransCount;


        //        //var totalTalabatSales = salesOfMonth.TotalTalabat ?? 0;
        //        //var talabatTransCount = salesOfMonth.TalabatTransCount;

        //        //var onlineSales = totalwebOrderSales + totalTalabatSales;
        //        //var onlineTransCount = webOrderCount + talabatTransCount;

        //        //ViewBag.WebOrderPercent = onlineSales != 0 ? Math.Round(100 / onlineSales * totalwebOrderSales, 2) : 0;
        //        //ViewBag.WebOrderCashPercent = (webOrderCash + webOrderKnet + webOrderCc) != 0 ? Math.Round(100 / (webOrderCash + webOrderKnet + webOrderCc) * webOrderCash, 2) : 0;
        //        //ViewBag.WebOrderKnetPercent = (webOrderCash + webOrderKnet + webOrderCc) != 0 ? Math.Round(100 / (webOrderCash + webOrderKnet + webOrderCc) * webOrderKnet, 2) : 0;
        //        //ViewBag.WebOrderCcPercent = (webOrderCash + webOrderKnet + webOrderCc) != 0 ? Math.Round(100 / (webOrderCash + webOrderKnet + webOrderCc) * webOrderCc, 2) : 0;
        //        //ViewBag.WebOrderReturnPercent = totalwebOrderSales != 0 ? Math.Abs(Math.Round(100 / totalwebOrderSales * webOrderReturns, 2)) : 0;
        //        //ViewBag.WebOrderTransPercent = onlineTransCount != 0 ? Math.Round(100 / (decimal)onlineTransCount * webOrderCount, 2) : 0;

        //        //ViewBag.TalabatPercent = onlineSales != 0 ? Math.Round(100 / onlineSales * totalTalabatSales, 2) : 0;
        //        //ViewBag.TalabatTransPercent = onlineTransCount != 0 ? Math.Round(100 / (decimal)onlineTransCount * talabatTransCount, 2) : 0;

        //        //#endregion

        //        //var totalSales = totalHOSales + totalBranchSales + onlineSales;
        //        //var totalCount = totalHoCount + totalBranchCount + onlineTransCount;


        //        //ViewBag.DSTop5BranchProdByVal = salesOfMonth.Top5ProductsByAmount;
        //        //ViewBag.DSTop5HoProdByVal = salesOfMonth.Top5HoProductsByAmount;

        //        //ViewBag.DSTop5ProdByKg = salesOfMonth.Top5ProductsByKg;
        //        //ViewBag.DSTop5ProdHoByKg = salesOfMonth.Top5ProductsHoByKg;

        //        //ViewBag.DSTop5ProdByQty = salesOfMonth.Top5ProductsByQty;
        //        //ViewBag.DSTop5ProdHoByQty = salesOfMonth.Top5ProductsHoByQty;

        //        //ViewBag.BRSalesPercent = totalSales != 0 ? Math.Round(100 / totalSales * totalBranchSales, 2) : 0;
        //        //ViewBag.HOSalesPercent = totalSales != 0 ? Math.Round(100 / totalSales * totalHOSales, 2) : 0;
        //        //ViewBag.OnlineSalesPercent = totalSales != 0 ? Math.Round(100 / totalSales * onlineSales, 2) : 0;

        //        //ViewBag.BRCashPercent = totalBranchSales != 0 ? Math.Round(100 / totalBranchSales * totalCashSales, 2) : 0;
        //        //ViewBag.BRKnetPercent = totalBranchSales != 0 ? Math.Round(100 / totalBranchSales * totalKnetSales, 2) : 0;
        //        //ViewBag.BRCCPercent = totalBranchSales != 0 ? Math.Round(100 / totalBranchSales * totalCreditCardSales, 2) : 0;
        //        //ViewBag.BRsalesReturnPercent = totalBranchSales != 0 ? Math.Round(100 / totalBranchSales * (salesReturnBranches - Math.Abs(webOrderReturns)), 2) : 0;
        //        //ViewBag.BRCountPercent = totalCount != 0 ? Math.Round(100 / (decimal)totalCount * totalBranchCount, 2) : 0;

        //        //ViewBag.HOSalesCashPercent = (totalHOSalesCash + totalHOSalesCredit) != 0 ? Math.Round(100 / (totalHOSalesCash + totalHOSalesCredit) * totalHOSalesCash, 2) : 0;
        //        //ViewBag.HOSalesCreditPercent = (totalHOSalesCash + totalHOSalesCredit) != 0 ? Math.Round(100 / (totalHOSalesCash + totalHOSalesCredit) * totalHOSalesCredit, 2) : 0;
        //        //ViewBag.HOSalesReturnPercent = totalHOSales != 0 ? Math.Abs(Math.Round(100 / totalHOSales * salesReturnHO, 2)) : 0;
        //        //ViewBag.HOCountPercent = totalCount != 0 ? Math.Round(100 / (decimal)totalCount * totalHoCount, 2) : 0;

        //        //ViewBag.OnlineCountPercent = totalCount != 0 ? Math.Round(100 / (decimal)totalCount * onlineTransCount, 2) : 0;

        //        //var dashboardJson = new BranchSalesDashboardViewModel
        //        //{
        //        //    GrandTotal = totalSales.ToString("0.###"),

        //        //    TotalBranchSales = totalBranchSales.ToString("0.###"),
        //        //    TotalCashSales = totalCashSales.ToString("0.###"),
        //        //    TotalKnetSales = totalKnetSales.ToString("0.###"),
        //        //    TotalCreditCardSales = totalCreditCardSales.ToString("0.###"),
        //        //    SalesReturnBranches = salesReturnBranches.ToString("0.###"),
        //        //    TotalBranchCount = totalBranchCount,

        //        //    TotalHOSales = totalHOSales.ToString("0.###"),
        //        //    TotalHOSalesCash = totalHOSalesCash.ToString("0.###"),
        //        //    TotalHOSalesCredit = totalHOSalesCredit.ToString("0.###"),
        //        //    SalesReturnHO = salesReturnHO.ToString("0.###"),
        //        //    TotalHoCount = totalHoCount,

        //        //    TotalWebOrderSales = totalwebOrderSales.ToString("0.###"),
        //        //    TotalWebOrderCash = webOrderCash,
        //        //    TotalWebOrderKnet = webOrderKnet,
        //        //    TotalWebOrderCc = webOrderCc,
        //        //    TotalWebOrderReturn = webOrderReturns,
        //        //    TotalWebOrderCount = webOrderCount,

        //        //    TotalTalabatSales = totalTalabatSales.ToString("0.###"),
        //        //    TotalTalabatTransCount = talabatTransCount,

        //        //    OnlineSales = onlineSales,
        //        //    OnlineTransCount = onlineTransCount

        //        //};

        //        //string JSONresult = JsonConvert.SerializeObject(dashboardJson);
        //        //t = new ChartData
        //        //{
        //        //    Chart1 = Json(JSONresult, JsonRequestBehavior.AllowGet),
        //        //    Chart2 = data2
        //        //};

        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message + "SOM :" + salesOfMonth;
        //    }

        //    return "true" + " SOM :" + salesOfMonth;


        //}

        // used

        // Used

        // Used
        public ActionResult BranchSalesRecord(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == null)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 03, 00, 00);

            if (toDate == null)
                toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 02, 59, 00).AddDays(1);

            if (toDate < fromDate) ViewBag.validation = "true";
            var data = BranchSalesRecordData(fromDate.ToString(), toDate.ToString());

            ViewBag.BarChart = data.Chart2.Data;

            return PartialView("_BranchGraphs");

        }

        // used
        private ChartData BranchSalesRecordData(string fromD, string toD)
        {
            var fromDate = DateTime.Parse(fromD);
            var toDate = DateTime.Parse(toD);

            var salesOfMonth = _dashboardRepository.GetSalesRecordOfMonth(fromDate, toDate);
            var data2 = BarChartData(salesOfMonth);

            ViewBag.SalesRecordCount = salesOfMonth.SalesRecordCount;

            #region HO

            var totalHOSalesCash = salesOfMonth.TotalHOSalesCash ?? 0;
            var totalHOSalesCredit = salesOfMonth.TotalHOSalesCredit ?? 0;
            var salesReturnHO = salesOfMonth.SalesReturnHO ?? 0;
            var totalHoCount = salesOfMonth.TotalHOCount;
            var totalHOSales = totalHOSalesCash + totalHOSalesCredit - Math.Abs(salesReturnHO);

            #endregion

            #region Branch

            var totalCashSales = salesOfMonth.TotalBranchCash ?? 0;
            var totalKnetSales = salesOfMonth.TotalBranchKnet ?? 0;
            var totalCreditCardSales = salesOfMonth.TotalBranchCC ?? 0;
            var salesReturnBranches = salesOfMonth.SalesReturnBranches ?? 0;
            var totalBranchCount = salesOfMonth.TotalBranchCount;
            var totalBranchSales = totalCashSales + totalKnetSales + totalCreditCardSales - Math.Abs(salesReturnBranches);
            // + Math.Abs((salesReturnBranches + Math.Abs(brOnlineReturn)));

            #endregion

            #region Website Orders

            var webOrderCash = salesOfMonth.TotalBranchOnlineCash != null ? (decimal)salesOfMonth.TotalBranchOnlineCash : 0;
            var webOrderKnet = salesOfMonth.TotalBranchOnlineKnet != null ? (decimal)salesOfMonth.TotalBranchOnlineKnet : 0;
            var webOrderCc = salesOfMonth.TotalBranchOnlineCc != null ? (decimal)salesOfMonth.TotalBranchOnlineCc : 0;
            var webOrderReturns = salesOfMonth.TotalBranchOnlineReturn != null ? (decimal)salesOfMonth.TotalBranchOnlineReturn : 0;
            var totalwebOrderSales = webOrderCash + webOrderKnet + webOrderCc - Math.Abs(webOrderReturns);
            var webOrderCount = salesOfMonth.TotalOnlineTransCount;

            var totalTalabatSales = salesOfMonth.TotalTalabat ?? 0;
            var talabatTransCount = salesOfMonth.TalabatTransCount;

            var totalDeliverooSales = salesOfMonth.TotalDeliveroo ?? 0;
            var deliverooTransCount = salesOfMonth.DeliverooTransCount;

            var onlineSales = totalwebOrderSales + totalTalabatSales + totalDeliverooSales;
            var onlineTransCount = webOrderCount + talabatTransCount + deliverooTransCount;

            ViewBag.WebOrderPercent = onlineSales != 0 ? Math.Round(100 / onlineSales * totalwebOrderSales, 2) : 0;
            ViewBag.WebOrderCashPercent = (webOrderCash + webOrderKnet + webOrderCc) != 0 ? Math.Round(100 / (webOrderCash + webOrderKnet + webOrderCc) * webOrderCash, 2) : 0;
            ViewBag.WebOrderKnetPercent = (webOrderCash + webOrderKnet + webOrderCc) != 0 ? Math.Round(100 / (webOrderCash + webOrderKnet + webOrderCc) * webOrderKnet, 2) : 0;
            ViewBag.WebOrderCcPercent = (webOrderCash + webOrderKnet + webOrderCc) != 0 ? Math.Round(100 / (webOrderCash + webOrderKnet + webOrderCc) * webOrderCc, 2) : 0;
            ViewBag.WebOrderReturnPercent = totalwebOrderSales != 0 ? Math.Abs(Math.Round(100 / totalwebOrderSales * webOrderReturns, 2)) : 0;
            ViewBag.WebOrderTransPercent = onlineTransCount != 0 ? Math.Round(100 / (decimal)onlineTransCount * webOrderCount, 2) : 0;

            ViewBag.TalabatPercent = onlineSales != 0 ? Math.Round(100 / onlineSales * totalTalabatSales, 2) : 0;
            ViewBag.TalabatTransPercent = onlineTransCount != 0 ? Math.Round(100 / (decimal)onlineTransCount * talabatTransCount, 2) : 0;

            ViewBag.DeliverooPercent = onlineSales != 0 ? Math.Round(100 / onlineSales * totalDeliverooSales, 2) : 0;
            ViewBag.DeliverooTransPercent = onlineTransCount != 0 ? Math.Round(100 / (decimal)onlineTransCount * deliverooTransCount, 2) : 0;

            #endregion


            var totalSales = totalHOSales + totalBranchSales + onlineSales;
            var totalCount = totalHoCount + totalBranchCount + onlineTransCount;

            ViewBag.BRSalesPercent = totalSales != 0 ? Math.Round(100 / totalSales * totalBranchSales, 2) : 0;
            ViewBag.HOSalesPercent = totalSales != 0 ? Math.Round(100 / totalSales * totalHOSales, 2) : 0;
            ViewBag.OnlineSalesPercent = totalSales != 0 ? Math.Round(100 / totalSales * onlineSales, 2) : 0;

            ViewBag.BRCashPercent = totalBranchSales != 0 ? Math.Round(100 / totalBranchSales * totalCashSales, 2) : 0;
            ViewBag.BRKnetPercent = totalBranchSales != 0 ? Math.Round(100 / totalBranchSales * totalKnetSales, 2) : 0;
            ViewBag.BRCCPercent = totalBranchSales != 0 ? Math.Round(100 / totalBranchSales * totalCreditCardSales, 2) : 0;
            ViewBag.BRsalesReturnPercent = totalBranchSales != 0 ? Math.Round(100 / totalBranchSales * (salesReturnBranches - Math.Abs(webOrderReturns)), 2) : 0;
            ViewBag.BRCountPercent = totalCount != 0 ? Math.Round(100 / (decimal)totalCount * totalBranchCount, 2) : 0;

            ViewBag.HOSalesCashPercent = (totalHOSalesCash + totalHOSalesCredit) != 0 ? Math.Round(100 / (totalHOSalesCash + totalHOSalesCredit) * totalHOSalesCash, 2) : 0;
            ViewBag.HOSalesCreditPercent = (totalHOSalesCash + totalHOSalesCredit) != 0 ? Math.Round(100 / (totalHOSalesCash + totalHOSalesCredit) * totalHOSalesCredit, 2) : 0;
            ViewBag.HOSalesReturnPercent = totalHOSales != 0 ? Math.Abs(Math.Round(100 / totalHOSales * salesReturnHO, 2)) : 0;
            ViewBag.HOCountPercent = totalCount != 0 ? Math.Round(100 / (decimal)totalCount * totalHoCount, 2) : 0;

            ViewBag.OnlineCountPercent = totalCount != 0 ? Math.Round(100 / (decimal)totalCount * onlineTransCount, 2) : 0;


            ViewBag.GrandTotal = totalSales.ToString("0.###");
            ViewBag.TotalBranchSales = totalBranchSales.ToString("0.###");
            ViewBag.TotalCashSales = totalCashSales.ToString("0.###");
            ViewBag.TotalKnetSales = totalKnetSales.ToString("0.###");
            ViewBag.TotalCreditCardSales = totalCreditCardSales.ToString("0.###");
            ViewBag.SalesReturnBranches = salesReturnBranches.ToString("0.###");
            ViewBag.TotalHOSales = totalHOSales.ToString("0.###");
            ViewBag.TotalHOSalesCash = totalHOSalesCash.ToString("0.###");
            ViewBag.TotalHOSalesCredit = totalHOSalesCredit.ToString("0.###");
            ViewBag.SalesReturnHO = salesReturnHO.ToString("0.###");
            ViewBag.TotalWebOrderSales = totalwebOrderSales.ToString("0.###");
            ViewBag.TotalWebOrderCash = webOrderCash;
            ViewBag.TotalWebOrderKnet = webOrderKnet;
            ViewBag.TotalWebOrderCc = webOrderCc;
            ViewBag.TotalWebOrderReturn = webOrderReturns;
            ViewBag.TotalWebOrderCount = webOrderCount;
            ViewBag.TotalTalabatSales = totalTalabatSales.ToString("0.###");
            ViewBag.TotalTalabatTransCount = talabatTransCount;
            ViewBag.TotalDeliverooSales = totalDeliverooSales.ToString("0.###");
            ViewBag.TotalDeliverooTransCount = deliverooTransCount;
            ViewBag.OnlineSales = onlineSales;
            ViewBag.OnlineTransCount = onlineTransCount;

            var dashboardJson = new BranchSalesDashboardViewModel
            {
                GrandTotal = totalSales.ToString("0.###"),

                TotalBranchSales = totalBranchSales.ToString("0.###"),
                TotalCashSales = totalCashSales.ToString("0.###"),
                TotalKnetSales = totalKnetSales.ToString("0.###"),
                TotalCreditCardSales = totalCreditCardSales.ToString("0.###"),
                SalesReturnBranches = salesReturnBranches.ToString("0.###"),
                TotalBranchCount = totalBranchCount,

                TotalHOSales = totalHOSales.ToString("0.###"),
                TotalHOSalesCash = totalHOSalesCash.ToString("0.###"),
                TotalHOSalesCredit = totalHOSalesCredit.ToString("0.###"),
                SalesReturnHO = salesReturnHO.ToString("0.###"),
                TotalHoCount = totalHoCount,

                TotalWebOrderSales = totalwebOrderSales.ToString("0.###"),
                TotalWebOrderCash = webOrderCash,
                TotalWebOrderKnet = webOrderKnet,
                TotalWebOrderCc = webOrderCc,
                TotalWebOrderReturn = webOrderReturns,
                TotalWebOrderCount = webOrderCount,

                TotalTalabatSales = totalTalabatSales.ToString("0.###"),
                TotalTalabatTransCount = talabatTransCount,

                TotalDeliverooSales = totalDeliverooSales.ToString("0.###"),
                TotalDeliverooTransCount = deliverooTransCount,

                OnlineSales = onlineSales,
                OnlineTransCount = onlineTransCount

            };

            string JSONresult = JsonConvert.SerializeObject(dashboardJson);
            return new ChartData
            {
                Chart1 = Json(JSONresult, JsonRequestBehavior.AllowGet),
                Chart2 = data2
            };
        }

        // used
        public ActionResult BranchSalesDetailRecord(string fromD, string toD)
        {
            var fromDate = DateTime.Parse(fromD);
            var toDate = DateTime.Parse(toD);

            var salesOfMonth = _dashboardRepository.GetSalesRecordDetailOfMonth(fromDate, toDate);

            ViewBag.DSTop5BranchProdByVal = salesOfMonth.Top5ProductsByAmount;
            ViewBag.DSTop5HoProdByVal = salesOfMonth.Top5HoProductsByAmount;

            ViewBag.DSTop5ProdByKg = salesOfMonth.Top5ProductsByKg;
            ViewBag.DSTop5ProdHoByKg = salesOfMonth.Top5ProductsHoByKg;

            ViewBag.DSTop5ProdByQty = salesOfMonth.Top5ProductsByQty;
            ViewBag.DSTop5ProdHoByQty = salesOfMonth.Top5ProductsHoByQty;

            ViewBag.Top10HoCustomerCredit = salesOfMonth.Top10HoCustomerCredit;
            ViewBag.Top10HoCustomerCash = salesOfMonth.Top10HoCustomerCash;

            return PartialView("_BranchSalesDetails");

        }

        [AllowAnonymous]
        public ActionResult BranchSalesForPrint(int reportType)
        {
            //var settings = reportType == 1
            //    ? _dashboardRepository.GetLastEmailSettings(true, false)
            //    : reportType == 2
            //    ? _dashboardRepository.GetLastEmailSettings(false, true)
            //    : _dashboardRepository.GetLastEmailSettings(false, false);

            //var todayDate = settings.LastEmailDate;

            var fromDate = DateTime.Now;
            var toDate = DateTime.Now;

            if (reportType == 1)
            {
                var backDate = DateTime.Now.Date.AddDays(-7);
                fromDate = new DateTime(backDate.Year, backDate.Month, backDate.Day, 03, 00, 00);
                toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 02, 59, 00);

                // fromDate = new DateTime(2021, 1, 1, 3, 0, 0);
                // toDate = new DateTime(2021, 1, 6, 2, 59, 0);
            }
            else if (reportType == 2)
            {
                //var lastDay = DateTime.DaysInMonth(todayDate.Year, todayDate.Month);
                //fromDate = new DateTime(todayDate.Year, todayDate.Month, 01, 00, 00, 00);
                //toDate = new DateTime(todayDate.Year, todayDate.Month, lastDay, 23, 59, 59);
            }
            else
            {
                fromDate = new DateTime(2021, 1, 1, 3, 0, 0);
                toDate = new DateTime(2021, 12, 31, 2, 59, 0);
            }

            //fromDate = new DateTime(2021, 01, 01, 12, 00, 00);
            //toDate = new DateTime(2021, 01, 01, 23, 59, 00);

            ViewBag.fromDate = fromDate.ToString();
            ViewBag.toDate = toDate.ToString();
            ViewBag.fromD = fromDate.ToString("dd/MMM/yyyy hh:mm tt");
            ViewBag.toD = toDate.ToString("dd/MMM/yyyy hh:mm tt");
            ViewBag.ReportType = reportType;

            if (toDate < fromDate) ViewBag.validation = "true";
            var data = BranchSalesData(fromDate.ToString(), toDate.ToString());

            ViewBag.jsonResult = data.Chart1;
            ViewBag.jsonResult2 = data.Chart2;

            return View();
        }

        [AllowAnonymous]
        public void GeneratePdf()
        {
            var fromDate = new DateTime(2021, 01, 01, 12, 00, 00);
            var toDate = new DateTime(2021, 01, 01, 23, 59, 59);

            var actionResult = new Rotativa.ActionAsPdf("BranchSalesForPrint", new { fromDate, toDate })
            {
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Landscape,
                IsLowQuality = false
            };
            var byteArray = actionResult.BuildPdf(ControllerContext);
            var pathName = "~/Content/Print/";
            var path = Path.Combine(Server.MapPath(pathName));
            var fileName = "Dashboard.pdf";
            var fullPath = Path.Combine(path, fileName);

            var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            fileStream.Write(byteArray, 0, byteArray.Length);
            fileStream.Close();

            //var body = BranchSales(fromDate, toDate);

            //MailMessage mailMessage = new MailMessage
            //{
            //    Subject = $"testing",
            //    Body = body.ToString(),
            //    IsBodyHtml = true,
            //    From = new MailAddress("imillmaterialreq@gmail.com"),
            //};


            //mailMessage.To.Add(new MailAddress("shabbir7252@gmail.com"));

            //SmtpClient smtp = new SmtpClient
            //{
            //    Host = "smtp.gmail.com",
            //    Port = 587,
            //    EnableSsl = true,
            //    DeliveryMethod = SmtpDeliveryMethod.Network,
            //    UseDefaultCredentials = false,
            //    Credentials = new NetworkCredential("imillmaterialreq@gmail.com", "M@ter!alReq$t")
            //};

            //smtp.Send(mailMessage);
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult SaveWeeklyDashboard()
        {
            try
            {
                var path = Path.Combine(Server.MapPath("~/Content/Print"), "dashboard-weekly.jpeg");
                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        var stream = fileContent.InputStream;
                        using (var fileStream = System.IO.File.Create(path))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                }

                string mailbody = "<html><body><div style=\"width:100%\"><img src=\"cid:Email\" style=\"width:100%\"></div></body></html>";
                // string mailbody = "<html lang='en'><head><meta charset='UTF-8'> <meta name='viewport' content='width=device-width, initial-scale=1.0'> <title>Intlmill Email</title></head><body> <table width='250px'> <tr> <td> <img src='cid:Email' alt='' style='width: 100%;'> </td></tr></table></body></html>";
                AlternateView AlternateView_Html = AlternateView.CreateAlternateViewFromString(mailbody, null, MediaTypeNames.Text.Html);
                var Picture1 = new LinkedResource(path, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = "Email"
                };
                AlternateView_Html.LinkedResources.Add(Picture1);
                var newAttachment = new Attachment(path);
                // newAttachment.ContentDisposition.Inline = true;


                var emails = _dashboardRepository.GetEmails();

                foreach (var item in emails)
                {
                    

                    MailMessage mailMessage = new MailMessage
                    {
                        Subject = $"Dashboard as Weekly Report",
                        Body = mailbody,
                        From = new MailAddress("imillmaterialreq@gmail.com"),
                        IsBodyHtml = true
                    };

                    mailMessage.AlternateViews.Add(AlternateView_Html);

                    mailMessage.Attachments.Add(newAttachment);

                    mailMessage.To.Add(new MailAddress(item.Email));

                    SmtpClient smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential("imillmaterialreq@gmail.com", "M@ter!alReq$t")
                    };

                    smtp.Send(mailMessage);

                    _dashboardRepository.UpdateWeeklyRptTransactions(item.Oid);
                }

                newAttachment.Dispose();
                AlternateView_Html.Dispose();
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json($"Upload failed : {ex.Message}");
            }

            return Json("File uploaded successfully");
        }

        [HttpGet]
        public bool SendEmailAsReport(bool isWeekly, bool isMonthly)
        {
            var settings = _dashboardRepository.GetLastEmailSettings(isWeekly, isMonthly);
            //var lastDate = settings.LastEmailDate;
            //// return true;
            //return lastDate.Date == DateTime.Now.Date &&
            //       DateTime.Now > new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 00, 00, 00) &&
            //       !settings.WeekRptEmailSent;

            return settings.WeekRptEmailSent;
        }


        [HttpGet]
        public bool SendWeeklyReport()
        {
            return _dashboardRepository.GetSettings();
        }

    }
}