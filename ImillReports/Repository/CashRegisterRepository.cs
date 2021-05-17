using System;
using System.Linq;
using ImillReports.Models;
using ImillReports.Contracts;
using ImillReports.ViewModels;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace ImillReports.Repository
{
    public class CashRegisterRepository : ICashRegisterRepository
    {
        private readonly IMILLEntities _context;
        private readonly ILocationRepository _locationRepository;
        private readonly ISalesReportRepository _salesReportRepository;

        public CashRegisterRepository(
            IMILLEntities context,
            ISalesReportRepository salesReportRepository,
            ILocationRepository locationRepository)
        {
            _context = context;
            _locationRepository = locationRepository;
            _salesReportRepository = salesReportRepository;
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
                }
                else
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

                var deliveroo = item.deliveroo == null ? 0 : item.deliveroo;

                cashRegisterItem.Oid = item.id;
                cashRegisterItem.Carriage = item.carriage;
                cashRegisterItem.Deliveroo = deliveroo;
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

                var totalSales = item.cheque + item.carriage + deliveroo + item.online + item.knet + item.visa + item.total_cash + item.expense;
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
                                                                                (!x.IsDeleted.Value || x.IsDeleted == null) &&
                                                                                x.location != "نموذج" &&
                                                                                x.location != "نموذج\r\n" &&
                                                                                x.Locat_Cd != 73).ToList();

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
                    try
                    {
                        var staffDate = Convert.ToDateTime(rec.staff_date, System.Globalization.CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat);
                        rec.ActualStaffDate = staffDate;

                    }
                    catch (Exception ex)
                    {
                        rec.ActualStaffDate = rec.trans_date_time.Date;
                        rec.staff_date = rec.trans_date_time.Date.ToString("dd-MM-yyyy");
                        Console.WriteLine(ex);
                    }
                }
            }

            _context.SaveChanges();

            var from = fromDate.Value.Date;
            var to = toDate.Value.Date;

            var cashRegister = staffDateCRContext.Where(x => x.ActualStaffDate >= from && x.ActualStaffDate <= to).ToList();

            var result = GetCashRegisterItems(cashRegister);

            if (!takeCount)
            {
                var modifiedResult = result.GroupBy(x => new { x.StaffDate, x.SalesmanType }).Select(x => x.OrderByDescending(t => t.ShiftCount).FirstOrDefault());
                return new CashRegisterViewModel
                {
                    CashRegisterItems = modifiedResult.ToList()
                };
            }

            return new CashRegisterViewModel
            {
                CashRegisterItems = result
            };
        }

        //public CashRegVsSalesViewModel GetCashRegisterVsSalesRpt_old(DateTime? fromDate, DateTime? _toDate)
        //{
        //    var result = GetCashRegister(fromDate, fromDate, false).CashRegisterItems;

        //    var cashRegisters = result.GroupBy(x => new { x.Location, x.StaffDate });

        //    var locations = _locationRepository.GetLocations().LocationItems;

        //    var cashRegVsSalesItems = new List<CashRegVsSalesItem>();

        //    foreach (var item in cashRegisters)
        //    {
        //        var locationId = _locationRepository.GetLocations().LocationItems.FirstOrDefault(x => x.NameAr == item.Key.Location).LocationId;

        //        var salesReportFromDate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 05, 00, 00);
        //        var salesReportToDate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 04, 59, 00).AddDays(1);

        //        var salesReportItems = _salesReportRepository.GetSalesTransaction(salesReportFromDate, salesReportToDate, locationId.ToString(), "")
        //                                                     .SalesReportItems.Where(x => x.GroupCD != 329);

        //        var crKnet = item.Sum(x => x.Knet);
        //        var crVisa = item.Sum(x => x.Visa);
        //        var crOnline = item.Sum(x => x.Online);
        //        var crTalabat = item.Sum(x => x.Carriage);
        //        var crDeliveroo = item.Sum(x => x.Deliveroo);
        //        var crReserve = item.Sum(x => x.Reserve);
        //        var crExpense = item.Sum(x => x.Expense);
        //        var crCash = item.Sum(x => x.NetAmount) + crExpense;

        //        var totalCrSales = crKnet + crVisa + crOnline + crTalabat + crDeliveroo + crCash;


        //        #region Innova Talabat

        //        var talabatCash = salesReportItems.Where(a => a.VoucherId == 2025).Sum(a => a.Cash);
        //        var talabatCashReturn = salesReportItems.Where(a => a.VoucherId == 2035).Sum(a => a.Cash);
        //        var talabatCashNet = talabatCash - Math.Abs(talabatCashReturn != null ? (decimal)talabatCashReturn : 0);

        //        var talabatKnet = salesReportItems.Where(a => a.VoucherId == 2025).Sum(a => a.Knet);
        //        var talabatKnetReturn = salesReportItems.Where(a => a.VoucherId == 2035).Sum(a => a.Knet);
        //        var talabatKnetNet = talabatKnet - Math.Abs(talabatKnetReturn != null ? (decimal)talabatKnetReturn : 0);

        //        var talabatCc = salesReportItems.Where(a => a.VoucherId == 2025).Sum(a => a.CreditCard);
        //        var talabatCcReturn = salesReportItems.Where(a => a.VoucherId == 2035).Sum(a => a.CreditCard);
        //        var talabatCcNet = talabatCc - Math.Abs(talabatCcReturn != null ? (decimal)talabatCcReturn : 0);

        //        var talabatNet = talabatCashNet + talabatKnetNet + talabatCcNet;

        //        var talabat = talabatNet != null ? (decimal)talabatNet : 0;

        //        #endregion

        //        #region Innova Deliveroo

        //        var deliverooCash = salesReportItems.Where(a => a.VoucherId == 2030).Sum(a => a.Cash);
        //        var deliverooCashReturn = salesReportItems.Where(a => a.VoucherId == 2037).Sum(a => a.Cash);
        //        var deliverooCashNet = deliverooCash - Math.Abs(deliverooCashReturn != null ? (decimal)deliverooCashReturn : 0);

        //        var deliverooKnet = salesReportItems.Where(a => a.VoucherId == 2030).Sum(a => a.Knet);
        //        var deliverooKnetReturn = salesReportItems.Where(a => a.VoucherId == 2037).Sum(a => a.Knet);
        //        var deliverooKnetNet = deliverooKnet - Math.Abs(deliverooKnetReturn != null ? (decimal)deliverooKnetReturn : 0);

        //        var deliverooCc = salesReportItems.Where(a => a.VoucherId == 2030).Sum(a => a.CreditCard);
        //        var deliverooCcReturn = salesReportItems.Where(a => a.VoucherId == 2037).Sum(a => a.CreditCard);
        //        var deliverooCcNet = deliverooCc - Math.Abs(deliverooCcReturn != null ? (decimal)deliverooCcReturn : 0);

        //        var deliverooNet = deliverooCashNet + deliverooKnetNet + deliverooCcNet;

        //        var deliveroo = deliverooNet != null ? (decimal)deliverooNet : 0;

        //        #endregion

        //        #region Innova Online
        //        var onlineCash = salesReportItems.Where(a => a.VoucherId == 2026).Sum(a => a.Cash);
        //        var onlineCashReturn = salesReportItems.Where(a => a.VoucherId == 2036).Sum(a => a.Cash);
        //        var onlineCashNet = onlineCash - Math.Abs(onlineCashReturn != null ? (decimal)onlineCashReturn : 0);

        //        var onlineKnet = salesReportItems.Where(a => a.VoucherId == 2026).Sum(a => a.Knet);
        //        var onlineKnetReturn = salesReportItems.Where(a => a.VoucherId == 2036).Sum(a => a.Knet);
        //        var onlineKnetNet = onlineKnet - Math.Abs(onlineKnetReturn != null ? (decimal)onlineKnetReturn : 0);

        //        var onlineCc = salesReportItems.Where(a => a.VoucherId == 2026).Sum(a => a.CreditCard);
        //        var onlineCcReturn = salesReportItems.Where(a => a.VoucherId == 2036).Sum(a => a.CreditCard);
        //        var onlineCcNet = onlineCc - Math.Abs(onlineCcReturn != null ? (decimal)onlineCcReturn : 0);

        //        var onlineNet = onlineCashNet + onlineKnetNet + onlineCcNet;

        //        var online = onlineNet != null ? (decimal)onlineNet : 0;
        //        #endregion

        //        #region Innova Sales Return

        //        var cReturn = salesReportItems.Where(a => a.VoucherId == 2023 || a.VoucherId == 202).Sum(a => a.Cash);
        //        var cashReturn = cReturn != null ? (decimal)cReturn : 0;

        //        var kReturn = salesReportItems.Where(a => a.VoucherId == 2023 || a.VoucherId == 202).Sum(a => a.Knet);
        //        var knetReturn = kReturn != null ? (decimal)kReturn : 0;

        //        var creditReturn = salesReportItems.Where(a => a.VoucherId == 2023 || a.VoucherId == 202).Sum(a => a.CreditCard);
        //        var ccReturn = creditReturn != null ? (decimal)creditReturn : 0;

        //        #endregion

        //        var cash = salesReportItems.Sum(a => a.Cash) - onlineCashNet - talabatCashNet - deliverooCashNet - Math.Abs(knetReturn);
        //        var knet = salesReportItems.Sum(a => a.Knet) - onlineKnetNet - talabatKnetNet - deliverooKnetNet + Math.Abs(knetReturn);
        //        var cc = salesReportItems.Sum(a => a.CreditCard) - onlineCcNet - talabatCcNet - deliverooCcNet;
        //        var totalInnovaSales = cash + knet + cc + talabat + deliveroo + online;

        //        var cashRegVsSalesItem = new CashRegVsSalesItem
        //        {
        //            Location = item.Key.Location,
        //            LocationShortName = locations.FirstOrDefault(a => a.NameAr == item.Key.Location) != null
        //                                        ? locations.FirstOrDefault(a => a.NameAr == item.Key.Location).ShortName
        //                                        : "",
        //            TransDate = item.Key.StaffDate,

        //            CRCash = crCash,
        //            CRKnet = crKnet,
        //            CRVisa = crVisa,
        //            CROnline = crOnline,
        //            CRTalabat = crTalabat,
        //            CRDeliveroo = crDeliveroo,
        //            CRReserve = crReserve,
        //            CRKnetVisa = crKnet + crVisa,
        //            TotalCRSales = totalCrSales,

        //            Cash = cash,
        //            Knet = knet,
        //            CreditCard = cc,
        //            Online = online,
        //            Talabat = talabat,
        //            Deliveroo = deliveroo,
        //            KnetVisa = knet + cc,
        //            TotalInnovaSales = totalInnovaSales,

        //            CashDiff = crCash - cash,
        //            KnetDiff = crKnet - knet,
        //            CcDiff = crVisa - cc,
        //            TalabatDiff = crTalabat - talabat,
        //            DeliverooDiff = crDeliveroo - deliveroo,
        //            OnlineDiff = crOnline - online,
        //            KnetVisaDiff = (crKnet + crVisa) - (knet + cc),
        //            TotalDiff = totalCrSales - totalInnovaSales

        //        };

        //        cashRegVsSalesItems.Add(cashRegVsSalesItem);
        //    }

        //    return new CashRegVsSalesViewModel
        //    {
        //        CashRegVsSalesItems = cashRegVsSalesItems
        //    };
        //}

        //public CashRegVsSalesViewModel GetCashRegisterVsSalesRpt_old2(DateTime? fromDate, DateTime? toDate)
        //{
        //    var result = GetCashRegister(fromDate, toDate, false).CashRegisterItems;

        //    var cashRegisters = result.GroupBy(x => new { x.Location, x.StaffDate });

        //    var locations = _locationRepository.GetLocations().LocationItems;

        //    var cashRegVsSalesItems = new List<CashRegVsSalesItem>();

        //    foreach (var item in cashRegisters)
        //    {
        //        var locationId = _locationRepository.GetLocations().LocationItems.FirstOrDefault(x => x.NameAr == item.Key.Location).LocationId;

        //        var salesReportFromDate = new DateTime(item.Key.StaffDate.Year, item.Key.StaffDate.Month, item.Key.StaffDate.Day, 05, 00, 00);
        //        var salesReportToDate = new DateTime(item.Key.StaffDate.Year, item.Key.StaffDate.Month, item.Key.StaffDate.Day, 04, 59, 00).AddDays(1);

        //        var salesReportItems = _salesReportRepository.GetSalesTransaction(salesReportFromDate, salesReportToDate, locationId.ToString(), "")
        //                                                     .SalesReportItems.Where(x => x.GroupCD != 329);

        //        var crKnet = item.Sum(x => x.Knet);
        //        var crVisa = item.Sum(x => x.Visa);
        //        var crOnline = item.Sum(x => x.Online);
        //        var crTalabat = item.Sum(x => x.Carriage);
        //        var crDeliveroo = item.Sum(x => x.Deliveroo);
        //        var crReserve = item.Sum(x => x.Reserve);
        //        var crExpense = item.Sum(x => x.Expense);
        //        var crCash = item.Sum(x => x.NetAmount) + crExpense;

        //        var totalCrSales = crKnet + crVisa + crOnline + crTalabat + crDeliveroo + crCash;


        //        #region Innova Talabat

        //        var talabatCash = salesReportItems.Where(a => a.VoucherId == 2025).Sum(a => a.Cash);
        //        var talabatCashReturn = salesReportItems.Where(a => a.VoucherId == 2035).Sum(a => a.Cash);
        //        var talabatCashNet = talabatCash - Math.Abs(talabatCashReturn != null ? (decimal)talabatCashReturn : 0);

        //        var talabatKnet = salesReportItems.Where(a => a.VoucherId == 2025).Sum(a => a.Knet);
        //        var talabatKnetReturn = salesReportItems.Where(a => a.VoucherId == 2035).Sum(a => a.Knet);
        //        var talabatKnetNet = talabatKnet - Math.Abs(talabatKnetReturn != null ? (decimal)talabatKnetReturn : 0);

        //        var talabatCc = salesReportItems.Where(a => a.VoucherId == 2025).Sum(a => a.CreditCard);
        //        var talabatCcReturn = salesReportItems.Where(a => a.VoucherId == 2035).Sum(a => a.CreditCard);
        //        var talabatCcNet = talabatCc - Math.Abs(talabatCcReturn != null ? (decimal)talabatCcReturn : 0);

        //        var talabatNet = talabatCashNet + talabatKnetNet + talabatCcNet;

        //        var talabat = talabatNet != null ? (decimal)talabatNet : 0;

        //        #endregion

        //        #region Innova Deliveroo

        //        var deliverooCash = salesReportItems.Where(a => a.VoucherId == 2030).Sum(a => a.Cash);
        //        var deliverooCashReturn = salesReportItems.Where(a => a.VoucherId == 2037).Sum(a => a.Cash);
        //        var deliverooCashNet = deliverooCash - Math.Abs(deliverooCashReturn != null ? (decimal)deliverooCashReturn : 0);

        //        var deliverooKnet = salesReportItems.Where(a => a.VoucherId == 2030).Sum(a => a.Knet);
        //        var deliverooKnetReturn = salesReportItems.Where(a => a.VoucherId == 2037).Sum(a => a.Knet);
        //        var deliverooKnetNet = deliverooKnet - Math.Abs(deliverooKnetReturn != null ? (decimal)deliverooKnetReturn : 0);

        //        var deliverooCc = salesReportItems.Where(a => a.VoucherId == 2030).Sum(a => a.CreditCard);
        //        var deliverooCcReturn = salesReportItems.Where(a => a.VoucherId == 2037).Sum(a => a.CreditCard);
        //        var deliverooCcNet = deliverooCc - Math.Abs(deliverooCcReturn != null ? (decimal)deliverooCcReturn : 0);

        //        var deliverooNet = deliverooCashNet + deliverooKnetNet + deliverooCcNet;

        //        var deliveroo = deliverooNet != null ? (decimal)deliverooNet : 0;

        //        #endregion

        //        #region Innova Online
        //        var onlineCash = salesReportItems.Where(a => a.VoucherId == 2026).Sum(a => a.Cash);
        //        var onlineCashReturn = salesReportItems.Where(a => a.VoucherId == 2036).Sum(a => a.Cash);
        //        var onlineCashNet = onlineCash - Math.Abs(onlineCashReturn != null ? (decimal)onlineCashReturn : 0);

        //        var onlineKnet = salesReportItems.Where(a => a.VoucherId == 2026).Sum(a => a.Knet);
        //        var onlineKnetReturn = salesReportItems.Where(a => a.VoucherId == 2036).Sum(a => a.Knet);
        //        var onlineKnetNet = onlineKnet - Math.Abs(onlineKnetReturn != null ? (decimal)onlineKnetReturn : 0);

        //        var onlineCc = salesReportItems.Where(a => a.VoucherId == 2026).Sum(a => a.CreditCard);
        //        var onlineCcReturn = salesReportItems.Where(a => a.VoucherId == 2036).Sum(a => a.CreditCard);
        //        var onlineCcNet = onlineCc - Math.Abs(onlineCcReturn != null ? (decimal)onlineCcReturn : 0);

        //        var onlineNet = onlineCashNet + onlineKnetNet + onlineCcNet;

        //        var online = onlineNet != null ? (decimal)onlineNet : 0;
        //        #endregion

        //        #region Innova Sales Return

        //        var cReturn = salesReportItems.Where(a => a.VoucherId == 2023 || a.VoucherId == 202).Sum(a => a.Cash);
        //        var cashReturn = cReturn != null ? (decimal)cReturn : 0;

        //        var kReturn = salesReportItems.Where(a => a.VoucherId == 2023 || a.VoucherId == 202).Sum(a => a.Knet);
        //        var knetReturn = kReturn != null ? (decimal)kReturn : 0;

        //        var creditReturn = salesReportItems.Where(a => a.VoucherId == 2023 || a.VoucherId == 202).Sum(a => a.CreditCard);
        //        var ccReturn = creditReturn != null ? (decimal)creditReturn : 0;

        //        #endregion

        //        var cash = salesReportItems.Sum(a => a.Cash) - onlineCashNet - talabatCashNet - deliverooCashNet - Math.Abs(knetReturn);
        //        var knet = salesReportItems.Sum(a => a.Knet) - onlineKnetNet - talabatKnetNet - deliverooKnetNet + Math.Abs(knetReturn);
        //        var cc = salesReportItems.Sum(a => a.CreditCard) - onlineCcNet - talabatCcNet - deliverooCcNet;
        //        var totalInnovaSales = cash + knet + cc + talabat + deliveroo + online;

        //        var cashRegVsSalesItem = new CashRegVsSalesItem
        //        {
        //            Location = item.Key.Location,
        //            LocationShortName = locations.FirstOrDefault(a => a.NameAr == item.Key.Location) != null
        //                                        ? locations.FirstOrDefault(a => a.NameAr == item.Key.Location).ShortName
        //                                        : "",
        //            TransDate = item.Key.StaffDate,

        //            CRCash = crCash,
        //            CRKnet = crKnet,
        //            CRVisa = crVisa,
        //            CROnline = crOnline,
        //            CRTalabat = crTalabat,
        //            CRDeliveroo = crDeliveroo,
        //            CRReserve = crReserve,
        //            CRKnetVisa = crKnet + crVisa,
        //            TotalCRSales = totalCrSales,

        //            Cash = cash,
        //            Knet = knet,
        //            CreditCard = cc,
        //            Online = online,
        //            Talabat = talabat,
        //            Deliveroo = deliveroo,
        //            KnetVisa = knet + cc,
        //            TotalInnovaSales = totalInnovaSales,

        //            CashDiff = crCash - cash,
        //            KnetDiff = crKnet - knet,
        //            CcDiff = crVisa - cc,
        //            TalabatDiff = crTalabat - talabat,
        //            DeliverooDiff = crDeliveroo - deliveroo,
        //            OnlineDiff = crOnline - online,
        //            KnetVisaDiff = (crKnet + crVisa) - (knet + cc),
        //            TotalDiff = totalCrSales - totalInnovaSales

        //        };

        //        cashRegVsSalesItems.Add(cashRegVsSalesItem);
        //    }

        //    return new CashRegVsSalesViewModel
        //    {
        //        CashRegVsSalesItems = cashRegVsSalesItems
        //    };
        //}

        public CashRegVsSalesViewModel GetCashRegisterVsSalesRpt(DateTime? fromDate, DateTime? toDate)
        {
            var result = GetCashRegister(fromDate, toDate, false).CashRegisterItems;

            var cashRegisters = result.GroupBy(x => new { x.Location, x.StaffDate });

            var locations = _locationRepository.GetLocations().LocationItems;

            var cashRegVsSalesItems = new List<CashRegVsSalesItem>();

            for (var i = fromDate.Value; i <= toDate.Value;)
            {
                foreach (var item in cashRegisters.Where(x => x.Key.StaffDate == i.Date))
                {
                    var locationId = _locationRepository.GetLocations().LocationItems.FirstOrDefault(x => x.NameAr == item.Key.Location).LocationId;

                    var salesReportFromDate = new DateTime(i.Year, i.Month, i.Day, 05, 00, 00);
                    var salesReportToDate = new DateTime(i.Year, i.Month, i.Day, 04, 59, 00).AddDays(1);

                    var salesReportItems = _salesReportRepository.GetSalesTransaction(salesReportFromDate, salesReportToDate, locationId.ToString(), "")
                                                                 .SalesReportItems.Where(x => x.GroupCD != 329);

                    var crKnet = item.Sum(x => x.Knet);
                    var crVisa = item.Sum(x => x.Visa);
                    var crOnline = item.Sum(x => x.Online);
                    var crTalabat = item.Sum(x => x.Carriage);
                    var crDeliveroo = item.Sum(x => x.Deliveroo);
                    var crReserve = item.Sum(x => x.Reserve);
                    var crExpense = item.Sum(x => x.Expense);
                    var crCash = item.Sum(x => x.NetAmount) + crExpense;

                    var totalCrSales = crKnet + crVisa + crOnline + crTalabat + crDeliveroo + crCash;


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

                    #region Innova Deliveroo

                    var deliverooCash = salesReportItems.Where(a => a.VoucherId == 2030).Sum(a => a.Cash);
                    var deliverooCashReturn = salesReportItems.Where(a => a.VoucherId == 2037).Sum(a => a.Cash);
                    var deliverooCashNet = deliverooCash - Math.Abs(deliverooCashReturn != null ? (decimal)deliverooCashReturn : 0);

                    var deliverooKnet = salesReportItems.Where(a => a.VoucherId == 2030).Sum(a => a.Knet);
                    var deliverooKnetReturn = salesReportItems.Where(a => a.VoucherId == 2037).Sum(a => a.Knet);
                    var deliverooKnetNet = deliverooKnet - Math.Abs(deliverooKnetReturn != null ? (decimal)deliverooKnetReturn : 0);

                    var deliverooCc = salesReportItems.Where(a => a.VoucherId == 2030).Sum(a => a.CreditCard);
                    var deliverooCcReturn = salesReportItems.Where(a => a.VoucherId == 2037).Sum(a => a.CreditCard);
                    var deliverooCcNet = deliverooCc - Math.Abs(deliverooCcReturn != null ? (decimal)deliverooCcReturn : 0);

                    var deliverooNet = deliverooCashNet + deliverooKnetNet + deliverooCcNet;

                    var deliveroo = deliverooNet != null ? (decimal)deliverooNet : 0;

                    #endregion

                    #region Innova Online

                    var onlineCash = salesReportItems.Where(a => a.VoucherId == 2026).Sum(a => a.Cash);
                    var onlineCashReturn = salesReportItems.Where(a => a.VoucherId == 2036).Sum(a => a.Cash);
                    var onlineCashNet = onlineCash - Math.Abs(onlineCashReturn.HasValue ? (decimal)onlineCashReturn : 0);

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

                    var cash = salesReportItems.Sum(a => a.Cash) - onlineCashNet - talabatCashNet - deliverooCashNet - Math.Abs(knetReturn);
                    var knet = salesReportItems.Sum(a => a.Knet) - onlineKnetNet - talabatKnetNet - deliverooKnetNet + Math.Abs(knetReturn);
                    var cc = salesReportItems.Sum(a => a.CreditCard) - onlineCcNet - talabatCcNet - deliverooCcNet;
                    var totalInnovaSales = cash + knet + cc + talabat + deliveroo + online;

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
                        CRDeliveroo = crDeliveroo,
                        CRReserve = crReserve,
                        CRKnetVisa = crKnet + crVisa,
                        TotalCRSales = totalCrSales,

                        Cash = cash,
                        Knet = knet,
                        CreditCard = cc,
                        Online = online,
                        Talabat = talabat,
                        Deliveroo = deliveroo,
                        KnetVisa = knet + cc,
                        TotalInnovaSales = totalInnovaSales,

                        CashDiff = crCash - cash,
                        KnetDiff = crKnet - knet,
                        CcDiff = crVisa - cc,
                        TalabatDiff = crTalabat - talabat,
                        DeliverooDiff = crDeliveroo - deliveroo,
                        OnlineDiff = crOnline - online,
                        KnetVisaDiff = (crKnet + crVisa) - (knet + cc),
                        TotalDiff = totalCrSales - totalInnovaSales

                    };

                    cashRegVsSalesItems.Add(cashRegVsSalesItem);
                }

                i = i.AddDays(1);
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
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }

        public string Update(CashRegUpdateVM cashRegUpdateVM, string jsonPath)
        {
            try
            {
                var cashRegister = _context.intlmill_cash_register.FirstOrDefault(x => x.id == cashRegUpdateVM.Oid);
                var isChanged = false;
                // var logs = new List<CRLogViewModel>();

                using (var fileStream = new FileStream(jsonPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    var streamReader = new StreamReader(fileStream, Encoding.UTF8);
                    var crLogFile = streamReader.ReadToEnd();
                    var logs = JsonConvert.DeserializeObject<List<CRLogViewModel>>(crLogFile);

                    if (cashRegister.staff_date != cashRegUpdateVM.StaffDate)
                    {
                        logs.Add(new CRLogViewModel
                        {
                            Oid = cashRegister.id,
                            UpdatedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                            PropertyName = "staff_date",
                            OldValue = cashRegister.staff_date,
                            NewValue = cashRegUpdateVM.StaffDate
                        });
                        cashRegister.staff_date = cashRegUpdateVM.StaffDate;
                        isChanged = true;
                    }

                    if (cashRegister.shift_type != cashRegUpdateVM.ShiftType)
                    {
                        logs.Add(new CRLogViewModel
                        {
                            Oid = cashRegister.id,
                            UpdatedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                            PropertyName = "shift_type",
                            OldValue = cashRegister.shift_type.ToString(),
                            NewValue = cashRegUpdateVM.ShiftType.ToString()
                        });
                        cashRegister.shift_type = cashRegUpdateVM.ShiftType;
                        isChanged = true;
                    }

                    if (cashRegister.carriage != cashRegUpdateVM.Talabat)
                    {
                        logs.Add(new CRLogViewModel
                        {
                            Oid = cashRegister.id,
                            UpdatedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                            PropertyName = "Talabat",
                            OldValue = cashRegister.carriage.ToString(),
                            NewValue = cashRegUpdateVM.Talabat.ToString()
                        });
                        cashRegister.carriage = cashRegUpdateVM.Talabat;
                        isChanged = true;
                    }

                    if (cashRegister.deliveroo != cashRegUpdateVM.Deliveroo)
                    {
                        logs.Add(new CRLogViewModel
                        {
                            Oid = cashRegister.id,
                            UpdatedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                            PropertyName = "deliveroo",
                            OldValue = cashRegister.deliveroo.ToString(),
                            NewValue = cashRegUpdateVM.Deliveroo.ToString()
                        });

                        cashRegister.deliveroo = cashRegUpdateVM.Deliveroo;
                        isChanged = true;
                    }

                    if (cashRegister.cheque != cashRegUpdateVM.Cheque)
                    {
                        logs.Add(new CRLogViewModel
                        {
                            Oid = cashRegister.id,
                            UpdatedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                            PropertyName = "cheque",
                            OldValue = cashRegister.cheque.ToString(),
                            NewValue = cashRegUpdateVM.Cheque.ToString()
                        });

                        cashRegister.cheque = cashRegUpdateVM.Cheque;
                        isChanged = true;
                    }

                    if (cashRegister.online != cashRegUpdateVM.Online)
                    {
                        logs.Add(new CRLogViewModel
                        {
                            Oid = cashRegister.id,
                            UpdatedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                            PropertyName = "online",
                            OldValue = cashRegister.online.ToString(),
                            NewValue = cashRegUpdateVM.Online.ToString()
                        });

                        cashRegister.online = cashRegUpdateVM.Online;
                        isChanged = true;
                    }

                    if (cashRegister.knet != cashRegUpdateVM.Knet)
                    {
                        logs.Add(new CRLogViewModel
                        {
                            Oid = cashRegister.id,
                            UpdatedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                            PropertyName = "knet",
                            OldValue = cashRegister.knet.ToString(),
                            NewValue = cashRegUpdateVM.Knet.ToString()
                        });

                        cashRegister.knet = cashRegUpdateVM.Knet;
                        isChanged = true;
                    }

                    if (cashRegister.visa != cashRegUpdateVM.Visa)
                    {
                        logs.Add(new CRLogViewModel
                        {
                            Oid = cashRegister.id,
                            UpdatedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                            PropertyName = "visa",
                            OldValue = cashRegister.visa.ToString(),
                            NewValue = cashRegUpdateVM.Visa.ToString()
                        });

                        cashRegister.visa = cashRegUpdateVM.Visa;
                        isChanged = true;
                    }

                    if (cashRegister.total_cash != cashRegUpdateVM.Cash)
                    {
                        logs.Add(new CRLogViewModel
                        {
                            Oid = cashRegister.id,
                            UpdatedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                            PropertyName = "total_cash",
                            OldValue = cashRegister.total_cash.ToString(),
                            NewValue = cashRegUpdateVM.Cash.ToString()
                        });

                        cashRegister.total_cash = cashRegUpdateVM.Cash;
                        isChanged = true;
                    }

                    if (cashRegister.expense != cashRegUpdateVM.Expense)
                    {
                        logs.Add(new CRLogViewModel
                        {
                            Oid = cashRegister.id,
                            UpdatedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                            PropertyName = "expense",
                            OldValue = cashRegister.expense.ToString(),
                            NewValue = cashRegUpdateVM.Expense.ToString()
                        });

                        cashRegister.expense = cashRegUpdateVM.Expense;
                        isChanged = true;
                    }

                    if (cashRegister.reserve != cashRegUpdateVM.Reserve)
                    {
                        logs.Add(new CRLogViewModel
                        {
                            Oid = cashRegister.id,
                            UpdatedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                            PropertyName = "reserve",
                            OldValue = cashRegister.reserve.ToString(),
                            NewValue = cashRegUpdateVM.Reserve.ToString()
                        });

                        cashRegister.reserve = cashRegUpdateVM.Reserve;
                        isChanged = true;
                    }

                    

                    if (isChanged)
                    {
                        var netCash = cashRegUpdateVM.Cash - cashRegUpdateVM.Reserve;


                        if (cashRegister.net_cash != netCash)
                        {
                            logs.Add(new CRLogViewModel
                            {
                                Oid = cashRegister.id,
                                UpdatedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                                PropertyName = "reserve",
                                OldValue = cashRegister.net_cash.ToString(),
                                NewValue = netCash.ToString()
                            });

                            cashRegister.net_cash = netCash;
                        }

                        var total_sales = cashRegUpdateVM.Cheque + cashRegUpdateVM.Talabat + cashRegUpdateVM.Online + cashRegUpdateVM.Knet + cashRegUpdateVM.Visa + cashRegUpdateVM.Cash;
                        var netSales = total_sales - cashRegUpdateVM.Reserve;

                        logs.Add(new CRLogViewModel
                        {
                            Oid = cashRegister.id,
                            UpdatedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                            PropertyName = "total_sales",
                            OldValue = cashRegister.total_sales.ToString(),
                            NewValue = total_sales.ToString()
                        });

                        logs.Add(new CRLogViewModel
                        {
                            Oid = cashRegister.id,
                            UpdatedOn = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                            PropertyName = "net_sales",
                            OldValue = cashRegister.net_sales.ToString(),
                            NewValue = netSales.ToString()
                        });


                        cashRegister.total_sales = total_sales;
                        cashRegister.net_sales = netSales;
                        cashRegister.UpdatedOn = DateTime.Now;

                        _context.SaveChanges();

                        streamReader.Close();
                        var convertedJson = JsonConvert.SerializeObject(logs, Formatting.Indented);
                        File.WriteAllText(jsonPath, convertedJson);
                        fileStream.Close();

                        //using (StreamWriter file = path)
                        //{
                        //    JsonSerializer serializer = new JsonSerializer();
                        //    serializer.Serialize(file, logs);
                        //}
                    }
                }

                

                return "Record Updated Successfully";

            }
            catch (Exception ex)
            {
                return $"Error Occurred.{ex.Message}";
            }
        }
    }
}