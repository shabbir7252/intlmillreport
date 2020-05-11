using System;
using System.Linq;
using ImillReports.Models;
using ImillReports.Contracts;
using ImillReports.ViewModels;
using System.Collections.Generic;

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

        private List<CashRegisterItem> GetCashRegisterItems(IQueryable<intlmill_cash_register> cashRegister)
        {
            var cashRegisterItems = new List<CashRegisterItem>();
            var locations = _locationRepository.GetLocations().LocationItems;

            foreach (var item in cashRegister)
            {
                var cashRegisterItem = new CashRegisterItem
                {
                    Carriage = item.carriage,
                    Cash = item.total_cash,
                    Cheque = item.cheque,
                    Knet = item.knet,
                    Expense = item.expense,
                    Location = item.location,
                    LocationShortName = locations.FirstOrDefault(a => a.NameAr == item.location).ShortName,
                    Online = item.online,
                    Salesman = item.salesman,
                    TransDateTime = item.trans_date_time,
                    Visa = item.visa,
                    Reserve = item.reserve,
                    ShiftCount = item.shift_count,
                    ShiftType = item.shift_type,
                    TransDate = item.trans_date_time.Date
                };

                var totalSales = item.cheque + item.carriage + item.online + item.knet + item.visa + item.total_cash;
                var netSales = totalSales - item.reserve;
                cashRegisterItem.TotalSales = totalSales;
                cashRegisterItem.NetSales = netSales;
                cashRegisterItem.NetAmount = item.total_cash - item.reserve - item.expense;


                if (!(totalSales <= 0 || netSales <= 0))
                    cashRegisterItems.Add(cashRegisterItem);
            }

            return cashRegisterItems;
        }

        public CashRegisterViewModel GetCashRegister(DateTime? fromDate, DateTime? toDate, bool takeCount)
        {
            var cashRegister = _context.intlmill_cash_register.Where(x => x.trans_date_time >= fromDate && x.trans_date_time <= toDate);

            var result = GetCashRegisterItems(cashRegister);

            if (!takeCount)
            {
                // cashRegister = cashRegister.GroupBy(x => new { x.salesman }).Select(x => x.OrderByDescending(t => t.shift_count).FirstOrDefault());
                var modifiedResult = result.GroupBy(x => new { x.Salesman, x.TransDate }).Select(x => x.OrderByDescending(t => t.ShiftCount).FirstOrDefault());
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
            var cashRegister = _context.intlmill_cash_register.Where(x => x.trans_date_time >= fromDate && x.trans_date_time <= toDate);

            var result = GetCashRegisterItems(cashRegister);

            foreach (var item in result)
            {
                if (item.TransDate.Date != fromDate.Value.Date)
                    item.TransDate = fromDate.Value.Date;
            }

            var modifiedResult = result.GroupBy(x => new { x.Salesman, x.TransDate }).Select(x => x.OrderByDescending(t => t.ShiftCount).FirstOrDefault());

            var cashRegisters = modifiedResult.GroupBy(x => new { x.Location, x.TransDate });

            var locations = _locationRepository.GetLocations().LocationItems;

            var cashRegVsSalesItems = new List<CashRegVsSalesItem>();

            foreach (var item in cashRegisters)
            {
                var locationId = _locationRepository.GetLocations().LocationItems.FirstOrDefault(x => x.NameAr == item.Key.Location).LocationId;

                //var toDate = new DateTime(item.Key.TransDate.Year, item.Key.TransDate.Month, item.Key.TransDate.Day, 00, 00, 00);
                //var fromD = new DateTime(item.Key.TransDate.Year, item.Key.TransDate.Month, item.Key.TransDate.Day, 11, 59, 59);

                var salesReportFromDate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 00, 00, 00);
                var salesReportToDate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 23, 59, 00);

                var salesReportItems = _salesReportRepository.GetSalesReport(salesReportFromDate, salesReportToDate, locationId.ToString(), "").SalesReportItems;

                var cashRegVsSalesItem = new CashRegVsSalesItem
                {
                    Location = item.Key.Location,
                    LocationShortName = locations.FirstOrDefault(a => a.NameAr == item.Key.Location).ShortName,
                    TransDate = item.Key.TransDate,
                    CRCash = item.Sum(x => x.NetAmount),
                    CRKnet = item.Sum(x => x.Knet),
                    CRVisa = item.Sum(x => x.Visa),
                    CROnline = item.Sum(x => x.Knet) + item.Sum(x => x.Visa),
                    Reserve = item.Sum(a => a.Reserve),
                    Cash = salesReportItems.Sum(a => a.Cash),
                    Knet = salesReportItems.Sum(a => a.Knet),
                    CreditCard = salesReportItems.Sum(a => a.CreditCard),
                    Online = salesReportItems.Sum(a => a.Knet) + salesReportItems.Sum(a => a.CreditCard),
                };

                cashRegVsSalesItems.Add(cashRegVsSalesItem);
            }

            return new CashRegVsSalesViewModel
            {
                CashRegVsSalesItems = cashRegVsSalesItems
            };
        }
    }
}