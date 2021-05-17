using ImillReports.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImillReports.Controllers
{
    public class BankDepositController : Controller
    {
        private readonly IBankDepositRepo _bankDepositRepo;

        public BankDepositController(IBankDepositRepo bankDepositRepo)
        {
            _bankDepositRepo = bankDepositRepo;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Index(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate == null)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(-7).Day, 00, 00, 00);

            if (toDate == null)
            {
                // var daysInAMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
            }

            ViewBag.startDate = fromDate;
            ViewBag.endDate = toDate;

            ViewBag.transDate = DateTime.Now;
            ViewBag.DataSource = _bankDepositRepo.GetBankDeposits(fromDate, toDate);

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public bool Update(string transDate, long oid, decimal misc, decimal misc2, decimal cash, 
            string depositby, string comments)
        {
            var recordDate = DateTime.Now;
            if (!string.IsNullOrEmpty(transDate))
                recordDate = Convert.ToDateTime(transDate, System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);

            var bankDeposit = _bankDepositRepo.SaveTransaction(recordDate, oid, misc, misc2, cash, depositby, comments);

            return bankDeposit;
        }
    }
}