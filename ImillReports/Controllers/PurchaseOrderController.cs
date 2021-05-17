using System;
using System.Web.Mvc;
using ImillReports.Contracts;

namespace ImillReports.Controllers
{
    [Authorize]
    public class PurchaseOrderController : Controller
    {
        private readonly IPurchaseOrderRepo _purchaseOrderRepo;
        public PurchaseOrderController(IPurchaseOrderRepo purchaseOrderRepo)
        {
            _purchaseOrderRepo = purchaseOrderRepo;
        }

        public ActionResult Index(DateTime? fromDate, DateTime? toDate)
        {
            var username = User.Identity.Name;
            
            var from = username == "admin" 
                ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 00, 00, 00).AddDays(-7) 
                : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 00, 00, 00);

            var to = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

            fromDate = fromDate != null ? fromDate : from;
            toDate = toDate != null ? toDate : to;

            ViewBag.startDate = fromDate;
            ViewBag.endDate = toDate;

            ViewBag.Username = username;
            ViewBag.DataSource = _purchaseOrderRepo.GetPurchaseOrders(fromDate, toDate, username);

            return View();
        }

        [HttpPost]
        public bool UpdateLpoStatus(long oid, long entryId, int ldgrCd, string transDate, int lpoStatus, int paymentStatus, int qaStatus, 
            string gmcomment, string pmStatus, string qaRemarks, string username)
        {
            var recordDate = DateTime.Now;
            if (!string.IsNullOrEmpty(transDate))
                recordDate = Convert.ToDateTime(transDate, System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
            return _purchaseOrderRepo.UpdateLpoStatus(oid, ldgrCd, entryId, recordDate, lpoStatus, paymentStatus, qaStatus, gmcomment, 
                pmStatus, qaRemarks, username);
        }

        public ActionResult GetDetails(long entryId, string transDate)
        {
            var recordDate = DateTime.Now;
            if (!string.IsNullOrEmpty(transDate))
                recordDate = Convert.ToDateTime(transDate, System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
            ViewBag.DataSource = _purchaseOrderRepo.GetDetails(entryId, recordDate);
            return View();
        }


        [HttpGet]
        public bool EmailNewPurchaseOrder(int year, int month, int from, int to)
        {
            //var recordDate = DateTime.Now;
            //if (!string.IsNullOrEmpty(transDate))
            //    recordDate = Convert.ToDateTime(transDate, System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);

            //var fromDate = new DateTime(recordDate.Year, recordDate.Month, recordDate.Day, 00, 00, 00);
            //var toDate = new DateTime(recordDate.Year, recordDate.Month, recordDate.Day, 23, 59, 59);

            var fromDate = new DateTime(year, month, from, 00, 00, 00);
            var toDate = new DateTime(year, month, to, 23, 59, 59);

            return _purchaseOrderRepo.GetPurchaseEmailOrder(fromDate, toDate, User.Identity.Name);
        }
    }
}