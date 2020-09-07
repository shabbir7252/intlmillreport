using System;
using System.Linq;
using System.Web.Mvc;
using Cash_Register.Contracts;
using Cash_Register.ViewModels;

namespace Cash_Register.Controllers
{
    public class CashRegisterController : Controller
    {
        private readonly DateTime SessionExpiryPeriod = DateTime.Now.AddMinutes(60);
        private readonly ISalesmanRepository _salesmanRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ICashRegisterRepository _cashRegisterRepository;

        public CashRegisterController(ISalesmanRepository salesmanRepository, 
            ILocationRepository locationRepository,
            ICashRegisterRepository cashRegisterRepository)
        {
            _salesmanRepository = salesmanRepository;
            _locationRepository = locationRepository;
            _cashRegisterRepository = cashRegisterRepository;
        }

        public ActionResult Index()
        {
            if (Request.Cookies["login"] != null && Request.Cookies["login"].Value == "true" &&
                Request.Cookies["CRLocationEn"] != null && Request.Cookies["CRLocationAr"] != null && Request.Cookies["locationId"] != null)
            {
                ViewBag.CRLocationEn = Request.Cookies["CRLocationEn"].Value;
                ViewBag.CRLocationAr = Server.UrlDecode(Request.Cookies["CRLocationAr"].Value);

                short locationId = short.Parse(Request.Cookies["locationId"].Value);
                ViewBag.Salesmans = _salesmanRepository.GetSalesmans(locationId);
                ViewBag.reserveAmount = _locationRepository.GetReserveAmount(locationId);

                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        public int SaveCashRegister(DateTime date, short salesman, string period, decimal? cheques, decimal? talabat,
            decimal? online, decimal? knet, decimal? visa, decimal? expense, int? twentykd, int? tenkd, int? fivekd, int? onekd,
            int? halfkd, int? quarterkd, int? hundfils, int? fiftyfils, int? twentyfils, int? tenfils, int? fivefils, decimal? netBalance)
        {
            var locationId = short.Parse(Request.Cookies["locationId"].Value);
            var shiftType = period == "evening" ? "pm" : "am";

            var shiftCount = new ShiftCount()
            {
                LocationId = locationId,
                ShiftType = shiftType,
                Salesman = salesman,
                StaffDate = date
            };

            var cRegister = new CRegister()
            {
                Oid = _cashRegisterRepository.GetOid(),
                StaffDate = date,
                TransDate = DateTime.Now,
                Cheques = cheques ?? 0,
                Expense = expense ?? 0,
                FiftyFils = fiftyfils ?? 0,
                FiveFils = fivefils ?? 0,
                FiveKd = fivekd ?? 0,
                HalfKd = halfkd ?? 0,
                HundFils = hundfils ?? 0,
                Knet = knet ?? 0,
                LocationId = locationId,
                NetBalance = netBalance ?? 0,
                OneKd = onekd ?? 0,
                Online = online ?? 0,
                ShiftType = shiftType,
                QuarterKd = quarterkd ?? 0,
                Reserve = _locationRepository.GetReserveAmount(locationId),
                Salesman = salesman,
                Talabat = talabat ?? 0,
                TenFils = tenfils ?? 0,
                TenKd = tenkd ?? 0,
                TwentyFils = twentyfils ?? 0,
                TwentyKd = twentykd ?? 0,
                Visa = visa ?? 0,
                ShiftCount = _cashRegisterRepository.GetShiftCount(shiftCount),
                IsDeleted = false,
                IsSynced = false
            };

            var result = _cashRegisterRepository.SaveCashRegister(cRegister);

            if (result != 0)
            {
                return result;
            }

            return -1;
        }


        public int UpdateCashRegister(int oid, DateTime date, short salesman, string period, decimal? cheques, decimal? talabat,
           decimal? online, decimal? knet, decimal? visa, decimal? expense, int? twentykd, int? tenkd, int? fivekd, int? onekd,
           int? halfkd, int? quarterkd, int? hundfils, int? fiftyfils, int? twentyfils, int? tenfils, int? fivefils, decimal? netBalance)
        {
            var locationId = short.Parse(Request.Cookies["locationId"].Value);
            var shiftType = period == "evening" ? "pm" : "am";

            var cRegister = new CRegister()
            {
                Oid = oid,
                Salesman = salesman,
                StaffDate = date,
                TransDate = DateTime.Now,
                Cheques = cheques ?? 0,
                Expense = expense ?? 0,
                FiftyFils = fiftyfils ?? 0,
                FiveFils = fivefils ?? 0,
                FiveKd = fivekd ?? 0,
                HalfKd = halfkd ?? 0,
                HundFils = hundfils ?? 0,
                Knet = knet ?? 0,
                LocationId = locationId,
                NetBalance = netBalance ?? 0,
                OneKd = onekd ?? 0,
                Online = online ?? 0,
                ShiftType = shiftType,
                QuarterKd = quarterkd ?? 0,
                Reserve = _locationRepository.GetReserveAmount(locationId),
                Talabat = talabat ?? 0,
                TenFils = tenfils ?? 0,
                TenKd = tenkd ?? 0,
                TwentyFils = twentyfils ?? 0,
                TwentyKd = twentykd ?? 0,
                Visa = visa ?? 0,
                ShiftCount = 1,
                IsDeleted = false,
                IsSynced = false
            };

            var result = _cashRegisterRepository.UpdateCashRegister(cRegister);

            if (result != 0)
            {
                return result;
            }

            return -1;
        }

        public ActionResult Edit(int oid)
        {
            var cRegister = _cashRegisterRepository.GetCashRegister(oid);
            ViewBag.Location = _locationRepository.GetLocation(cRegister.LocationId).NameAr;

            var salesman = _salesmanRepository.GetSalesman(cRegister.Salesman);
            ViewBag.Salesman = salesman.NameAr;
            ViewBag.SalesmanId = salesman.Sman_Cd;

            ViewBag.CRLocationEn = Request.Cookies["CRLocationEn"].Value;
            ViewBag.CRLocationAr = Server.UrlDecode(Request.Cookies["CRLocationAr"].Value);

            short locationId = short.Parse(Request.Cookies["locationId"].Value);
            ViewBag.Salesmans = _salesmanRepository.GetSalesmans(locationId);
            ViewBag.reserveAmount = _locationRepository.GetReserveAmount(locationId);

            return View(cRegister);
        }

        public ActionResult Print(int oid)
        {
            var cRegister = _cashRegisterRepository.GetCashRegister(oid);
            ViewBag.Location = _locationRepository.GetLocation(cRegister.LocationId).NameAr;
            ViewBag.Salesman = _salesmanRepository.GetSalesman(cRegister.Salesman).NameAr;
            return View(cRegister);
        }

        public ActionResult CRRecords(short salesmanOid)
        {
            ViewBag.Message = TempData["Message"];
            var records = _cashRegisterRepository.GetSalesmanCashRegister(salesmanOid);
            ViewBag.Salesman = _salesmanRepository.GetSalesman(salesmanOid).NameAr;
            ViewBag.CRLocationAr = Server.UrlDecode(Request.Cookies["CRLocationAr"].Value);
            return View(records);
        }

        public ActionResult Delete(int oid, short salesmanOid)
        {
            var result = _cashRegisterRepository.DeleteCashRegRecord(oid);
            if (result)
                TempData["Message"] = "Record Deleted!";
            else
                TempData["Message"] = "Could not delete the record!";
            return RedirectToAction("CRRecords", new { salesmanOid });
        }
    }
}