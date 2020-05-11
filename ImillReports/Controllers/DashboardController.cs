using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using ImillReports.Models;
using ImillReports.Contracts;
using ImillReports.ViewModels;
using System.Collections.Generic;

namespace ImillReports.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardController(IDashboardRepository dashboardRepository) =>
            _dashboardRepository = dashboardRepository;

        public DashboardController() { }

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

            var jsonResult = BarChartData(fromDate.ToString(), toDate.ToString());
            ViewBag.jsonResult = jsonResult.Data;
            return View();
        }

        public JsonResult BarChartData(string fromD, string toD)
        {
            var fromDate = DateTime.Parse(fromD);
            var toDate = DateTime.Parse(toD);

            var salesOfMonth = _dashboardRepository.GetSalesOfMonth(fromDate, toDate);

            var labelsArray = new string[salesOfMonth.SalesMonthItems.Count];
            var dataArray = new decimal?[salesOfMonth.SalesMonthItems.Count];

            var colorArray = new string[]
            {
                //"#FF6633",
                "#FFB399", "#FF33FF", "#FFFF99", "#00B3E6",
                "#E6B333", "#3366E6", "#999966", "#99FF99", "#B34D4D",
                "#80B300", "#809900", "#E6B3B3", "#6680B3", "#66991A",
                "#FF99E6", "#CCFF1A", "#FF1A66", "#E6331A", "#33FFCC",
                "#66994D", "#B366CC", "#4D8000", "#B33300", "#CC80CC",
                "#66664D", "#991AFF", "#E666FF", "#4DB3FF", "#1AB399",
                "#E666B3", "#33991A", "#CC9999", "#B3B31A", "#00E680",
                "#4D8066", "#809980", "#E6FF80", "#1AFF33", "#999933",
                "#FF3380", "#CCCC00", "#66E64D", "#4D80CC", "#9900B3",
                "#E64D66", "#4DB380", "#FF4D4D", "#99E6E6"
                //, "#6666FF"
            };

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

            List<Datasets> _dataSet = new List<Datasets>();
            _dataSet.Add(new Datasets()
            {
                label = "",
                data = dataArray,
                backgroundColor = colorArray,
                borderColor = colorArray,
                borderWidth = "1"
            });
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

        public ActionResult BranchSales(DateTime? fromDate, DateTime? toDate)
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

            var jsonResult = BranchSalesData(fromDate.ToString(), toDate.ToString());
            ViewBag.jsonResult = jsonResult.Data;
            return View();

        }

        private JsonResult BranchSalesData(string fromD, string toD)
        {
            var fromDate = DateTime.Parse(fromD);
            var toDate = DateTime.Parse(toD);

            var salesOfMonth = _dashboardRepository.GetSalesOfMonth(fromDate, toDate);

            ViewBag.DSTop5BranchProdByVal = salesOfMonth.Top5ProductsByAmount;
            ViewBag.DSTop5HoProdByVal = salesOfMonth.Top5HoProductsByAmount;

            var totalSales = salesOfMonth.TotalSales ?? 0;
            var totalBranchSales = salesOfMonth.TotalBranchSales ?? 0;
            var totalCashSales = salesOfMonth.TotalBranchCash ?? 0;
            var totalKnetSales = salesOfMonth.TotalBranchKnet ?? 0;
            var totalCreditCardSales = salesOfMonth.TotalBranchCC ?? 0;
            var totalCarraigeSales = salesOfMonth.TotalBranchCarraige ?? 0;
            var totalOnlineSales = salesOfMonth.TotalBranchOnline ?? 0;
            var salesReturnBranches = salesOfMonth.SalesReturnBranches ?? 0;
            
            var totalHOSales = salesOfMonth.TotalHOSales ?? 0;
            var totalHOSalesCash = salesOfMonth.TotalHOSalesCash ?? 0;
            var totalHOSalesCredit = salesOfMonth.TotalHOSalesCredit ?? 0;
            var salesReturnHO = salesOfMonth.SalesReturnHO ?? 0;

            ViewBag.BRSalesPercent = 0;
            ViewBag.BRCashPercent = 0;
            ViewBag.BRKnetPercent = 0;
            ViewBag.BRCCPercent = 0;
            ViewBag.BRCarraigePercent = 0;
            ViewBag.BROnlinePercent = 0;
            ViewBag.BRsalesReturnPercent = 0;

            ViewBag.HOSalesPercent = 0;
            ViewBag.HOSalesCashPercent = 0;
            ViewBag.HOSalesCreditPercent = 0;
            ViewBag.HOSalesReturnPercent = 0;

            if (totalSales != 0)
            {
                ViewBag.BRSalesPercent = Math.Round(100 / totalSales * totalBranchSales, 2);
                ViewBag.BRCashPercent = Math.Round(100 / totalSales * totalCashSales, 2);
                ViewBag.BRKnetPercent = Math.Round(100 / totalSales * totalKnetSales, 2);
                ViewBag.BRCCPercent = Math.Round(100 / totalSales * totalCreditCardSales, 2);
                ViewBag.BRCarraigePercent = Math.Round(100 / totalSales * totalCarraigeSales, 2);
                ViewBag.BROnlinePercent = Math.Round(100 / totalSales * totalOnlineSales, 2);
                ViewBag.BRsalesReturnPercent = Math.Round(100 / totalSales * salesReturnBranches, 2);

                ViewBag.HOSalesPercent = Math.Round(100 / totalSales * totalHOSales, 2);
                ViewBag.HOSalesCashPercent = Math.Round(100 / totalSales * totalHOSalesCash, 2);
                ViewBag.HOSalesCreditPercent = Math.Round(100 / totalSales * totalHOSalesCredit, 2);
                ViewBag.HOSalesReturnPercent = Math.Round(100 / totalSales * salesReturnHO, 2);
            }

            var dashboardJson = new BranchSalesDashboardViewModel
            {
                TotalSales = totalSales.ToString("0.###"),
                TotalBranchSales = totalBranchSales.ToString("0.###"),
                TotalCashSales = totalCashSales.ToString("0.###"),
                TotalKnetSales = totalKnetSales.ToString("0.###"),
                TotalCreditCardSales = totalCreditCardSales.ToString("0.###"),
                TotalCarraigeSales = totalCarraigeSales.ToString("0.###"),
                TotalOnlineSales = totalOnlineSales.ToString("0.###"),
                SalesReturnBranches = salesReturnBranches.ToString("0.###"),

                TotalHOSales = totalHOSales.ToString("0.###"),
                TotalHOSalesCash = totalHOSalesCash.ToString("0.###"),
                TotalHOSalesCredit = totalHOSalesCredit.ToString("0.###"),
                SalesReturnHO = salesReturnHO.ToString("0.###")
            };

            string JSONresult = JsonConvert.SerializeObject(dashboardJson);
            return Json(JSONresult, JsonRequestBehavior.AllowGet);
        }

    }
}