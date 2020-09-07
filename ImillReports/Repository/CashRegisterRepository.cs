using System;
using System.Linq;
using ImillReports.Models;
using ImillReports.Contracts;
using ImillReports.ViewModels;
using System.Collections.Generic;
using System.Data.Entity.Migrations;

namespace ImillReports.Repository
{
    public class CashRegisterRepository : ICashRegisterRepository
    {
        private readonly IMILLEntities _context;
        private readonly ILocationRepository _locationRepository;
        private readonly ISalesReportRepository _salesReportRepository;
        private readonly IBaseRepository _baseRepository;

        public CashRegisterRepository(
            IMILLEntities context,
            ISalesReportRepository salesReportRepository,
            ILocationRepository locationRepository,
            IBaseRepository baseRepository)
        {
            _context = context;
            _locationRepository = locationRepository;
            _salesReportRepository = salesReportRepository;
            _baseRepository = baseRepository;
        }

        private List<CashRegisterItem> GetCashRegisterItems(List<intlmill_cash_register> cashRegister)
        {
            var cashRegisterItems = new List<CashRegisterItem>();
            var locations = _locationRepository.GetLocations().LocationItems;
            var locationName = "";
            short? locationId = 0;
            var locationShort = "";

            foreach (var item in cashRegister)
            {

                if (item.Locat_Cd != null || item.Locat_Cd.HasValue)
                {
                    locationId = item.Locat_Cd.Value;
                    locationName = locations.FirstOrDefault(a => a.LocationId == item.Locat_Cd.Value).NameAr;
                    locationShort = locations.FirstOrDefault(a => a.LocationId == item.Locat_Cd.Value).ShortName;
                } else
                {
                    var keyName = item.location == "الأفنيوز"
                        ? "مجمع الأفنيوز"
                        : item.location == "شارع بغداد - السالمية "
                            ? "السالمية - شارع بغداد"
                            : item.location == "ضاحية عبدالله السالم "
                                ? "جمعية ضاحية عبدالله السالم"
                                : item.location == "بنيد القار"
                                    ? "جمعية بنيد القار"
                                    : item.location == "شهداء"
                                        ? "جمعية الشهداء"
                                        : item.location;

                    locationName = keyName;
                    locationShort = locations.FirstOrDefault(a => a.NameAr == keyName).ShortName;
                }

                var staffDate = item.staff_date != null
                ? Convert.ToDateTime(item.staff_date, System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat) : DateTime.Now;
                // var locationName = item.location.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
                var cashRegisterItem = new CashRegisterItem();

                cashRegisterItem.Oid = item.id;
                cashRegisterItem.Carriage = item.carriage;
                cashRegisterItem.Cash = item.total_cash;
                cashRegisterItem.Cheque = item.cheque;
                cashRegisterItem.Knet = item.knet;
                cashRegisterItem.Expense = item.expense;
                cashRegisterItem.LocationId = locationId;
                cashRegisterItem.Location = locationName;
                cashRegisterItem.LocationShortName = locationShort;
                cashRegisterItem.Online = item.online;
                cashRegisterItem.Salesman = item.salesman;
                cashRegisterItem.TransDateTime = item.trans_date_time;
                cashRegisterItem.Visa = item.visa;
                cashRegisterItem.Reserve = item.reserve;
                cashRegisterItem.ShiftCount = item.shift_count;
                cashRegisterItem.ShiftType = item.shift_type;
                cashRegisterItem.TransDate = item.trans_date_time.Date;
                cashRegisterItem.StaffDate = staffDate;
                cashRegisterItem.IsVerified = item.IsVerified == null ? false : item.IsVerified;

                var totalSales = item.cheque + item.carriage + item.online + item.knet + item.visa + item.total_cash + item.expense;
                var netSales = totalSales - item.reserve;
                cashRegisterItem.TotalSales = totalSales;
                cashRegisterItem.NetSales = netSales;
                // cashRegisterItem.NetAmount = item.total_cash - item.reserve - item.expense;
                cashRegisterItem.NetAmount = item.total_cash - item.reserve;

                cashRegisterItem.SalesmanType = item.salesman + "-" + item.shift_type;

                if (!(totalSales <= 0 || netSales <= 0))
                    cashRegisterItems.Add(cashRegisterItem);
            }

            return cashRegisterItems;
        }

