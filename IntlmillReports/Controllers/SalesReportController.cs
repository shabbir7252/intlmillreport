using IntlmillReports.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace IntlmillReports.Controllers
{
    [Authorize]
    public class SalesReportController : Controller
    {
        private readonly ISalesReportRepository _salesReportRepository;
        private readonly ILocationRepository _locationRepository;
        public SalesReportController(
            ISalesReportRepository salesReportRepository,
            ILocationRepository locationRepository
            )
        {
            _salesReportRepository = salesReportRepository;
            _locationRepository = locationRepository;
        }

        public SalesReportController() { }

        [HttpGet]
        public ActionResult Index(DateTime? fromDate, DateTime? toDate, int? locationId)
        {
            //var fromDate = new DateTime(2019, 12, 05).ToString();
            //var toDate = new DateTime(2019, 12, 08).ToString();
            //var locationId = 0;

            if (fromDate == null)
            {
                fromDate = DateTime.Now;
                ViewBag.startDate = DateTime.Now;
            }

            if (toDate == null)
            {
                toDate = DateTime.Now;
                ViewBag.endDate = DateTime.Now;
            }

            ViewBag.startDate = fromDate;
            ViewBag.endDate = toDate;

            var locations = _locationRepository.GetLocations();
            ViewBag.locations = new SelectList(locations.LocationItems, "LocationId", "Name");

            var salesReportViewModel = _salesReportRepository.GetSalesReport(fromDate, toDate, locationId);
            ViewBag.DataSource = salesReportViewModel.SalesReportItems;
            return View();
        }

        public ContentResult GetSalesReport(DateTime fromDate, DateTime toDate, int? locationId)
        {
            var salesReportViewModel = _salesReportRepository.GetSalesReport(fromDate, toDate, locationId);

            var serializer = new JavaScriptSerializer();

            // For simplicity just use Int32's max value.
            // You could always read the value from the config section mentioned above.
            serializer.MaxJsonLength = Int32.MaxValue;

            var result = new ContentResult
            {
                Content = serializer.Serialize(salesReportViewModel.SalesReportItems),
                ContentType = "application/json"
            };

            return result;
            //var json = JsonConvert.SerializeObject(salesReportViewModel.SalesReportItems);
            //return Json(json, JsonRequestBehavior.AllowGet); 
        }
    }
}