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
    public class HomeController : Controller
    {
        private readonly IDashboardRepository _dashboardRepository;

        public HomeController(IDashboardRepository dashboardRepository) =>
            _dashboardRepository = dashboardRepository;

        public HomeController() { }

        public ActionResult Index(DateTime? fromDate, DateTime? toDate)
        {
            if (User.IsInRole("Sales"))
                return RedirectToAction("Index", "SalesReport");

            if (fromDate == null)
            {
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 03, 00, 00);
                ViewBag.fromD = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 03, 00, 00).ToString("dd/MMM/yyyy hh:mm tt");
            }
            else
            {
                ViewBag.fromD = fromDate.Value.ToString("dd/MMM/yyyy hh:mm tt");
            }

            if (toDate == null)
            {
                toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 02, 59, 00).AddDays(1);
                ViewBag.toD = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 02, 59, 00).AddDays(1).ToString("dd/MMM/yyyy hh:mm tt");
            }
            else
            {
                ViewBag.toD = toDate.Value.ToString("dd/MMM/yyyy hh:mm tt");
            }

            ViewBag.fromDate = fromDate.ToString();
            ViewBag.toDate = toDate.ToString();

            if (toDate < fromDate)
            {
                ViewBag.validation = "true";
            }

            var test = BarChartData(fromDate.ToString(), toDate.ToString());
            var json = test.Data;
            ViewBag.jsonResult = json;
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

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}