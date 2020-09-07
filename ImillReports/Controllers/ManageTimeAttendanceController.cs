using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using ImillReports.Contracts;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System;

namespace ImillReports.Controllers
{
    public class ManageTimeAttendanceController : Controller
    {
        private readonly ITARepository _iTARepository;

        public ManageTimeAttendanceController(ITARepository iTARepository)
        {
            _iTARepository = iTARepository;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AllocateShiftLocation()
        {
            ViewBag.Employees = _iTARepository.GetEmployees();
            ViewBag.Shifts = _iTARepository.GetShifts().OrderBy(x => x.Code);
            ViewBag.Locations = _iTARepository.GetLocations().OrderBy(x => x.NameEn);
            ViewBag.DataSource = _iTARepository.GetEmpAllocations();
            return View();
        }

        [HttpPost]
        public ContentResult AllocateShiftLocation(string locations, string shifts, List<int> verifiedIds)
        {

            _iTARepository.UpdateEmpAllocations(locations, shifts, verifiedIds);

            var source = _iTARepository.GetEmpAllocations();
            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Employees()
        {
            ViewBag.DataSource = _iTARepository.GetEmployees();
            return View();
        }

        [HttpPost]
        public ContentResult AddEmployee(int empId, string NameEn, string NameAr)
        {
            _iTARepository.AddEmployee(empId, NameEn, NameAr);

            var source = _iTARepository.GetEmployees();
            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpPost]
        public ContentResult DeleteEmployees(List<int> verifiedIds)
        {
            _iTARepository.DeleteEmployees(verifiedIds);

            var source = _iTARepository.GetEmployees();
            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Locations()
        {
            ViewBag.DataSource = _iTARepository.GetLocations();
            return View();
        }

        [HttpPost]
        public ContentResult AddLocation(string deviceCode, string NameEn, string NameAr)
        {
            _iTARepository.AddLocations(deviceCode, NameEn, NameAr);

            var source = _iTARepository.GetLocations();
            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpPost]
        public ContentResult DeleteLocations(List<int> verifiedIds)
        {
            _iTARepository.DeleteLocations(verifiedIds);

            var source = _iTARepository.GetLocations();
            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Shifts()
        {
            ViewBag.DataSource = _iTARepository.GetShifts();
            return View();
        }

        [HttpPost]
        public ContentResult AddShift(string code, string startTime, string endTime)
        {
            // var test = TimeSpan.Parse(startTime);
            _iTARepository.AddShift(code, TimeSpan.Parse(startTime), TimeSpan.Parse(endTime));

            var source = _iTARepository.GetShifts();
            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpPost]
        public ContentResult DeleteShifts(List<int> verifiedIds)
        {
            _iTARepository.DeleteShifts(verifiedIds);

            var source = _iTARepository.GetShifts();
            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpPost]
        public int AddAllocation(string fromDate, string toDate, List<int> employees, string shift, string location)
        {
            var from = (string.IsNullOrEmpty(fromDate) ? DateTime.Parse("2020-06-01") : DateTime.Parse(fromDate)).AddMonths(1);
            var to = string.IsNullOrEmpty(toDate) ? DateTime.Parse("9999-12-31") : DateTime.Parse(toDate).AddMonths(1);
            return _iTARepository.AddAllocation(from, to, employees, shift, location);
        }

        [HttpPost]
        public void DeleteAllocations(List<int> verifiedIds)
        {
            _iTARepository.DeleteAllocations(verifiedIds);
        }
    }
}