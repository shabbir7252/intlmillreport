using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using ImillReports.Contracts;
using System.Collections.Generic;
using ImillReports.ViewModels;

namespace ImillReports.Controllers
{
    [Authorize]
    public class CashRegisterController : Controller
    {

        private readonly ICashRegisterRepository _cashRegisterRepository;

        public CashRegisterController(ICashRegisterRepository cashRegisterRepository) =>
            _cashRegisterRepository = cashRegisterRepository;

        public CashRegisterController() { }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Index(DateTime? fromDate, DateTime? toDate, bool? takeCount)
        {
            // var _takeCount = takeCount == null ? false : takeCount == true ? true : false;
            var _takeCount = takeCount != null && takeCount == true;

            if (fromDate == null)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 03, 00, 00);

            if (toDate == null)
                toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 02, 59, 00).AddDays(1);

            ViewBag.startDate = fromDate;
            ViewBag.endDate = toDate;
            ViewBag.validation = "false";
            ViewBag.takeCount = _takeCount;

            if (toDate < fromDate) ViewBag.validation = "true";

            var cashRegister = _cashRegisterRepository.GetCashRegister(fromDate, toDate, _takeCount);
            ViewBag.DataSource = cashRegister.CashRegisterItems;

            return View();
        }

        public ContentResult GetCashRegister(string from, string to)
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

            var cashRegister = _cashRegisterRepository.GetCashRegister(fromDate, toDate, false);
            var source = cashRegister.CashRegisterItems;

            //var serializer = new JavaScriptSerializer
            //{
            //    MaxJsonLength = Int32.MaxValue
            //};

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult CashRegVsSalesRpt(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate == null)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 03, 00, 00);

            if (toDate == null)
                toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 02, 59, 00).AddDays(1);

            ViewBag.startDate = fromDate;
            ViewBag.endDate = toDate;
            ViewBag.validation = "false";

            if (toDate < fromDate) ViewBag.validation = "true";

            var cashRegVsSalesRpt = _cashRegisterRepository.GetCashRegisterVsSalesRpt(fromDate, toDate);

            ViewBag.DataSource = cashRegVsSalesRpt.CashRegVsSalesItems;

            return View();
        }

        [HttpPost]
        public JsonResult UpdateVerifiedIds(List<int> verifiedIds, List<int> deVerifiedIds)
        {
            var result = _cashRegisterRepository.UpdateVerifiedIds(verifiedIds, deVerifiedIds);
            return Json(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public string Update(int oid, string staffDate, string shiftType, decimal talabat,
                                   decimal deliveroo, decimal cheque, decimal online, decimal knet,
                                   decimal visa, decimal cash, decimal expense, decimal reserve)
        {
            var cashRegUpdateVM = new CashRegUpdateVM
            {
                Oid = oid,
                Cash = cash,
                Cheque = cheque,
                Deliveroo = deliveroo,
                Expense = expense,
                Knet = knet,
                Online = online,
                Reserve = reserve,
                ShiftType = shiftType,
                StaffDate = staffDate,
                Talabat = talabat,
                Visa = visa
            };

           //  System.IO.StreamWriter path = System.IO.File.CreateText(Server.MapPath("~/App_Data/cr_log.json"));

            return _cashRegisterRepository.Update(cashRegUpdateVM, Server.MapPath("~/App_Data/cr_log.json"));
        }

    }
}