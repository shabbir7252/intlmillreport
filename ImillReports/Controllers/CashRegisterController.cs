using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using ImillReports.Contracts;
using ImillReports.ViewModels;
using System.Collections.Generic;

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
        public ActionResult Index(DateTime? fromDate, DateTime? toDate, bool? takeCount)
        {
            var _takeCount = takeCount == null ? false : takeCount == true ? true : false;

            if (fromDate == null)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 04, 00, 00);

            if (toDate == null)
                toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 01, 00, 00).AddDays(1);

            ViewBag.startDate = fromDate;
            ViewBag.endDate = toDate;
            ViewBag.validation = "false";
            ViewBag.takeCount = _takeCount;

            if (toDate < fromDate) ViewBag.validation = "true";


            if (fromDate.Value.Date == DateTime.Now.Date || fromDate == null)
            {
                var cashRegisterItems = new List<CashRegisterItem>();
                var cashRegisterItem = new CashRegisterItem
                {
                    Carriage = 0,
                    Cash = 0,
                    Cheque = 0,
                    Expense = 0,
                    Knet = 0,
                    Location = "",
                    NetAmount = 0,
                    NetSales = 0,
                    Online = 0,
                    Reserve = 0,
                    Salesman = "",
                    ShiftCount = 0,
                    ShiftType = "",
                    TotalSales = 0,
                    TransDate = DateTime.Now,
                    TransDateTime = DateTime.Now,
                    Visa = 0
                };

                cashRegisterItems.Add(cashRegisterItem);
                ViewBag.DataSource = cashRegisterItems;
                return View();
            }


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
        public ActionResult CashRegVsSalesRpt(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate == null)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 04, 00, 00);

            if (toDate == null)
                toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 01, 00, 00).AddDays(1);

            ViewBag.startDate = fromDate;
            ViewBag.endDate = toDate;
            ViewBag.validation = "false";

            if (toDate < fromDate) ViewBag.validation = "true";
            
            var cashRegVsSalesRpt = _cashRegisterRepository.GetCashRegisterVsSalesRpt(fromDate, toDate);

            ViewBag.DataSource = cashRegVsSalesRpt.CashRegVsSalesItems;

            return View();
        }

    }
}