        public CashRegisterViewModel GetCashRegister(DateTime? fromDate, DateTime? toDate, bool takeCount)
        {

            var fromDateDed1 = fromDate.Value.AddDays(-3);
            var toDateAdd1 = toDate.Value.AddDays(3);

            var staffDateCRContext = _context.intlmill_cash_register.Where(x => x.trans_date_time >= fromDateDed1 &&
                                                                                x.trans_date_time <= toDateAdd1 &&
                                                                                // !string.IsNullOrEmpty(x.staff_date) &&
                                                                                x.location != "نموذج" &&
                                                                                x.location != "نموذج\r\n").ToList();

            foreach (var rec in staffDateCRContext)
            {
                if (string.IsNullOrEmpty(rec.shift_type))
                {
                    var startTime = new TimeSpan(06, 00, 00);
                    var endTime = new TimeSpan(19, 00, 00);
                    rec.shift_type = rec.trans_date_time.TimeOfDay >= startTime && rec.trans_date_time.TimeOfDay <= endTime ? "am" : "pm";
                }

                if (string.IsNullOrEmpty(rec.staff_date))
                {
                    rec.staff_date = rec.trans_date_time.Date.ToString("dd-MM-yyyy");
                    rec.ActualStaffDate = rec.trans_date_time.Date;
                }
                else
                {
                    //if (_baseRepository.CheckDate(rec.staff_date))
                    //{
                    try
                    {
                        var staffDate = Convert.ToDateTime(rec.staff_date, System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
                        rec.ActualStaffDate = staffDate;

                    }
                    catch (Exception ex)
                    {
                        rec.ActualStaffDate = rec.trans_date_time.Date;
                        rec.staff_date = rec.trans_date_time.Date.ToString("dd-MM-yyyy");
                    }
                    //}
                    //else
                    //{
                    //    rec.ActualStaffDate = rec.trans_date_time.Date;
                    //    rec.staff_date = rec.trans_date_time.Date.ToString("dd-MM-yyyy");
                    //}
                }
            }

            _context.SaveChanges();

            var from = fromDate.Value.Date;
            var to = toDate.Value.Date;

            var cashRegister = staffDateCRContext.Where(x => x.ActualStaffDate >= from && x.ActualStaffDate <= to).ToList();

            var result = GetCashRegisterItems(cashRegister);

            if (!takeCount)
            {
                // cashRegister = cashRegister.GroupBy(x => new { x.salesman }).Select(x => x.OrderByDescending(t => t.shift_count).FirstOrDefault());
                var modifiedResult = result.GroupBy(x => new { x.StaffDate, x.SalesmanType }).Select(x => x.OrderByDescending(t => t.ShiftCount).FirstOrDefault());
                return new CashRegisterViewModel
                {
                    CashRegisterItems = modifiedResult.ToList()
                };
            }

            // var cashRegisterItems = new List<CashRegisterItem>();

            //foreach (var item in cashRegister)
            //{
            //    var cashRegisterItem = new CashRegisterItem
            //    {
            //        Carriage = item.carriage,
            //        Cash = item.total_cash,
            //        Cheque = item.cheque,
            //        Knet = item.knet,
            //        Expense = item.expense,
            //        Location = item.location,
            //        Online = item.online,
            //        Salesman = item.salesman,
            //        TransDateTime = item.trans_date_time,
            //        Visa = item.visa,
            //        Reserve = item.reserve,
            //        ShiftCount = item.shift_count
            //    };

            //    var totalSales = item.cheque + item.carriage + item.online + item.knet + item.visa + item.total_cash;
            //    cashRegisterItem.TotalSales = totalSales;
            //    cashRegisterItem.NetSales = totalSales - item.reserve;
            //    cashRegisterItem.NetAmount = item.total_cash - item.reserve - item.expense;

            //    cashRegisterItems.Add(cashRegisterItem);
            //}

            return new CashRegisterViewModel
            {
                CashRegisterItems = result
            };
        }

        public CashRegVsSalesViewModel GetCashRegisterVsSalesRpt(DateTime? fromDate, DateTime? toDate)
        {
            //var date = fromDate.Value.Date.ToString("dd-MM-yyyy");
            //var cashRegister = _context.intlmill_cash_register.Where(x => x.staff_date == date && x.staff_date != null && (x.location != "نموذج" && x.location != "نموذج\r\n")).ToList();

            //var result = GetCashRegisterItems(cashRegister);

            //foreach (var item in result)
            //{
            //    if (item.StaffDate.Date != fromDate.Value.Date)
            //        item.StaffDate = fromDate.Value.Date;
            //}

            // var modifiedResult = result.GroupBy(x => new { x.Salesman, x.StaffDate }).Select(x => x.OrderByDescending(t => t.ShiftCount).FirstOrDefault());

            var result = GetCashRegister(fromDate, fromDate, false).CashRegisterItems;

            var cashRegisters = result.GroupBy(x => new { x.Location, x.StaffDate });

            var locations = _locationRepository.GetLocations().LocationItems;

            var cashRegVsSalesItems = new List<CashRegVsSalesItem>();

            foreach (var item in cashRegisters)
            {
                var locationId = _locationRepository.GetLocations().LocationItems.FirstOrDefault(x => x.NameAr == item.Key.Location).LocationId;

                var salesReportFromDate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 05, 00, 00);
                var salesReportToDate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 04, 59, 00).AddDays(1);

                var salesReportItems = _salesReportRepository.GetSalesTransaction(salesReportFromDate, salesReportToDate, locationId.ToString(), "").SalesReportItems;

                var crKnet = item.Sum(x => x.Knet);
                var crVisa = item.Sum(x => x.Visa);
                var crOnline = item.Sum(x => x.Online);
                var crTalabat = item.Sum(x => x.Carriage);
                var crReserve = item.Sum(x => x.Reserve);
                var crExpense = item.Sum(x => x.Expense);
                var crCash = item.Sum(x => x.NetAmount) + crExpense;

                var totalCrSales = crKnet + crVisa + crOnline + crTalabat + crCash;


                #region Innova Talabat

                var talabatCash = salesReportItems.Where(a => a.VoucherId == 2025).Sum(a => a.Cash);
                var talabatCashReturn = salesReportItems.Where(a => a.VoucherId == 2035).Sum(a => a.Cash);
                var talabatCashNet = talabatCash - Math.Abs(talabatCashReturn != null ? (decimal)talabatCashReturn : 0);

                var talabatKnet = salesReportItems.Where(a => a.VoucherId == 2025).Sum(a => a.Knet);
                var talabatKnetReturn = salesReportItems.Where(a => a.VoucherId == 2035).Sum(a => a.Knet);
                var talabatKnetNet = talabatKnet - Math.Abs(talabatKnetReturn != null ? (decimal)talabatKnetReturn : 0);

                var talabatCc = salesReportItems.Where(a => a.VoucherId == 2025).Sum(a => a.CreditCard);
                var talabatCcReturn = salesReportItems.Where(a => a.VoucherId == 2035).Sum(a => a.CreditCard);
                var talabatCcNet = talabatCc - Math.Abs(talabatCcReturn != null ? (decimal)talabatCcReturn : 0);

                var talabatNet = talabatCashNet + talabatKnetNet + talabatCcNet;

                var talabat = talabatNet != null ? (decimal)talabatNet : 0;

                #endregion

                #region Innova Online
                var onlineCash = salesReportItems.Where(a => a.VoucherId == 2026).Sum(a => a.Cash);
                var onlineCashReturn = salesReportItems.Where(a => a.VoucherId == 2036).Sum(a => a.Cash);
                var onlineCashNet = onlineCash - Math.Abs(onlineCashReturn != null ? (decimal)onlineCashReturn : 0);

                var onlineKnet = salesReportItems.Where(a => a.VoucherId == 2026).Sum(a => a.Knet);
                var onlineKnetReturn = salesReportItems.Where(a => a.VoucherId == 2036).Sum(a => a.Knet);
                var onlineKnetNet = onlineKnet - Math.Abs(onlineKnetReturn != null ? (decimal)onlineKnetReturn : 0);

                var onlineCc = salesReportItems.Where(a => a.VoucherId == 2026).Sum(a => a.CreditCard);
                var onlineCcReturn = salesReportItems.Where(a => a.VoucherId == 2036).Sum(a => a.CreditCard);
                var onlineCcNet = onlineCc - Math.Abs(onlineCcReturn != null ? (decimal)onlineCcReturn : 0);

                var onlineNet = onlineCashNet + onlineKnetNet + onlineCcNet;

                var online = onlineNet != null ? (decimal)onlineNet : 0;
                #endregion

                #region Innova Sales Return

                var cReturn = salesReportItems.Where(a => a.VoucherId == 2023 || a.VoucherId == 202).Sum(a => a.Cash);
                var cashReturn = cReturn != null ? (decimal)cReturn : 0;

                var kReturn = salesReportItems.Where(a => a.VoucherId == 2023 || a.VoucherId == 202).Sum(a => a.Knet);
                var knetReturn = kReturn != null ? (decimal)kReturn : 0;

                var creditReturn = salesReportItems.Where(a => a.VoucherId == 2023 || a.VoucherId == 202).Sum(a => a.CreditCard);
                var ccReturn = creditReturn != null ? (decimal)creditReturn : 0;

                #endregion

                var cash = salesReportItems.Sum(a => a.Cash) - onlineCashNet - talabatCashNet - Math.Abs(knetReturn);
                var knet = salesReportItems.Sum(a => a.Knet) - onlineKnetNet - talabatKnetNet + Math.Abs(knetReturn);
                var cc = salesReportItems.Sum(a => a.CreditCard) - onlineCcNet - talabatCcNet;
                var totalInnovaSales = cash + knet + cc + talabat + online;

                var cashRegVsSalesItem = new CashRegVsSalesItem
                {
                    Location = item.Key.Location,
                    LocationShortName = locations.FirstOrDefault(a => a.NameAr == item.Key.Location) != null
                                                ? locations.FirstOrDefault(a => a.NameAr == item.Key.Location).ShortName
                                                : "",
                    TransDate = item.Key.StaffDate,

                    CRCash = crCash,
                    CRKnet = crKnet,
                    CRVisa = crVisa,
                    CROnline = crOnline,
                    CRTalabat = crTalabat,
                    CRReserve = crReserve,
                    CRKnetVisa = crKnet + crVisa,
                    TotalCRSales = totalCrSales,

                    Cash = cash,
                    Knet = knet,
                    CreditCard = cc,
                    Online = online,
                    Talabat = talabat,
                    KnetVisa = knet + cc,
                    TotalInnovaSales = totalInnovaSales,

                    CashDiff = crCash - cash,
                    KnetDiff = crKnet - knet,
                    CcDiff = crVisa - cc,
                    TalabatDiff = crTalabat - talabat,
                    OnlineDiff = crOnline - online,
                    KnetVisaDiff = (crKnet + crVisa) - (knet + cc),
                    TotalDiff = totalCrSales - totalInnovaSales

                };

                cashRegVsSalesItems.Add(cashRegVsSalesItem);
            }

