using System;
using System.Web.Mvc;
using ImillReports.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace ImillReports.Controllers
{
    public class TAReportController : Controller
    {
        private readonly ITARepository _iTARepository;

        public TAReportController(ITARepository iTARepository)
        {
            _iTARepository = iTARepository;
        }

        public TAReportController()
        {
        }

        public ActionResult Index(DateTime? fromDate, DateTime? toDate, int[] employees)
        {

            if (fromDate == null)
            {
                var fDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 03, 00, 00);
                fromDate = fDate;
                ViewBag.startDate = fDate;
            } else
            {
                fromDate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 00, 00, 00);
            }

            if (toDate == null)
            {
                var tDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 02, 59, 00).AddDays(1);
                toDate = tDate;
                ViewBag.endDate = tDate;
            }
            else
            {
                toDate = new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, 23, 59, 59);
            }

            ViewBag.startDate = fromDate;
            ViewBag.endDate = toDate;
            ViewBag.validation = "false";

            if (toDate < fromDate) ViewBag.validation = "true";

            var employeeList = _iTARepository.GetEmployees();
            ViewBag.Employees = employeeList;

            var employeesArray = new List<int>();
            if (employees != null)
            {
                employeesArray.AddRange(from item in employees select item);
                foreach (var currentLocation in
                    from employee in employeeList
                    from id in employeesArray
                    where employee.EmployeeId == id
                    select employee)
                {
                    currentLocation.IsSelected = true;
                }
            }

            ViewBag.locationVal = employeesArray;


            var tAreport = _iTARepository.GetTAReport(fromDate, toDate, employees);
            ViewBag.DataSource = tAreport;
            return View();
        }

        public string GetTASync(int year, int month, int from, int toYear, int toMonth, int to)
        {
            return $"{_iTARepository.SyncTAReport(year, month, from, toYear, toMonth, to)} Date : ({from}-{month}-{year}) to ({to}-{toMonth}-{toYear})";
        }

        [HttpPost]
        public bool DeleteTransactions(List<int> verifiedIds)
        {
            _iTARepository.DeleteTransactions(verifiedIds);
            return true;
        }
    }
}