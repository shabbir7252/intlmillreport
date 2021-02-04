using System;
using System.Web.Mvc;
using ImillReports.Contracts;
using System.Collections.Generic;
using System.Linq;
using ImillReports.ViewModels;

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

        public ActionResult Index(DateTime? fromDate, DateTime? toDate, int[] employees, string type)
        {
            if (fromDate == null)
            {
                var fDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 03, 00, 00);
                fromDate = fDate;
                ViewBag.startDate = fDate;
            }
            else
            {
                fromDate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 00, 00, 00);
            }

            if (toDate == null)
            {
                var tDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 02, 59, 00).AddDays(1);
                toDate = tDate;
                ViewBag.endDate = tDate;
            }
            else if (toDate > DateTime.Now)
            {
                toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
            }
            else
            {
                toDate = new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, 23, 59, 59);
            }

            ViewBag.type = string.IsNullOrEmpty(type) ? "default" : type;

            ViewBag.startDate = fromDate;
            ViewBag.endDate = toDate;
            ViewBag.validation = "false";

            if (toDate < fromDate) ViewBag.validation = "true";

            GetTASync(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, toDate.Value.Year, toDate.Value.Month, toDate.Value.Day);

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

            var emps = employeesArray.Select(i => i.ToString()).ToArray();
            ViewBag.employeesVal = emps;

            var tAreport = _iTARepository.GetTAReport(fromDate, toDate, employees, type);
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

        public string SendShiftStartEmail()
        {
            return _iTARepository.SendShiftStartDetailReport();
        }

        public string SendShiftEndEmail()
        {
            return _iTARepository.SendShiftEndDetailReport();
        }

        [AllowAnonymous]
        public ActionResult GetTransactionDetails(List<TimeAttendanceViewModel> model)
        {
            return View(model);
        }


        [AllowAnonymous]
        [HttpGet]
        public string SyncHoDevice(int year, int month, int from, int toYear, int toMonth, int to, string ipAddress) {

            var fromDate = new DateTime(year, month, from, 0, 0, 0, DateTimeKind.Local);
            var toDate = new DateTime(toYear, toMonth, to, 23, 59, 59, DateTimeKind.Local);

            return _iTARepository.SyncHoDevice(fromDate, toDate, ipAddress);
        } 
    }
}