            return new CashRegVsSalesViewModel
            {
                CashRegVsSalesItems = cashRegVsSalesItems
            };
        }

        public bool UpdateVerifiedIds(List<int> verifiedIds, List<int> deVerifiedIds)
        {
            try
            {

                IQueryable<intlmill_cash_register> cashRegisters = null;

                if (verifiedIds == null && deVerifiedIds == null)
                    return false;
                else if (verifiedIds == null && deVerifiedIds != null)
                    cashRegisters = _context.intlmill_cash_register.Where(x => deVerifiedIds.Contains(x.id));
                else if (verifiedIds != null && deVerifiedIds == null)
                    cashRegisters = _context.intlmill_cash_register.Where(x => verifiedIds.Contains(x.id));
                else
                    cashRegisters = _context.intlmill_cash_register.Where(x => verifiedIds.Contains(x.id) || deVerifiedIds.Contains(x.id));


                if (cashRegisters != null)
                {
                    foreach (var record in cashRegisters)
                    {
                        if (verifiedIds != null)
                            foreach (var id in verifiedIds)
                                if (id == record.id)
                                    record.IsVerified = true;

                        if (deVerifiedIds != null)
                            foreach (var id in deVerifiedIds)
                                if (id == record.id)
                                    record.IsVerified = false;
                    }

                    _context.SaveChanges();
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}