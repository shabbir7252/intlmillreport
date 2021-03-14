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
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 7, 00, 00, 00);

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
        public ActionResult SaveTransaction(DateTime? transDate, decimal salheya, decimal salmiya, decimal nuzha,
            decimal dahiya, decimal khaldiya, decimal qortuba, decimal kaifan, decimal avenues, decimal kpc, decimal yarmouk,
            decimal zahara, decimal mall360, decimal dasma, decimal gate, decimal shahuda, decimal surra, decimal hamra, decimal bnied,
            decimal jahra, decimal kout, decimal av4, decimal fintas, decimal misc, decimal misc2, decimal cash, string depositby,
            string comments)
        {
            if (transDate == null)
                transDate = DateTime.Now;

            var bankDeposit = _bankDepositRepo.SaveTransaction(transDate.Value, salheya, salmiya, nuzha, dahiya, khaldiya, qortuba,
                kaifan, avenues, kpc, yarmouk, zahara, mall360, dasma, gate, shahuda, surra, hamra, bnied, jahra, kout, av4, fintas, 
                misc, misc2, cash, depositby, comments);


            return RedirectToAction("Index");
        }
    }
}