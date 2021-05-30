using System;
using System.Linq;
using System.Data.Entity;
using ImillReports.Models;
using ImillReports.Contracts;
using ImillReports.ViewModels;
using System.Collections.Generic;

namespace ImillReports.Repository
{
    public class SalesReportRepository : ISalesReportRepository
    {
        private readonly IMILLEntities _context;
        private readonly ILocationRepository _locationRepository;
        private readonly IProductRepository _productRepository;
        private readonly ImillReportsEntities _report;

        public SalesReportRepository(IMILLEntities context, ILocationRepository locationRepository,
            ImillReportsEntities report, IProductRepository productRepository)
        {
            _context = context;
            _locationRepository = locationRepository;
            _report = report;
            _productRepository = productRepository;
        }

        struct DateTimeRange
        {
            public DateTimeRange(DateTime start, DateTime end)
            {
                Start = start;
                End = end;
            }
            public DateTime Start { get; }
            public DateTime End { get; }
        }

        // https://stackoverflow.com/questions/13690617/split-date-range-into-date-range-chunks/13691511
        public static IEnumerable<Tuple<DateTime, DateTime>> SplitDateRange(DateTime start, DateTime end, int dayChunkSize)
        {
            DateTime chunkEnd;
            while ((chunkEnd = start.AddDays(dayChunkSize)) < end)
            {
                yield return Tuple.Create(start, chunkEnd);
                start = chunkEnd;
            }
            yield return Tuple.Create(start, end);
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> entry, int nSize = 30)
        {
            for (int i = 0; i < entry.Count; i += nSize)
            {
                yield return entry.GetRange(i, Math.Min(nSize, entry.Count - i));
            }
        }

        private IEnumerable<DateTimeRange> GetYearsBetweenDates(DateTime startDate, DateTime stopDate)
        {
            var count = 0;
            for (int i = startDate.Year; i <= stopDate.Year; i++)
            {
                yield return new DateTimeRange
                (
                    start: (i == stopDate.Year || count > 0) && stopDate.Year != startDate.Year
                    ? new DateTime(i, 01, 01, startDate.Hour, startDate.Minute, startDate.Second)
                    : new DateTime(i, startDate.Month, startDate.Day, startDate.Hour, startDate.Minute, startDate.Second),
                    end:
                     i == stopDate.Year
                     ? new DateTime(i, stopDate.Month, stopDate.Day, stopDate.Hour, stopDate.Minute, stopDate.Second)
                     : new DateTime(i, 12, 31, stopDate.Hour, stopDate.Minute, stopDate.Second)
                );
                count += 1;
            }
        }

        // In Use
        private IQueryable<ICS_Transaction> GetTransactions(DateTime? fromDate, DateTime? toDate, DateTime fromHoDate, DateTime toHoDate)
        {
            return _context.ICS_Transaction
                            .Include(a => a.SM_Location)
                            .Include(a => a.SM_SALESMAN)
                            .Include(a => a.ICS_Transaction_Types)
                            .Include(a => a.GL_Ledger)
                            .Include(a => a.GL_Ledger1)
                            .Include(a => a.GL_Ledger2)
                            .Where(x => ((x.Voucher_Date >= fromDate &&
                                        x.Voucher_Date <= toDate &&
                                        x.Locat_Cd != 1) ||
                                        (x.Voucher_Date >= fromHoDate &&
                                        x.Voucher_Date <= toHoDate &&
                                        x.Locat_Cd == 1)) &&

                                        // Sales Voucher Type Id
                                        (x.ICS_Transaction_Types.Voucher_Type == 201 ||
                                        x.ICS_Transaction_Types.Voucher_Type == 2021 ||
                                        x.ICS_Transaction_Types.Voucher_Type == 2022 ||
                                        x.ICS_Transaction_Types.Voucher_Type == 2025 ||
                                        x.ICS_Transaction_Types.Voucher_Type == 2026 ||
                                        x.ICS_Transaction_Types.Voucher_Type == 2030 ||

                                        // Sales Return Voucher Type Id
                                        x.ICS_Transaction_Types.Voucher_Type == 202 ||
                                        x.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                        x.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                        x.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                        x.ICS_Transaction_Types.Voucher_Type == 2037)
                                        // && x.GL_Ledger2.Group_Cd != 329
                                        );
        }

        // Used in Dashboard-Detail-Ajax
        public SalesReportViewModel GetSalesTransaction(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            if (fromDate == null) fromDate = DateTime.Now;
            if (toDate == null) toDate = DateTime.Now;

            var salesReportItems = new List<SalesReportItem>();

            if (fromDate.Value.Date > new DateTime(2019, 12, 31) && fromDate.Value.Date <= new DateTime(2020, 12, 31)
                && toDate.Value.Date > new DateTime(2020, 12, 31) && toDate.Value.Date <= new DateTime(2021, 12, 31))
            {
                for (var i = 1; i <= 2; i++)
                {
                    var startdate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, fromDate.Value.Hour, fromDate.Value.Minute, fromDate.Value.Second);
                    var enddate = new DateTime(2020, 12, 31, 23, 59, 59);

                    if (i == 2)
                    {
                        startdate = new DateTime(2021, 1, 1, 00, 00, 00);
                        enddate = new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, toDate.Value.Hour, toDate.Value.Minute, toDate.Value.Second);
                    }

                    var fromHoDate = startdate.Date;

                    var toHoDate = enddate;

                    if (startdate.Year == 2020)
                    {
                        var transactions = _report.Transaction_2020.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (startdate.Year == 2021)
                    {

                        var transactions = _report.Transaction_2021.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }
                }
            }

            else if (fromDate.Value.Date > new DateTime(2018, 12, 31) && fromDate.Value.Date <= new DateTime(2019, 12, 31)
                && toDate.Value.Date > new DateTime(2019, 12, 31) && toDate.Value.Date <= new DateTime(2020, 12, 31))
            {
                for (var i = 1; i <= 2; i++)
                {
                    var startdate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, fromDate.Value.Hour, fromDate.Value.Minute, fromDate.Value.Second);
                    var enddate = new DateTime(2019, 12, 31, 23, 59, 59);

                    if (i == 2)
                    {
                        startdate = new DateTime(2020, 1, 1, 00, 00, 00);
                        enddate = new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, toDate.Value.Hour, toDate.Value.Minute, toDate.Value.Second);
                    }

                    var fromHoDate = startdate.Date;

                    var toHoDate = enddate;

                    if (startdate.Year == 2019)
                    {

                        var transactions = _report.Transaction_2019.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (startdate.Year == 2020)
                    {
                        var transactions = _report.Transaction_2020.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }
                }
            }

            else if (fromDate.Value.Date > new DateTime(2017, 12, 31) && fromDate.Value.Date <= new DateTime(2018, 12, 31)
                && toDate.Value.Date > new DateTime(2018, 12, 31) && toDate.Value.Date <= new DateTime(2019, 12, 31))
            {
                for (var i = 1; i <= 2; i++)
                {
                    var startdate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, fromDate.Value.Hour, fromDate.Value.Minute, fromDate.Value.Second);
                    var enddate = new DateTime(2018, 12, 31, 23, 59, 59);

                    if (i == 2)
                    {
                        startdate = new DateTime(2019, 1, 1, 00, 00, 00);
                        enddate = new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, toDate.Value.Hour, toDate.Value.Minute, toDate.Value.Second);
                    }

                    var fromHoDate = startdate.Date;

                    var toHoDate = enddate;

                    if (startdate.Year == 2018)
                    {
                        var transactions = _report.Transaction_2018.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (startdate.Year == 2019)
                    {

                        var transactions = _report.Transaction_2019.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }


                }
            }

            else if (fromDate.Value.Date > new DateTime(2016, 12, 31) && fromDate.Value.Date <= new DateTime(2017, 12, 31)
                && toDate.Value.Date > new DateTime(2017, 12, 31) && toDate.Value.Date <= new DateTime(2018, 12, 31))
            {
                for (var i = 1; i <= 2; i++)
                {
                    var startdate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, fromDate.Value.Hour, fromDate.Value.Minute, fromDate.Value.Second);
                    var enddate = new DateTime(2017, 12, 31, 23, 59, 59);

                    if (i == 2)
                    {
                        startdate = new DateTime(2018, 1, 1, 00, 00, 00);
                        enddate = new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, toDate.Value.Hour, toDate.Value.Minute, toDate.Value.Second);
                    }

                    var fromHoDate = startdate.Date;

                    var toHoDate = enddate;

                    if (startdate.Year == 2017)
                    {

                        var transactions = _report.Transaction_2017.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (startdate.Year == 2018)
                    {
                        var transactions = _report.Transaction_2018.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }




                }
            }

            else if (fromDate.Value.Date > new DateTime(2015, 12, 31) && fromDate.Value.Date <= new DateTime(2016, 12, 31)
                && toDate.Value.Date > new DateTime(2016, 12, 31) && toDate.Value.Date <= new DateTime(2017, 12, 31))
            {
                for (var i = 1; i <= 2; i++)
                {
                    var startdate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, fromDate.Value.Hour, fromDate.Value.Minute, fromDate.Value.Second);
                    var enddate = new DateTime(2016, 12, 31, 23, 59, 59);

                    if (i == 2)
                    {
                        startdate = new DateTime(2017, 1, 1, 00, 00, 00);
                        enddate = new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, toDate.Value.Hour, toDate.Value.Minute, toDate.Value.Second);
                    }

                    var fromHoDate = startdate.Date;

                    var toHoDate = enddate;

                    if (startdate.Year == 2016)
                    {
                        var transactions = _report.Transaction_2016.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (startdate.Year == 2017)
                    {

                        var transactions = _report.Transaction_2017.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }



                }
            }

            else if (fromDate.Value.Date > new DateTime(2014, 12, 31) && fromDate.Value.Date <= new DateTime(2015, 12, 31)
                && toDate.Value.Date > new DateTime(2015, 12, 31) && toDate.Value.Date <= new DateTime(2016, 12, 31))
            {
                for (var i = 1; i <= 2; i++)
                {
                    var startdate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, fromDate.Value.Hour, fromDate.Value.Minute, fromDate.Value.Second);
                    var enddate = new DateTime(2015, 12, 31, 23, 59, 59);

                    if (i == 2)
                    {
                        startdate = new DateTime(2016, 1, 1, 00, 00, 00);
                        enddate = new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, toDate.Value.Hour, toDate.Value.Minute, toDate.Value.Second);
                    }

                    var fromHoDate = startdate.Date;

                    var toHoDate = enddate;

                    if (startdate.Year == 2015)
                    {

                        var transactions = _report.Transaction_2015.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (startdate.Year == 2016)
                    {
                        var transactions = _report.Transaction_2016.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                }
            }

            else if (fromDate.Value.Date <= new DateTime(2015, 12, 31, 0, 0, 0) && toDate.Value.Date <= new DateTime(2021, 12, 31))
            {
                for (var i = 1; i <= 7; i++)
                {
                    var startdate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, fromDate.Value.Hour, fromDate.Value.Minute, fromDate.Value.Second);
                    var enddate = new DateTime(2015, 12, 31, 23, 59, 59);

                    if (i == 2)
                    {
                        startdate = new DateTime(2016, 1, 1, 00, 00, 00);
                        enddate = new DateTime(2016, 12, 31, 23, 59, 59);
                    }

                    if (i == 3)
                    {
                        startdate = new DateTime(2017, 1, 1, 00, 00, 00);
                        enddate = new DateTime(2017, 12, 31, 23, 59, 59);
                    }

                    if (i == 4)
                    {
                        startdate = new DateTime(2018, 1, 1, 00, 00, 00);
                        enddate = new DateTime(2018, 12, 31, 23, 59, 59);
                    }

                    if (i == 5)
                    {
                        startdate = new DateTime(2019, 1, 1, 00, 00, 00);
                        enddate = new DateTime(2019, 12, 31, 23, 59, 59);
                    }

                    if (i == 6)
                    {
                        startdate = new DateTime(2020, 1, 1, 00, 00, 00);
                        enddate = new DateTime(2020, 12, 31, 23, 59, 59);
                    }

                    if (i == 7)
                    {
                        startdate = new DateTime(2021, 1, 1, 00, 00, 00);
                        enddate = new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, toDate.Value.Hour, toDate.Value.Minute, toDate.Value.Second);
                    }

                    var fromHoDate = startdate.Date;

                    var toHoDate = enddate;

                    if (startdate.Year == 2015)
                    {

                        var transactions = _report.Transaction_2016.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (startdate.Year == 2016)
                    {

                        var transactions = _report.Transaction_2016.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (startdate.Year == 2017)
                    {

                        var transactions = _report.Transaction_2017.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (startdate.Year == 2018)
                    {

                        var transactions = _report.Transaction_2018.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (startdate.Year == 2019)
                    {

                        var transactions = _report.Transaction_2019.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (startdate.Year == 2020)
                    {
                        var transactions = _report.Transaction_2020.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (startdate.Year == 2021)
                    {

                        var transactions = _report.Transaction_2021.Where(x => (x.InvDateTime >= startdate &&
                                                                                      x.InvDateTime <= enddate &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }
                }
            }

            else
            {
                var dateRange = GetYearsBetweenDates(fromDate.Value, toDate.Value).ToList();

                foreach (var date in dateRange)
                {
                    var fromHoDate = date.Start.TimeOfDay >= new TimeSpan(06, 00, 00)
                        ? date.Start.Date.AddDays(1)
                        : date.Start.Date;

                    DateTime toHoDate = new DateTime();
                    if (date.Start.Date == date.End.Date && date.End.TimeOfDay >= new TimeSpan(06, 00, 00))
                    {
                        toHoDate = new DateTime(date.End.Date.Year, date.End.Date.Month, date.End.Date.Day, 23, 59, 00);
                    }
                    else if (date.Start.Date < date.End.Date && date.End.TimeOfDay >= new TimeSpan(06, 00, 00))
                    {
                        toHoDate = new DateTime(date.End.Date.Year, date.End.Date.Month, date.End.Date.Day, 23, 59, 00).AddDays(1);
                    }
                    else if (date.Start.Date < date.End.Date && date.End.TimeOfDay <= new TimeSpan(06, 00, 00))
                    {
                        fromHoDate = date.Start.Date;
                        toHoDate = new DateTime(date.End.Date.Year, date.End.Date.Month, date.End.Date.Day, 23, 59, 00).AddDays(-1);
                    }
                    else
                    {
                        fromHoDate = date.Start.Date;
                        toHoDate = new DateTime(date.End.Date.Year, date.End.Date.Month, date.End.Date.Day, 23, 59, 00);
                    }

                    if (toHoDate < fromHoDate)
                        toHoDate = fromHoDate.AddDays(1).AddMinutes(-1);

                    if (date.Start.Year == 2015)
                    {

                        var transactions = _report.Transaction_2016.Where(x => (x.InvDateTime >= date.Start &&
                                                                                      x.InvDateTime <= date.End &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (date.Start.Year == 2016)
                    {

                        var transactions = _report.Transaction_2016.Where(x => (x.InvDateTime >= date.Start &&
                                                                                      x.InvDateTime <= date.End &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (date.Start.Year == 2017)
                    {

                        var transactions = _report.Transaction_2017.Where(x => (x.InvDateTime >= date.Start &&
                                                                                      x.InvDateTime <= date.End &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (date.Start.Year == 2018)
                    {

                        var transactions = _report.Transaction_2018.Where(x => (x.InvDateTime >= date.Start &&
                                                                                      x.InvDateTime <= date.End &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (date.Start.Year == 2019)
                    {

                        var transactions = _report.Transaction_2019.Where(x => (x.InvDateTime >= date.Start &&
                                                                                      x.InvDateTime <= date.End &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }

                    if (date.Start.Year == 2020)
                    {

                        var transactions = _report.Transaction_2020.Where(x => (x.InvDateTime >= date.Start &&
                                                                                      x.InvDateTime <= date.End &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);

                    }

                    if (date.Start.Year == 2021)
                    {

                        var transactions = _report.Transaction_2021.Where(x => (x.InvDateTime >= date.Start &&
                                                                                      x.InvDateTime <= date.End &&
                                                                                      x.LocationId != 1) ||
                                                                                      (x.InvDateTime >= fromHoDate &&
                                                                                      x.InvDateTime <= toHoDate &&
                                                                                      x.LocationId == 1));

                        if (!string.IsNullOrEmpty(locationArray))
                        {
                            var locationIds = (from id in locationArray.Split(',')
                                               select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value));
                        }

                        if (!string.IsNullOrEmpty(voucherTypesArray))
                        {
                            var voucherIds = (from id in voucherTypesArray.Split(',')
                                              select short.Parse(id)).ToList();
                            transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value));
                        }

                        salesReportItems.AddRange(from transaction in transactions
                                                  let calcDiscount = transaction.VoucherId.Value == 202 ||
                                                                     transaction.VoucherId.Value == 2023 ||
                                                                     transaction.VoucherId.Value == 2035 ||
                                                                     transaction.VoucherId.Value == 2036 ||
                                                                     transaction.VoucherId.Value == 2037
                                                                     ? transaction.Discount.Value
                                                                     : -transaction.Discount.Value
                                                  let salesReportItem = new SalesReportItem
                                                  {
                                                      Amount = transaction.Amount.Value,
                                                      AmountRecieved = transaction.AmountRecieved,
                                                      BaseQuantity = transaction.BaseQuantity.Value,
                                                      BaseUnit = transaction.BaseUnit,
                                                      BaseUnitId = transaction.BaseUnitId.Value,
                                                      Cash = transaction.Cash.Value,
                                                      CreditCard = transaction.CreditCard.Value,
                                                      CreditCardType = transaction.CreditCardType,
                                                      CustomerId = transaction.CustomerId.Value,
                                                      CustomerName = transaction.CustomerName,
                                                      CustomerNameAr = transaction.CustomerNameAr,
                                                      Date = transaction.Date.Value,
                                                      Discount = calcDiscount,
                                                      EntryId = transaction.EntryId.Value,
                                                      GroupCD = transaction.GroupCD.Value,
                                                      InvDateTime = transaction.InvDateTime.Value,
                                                      InvoiceNumber = transaction.InvoiceNumber.Value,
                                                      Knet = transaction.Knet.Value,
                                                      Location = transaction.Location,
                                                      LocationId = transaction.LocationId.Value,
                                                      NetAmount = transaction.NetAmount.Value,
                                                      ProdId = transaction.ProdId.Value,
                                                      ProductNameAr = transaction.ProductNameAr,
                                                      ProductNameEn = transaction.ProductNameEn,
                                                      Salesman = transaction.Salesman,
                                                      SalesReturn = transaction.SalesReturn.Value,
                                                      SellQuantity = transaction.SellQuantity.Value,
                                                      SellUnit = transaction.SellUnit,
                                                      SellUnitId = transaction.SellUnitId.Value,
                                                      UnitPrice = transaction.UnitPrice.Value,
                                                      Voucher = transaction.Voucher,
                                                      VoucherId = transaction.VoucherId.Value,
                                                      Year = transaction.Year.Value
                                                  }
                                                  select salesReportItem);
                    }
                }
            }

            return new SalesReportViewModel
            {
                SalesReportItems = salesReportItems
            };
        }

        // Used in Dashboard-Detail-Ajax
        public SalesReportDashboard GetSalesDashboardTransaction(
            DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray, string productStringArray, bool showGroupCD)
        {
            try
            {
                if (fromDate == null) fromDate = DateTime.Now;
                if (toDate == null) toDate = DateTime.Now;

                var transactions = GetSalesTransaction(fromDate, toDate, locationArray, voucherTypesArray).SalesReportItems;

                if (!showGroupCD)
                    transactions = transactions.Where(x => x.GroupCD != 329).ToList();


                var entryIdArray = new List<long>();
                entryIdArray.AddRange(from item in transactions select item.EntryId);

                var dividedEntry = SplitList(entryIdArray, 5000);

                long[] productIdArray = null;
                if (!string.IsNullOrEmpty(productStringArray))
                {
                    var productIdStringArray = productStringArray.Split(',');
                    productIdArray = Array.ConvertAll(productIdStringArray, s => long.Parse(s));
                }

                var transactionDetails = new List<TransDetailsViewModel>();


                var startYear = fromDate.Value.Year;
                var endYear = toDate.Value.Year;

                var startDate = fromDate.Value.Date;
                var endDate = toDate.Value;

                IQueryable<Trans_Detail_2015> transDetails2015 = null;
                IQueryable<Trans_Detail_2016> transDetails2016 = null;
                IQueryable<Trans_Detail_2017> transDetails2017 = null;
                IQueryable<Trans_Detail_2018> transDetails2018 = null;
                IQueryable<Trans_Detail_2019> transDetails2019 = null;
                IQueryable<Trans_Detail_2020> transDetails2020 = null;
                IQueryable<Trans_Detail_2021> transDetails2021 = null;

                for (var i = startYear; i <= endYear; i++)
                {
                    if (i == 2015)
                    {
                        if (productIdArray != null && productIdArray.Length > 0)
                            transDetails2015 = _report.Trans_Detail_2015.Where(x => x.InvDateTime >= startDate && x.InvDateTime <= endDate && productIdArray.Contains(x.ProdId.Value));
                        else
                            transDetails2015 = _report.Trans_Detail_2015.Where(x => x.InvDateTime >= startDate && x.InvDateTime <= endDate);

                        foreach (var entries in dividedEntry)
                        {
                            transactionDetails.AddRange(from detail in transDetails2015
                                                        where entries.Contains(detail.EntryId.Value)
                                                        select new TransDetailsViewModel
                                                        {
                                                            Amount = detail.Amount,
                                                            AmountRecieved = detail.AmountRecieved,
                                                            BaseQuantity = detail.BaseQuantity,
                                                            BaseUnit = detail.BaseUnit,
                                                            BaseUnitId = detail.BaseUnitId,
                                                            Cash = detail.Cash,
                                                            CreditCard = detail.CreditCard,
                                                            CreditCardType = detail.CreditCardType,
                                                            CustomerId = detail.CustomerId,
                                                            CustomerName = detail.CustomerName,
                                                            CustomerNameAr = detail.CustomerNameAr,
                                                            Date = detail.Date,
                                                            Discount = detail.Discount,
                                                            EntryId = detail.EntryId,
                                                            GroupCD = detail.GroupCD,
                                                            InvDateTime = detail.InvDateTime,
                                                            InvoiceNumber = detail.InvoiceNumber,
                                                            Knet = detail.Knet,
                                                            Location = detail.Location,
                                                            LocationId = detail.LocationId,
                                                            NetAmount = detail.NetAmount,
                                                            Oid = detail.Oid,
                                                            ProdId = detail.ProdId,
                                                            ProductNameAr = detail.ProductNameAr,
                                                            ProductNameEn = detail.ProductNameEn,
                                                            Salesman = detail.Salesman,
                                                            SalesReturn = detail.SalesReturn,
                                                            SellQuantity = detail.SellQuantity,
                                                            SellUnit = detail.SellUnit,
                                                            SellUnitId = detail.SellUnitId,
                                                            UnitPrice = detail.UnitPrice,
                                                            Voucher = detail.Voucher,
                                                            VoucherId = detail.VoucherId,
                                                            Year = detail.Year
                                                        });
                        }
                    }

                    if (i == 2016)
                    {
                        if (productIdArray != null && productIdArray.Length > 0)
                            transDetails2016 = _report.Trans_Detail_2016.Where(x => x.InvDateTime >= startDate && x.InvDateTime <= endDate && productIdArray.Contains(x.ProdId.Value));
                        else
                            transDetails2016 = _report.Trans_Detail_2016.Where(x => x.InvDateTime >= startDate && x.InvDateTime <= endDate);

                        foreach (var entries in dividedEntry)
                        {
                            transactionDetails.AddRange(from detail in transDetails2016
                                                        where entries.Contains(detail.EntryId.Value)
                                                        select new TransDetailsViewModel
                                                        {
                                                            Amount = detail.Amount,
                                                            AmountRecieved = detail.AmountRecieved,
                                                            BaseQuantity = detail.BaseQuantity,
                                                            BaseUnit = detail.BaseUnit,
                                                            BaseUnitId = detail.BaseUnitId,
                                                            Cash = detail.Cash,
                                                            CreditCard = detail.CreditCard,
                                                            CreditCardType = detail.CreditCardType,
                                                            CustomerId = detail.CustomerId,
                                                            CustomerName = detail.CustomerName,
                                                            CustomerNameAr = detail.CustomerNameAr,
                                                            Date = detail.Date,
                                                            Discount = detail.Discount,
                                                            EntryId = detail.EntryId,
                                                            GroupCD = detail.GroupCD,
                                                            InvDateTime = detail.InvDateTime,
                                                            InvoiceNumber = detail.InvoiceNumber,
                                                            Knet = detail.Knet,
                                                            Location = detail.Location,
                                                            LocationId = detail.LocationId,
                                                            NetAmount = detail.NetAmount,
                                                            Oid = detail.Oid,
                                                            ProdId = detail.ProdId,
                                                            ProductNameAr = detail.ProductNameAr,
                                                            ProductNameEn = detail.ProductNameEn,
                                                            Salesman = detail.Salesman,
                                                            SalesReturn = detail.SalesReturn,
                                                            SellQuantity = detail.SellQuantity,
                                                            SellUnit = detail.SellUnit,
                                                            SellUnitId = detail.SellUnitId,
                                                            UnitPrice = detail.UnitPrice,
                                                            Voucher = detail.Voucher,
                                                            VoucherId = detail.VoucherId,
                                                            Year = detail.Year
                                                        });
                        }
                    }

                    if (i == 2017)
                    {
                        if (productIdArray != null && productIdArray.Length > 0)
                            transDetails2017 = _report.Trans_Detail_2017.Where(x => x.InvDateTime >= startDate && x.InvDateTime <= endDate && productIdArray.Contains(x.ProdId.Value));
                        else
                            transDetails2017 = _report.Trans_Detail_2017.Where(x => x.InvDateTime >= startDate && x.InvDateTime <= endDate);

                        foreach (var entries in dividedEntry)
                        {
                            transactionDetails.AddRange(from detail in transDetails2017
                                                        where entries.Contains(detail.EntryId.Value)
                                                        select new TransDetailsViewModel
                                                        {
                                                            Amount = detail.Amount,
                                                            AmountRecieved = detail.AmountRecieved,
                                                            BaseQuantity = detail.BaseQuantity,
                                                            BaseUnit = detail.BaseUnit,
                                                            BaseUnitId = detail.BaseUnitId,
                                                            Cash = detail.Cash,
                                                            CreditCard = detail.CreditCard,
                                                            CreditCardType = detail.CreditCardType,
                                                            CustomerId = detail.CustomerId,
                                                            CustomerName = detail.CustomerName,
                                                            CustomerNameAr = detail.CustomerNameAr,
                                                            Date = detail.Date,
                                                            Discount = detail.Discount,
                                                            EntryId = detail.EntryId,
                                                            GroupCD = detail.GroupCD,
                                                            InvDateTime = detail.InvDateTime,
                                                            InvoiceNumber = detail.InvoiceNumber,
                                                            Knet = detail.Knet,
                                                            Location = detail.Location,
                                                            LocationId = detail.LocationId,
                                                            NetAmount = detail.NetAmount,
                                                            Oid = detail.Oid,
                                                            ProdId = detail.ProdId,
                                                            ProductNameAr = detail.ProductNameAr,
                                                            ProductNameEn = detail.ProductNameEn,
                                                            Salesman = detail.Salesman,
                                                            SalesReturn = detail.SalesReturn,
                                                            SellQuantity = detail.SellQuantity,
                                                            SellUnit = detail.SellUnit,
                                                            SellUnitId = detail.SellUnitId,
                                                            UnitPrice = detail.UnitPrice,
                                                            Voucher = detail.Voucher,
                                                            VoucherId = detail.VoucherId,
                                                            Year = detail.Year
                                                        });
                        }
                    }

                    if (i == 2018)
                    {
                        if (productIdArray != null && productIdArray.Length > 0)
                            transDetails2018 = _report.Trans_Detail_2018.Where(x => x.InvDateTime >= startDate && x.InvDateTime <= endDate && productIdArray.Contains(x.ProdId.Value));
                        else
                            transDetails2018 = _report.Trans_Detail_2018.Where(x => x.InvDateTime >= startDate && x.InvDateTime <= endDate);

                        foreach (var entries in dividedEntry)
                        {
                            transactionDetails.AddRange(from detail in transDetails2018
                                                        where entries.Contains(detail.EntryId.Value)
                                                        select new TransDetailsViewModel
                                                        {
                                                            Amount = detail.Amount,
                                                            AmountRecieved = detail.AmountRecieved,
                                                            BaseQuantity = detail.BaseQuantity,
                                                            BaseUnit = detail.BaseUnit,
                                                            BaseUnitId = detail.BaseUnitId,
                                                            Cash = detail.Cash,
                                                            CreditCard = detail.CreditCard,
                                                            CreditCardType = detail.CreditCardType,
                                                            CustomerId = detail.CustomerId,
                                                            CustomerName = detail.CustomerName,
                                                            CustomerNameAr = detail.CustomerNameAr,
                                                            Date = detail.Date,
                                                            Discount = detail.Discount,
                                                            EntryId = detail.EntryId,
                                                            GroupCD = detail.GroupCD,
                                                            InvDateTime = detail.InvDateTime,
                                                            InvoiceNumber = detail.InvoiceNumber,
                                                            Knet = detail.Knet,
                                                            Location = detail.Location,
                                                            LocationId = detail.LocationId,
                                                            NetAmount = detail.NetAmount,
                                                            Oid = detail.Oid,
                                                            ProdId = detail.ProdId,
                                                            ProductNameAr = detail.ProductNameAr,
                                                            ProductNameEn = detail.ProductNameEn,
                                                            Salesman = detail.Salesman,
                                                            SalesReturn = detail.SalesReturn,
                                                            SellQuantity = detail.SellQuantity,
                                                            SellUnit = detail.SellUnit,
                                                            SellUnitId = detail.SellUnitId,
                                                            UnitPrice = detail.UnitPrice,
                                                            Voucher = detail.Voucher,
                                                            VoucherId = detail.VoucherId,
                                                            Year = detail.Year
                                                        });
                        }
                    }

                    if (i == 2019)
                    {
                        if (productIdArray != null && productIdArray.Length > 0)
                            transDetails2019 = _report.Trans_Detail_2019.Where(x => x.InvDateTime >= startDate && x.InvDateTime <= endDate && productIdArray.Contains(x.ProdId.Value));
                        else
                            transDetails2019 = _report.Trans_Detail_2019.Where(x => x.InvDateTime >= startDate && x.InvDateTime <= endDate);

                        foreach (var entries in dividedEntry)
                        {
                            transactionDetails.AddRange(from detail in transDetails2019
                                                        where entries.Contains(detail.EntryId.Value)
                                                        select new TransDetailsViewModel
                                                        {
                                                            Amount = detail.Amount,
                                                            AmountRecieved = detail.AmountRecieved,
                                                            BaseQuantity = detail.BaseQuantity,
                                                            BaseUnit = detail.BaseUnit,
                                                            BaseUnitId = detail.BaseUnitId,
                                                            Cash = detail.Cash,
                                                            CreditCard = detail.CreditCard,
                                                            CreditCardType = detail.CreditCardType,
                                                            CustomerId = detail.CustomerId,
                                                            CustomerName = detail.CustomerName,
                                                            CustomerNameAr = detail.CustomerNameAr,
                                                            Date = detail.Date,
                                                            Discount = detail.Discount,
                                                            EntryId = detail.EntryId,
                                                            GroupCD = detail.GroupCD,
                                                            InvDateTime = detail.InvDateTime,
                                                            InvoiceNumber = detail.InvoiceNumber,
                                                            Knet = detail.Knet,
                                                            Location = detail.Location,
                                                            LocationId = detail.LocationId,
                                                            NetAmount = detail.NetAmount,
                                                            Oid = detail.Oid,
                                                            ProdId = detail.ProdId,
                                                            ProductNameAr = detail.ProductNameAr,
                                                            ProductNameEn = detail.ProductNameEn,
                                                            Salesman = detail.Salesman,
                                                            SalesReturn = detail.SalesReturn,
                                                            SellQuantity = detail.SellQuantity,
                                                            SellUnit = detail.SellUnit,
                                                            SellUnitId = detail.SellUnitId,
                                                            UnitPrice = detail.UnitPrice,
                                                            Voucher = detail.Voucher,
                                                            VoucherId = detail.VoucherId,
                                                            Year = detail.Year
                                                        });
                        }
                    }

                    if (i == 2020)
                    {
                        if (productIdArray != null && productIdArray.Length > 0)
                            transDetails2020 = _report.Trans_Detail_2020.Where(x => x.InvDateTime >= startDate && x.InvDateTime <= endDate && productIdArray.Contains(x.ProdId.Value));
                        else
                            transDetails2020 = _report.Trans_Detail_2020.Where(x => x.InvDateTime >= startDate && x.InvDateTime <= endDate);

                        foreach (var entries in dividedEntry)
                        {
                            transactionDetails.AddRange(from detail in transDetails2020
                                                        where entries.Contains(detail.EntryId.Value)
                                                        select new TransDetailsViewModel
                                                        {
                                                            Amount = detail.Amount,
                                                            AmountRecieved = detail.AmountRecieved,
                                                            BaseQuantity = detail.BaseQuantity,
                                                            BaseUnit = detail.BaseUnit,
                                                            BaseUnitId = detail.BaseUnitId,
                                                            Cash = detail.Cash,
                                                            CreditCard = detail.CreditCard,
                                                            CreditCardType = detail.CreditCardType,
                                                            CustomerId = detail.CustomerId,
                                                            CustomerName = detail.CustomerName,
                                                            CustomerNameAr = detail.CustomerNameAr,
                                                            Date = detail.Date,
                                                            Discount = detail.Discount,
                                                            EntryId = detail.EntryId,
                                                            GroupCD = detail.GroupCD,
                                                            InvDateTime = detail.InvDateTime,
                                                            InvoiceNumber = detail.InvoiceNumber,
                                                            Knet = detail.Knet,
                                                            Location = detail.Location,
                                                            LocationId = detail.LocationId,
                                                            NetAmount = detail.NetAmount,
                                                            Oid = detail.Oid,
                                                            ProdId = detail.ProdId,
                                                            ProductNameAr = detail.ProductNameAr,
                                                            ProductNameEn = detail.ProductNameEn,
                                                            Salesman = detail.Salesman,
                                                            SalesReturn = detail.SalesReturn,
                                                            SellQuantity = detail.SellQuantity,
                                                            SellUnit = detail.SellUnit,
                                                            SellUnitId = detail.SellUnitId,
                                                            UnitPrice = detail.UnitPrice,
                                                            Voucher = detail.Voucher,
                                                            VoucherId = detail.VoucherId,
                                                            Year = detail.Year
                                                        });
                        }
                    }

                    if (i == 2021)
                    {
                        if (productIdArray != null && productIdArray.Length > 0)
                            transDetails2021 = _report.Trans_Detail_2021.Where(x => x.InvDateTime >= startDate && x.InvDateTime <= endDate &&
                                                                                                     productIdArray.Contains(x.ProdId.Value));
                        else
                            transDetails2021 = _report.Trans_Detail_2021.Where(x => x.InvDateTime >= startDate && x.InvDateTime <= endDate);

                        foreach (var entries in dividedEntry)
                        {
                            transactionDetails.AddRange(from detail in transDetails2021
                                                        where entries.Contains(detail.EntryId.Value)
                                                        select new TransDetailsViewModel
                                                        {
                                                            Amount = detail.Amount,
                                                            AmountRecieved = detail.AmountRecieved,
                                                            BaseQuantity = detail.BaseQuantity,
                                                            BaseUnit = detail.BaseUnit,
                                                            BaseUnitId = detail.BaseUnitId,
                                                            Cash = detail.Cash,
                                                            CreditCard = detail.CreditCard,
                                                            CreditCardType = detail.CreditCardType,
                                                            CustomerId = detail.CustomerId,
                                                            CustomerName = detail.CustomerName,
                                                            CustomerNameAr = detail.CustomerNameAr,
                                                            Date = detail.Date,
                                                            Discount = detail.Discount,
                                                            EntryId = detail.EntryId,
                                                            GroupCD = detail.GroupCD,
                                                            InvDateTime = detail.InvDateTime,
                                                            InvoiceNumber = detail.InvoiceNumber,
                                                            Knet = detail.Knet,
                                                            Location = detail.Location,
                                                            LocationId = detail.LocationId,
                                                            NetAmount = detail.NetAmount,
                                                            Oid = detail.Oid,
                                                            ProdId = detail.ProdId,
                                                            ProductNameAr = detail.ProductNameAr,
                                                            ProductNameEn = detail.ProductNameEn,
                                                            Salesman = detail.Salesman,
                                                            SalesReturn = detail.SalesReturn,
                                                            SellQuantity = detail.SellQuantity,
                                                            SellUnit = detail.SellUnit,
                                                            SellUnitId = detail.SellUnitId,
                                                            UnitPrice = detail.UnitPrice,
                                                            Voucher = detail.Voucher,
                                                            VoucherId = detail.VoucherId,
                                                            Year = detail.Year
                                                        });
                        }
                    }
                }




                //foreach (var entries in dividedEntry)
                //{
                //    if (productIdArray != null && productIdArray.Length > 0)
                //    {
                //        var transDetails2015 = _report.Trans_Detail_2015.Where(x => entries.Contains(x.EntryId.Value) && productIdArray.Contains(x.ProdId.Value));
                //        transactionDetails.AddRange(from detail in transDetails2015
                //                                    select new TransDetailsViewModel
                //                                    {
                //                                        Amount = detail.Amount,
                //                                        AmountRecieved = detail.AmountRecieved,
                //                                        BaseQuantity = detail.BaseQuantity,
                //                                        BaseUnit = detail.BaseUnit,
                //                                        BaseUnitId = detail.BaseUnitId,
                //                                        Cash = detail.Cash,
                //                                        CreditCard = detail.CreditCard,
                //                                        CreditCardType = detail.CreditCardType,
                //                                        CustomerId = detail.CustomerId,
                //                                        CustomerName = detail.CustomerName,
                //                                        CustomerNameAr = detail.CustomerNameAr,
                //                                        Date = detail.Date,
                //                                        Discount = detail.Discount,
                //                                        EntryId = detail.EntryId,
                //                                        GroupCD = detail.GroupCD,
                //                                        InvDateTime = detail.InvDateTime,
                //                                        InvoiceNumber = detail.InvoiceNumber,
                //                                        Knet = detail.Knet,
                //                                        Location = detail.Location,
                //                                        LocationId = detail.LocationId,
                //                                        NetAmount = detail.NetAmount,
                //                                        Oid = detail.Oid,
                //                                        ProdId = detail.ProdId,
                //                                        ProductNameAr = detail.ProductNameAr,
                //                                        ProductNameEn = detail.ProductNameEn,
                //                                        Salesman = detail.Salesman,
                //                                        SalesReturn = detail.SalesReturn,
                //                                        SellQuantity = detail.SellQuantity,
                //                                        SellUnit = detail.SellUnit,
                //                                        SellUnitId = detail.SellUnitId,
                //                                        UnitPrice = detail.UnitPrice,
                //                                        Voucher = detail.Voucher,
                //                                        VoucherId = detail.VoucherId,
                //                                        Year = detail.Year
                //                                    });

                //        var transDetails2016 = _report.Trans_Detail_2016.Where(x => entries.Contains(x.EntryId.Value) && productIdArray.Contains(x.ProdId.Value));
                //        transactionDetails.AddRange(from detail in transDetails2016
                //                                    select new TransDetailsViewModel
                //                                    {
                //                                        Amount = detail.Amount,
                //                                        AmountRecieved = detail.AmountRecieved,
                //                                        BaseQuantity = detail.BaseQuantity,
                //                                        BaseUnit = detail.BaseUnit,
                //                                        BaseUnitId = detail.BaseUnitId,
                //                                        Cash = detail.Cash,
                //                                        CreditCard = detail.CreditCard,
                //                                        CreditCardType = detail.CreditCardType,
                //                                        CustomerId = detail.CustomerId,
                //                                        CustomerName = detail.CustomerName,
                //                                        CustomerNameAr = detail.CustomerNameAr,
                //                                        Date = detail.Date,
                //                                        Discount = detail.Discount,
                //                                        EntryId = detail.EntryId,
                //                                        GroupCD = detail.GroupCD,
                //                                        InvDateTime = detail.InvDateTime,
                //                                        InvoiceNumber = detail.InvoiceNumber,
                //                                        Knet = detail.Knet,
                //                                        Location = detail.Location,
                //                                        LocationId = detail.LocationId,
                //                                        NetAmount = detail.NetAmount,
                //                                        Oid = detail.Oid,
                //                                        ProdId = detail.ProdId,
                //                                        ProductNameAr = detail.ProductNameAr,
                //                                        ProductNameEn = detail.ProductNameEn,
                //                                        Salesman = detail.Salesman,
                //                                        SalesReturn = detail.SalesReturn,
                //                                        SellQuantity = detail.SellQuantity,
                //                                        SellUnit = detail.SellUnit,
                //                                        SellUnitId = detail.SellUnitId,
                //                                        UnitPrice = detail.UnitPrice,
                //                                        Voucher = detail.Voucher,
                //                                        VoucherId = detail.VoucherId,
                //                                        Year = detail.Year
                //                                    });

                //        var transDetails2017 = _report.Trans_Detail_2017.Where(x => entries.Contains(x.EntryId.Value) && productIdArray.Contains(x.ProdId.Value));
                //        transactionDetails.AddRange(from detail in transDetails2017
                //                                    select new TransDetailsViewModel
                //                                    {
                //                                        Amount = detail.Amount,
                //                                        AmountRecieved = detail.AmountRecieved,
                //                                        BaseQuantity = detail.BaseQuantity,
                //                                        BaseUnit = detail.BaseUnit,
                //                                        BaseUnitId = detail.BaseUnitId,
                //                                        Cash = detail.Cash,
                //                                        CreditCard = detail.CreditCard,
                //                                        CreditCardType = detail.CreditCardType,
                //                                        CustomerId = detail.CustomerId,
                //                                        CustomerName = detail.CustomerName,
                //                                        CustomerNameAr = detail.CustomerNameAr,
                //                                        Date = detail.Date,
                //                                        Discount = detail.Discount,
                //                                        EntryId = detail.EntryId,
                //                                        GroupCD = detail.GroupCD,
                //                                        InvDateTime = detail.InvDateTime,
                //                                        InvoiceNumber = detail.InvoiceNumber,
                //                                        Knet = detail.Knet,
                //                                        Location = detail.Location,
                //                                        LocationId = detail.LocationId,
                //                                        NetAmount = detail.NetAmount,
                //                                        Oid = detail.Oid,
                //                                        ProdId = detail.ProdId,
                //                                        ProductNameAr = detail.ProductNameAr,
                //                                        ProductNameEn = detail.ProductNameEn,
                //                                        Salesman = detail.Salesman,
                //                                        SalesReturn = detail.SalesReturn,
                //                                        SellQuantity = detail.SellQuantity,
                //                                        SellUnit = detail.SellUnit,
                //                                        SellUnitId = detail.SellUnitId,
                //                                        UnitPrice = detail.UnitPrice,
                //                                        Voucher = detail.Voucher,
                //                                        VoucherId = detail.VoucherId,
                //                                        Year = detail.Year
                //                                    });

                //        var transDetails2018 = _report.Trans_Detail_2018.Where(x => entries.Contains(x.EntryId.Value) && productIdArray.Contains(x.ProdId.Value));
                //        transactionDetails.AddRange(from detail in transDetails2018
                //                                    select new TransDetailsViewModel
                //                                    {
                //                                        Amount = detail.Amount,
                //                                        AmountRecieved = detail.AmountRecieved,
                //                                        BaseQuantity = detail.BaseQuantity,
                //                                        BaseUnit = detail.BaseUnit,
                //                                        BaseUnitId = detail.BaseUnitId,
                //                                        Cash = detail.Cash,
                //                                        CreditCard = detail.CreditCard,
                //                                        CreditCardType = detail.CreditCardType,
                //                                        CustomerId = detail.CustomerId,
                //                                        CustomerName = detail.CustomerName,
                //                                        CustomerNameAr = detail.CustomerNameAr,
                //                                        Date = detail.Date,
                //                                        Discount = detail.Discount,
                //                                        EntryId = detail.EntryId,
                //                                        GroupCD = detail.GroupCD,
                //                                        InvDateTime = detail.InvDateTime,
                //                                        InvoiceNumber = detail.InvoiceNumber,
                //                                        Knet = detail.Knet,
                //                                        Location = detail.Location,
                //                                        LocationId = detail.LocationId,
                //                                        NetAmount = detail.NetAmount,
                //                                        Oid = detail.Oid,
                //                                        ProdId = detail.ProdId,
                //                                        ProductNameAr = detail.ProductNameAr,
                //                                        ProductNameEn = detail.ProductNameEn,
                //                                        Salesman = detail.Salesman,
                //                                        SalesReturn = detail.SalesReturn,
                //                                        SellQuantity = detail.SellQuantity,
                //                                        SellUnit = detail.SellUnit,
                //                                        SellUnitId = detail.SellUnitId,
                //                                        UnitPrice = detail.UnitPrice,
                //                                        Voucher = detail.Voucher,
                //                                        VoucherId = detail.VoucherId,
                //                                        Year = detail.Year
                //                                    });

                //        var transDetails2019 = _report.Trans_Detail_2019.Where(x => entries.Contains(x.EntryId.Value) && productIdArray.Contains(x.ProdId.Value));
                //        transactionDetails.AddRange(from detail in transDetails2019
                //                                    select new TransDetailsViewModel
                //                                    {
                //                                        Amount = detail.Amount,
                //                                        AmountRecieved = detail.AmountRecieved,
                //                                        BaseQuantity = detail.BaseQuantity,
                //                                        BaseUnit = detail.BaseUnit,
                //                                        BaseUnitId = detail.BaseUnitId,
                //                                        Cash = detail.Cash,
                //                                        CreditCard = detail.CreditCard,
                //                                        CreditCardType = detail.CreditCardType,
                //                                        CustomerId = detail.CustomerId,
                //                                        CustomerName = detail.CustomerName,
                //                                        CustomerNameAr = detail.CustomerNameAr,
                //                                        Date = detail.Date,
                //                                        Discount = detail.Discount,
                //                                        EntryId = detail.EntryId,
                //                                        GroupCD = detail.GroupCD,
                //                                        InvDateTime = detail.InvDateTime,
                //                                        InvoiceNumber = detail.InvoiceNumber,
                //                                        Knet = detail.Knet,
                //                                        Location = detail.Location,
                //                                        LocationId = detail.LocationId,
                //                                        NetAmount = detail.NetAmount,
                //                                        Oid = detail.Oid,
                //                                        ProdId = detail.ProdId,
                //                                        ProductNameAr = detail.ProductNameAr,
                //                                        ProductNameEn = detail.ProductNameEn,
                //                                        Salesman = detail.Salesman,
                //                                        SalesReturn = detail.SalesReturn,
                //                                        SellQuantity = detail.SellQuantity,
                //                                        SellUnit = detail.SellUnit,
                //                                        SellUnitId = detail.SellUnitId,
                //                                        UnitPrice = detail.UnitPrice,
                //                                        Voucher = detail.Voucher,
                //                                        VoucherId = detail.VoucherId,
                //                                        Year = detail.Year
                //                                    });

                //        var transDetails2020 = _report.Trans_Detail_2020.Where(x => entries.Contains(x.EntryId.Value) && productIdArray.Contains(x.ProdId.Value));
                //        transactionDetails.AddRange(from detail in transDetails2020
                //                                    select new TransDetailsViewModel
                //                                    {
                //                                        Amount = detail.Amount,
                //                                        AmountRecieved = detail.AmountRecieved,
                //                                        BaseQuantity = detail.BaseQuantity,
                //                                        BaseUnit = detail.BaseUnit,
                //                                        BaseUnitId = detail.BaseUnitId,
                //                                        Cash = detail.Cash,
                //                                        CreditCard = detail.CreditCard,
                //                                        CreditCardType = detail.CreditCardType,
                //                                        CustomerId = detail.CustomerId,
                //                                        CustomerName = detail.CustomerName,
                //                                        CustomerNameAr = detail.CustomerNameAr,
                //                                        Date = detail.Date,
                //                                        Discount = detail.Discount,
                //                                        EntryId = detail.EntryId,
                //                                        GroupCD = detail.GroupCD,
                //                                        InvDateTime = detail.InvDateTime,
                //                                        InvoiceNumber = detail.InvoiceNumber,
                //                                        Knet = detail.Knet,
                //                                        Location = detail.Location,
                //                                        LocationId = detail.LocationId,
                //                                        NetAmount = detail.NetAmount,
                //                                        Oid = detail.Oid,
                //                                        ProdId = detail.ProdId,
                //                                        ProductNameAr = detail.ProductNameAr,
                //                                        ProductNameEn = detail.ProductNameEn,
                //                                        Salesman = detail.Salesman,
                //                                        SalesReturn = detail.SalesReturn,
                //                                        SellQuantity = detail.SellQuantity,
                //                                        SellUnit = detail.SellUnit,
                //                                        SellUnitId = detail.SellUnitId,
                //                                        UnitPrice = detail.UnitPrice,
                //                                        Voucher = detail.Voucher,
                //                                        VoucherId = detail.VoucherId,
                //                                        Year = detail.Year
                //                                    });

                //        var transDetails2021 = _report.Trans_Detail_2021.Where(x => entries.Contains(x.EntryId.Value) && productIdArray.Contains(x.ProdId.Value));
                //        transactionDetails.AddRange(from detail in transDetails2021
                //                                    select new TransDetailsViewModel
                //                                    {
                //                                        Amount = detail.Amount,
                //                                        AmountRecieved = detail.AmountRecieved,
                //                                        BaseQuantity = detail.BaseQuantity,
                //                                        BaseUnit = detail.BaseUnit,
                //                                        BaseUnitId = detail.BaseUnitId,
                //                                        Cash = detail.Cash,
                //                                        CreditCard = detail.CreditCard,
                //                                        CreditCardType = detail.CreditCardType,
                //                                        CustomerId = detail.CustomerId,
                //                                        CustomerName = detail.CustomerName,
                //                                        CustomerNameAr = detail.CustomerNameAr,
                //                                        Date = detail.Date,
                //                                        Discount = detail.Discount,
                //                                        EntryId = detail.EntryId,
                //                                        GroupCD = detail.GroupCD,
                //                                        InvDateTime = detail.InvDateTime,
                //                                        InvoiceNumber = detail.InvoiceNumber,
                //                                        Knet = detail.Knet,
                //                                        Location = detail.Location,
                //                                        LocationId = detail.LocationId,
                //                                        NetAmount = detail.NetAmount,
                //                                        Oid = detail.Oid,
                //                                        ProdId = detail.ProdId,
                //                                        ProductNameAr = detail.ProductNameAr,
                //                                        ProductNameEn = detail.ProductNameEn,
                //                                        Salesman = detail.Salesman,
                //                                        SalesReturn = detail.SalesReturn,
                //                                        SellQuantity = detail.SellQuantity,
                //                                        SellUnit = detail.SellUnit,
                //                                        SellUnitId = detail.SellUnitId,
                //                                        UnitPrice = detail.UnitPrice,
                //                                        Voucher = detail.Voucher,
                //                                        VoucherId = detail.VoucherId,
                //                                        Year = detail.Year
                //                                    });
                //    }
                //    else
                //    {
                //        IQueryable<Trans_Detail_2015> transDetails2015 = _report.Trans_Detail_2015.Where(x => entries.Contains(x.EntryId.Value));
                //        transactionDetails.AddRange(from detail in transDetails2015
                //                                    select new TransDetailsViewModel
                //                                    {
                //                                        Amount = detail.Amount,
                //                                        AmountRecieved = detail.AmountRecieved,
                //                                        BaseQuantity = detail.BaseQuantity,
                //                                        BaseUnit = detail.BaseUnit,
                //                                        BaseUnitId = detail.BaseUnitId,
                //                                        Cash = detail.Cash,
                //                                        CreditCard = detail.CreditCard,
                //                                        CreditCardType = detail.CreditCardType,
                //                                        CustomerId = detail.CustomerId,
                //                                        CustomerName = detail.CustomerName,
                //                                        CustomerNameAr = detail.CustomerNameAr,
                //                                        Date = detail.Date,
                //                                        Discount = detail.Discount,
                //                                        EntryId = detail.EntryId,
                //                                        GroupCD = detail.GroupCD,
                //                                        InvDateTime = detail.InvDateTime,
                //                                        InvoiceNumber = detail.InvoiceNumber,
                //                                        Knet = detail.Knet,
                //                                        Location = detail.Location,
                //                                        LocationId = detail.LocationId,
                //                                        NetAmount = detail.NetAmount,
                //                                        Oid = detail.Oid,
                //                                        ProdId = detail.ProdId,
                //                                        ProductNameAr = detail.ProductNameAr,
                //                                        ProductNameEn = detail.ProductNameEn,
                //                                        Salesman = detail.Salesman,
                //                                        SalesReturn = detail.SalesReturn,
                //                                        SellQuantity = detail.SellQuantity,
                //                                        SellUnit = detail.SellUnit,
                //                                        SellUnitId = detail.SellUnitId,
                //                                        UnitPrice = detail.UnitPrice,
                //                                        Voucher = detail.Voucher,
                //                                        VoucherId = detail.VoucherId,
                //                                        Year = detail.Year
                //                                    });

                //        var transDetails2016 = _report.Trans_Detail_2016.Where(x => entries.Contains(x.EntryId.Value));
                //        transactionDetails.AddRange(from detail in transDetails2016
                //                                    select new TransDetailsViewModel
                //                                    {
                //                                        Amount = detail.Amount,
                //                                        AmountRecieved = detail.AmountRecieved,
                //                                        BaseQuantity = detail.BaseQuantity,
                //                                        BaseUnit = detail.BaseUnit,
                //                                        BaseUnitId = detail.BaseUnitId,
                //                                        Cash = detail.Cash,
                //                                        CreditCard = detail.CreditCard,
                //                                        CreditCardType = detail.CreditCardType,
                //                                        CustomerId = detail.CustomerId,
                //                                        CustomerName = detail.CustomerName,
                //                                        CustomerNameAr = detail.CustomerNameAr,
                //                                        Date = detail.Date,
                //                                        Discount = detail.Discount,
                //                                        EntryId = detail.EntryId,
                //                                        GroupCD = detail.GroupCD,
                //                                        InvDateTime = detail.InvDateTime,
                //                                        InvoiceNumber = detail.InvoiceNumber,
                //                                        Knet = detail.Knet,
                //                                        Location = detail.Location,
                //                                        LocationId = detail.LocationId,
                //                                        NetAmount = detail.NetAmount,
                //                                        Oid = detail.Oid,
                //                                        ProdId = detail.ProdId,
                //                                        ProductNameAr = detail.ProductNameAr,
                //                                        ProductNameEn = detail.ProductNameEn,
                //                                        Salesman = detail.Salesman,
                //                                        SalesReturn = detail.SalesReturn,
                //                                        SellQuantity = detail.SellQuantity,
                //                                        SellUnit = detail.SellUnit,
                //                                        SellUnitId = detail.SellUnitId,
                //                                        UnitPrice = detail.UnitPrice,
                //                                        Voucher = detail.Voucher,
                //                                        VoucherId = detail.VoucherId,
                //                                        Year = detail.Year
                //                                    });

                //        var transDetails2017 = _report.Trans_Detail_2017.Where(x => entries.Contains(x.EntryId.Value));
                //        transactionDetails.AddRange(from detail in transDetails2017
                //                                    select new TransDetailsViewModel
                //                                    {
                //                                        Amount = detail.Amount,
                //                                        AmountRecieved = detail.AmountRecieved,
                //                                        BaseQuantity = detail.BaseQuantity,
                //                                        BaseUnit = detail.BaseUnit,
                //                                        BaseUnitId = detail.BaseUnitId,
                //                                        Cash = detail.Cash,
                //                                        CreditCard = detail.CreditCard,
                //                                        CreditCardType = detail.CreditCardType,
                //                                        CustomerId = detail.CustomerId,
                //                                        CustomerName = detail.CustomerName,
                //                                        CustomerNameAr = detail.CustomerNameAr,
                //                                        Date = detail.Date,
                //                                        Discount = detail.Discount,
                //                                        EntryId = detail.EntryId,
                //                                        GroupCD = detail.GroupCD,
                //                                        InvDateTime = detail.InvDateTime,
                //                                        InvoiceNumber = detail.InvoiceNumber,
                //                                        Knet = detail.Knet,
                //                                        Location = detail.Location,
                //                                        LocationId = detail.LocationId,
                //                                        NetAmount = detail.NetAmount,
                //                                        Oid = detail.Oid,
                //                                        ProdId = detail.ProdId,
                //                                        ProductNameAr = detail.ProductNameAr,
                //                                        ProductNameEn = detail.ProductNameEn,
                //                                        Salesman = detail.Salesman,
                //                                        SalesReturn = detail.SalesReturn,
                //                                        SellQuantity = detail.SellQuantity,
                //                                        SellUnit = detail.SellUnit,
                //                                        SellUnitId = detail.SellUnitId,
                //                                        UnitPrice = detail.UnitPrice,
                //                                        Voucher = detail.Voucher,
                //                                        VoucherId = detail.VoucherId,
                //                                        Year = detail.Year
                //                                    });

                //        var transDetails2018 = _report.Trans_Detail_2018.Where(x => entries.Contains(x.EntryId.Value));
                //        transactionDetails.AddRange(from detail in transDetails2018
                //                                    select new TransDetailsViewModel
                //                                    {
                //                                        Amount = detail.Amount,
                //                                        AmountRecieved = detail.AmountRecieved,
                //                                        BaseQuantity = detail.BaseQuantity,
                //                                        BaseUnit = detail.BaseUnit,
                //                                        BaseUnitId = detail.BaseUnitId,
                //                                        Cash = detail.Cash,
                //                                        CreditCard = detail.CreditCard,
                //                                        CreditCardType = detail.CreditCardType,
                //                                        CustomerId = detail.CustomerId,
                //                                        CustomerName = detail.CustomerName,
                //                                        CustomerNameAr = detail.CustomerNameAr,
                //                                        Date = detail.Date,
                //                                        Discount = detail.Discount,
                //                                        EntryId = detail.EntryId,
                //                                        GroupCD = detail.GroupCD,
                //                                        InvDateTime = detail.InvDateTime,
                //                                        InvoiceNumber = detail.InvoiceNumber,
                //                                        Knet = detail.Knet,
                //                                        Location = detail.Location,
                //                                        LocationId = detail.LocationId,
                //                                        NetAmount = detail.NetAmount,
                //                                        Oid = detail.Oid,
                //                                        ProdId = detail.ProdId,
                //                                        ProductNameAr = detail.ProductNameAr,
                //                                        ProductNameEn = detail.ProductNameEn,
                //                                        Salesman = detail.Salesman,
                //                                        SalesReturn = detail.SalesReturn,
                //                                        SellQuantity = detail.SellQuantity,
                //                                        SellUnit = detail.SellUnit,
                //                                        SellUnitId = detail.SellUnitId,
                //                                        UnitPrice = detail.UnitPrice,
                //                                        Voucher = detail.Voucher,
                //                                        VoucherId = detail.VoucherId,
                //                                        Year = detail.Year
                //                                    });

                //        var transDetails2019 = _report.Trans_Detail_2019.Where(x => entries.Contains(x.EntryId.Value));
                //        transactionDetails.AddRange(from detail in transDetails2019
                //                                    select new TransDetailsViewModel
                //                                    {
                //                                        Amount = detail.Amount,
                //                                        AmountRecieved = detail.AmountRecieved,
                //                                        BaseQuantity = detail.BaseQuantity,
                //                                        BaseUnit = detail.BaseUnit,
                //                                        BaseUnitId = detail.BaseUnitId,
                //                                        Cash = detail.Cash,
                //                                        CreditCard = detail.CreditCard,
                //                                        CreditCardType = detail.CreditCardType,
                //                                        CustomerId = detail.CustomerId,
                //                                        CustomerName = detail.CustomerName,
                //                                        CustomerNameAr = detail.CustomerNameAr,
                //                                        Date = detail.Date,
                //                                        Discount = detail.Discount,
                //                                        EntryId = detail.EntryId,
                //                                        GroupCD = detail.GroupCD,
                //                                        InvDateTime = detail.InvDateTime,
                //                                        InvoiceNumber = detail.InvoiceNumber,
                //                                        Knet = detail.Knet,
                //                                        Location = detail.Location,
                //                                        LocationId = detail.LocationId,
                //                                        NetAmount = detail.NetAmount,
                //                                        Oid = detail.Oid,
                //                                        ProdId = detail.ProdId,
                //                                        ProductNameAr = detail.ProductNameAr,
                //                                        ProductNameEn = detail.ProductNameEn,
                //                                        Salesman = detail.Salesman,
                //                                        SalesReturn = detail.SalesReturn,
                //                                        SellQuantity = detail.SellQuantity,
                //                                        SellUnit = detail.SellUnit,
                //                                        SellUnitId = detail.SellUnitId,
                //                                        UnitPrice = detail.UnitPrice,
                //                                        Voucher = detail.Voucher,
                //                                        VoucherId = detail.VoucherId,
                //                                        Year = detail.Year
                //                                    });

                //        var transDetails2020 = _report.Trans_Detail_2020.Where(x => entries.Contains(x.EntryId.Value));
                //        transactionDetails.AddRange(from detail in transDetails2020
                //                                    select new TransDetailsViewModel
                //                                    {
                //                                        Amount = detail.Amount,
                //                                        AmountRecieved = detail.AmountRecieved,
                //                                        BaseQuantity = detail.BaseQuantity,
                //                                        BaseUnit = detail.BaseUnit,
                //                                        BaseUnitId = detail.BaseUnitId,
                //                                        Cash = detail.Cash,
                //                                        CreditCard = detail.CreditCard,
                //                                        CreditCardType = detail.CreditCardType,
                //                                        CustomerId = detail.CustomerId,
                //                                        CustomerName = detail.CustomerName,
                //                                        CustomerNameAr = detail.CustomerNameAr,
                //                                        Date = detail.Date,
                //                                        Discount = detail.Discount,
                //                                        EntryId = detail.EntryId,
                //                                        GroupCD = detail.GroupCD,
                //                                        InvDateTime = detail.InvDateTime,
                //                                        InvoiceNumber = detail.InvoiceNumber,
                //                                        Knet = detail.Knet,
                //                                        Location = detail.Location,
                //                                        LocationId = detail.LocationId,
                //                                        NetAmount = detail.NetAmount,
                //                                        Oid = detail.Oid,
                //                                        ProdId = detail.ProdId,
                //                                        ProductNameAr = detail.ProductNameAr,
                //                                        ProductNameEn = detail.ProductNameEn,
                //                                        Salesman = detail.Salesman,
                //                                        SalesReturn = detail.SalesReturn,
                //                                        SellQuantity = detail.SellQuantity,
                //                                        SellUnit = detail.SellUnit,
                //                                        SellUnitId = detail.SellUnitId,
                //                                        UnitPrice = detail.UnitPrice,
                //                                        Voucher = detail.Voucher,
                //                                        VoucherId = detail.VoucherId,
                //                                        Year = detail.Year
                //                                    });

                //        var transDetails2021 = _report.Trans_Detail_2021.Where(x => entries.Contains(x.EntryId.Value));
                //        transactionDetails.AddRange(from detail in transDetails2021
                //                                    select new TransDetailsViewModel
                //                                    {
                //                                        Amount = detail.Amount,
                //                                        AmountRecieved = detail.AmountRecieved,
                //                                        BaseQuantity = detail.BaseQuantity,
                //                                        BaseUnit = detail.BaseUnit,
                //                                        BaseUnitId = detail.BaseUnitId,
                //                                        Cash = detail.Cash,
                //                                        CreditCard = detail.CreditCard,
                //                                        CreditCardType = detail.CreditCardType,
                //                                        CustomerId = detail.CustomerId,
                //                                        CustomerName = detail.CustomerName,
                //                                        CustomerNameAr = detail.CustomerNameAr,
                //                                        Date = detail.Date,
                //                                        Discount = detail.Discount,
                //                                        EntryId = detail.EntryId,
                //                                        GroupCD = detail.GroupCD,
                //                                        InvDateTime = detail.InvDateTime,
                //                                        InvoiceNumber = detail.InvoiceNumber,
                //                                        Knet = detail.Knet,
                //                                        Location = detail.Location,
                //                                        LocationId = detail.LocationId,
                //                                        NetAmount = detail.NetAmount,
                //                                        Oid = detail.Oid,
                //                                        ProdId = detail.ProdId,
                //                                        ProductNameAr = detail.ProductNameAr,
                //                                        ProductNameEn = detail.ProductNameEn,
                //                                        Salesman = detail.Salesman,
                //                                        SalesReturn = detail.SalesReturn,
                //                                        SellQuantity = detail.SellQuantity,
                //                                        SellUnit = detail.SellUnit,
                //                                        SellUnitId = detail.SellUnitId,
                //                                        UnitPrice = detail.UnitPrice,
                //                                        Voucher = detail.Voucher,
                //                                        VoucherId = detail.VoucherId,
                //                                        Year = detail.Year
                //                                    });
                //    }
                //}

                return new SalesReportDashboard
                {
                    SRItemsTrans = transactions,
                    SRItemsTransDetails = transactionDetails.OrderBy(x => x.InvDateTime.Value).ToList()
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // In Use
        public List<TransDetailsViewModel> GetSalesDetailTransaction(DateTime? fromDate, DateTime? toDate, string locationArray,
            string voucherTypesArray, string productStringArray)
        {
            try
            {
                var result = GetSalesDashboardTransaction(fromDate, toDate, locationArray, voucherTypesArray, productStringArray, false);
                return result.SRItemsTransDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SalesReportViewModel GetSalesReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            if (fromDate == null) fromDate = DateTime.Now;
            if (toDate == null) toDate = DateTime.Now;

            var salesReportItems = new List<SalesReportItem>();

            var fromHoDate = fromDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? fromDate.Value.Date.AddDays(1)
                : fromDate.Value.Date;

            var toHoDate = toDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(1)
                : new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(-1);

            if (toHoDate < fromHoDate)
                toHoDate = fromHoDate.AddDays(1).AddMinutes(-1);

            var transactions = GetTransactions(fromDate, toDate, fromHoDate, toHoDate);

            if (!string.IsNullOrEmpty(locationArray))
            {
                var locationIds = (from id in locationArray.Split(',')
                                   select short.Parse(id)).ToList();
                transactions = transactions.Where(x => locationIds.Contains(x.SM_Location.Locat_Cd));
            }

            if (!string.IsNullOrEmpty(voucherTypesArray))
            {
                var voucherIds = (from id in voucherTypesArray.Split(',')
                                  select short.Parse(id)).ToList();
                transactions = transactions.Where(x => voucherIds.Contains(x.ICS_Transaction_Types.Voucher_Type));
            }

            foreach (var transaction in transactions)
            {
                var salesReportItem = new SalesReportItem
                {
                    EntryId = transaction.Entry_Id,
                    Location = transaction.SM_Location.L_Short_Name,
                    LocationId = transaction.SM_Location.Locat_Cd,
                    Year = transaction.Voucher_Date.Year,
                    Date = transaction.Voucher_Date.Date,
                    InvDateTime = transaction.Voucher_Date,
                    Salesman = transaction.SM_SALESMAN.L_Sman_Name,
                    Voucher = transaction.ICS_Transaction_Types.L_Voucher_Name,
                    VoucherId = transaction.ICS_Transaction_Types.Voucher_Type,
                    InvoiceNumber = transaction.Voucher_No,
                    GroupCD = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.Group_Cd : 0,
                    CustomerId = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.Ldgr_Cd : 0,
                    CustomerName = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.L_Ldgr_Name : "",
                    CustomerNameAr = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.A_Ldgr_Name : "",

                    AmountRecieved = transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2037
                                    ? -transaction.Voucher_Amt_FC
                                    : transaction.Voucher_Amt_FC,

                    Discount = transaction.Discount_Amt_FC,

                    SalesReturn = transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2037
                                    ? -transaction.Net_Amt_FC
                                    : 0,

                    NetAmount = transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2037
                                    ? -transaction.Net_Amt_FC
                                    : transaction.Net_Amt_FC,

                    Cash = (transaction.ICS_Transaction_Types.Voucher_Type == 201 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2021 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2022 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2025 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2026 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2030) &&
                            string.IsNullOrEmpty(transaction.Credit_Card_Type)
                                ? transaction.Net_Amt_FC
                                : (transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                   transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                   transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                   transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                   transaction.ICS_Transaction_Types.Voucher_Type == 2037) &&
                                   string.IsNullOrEmpty(transaction.Credit_Card_Type)
                                        ? -transaction.Net_Amt_FC
                                        : 0,

                    Knet = (transaction.ICS_Transaction_Types.Voucher_Type == 201 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2021 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2022 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2025 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2026 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2030) &&
                           !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                           (transaction.Credit_Card_Type == "K-Net" || transaction.Credit_Card_Type == "Knet")
                                ? transaction.Net_Amt_FC
                                : (transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                   transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                   transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                   transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                   transaction.ICS_Transaction_Types.Voucher_Type == 2037) &&
                                   !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                                   (transaction.Credit_Card_Type == "K-Net" || transaction.Credit_Card_Type == "Knet")
                                             ? -transaction.Net_Amt_FC
                                             : 0,

                    CreditCard = (transaction.ICS_Transaction_Types.Voucher_Type == 201 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2021 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2022 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2025 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2026 ||
                            transaction.ICS_Transaction_Types.Voucher_Type == 2030) &&
                                 !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                                 transaction.Credit_Card_Type == "CC"
                                    ? transaction.Net_Amt_FC
                                    : (transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2037) &&
                                       !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                                       transaction.Credit_Card_Type == "CC"
                                           ? -transaction.Net_Amt_FC
                                           : 0,

                    CreditCardType = transaction.Credit_Card_Type
                };

                salesReportItems.Add(salesReportItem);
            }

            List<Transaction_2020> imillTrans2020 = null;
            var newTrans200 = new List<Transaction_2020>();

            List<Transaction_2021> imillTrans2021 = null;
            var newTrans2021 = new List<Transaction_2021>();

            foreach (var items in salesReportItems.GroupBy(x => x.Year).OrderBy(x => x.Key))
            {
                var startDate = items.OrderBy(x => x.Date).FirstOrDefault().Date;
                var endDate = items.OrderByDescending(x => x.Date).FirstOrDefault().Date;
                if (items.Key == 2020)
                {
                    foreach (var item in items)
                    {
                        imillTrans2020 = _report.Transaction_2020.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        if (!imillTrans2020.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                        {
                            var iTrans = new Transaction_2020
                            {
                                Amount = item.Amount,
                                AmountRecieved = item.AmountRecieved,
                                BaseQuantity = item.BaseQuantity,
                                BaseUnit = item.BaseUnit,
                                BaseUnitId = item.BaseUnitId,
                                Cash = item.Cash,
                                CreditCard = item.CreditCard,
                                CreditCardType = item.CreditCardType,
                                CustomerId = item.CustomerId,
                                CustomerName = item.CustomerName,
                                CustomerNameAr = item.CustomerNameAr,
                                Date = item.Date,
                                Discount = item.Discount,
                                EntryId = item.EntryId,
                                GroupCD = item.GroupCD,
                                InvDateTime = item.InvDateTime,
                                InvoiceNumber = item.InvoiceNumber,
                                Knet = item.Knet,
                                Location = item.Location,
                                LocationId = item.LocationId,
                                NetAmount = item.NetAmount,
                                ProdId = item.ProdId,
                                ProductNameAr = item.ProductNameAr,
                                ProductNameEn = item.ProductNameEn,
                                Salesman = item.Salesman,
                                SalesReturn = item.SalesReturn,
                                SellQuantity = item.SellQuantity,
                                SellUnit = item.SellUnit,
                                SellUnitId = item.SellUnitId,
                                UnitPrice = item.UnitPrice,
                                Voucher = item.Voucher,
                                VoucherId = item.VoucherId,
                                Year = item.Year

                            };

                            newTrans200.Add(iTrans);
                        }
                    }
                    if (newTrans200.Any())
                    {
                        _report.Transaction_2020.AddRange(newTrans200);
                        _report.SaveChanges();
                    }
                }

                if (items.Key == 2021)
                {
                    foreach (var item in items)
                    {
                        imillTrans2021 = _report.Transaction_2021.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        if (!imillTrans2021.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                        {
                            var iTrans = new Transaction_2021
                            {
                                Amount = item.Amount,
                                AmountRecieved = item.AmountRecieved,
                                BaseQuantity = item.BaseQuantity,
                                BaseUnit = item.BaseUnit,
                                BaseUnitId = item.BaseUnitId,
                                Cash = item.Cash,
                                CreditCard = item.CreditCard,
                                CreditCardType = item.CreditCardType,
                                CustomerId = item.CustomerId,
                                CustomerName = item.CustomerName,
                                CustomerNameAr = item.CustomerNameAr,
                                Date = item.Date,
                                Discount = item.Discount,
                                EntryId = item.EntryId,
                                GroupCD = item.GroupCD,
                                InvDateTime = item.InvDateTime,
                                InvoiceNumber = item.InvoiceNumber,
                                Knet = item.Knet,
                                Location = item.Location,
                                LocationId = item.LocationId,
                                NetAmount = item.NetAmount,
                                ProdId = item.ProdId,
                                ProductNameAr = item.ProductNameAr,
                                ProductNameEn = item.ProductNameEn,
                                Salesman = item.Salesman,
                                SalesReturn = item.SalesReturn,
                                SellQuantity = item.SellQuantity,
                                SellUnit = item.SellUnit,
                                SellUnitId = item.SellUnitId,
                                UnitPrice = item.UnitPrice,
                                Voucher = item.Voucher,
                                VoucherId = item.VoucherId,
                                Year = item.Year

                            };

                            newTrans2021.Add(iTrans);
                        }
                    }
                    if (newTrans2021.Any())
                    {
                        _report.Transaction_2021.AddRange(newTrans2021);
                        _report.SaveChanges();
                    }
                }
            }

            return new SalesReportViewModel
            {
                SalesReportItems = salesReportItems
            };
        }

        // Changed Here 
        public SalesReportViewModel GetSalesDetailReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray, string productStringArray)
        {
            long entryId = 0;
            // long prodId = 0;
            try
            {
                if (fromDate == null) fromDate = DateTime.Now;
                if (toDate == null) toDate = DateTime.Now;

                var fromHoDate = fromDate.Value.Date;
                var toHoDate = toDate.Value.Date;

                // var transactions = _context.spICSTrans_GetAll(fromDate, toDate, locationArray, voucherTypesArray, fromHoDate, toHoDate).ToList();
                var transactions = GetSalesTransaction(fromDate, toDate, locationArray, voucherTypesArray).SalesReportItems;

                var entryIdArray = new List<string>();
                entryIdArray.AddRange(from item in transactions select item.EntryId.ToString());

                //var entryIdString = entryIdArray != null && entryIdArray.Count > 0 ? string.Join(",", entryIdArray) : "";
                //var transactionDetails = _context.spICSTransDetail_GetAll(entryIdString, productStringArray).ToList();

                var entryIdString = entryIdArray != null && entryIdArray.Count > 0 ? string.Join(",", entryIdArray) : "";
                long[] entryIds = Array.ConvertAll(entryIdArray.ToArray(), s => long.Parse(s));
                // var transactionDetails = _context.spICSTransDetail_GetAll(entryIdString, "").ToList();
                var transactionDetails = _context.ICS_Transaction_Details.Where(x => entryIds.Contains(x.Entry_Id)).ToList();

                var dbItems = _context.ICS_Item.ToList();
                var itemUnits = _context.ICS_Unit.ToList();
                var itemUnitDetails = _context.ICS_Item_Unit_Details.ToList();

                var salesReportItems = new List<SalesReportItem>();

                foreach (var detail in transactionDetails)
                {
                    // prodId = detail.Prod_Cd;
                    var trans = transactions.FirstOrDefault(x => x.EntryId == detail.Entry_Id);
                    var IUD = itemUnitDetails.FirstOrDefault(x => x.Unit_Entry_Id == detail.Unit_Entry_ID);

                    var salesReportItem = new SalesReportItem
                    {
                        //Location = trans.L_Short_Name,
                        //LocationId = trans.Locat_Cd,
                        //InvDateTime = trans.Voucher_Date,
                        //Salesman = trans.L_Sman_Name,
                        //Voucher = trans.L_Voucher_Name,
                        //VoucherId = trans.Voucher_Type,
                        //InvoiceNumber = trans.Voucher_No,
                        //GroupCD = trans.Group_Cd,
                        //CustomerName = trans.L_Ldgr_Name,
                        //CustomerNameAr = trans.A_Ldgr_Name,
                        EntryId = detail.Entry_Id,
                        Location = trans.Location,
                        LocationId = trans.LocationId,
                        InvDateTime = trans.InvDateTime,
                        Salesman = trans.Salesman,
                        Voucher = trans.Voucher,
                        VoucherId = trans.VoucherId,
                        InvoiceNumber = trans.InvoiceNumber,
                        GroupCD = trans.GroupCD,
                        CustomerId = trans.CustomerId,
                        CustomerName = trans.CustomerName,
                        CustomerNameAr = trans.CustomerNameAr,
                        //ProdId = detail.ProdId,
                        //ProductNameEn = detail.ProdEn,
                        //ProductNameAr = detail.ProdAr,
                        //BaseQuantity = detail.IUDBaseQty,
                        //BaseUnit = detail.BaseUnit,
                        //BaseUnitId = detail.BaseUnitId,
                        //SellQuantity = detail.Qty,
                        //SellUnit = detail.SellUnit,
                        //SellUnitId = detail.SellUnitId,

                        ProdId = detail.Prod_Cd,
                        ProductNameEn = dbItems.FirstOrDefault(x => x.Prod_Cd == detail.Prod_Cd).L_Prod_Name,
                        ProductNameAr = dbItems.FirstOrDefault(x => x.Prod_Cd == detail.Prod_Cd).A_Prod_Name,
                        BaseQuantity = IUD.Base_Qty,
                        BaseUnit = itemUnits.FirstOrDefault(x => x.Unit_Cd == IUD.Base_Unit_Cd).L_Unit_Name,
                        BaseUnitId = IUD.Base_Unit_Cd,
                        SellQuantity = detail.Qty,
                        SellUnit = itemUnits.FirstOrDefault(x => x.Unit_Cd == IUD.Alt_Unit_Cd).L_Unit_Name,
                        SellUnitId = IUD.Alt_Unit_Cd,
                        LineNumber = detail.Line_No,


                        Discount = trans.VoucherId == 202 ||
                               trans.VoucherId == 2023 ||
                               trans.VoucherId == 2035 ||
                               trans.VoucherId == 2036 ||
                               trans.VoucherId == 2037
                                ? -detail.FC_Prod_Dis
                                : detail.FC_Prod_Dis,
                        Amount = trans.VoucherId == 202 ||
                               trans.VoucherId == 2023 ||
                               trans.VoucherId == 2035 ||
                               trans.VoucherId == 2036 ||
                               trans.VoucherId == 2037
                                ? -detail.FC_Amount
                                : detail.FC_Amount,
                        Year = trans.InvDateTime.Year,
                        Date = trans.InvDateTime.Date
                    };

                    salesReportItems.Add(salesReportItem);
                }


                List<Trans_Detail_2020> trans_detail_2020 = null;
                var newTransDetail2020 = new List<Trans_Detail_2020>();

                List<Trans_Detail_2021> trans_detail_2021 = null;
                var newTransDetail2021 = new List<Trans_Detail_2021>();

                foreach (var items in salesReportItems.GroupBy(x => x.Year).OrderBy(x => x.Key))
                {
                    var startDate = items.OrderBy(x => x.InvDateTime).FirstOrDefault().Date;
                    var endDate = items.OrderByDescending(x => x.InvDateTime).FirstOrDefault().Date;

                    if (items.Key == 2020)
                    {
                        foreach (var item in items)
                        {
                            entryId = item.EntryId;
                            trans_detail_2020 = _report.Trans_Detail_2020.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                            if (!trans_detail_2020.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                var iTrans = new Trans_Detail_2020
                                {
                                    Amount = item.Amount,
                                    AmountRecieved = item.AmountRecieved,
                                    BaseQuantity = item.BaseQuantity,
                                    BaseUnit = item.BaseUnit,
                                    BaseUnitId = item.BaseUnitId,
                                    Cash = item.Cash,
                                    CreditCard = item.CreditCard,
                                    CreditCardType = item.CreditCardType,
                                    CustomerId = item.CustomerId,
                                    CustomerName = item.CustomerName,
                                    CustomerNameAr = item.CustomerNameAr,
                                    Date = item.Date,
                                    Discount = item.Discount,
                                    EntryId = item.EntryId,
                                    GroupCD = item.GroupCD,
                                    InvDateTime = item.InvDateTime,
                                    InvoiceNumber = item.InvoiceNumber,
                                    Knet = item.Knet,
                                    Location = item.Location,
                                    LocationId = item.LocationId,
                                    NetAmount = item.NetAmount,
                                    ProdId = item.ProdId,
                                    ProductNameAr = item.ProductNameAr,
                                    ProductNameEn = item.ProductNameEn,
                                    Salesman = item.Salesman,
                                    SalesReturn = item.SalesReturn,
                                    SellQuantity = item.SellQuantity,
                                    SellUnit = item.SellUnit,
                                    SellUnitId = item.SellUnitId,
                                    UnitPrice = item.UnitPrice,
                                    Voucher = item.Voucher,
                                    VoucherId = item.VoucherId,
                                    Year = item.Year

                                };

                                newTransDetail2020.Add(iTrans);
                            }
                        }
                        if (newTransDetail2020.Any())
                        {
                            _report.Trans_Detail_2020.AddRange(newTransDetail2020);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2021)
                    {
                        foreach (var item in items)
                        {
                            entryId = item.EntryId;
                            trans_detail_2021 = _report.Trans_Detail_2021.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                            if (!trans_detail_2021.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                var iTrans = new Trans_Detail_2021
                                {
                                    Amount = item.Amount,
                                    AmountRecieved = item.AmountRecieved,
                                    BaseQuantity = item.BaseQuantity,
                                    BaseUnit = item.BaseUnit,
                                    BaseUnitId = item.BaseUnitId,
                                    Cash = item.Cash,
                                    CreditCard = item.CreditCard,
                                    CreditCardType = item.CreditCardType,
                                    CustomerId = item.CustomerId,
                                    CustomerName = item.CustomerName,
                                    CustomerNameAr = item.CustomerNameAr,
                                    Date = item.Date,
                                    Discount = item.Discount,
                                    EntryId = item.EntryId,
                                    GroupCD = item.GroupCD,
                                    InvDateTime = item.InvDateTime,
                                    InvoiceNumber = item.InvoiceNumber,
                                    Knet = item.Knet,
                                    Location = item.Location,
                                    LocationId = item.LocationId,
                                    NetAmount = item.NetAmount,
                                    ProdId = item.ProdId,
                                    ProductNameAr = item.ProductNameAr,
                                    ProductNameEn = item.ProductNameEn,
                                    Salesman = item.Salesman,
                                    SalesReturn = item.SalesReturn,
                                    SellQuantity = item.SellQuantity,
                                    SellUnit = item.SellUnit,
                                    SellUnitId = item.SellUnitId,
                                    UnitPrice = item.UnitPrice,
                                    Voucher = item.Voucher,
                                    VoucherId = item.VoucherId,
                                    Year = item.Year

                                };

                                newTransDetail2021.Add(iTrans);
                            }
                        }
                        if (newTransDetail2021.Any())
                        {
                            _report.Trans_Detail_2021.AddRange(newTransDetail2021);
                            _report.SaveChanges();
                        }
                    }
                }

                return new SalesReportViewModel
                {
                    SalesReportItems = salesReportItems
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // In Use
        public SalesPeakHourViewModel GetSalesHourlyReport(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            var salesReportItems = GetSalesTransaction(fromDate, toDate, locationArray, "").SalesReportItems.Where(x => x.GroupCD != 329);
            var salesPeakHourItems = new List<SalesPeakHourItem>();
            var hoursCount = 24;

            if (locationArray != "")
            {
                var locationListIds = new List<int>();
                var locationStringArray = locationArray.Split(',');

                foreach (var item in locationStringArray)
                {
                    var id = int.Parse(item);
                    locationListIds.Add(id);
                }

                var locations = _locationRepository.GetLocations().LocationItems.Where(x => locationListIds.Contains(x.LocationId));


                foreach (var location in locations)
                {
                    var currentstartHour = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 6, 00, 00);

                    for (int i = 1; i <= hoursCount; i++)
                    {
                        var salesItems = salesReportItems.Where(x => x.InvDateTime >= currentstartHour &&
                                                                     x.InvDateTime <= currentstartHour.Add(TimeSpan.FromMinutes(59)) &&
                                                                     x.LocationId == location.LocationId).ToList();

                        var salesPeakHourItem = new SalesPeakHourItem
                        {
                            LocationId = location.LocationId,
                            Location = location.Name,
                            Hour = currentstartHour,
                            Amount = salesItems.Sum(a => a.NetAmount),
                            TransCount = salesItems.Count()
                        };

                        if (salesPeakHourItem.Amount > 0)
                            salesPeakHourItems.Add(salesPeakHourItem);

                        currentstartHour = currentstartHour.Add(TimeSpan.FromHours(1));
                    }
                }
            }

            return new SalesPeakHourViewModel
            {
                SalesPeakHourItems = salesPeakHourItems
            };
        }

        // Method is used for Syncing Sales Data
        public string GetSales(int days)
        {
            try
            {
                var today = DateTime.Now;
                var fromDate = new DateTime(today.Year, today.Month, today.Day, 00, 00, 00).AddDays(-days);
                var toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);

                var salesReportItems = new List<SalesReportItem>();

                var transactions = GetTransactions(fromDate, toDate, fromDate, toDate);

                foreach (var transaction in transactions)
                {
                    var salesReportItem = new SalesReportItem
                    {
                        EntryId = transaction.Entry_Id,
                        Location = transaction.SM_Location.L_Short_Name,
                        LocationId = transaction.SM_Location.Locat_Cd,
                        Year = transaction.Voucher_Date.Year,
                        Date = transaction.Voucher_Date.Date,
                        InvDateTime = transaction.Voucher_Date,
                        Salesman = transaction.SM_SALESMAN.L_Sman_Name,
                        Voucher = transaction.ICS_Transaction_Types.L_Voucher_Name,
                        VoucherId = transaction.ICS_Transaction_Types.Voucher_Type,
                        InvoiceNumber = transaction.Voucher_No,
                        GroupCD = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.Group_Cd : 0,
                        CustomerId = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.Ldgr_Cd : 0,
                        CustomerName = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.L_Ldgr_Name : "",
                        CustomerNameAr = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.A_Ldgr_Name : "",

                        AmountRecieved = transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2037
                                        ? -transaction.Voucher_Amt_FC
                                        : transaction.Voucher_Amt_FC,

                        Discount = transaction.Discount_Amt_FC,

                        SalesReturn = transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2037
                                        ? -transaction.Net_Amt_FC
                                        : 0,

                        NetAmount = transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2037
                                        ? -transaction.Net_Amt_FC
                                        : transaction.Net_Amt_FC,

                        Cash = (transaction.ICS_Transaction_Types.Voucher_Type == 201 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2021 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2022 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2025 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2026 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2030) &&
                                string.IsNullOrEmpty(transaction.Credit_Card_Type)
                                    ? transaction.Net_Amt_FC
                                    : (transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2037) &&
                                       string.IsNullOrEmpty(transaction.Credit_Card_Type)
                                            ? -transaction.Net_Amt_FC
                                            : 0,

                        Knet = (transaction.ICS_Transaction_Types.Voucher_Type == 201 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2021 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2022 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2025 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2026 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2030) &&
                               !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                               (transaction.Credit_Card_Type == "K-Net" || transaction.Credit_Card_Type == "Knet")
                                    ? transaction.Net_Amt_FC
                                    : (transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2037) &&
                                       !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                                       (transaction.Credit_Card_Type == "K-Net" || transaction.Credit_Card_Type == "Knet")
                                                 ? -transaction.Net_Amt_FC
                                                 : 0,

                        CreditCard = (transaction.ICS_Transaction_Types.Voucher_Type == 201 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2021 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2022 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2025 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2026 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2030) &&
                                     !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                                     transaction.Credit_Card_Type == "CC"
                                        ? transaction.Net_Amt_FC
                                        : (transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                           transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                           transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                           transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
  transaction.ICS_Transaction_Types.Voucher_Type == 2037) &&
                                           !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                                           transaction.Credit_Card_Type == "CC"
                                               ? -transaction.Net_Amt_FC
                                               : 0,

                        CreditCardType = transaction.Credit_Card_Type
                    };

                    salesReportItems.Add(salesReportItem);
                }

                List<Transaction_2015> imillTrans2015 = null;
                var newTrans2015 = new List<Transaction_2015>();

                List<Transaction_2016> imillTrans2016 = null;
                var newTrans2016 = new List<Transaction_2016>();

                List<Transaction_2017> imillTrans2017 = null;
                var newTrans2017 = new List<Transaction_2017>();

                List<Transaction_2018> imillTrans2018 = null;
                var newTrans2018 = new List<Transaction_2018>();

                List<Transaction_2019> imillTrans2019 = null;
                var newTrans2019 = new List<Transaction_2019>();

                List<Transaction_2020> imillTrans2020 = null;
                var newTrans200 = new List<Transaction_2020>();

                List<Transaction_2021> imillTrans2021 = null;
                var newTrans2021 = new List<Transaction_2021>();


                foreach (var items in salesReportItems.GroupBy(x => x.Year).OrderBy(x => x.Key))
                {
                    var startDate = items.OrderBy(x => x.Date).FirstOrDefault().Date;
                    var endDate = items.OrderByDescending(x => x.Date).FirstOrDefault().Date;

                    if (items.Key == 2015)
                    {
                        imillTrans2015 = _report.Transaction_2015.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2015.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans2015.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2015
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans2015.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans2015.Any())
                        {
                            _report.Transaction_2015.AddRange(newTrans2015);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2016)
                    {
                        imillTrans2016 = _report.Transaction_2016.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2016.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans2016.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2016
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans2016.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans2016.Any())
                        {
                            _report.Transaction_2016.AddRange(newTrans2016);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2017)
                    {
                        imillTrans2017 = _report.Transaction_2017.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2017.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans2017.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2017
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans2017.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans2017.Any())
                        {
                            _report.Transaction_2017.AddRange(newTrans2017);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2018)
                    {
                        imillTrans2018 = _report.Transaction_2018.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2018.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans2018.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2018
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans2018.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans2018.Any())
                        {
                            _report.Transaction_2018.AddRange(newTrans2018);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2019)
                    {
                        imillTrans2019 = _report.Transaction_2019.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2019.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans2019.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2019
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans2019.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans2019.Any())
                        {
                            _report.Transaction_2019.AddRange(newTrans2019);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2020)
                    {
                        imillTrans2020 = _report.Transaction_2020.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2020.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans200.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2020
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans200.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans200.Any())
                        {
                            _report.Transaction_2020.AddRange(newTrans200);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2021)
                    {
                        imillTrans2021 = _report.Transaction_2021.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2021.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans2021.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2021
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans2021.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans2021.Any())
                        {
                            _report.Transaction_2021.AddRange(newTrans2021);
                            _report.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
                throw;
            }

            return "true";
        }

        // Method is used for Syncing Sales Detail Data
        // Changed here && x.ProdId == item.ProdId1
        public string GetSalesDetail(int days)
        {
            try
            {
                var today = DateTime.Now;
                var fromDate = new DateTime(today.Year, today.Month, today.Day, 00, 00, 00).AddDays(-days);
                var toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);

                var transactions = GetSalesTransaction(fromDate, toDate, "", "").SalesReportItems;

                var entryIdArray = new List<string>();
                entryIdArray.AddRange(from item in transactions select item.EntryId.ToString());

                //var entryIdString = entryIdArray != null && entryIdArray.Count > 0 ? string.Join(",", entryIdArray) : "";
                //var transactionDetails = _context.spICSTransDetail_GetAll(entryIdString, "").ToList();

                var entryIdString = entryIdArray != null && entryIdArray.Count > 0 ? string.Join(",", entryIdArray) : "";
                long[] entryIds = Array.ConvertAll(entryIdArray.ToArray(), s => long.Parse(s));
                // var transactionDetails = _context.spICSTransDetail_GetAll(entryIdString, "").ToList();
                var transactionDetails = _context.ICS_Transaction_Details.Where(x => entryIds.Contains(x.Entry_Id)).ToList();

                var dbItems = _context.ICS_Item.ToList();
                var itemUnits = _context.ICS_Unit.ToList();
                var itemUnitDetails = _context.ICS_Item_Unit_Details.ToList();

                var salesReportItems = new List<SalesReportItem>();

                foreach (var detail in transactionDetails)
                {
                    var trans = transactions.FirstOrDefault(x => x.EntryId == detail.Entry_Id);
                    var IUD = itemUnitDetails.FirstOrDefault(x => x.Unit_Entry_Id == detail.Unit_Entry_ID);

                    var salesReportItem = new SalesReportItem
                    {
                        EntryId = detail.Entry_Id,
                        Location = trans.Location,
                        LocationId = trans.LocationId,
                        InvDateTime = trans.InvDateTime,
                        Salesman = trans.Salesman,
                        Voucher = trans.Voucher,
                        VoucherId = trans.VoucherId,
                        InvoiceNumber = trans.InvoiceNumber,
                        GroupCD = trans.GroupCD,
                        CustomerId = trans.CustomerId,
                        CustomerName = trans.CustomerName,
                        CustomerNameAr = trans.CustomerNameAr,
                        //ProdId = detail.ProdId,
                        //ProductNameEn = detail.ProdEn,
                        //ProductNameAr = detail.ProdAr,
                        //BaseQuantity = detail.IUDBaseQty,
                        //BaseUnit = detail.BaseUnit,
                        //BaseUnitId = detail.BaseUnitId,
                        //SellQuantity = detail.Qty,
                        //SellUnit = detail.SellUnit,
                        //SellUnitId = detail.SellUnitId,

                        ProdId = detail.Prod_Cd,
                        ProductNameEn = dbItems.FirstOrDefault(x => x.Prod_Cd == detail.Prod_Cd).L_Prod_Name,
                        ProductNameAr = dbItems.FirstOrDefault(x => x.Prod_Cd == detail.Prod_Cd).A_Prod_Name,
                        BaseQuantity = IUD.Base_Qty,
                        BaseUnit = itemUnits.FirstOrDefault(x => x.Unit_Cd == IUD.Base_Unit_Cd).L_Unit_Name,
                        BaseUnitId = IUD.Base_Unit_Cd,
                        SellQuantity = detail.Qty,
                        SellUnit = itemUnits.FirstOrDefault(x => x.Unit_Cd == IUD.Alt_Unit_Cd).L_Unit_Name,
                        SellUnitId = IUD.Alt_Unit_Cd,
                        LineNumber = detail.Line_No,

                        Discount = trans.VoucherId == 202 ||
                               trans.VoucherId == 2023 ||
                               trans.VoucherId == 2035 ||
                               trans.VoucherId == 2036 ||
                               trans.VoucherId == 2037
                                ? -detail.FC_Prod_Dis
                                : detail.FC_Prod_Dis,
                        Amount = trans.VoucherId == 202 ||
                               trans.VoucherId == 2023 ||
                               trans.VoucherId == 2035 ||
                               trans.VoucherId == 2036 ||
                               trans.VoucherId == 2037
                                ? -detail.FC_Amount
                                : detail.FC_Amount,
                        Year = trans.InvDateTime.Year,
                        Date = trans.InvDateTime.Date
                    };

                    salesReportItems.Add(salesReportItem);
                }

                List<Trans_Detail_2015> trans_detail_2015 = null;
                var newTransDetail2015 = new List<Trans_Detail_2015>();

                List<Trans_Detail_2016> trans_detail_2016 = null;
                var newTransDetail2016 = new List<Trans_Detail_2016>();

                List<Trans_Detail_2017> trans_detail_2017 = null;
                var newTransDetail2017 = new List<Trans_Detail_2017>();

                List<Trans_Detail_2018> trans_detail_2018 = null;
                var newTransDetail2018 = new List<Trans_Detail_2018>();

                List<Trans_Detail_2019> trans_detail_2019 = null;
                var newTransDetail2019 = new List<Trans_Detail_2019>();

                List<Trans_Detail_2020> trans_detail_2020 = null;
                var newTransDetail2020 = new List<Trans_Detail_2020>();

                List<Trans_Detail_2021> trans_detail_2021 = null;
                var newTransDetail2021 = new List<Trans_Detail_2021>();

                foreach (var items in salesReportItems.GroupBy(x => x.Year).OrderBy(x => x.Key))
                {
                    var startDate = items.OrderBy(x => x.InvDateTime).FirstOrDefault().Date;
                    var endDate = items.OrderByDescending(x => x.InvDateTime).FirstOrDefault().Date;

                    if (items.Key == 2015)
                    {
                        trans_detail_2015 = _report.Trans_Detail_2015.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2015.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                if (!newTransDetail2015.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                                {
                                    var iTrans = new Trans_Detail_2015
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year,
                                        Line_No = item.LineNumber
                                    };

                                    newTransDetail2015.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2015.Any())
                        {
                            _report.Trans_Detail_2015.AddRange(newTransDetail2015);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2016)
                    {
                        trans_detail_2016 = _report.Trans_Detail_2016.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2016.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                if (!newTransDetail2016.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                                {
                                    var iTrans = new Trans_Detail_2016
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year,
                                        Line_No = item.LineNumber
                                    };

                                    newTransDetail2016.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2016.Any())
                        {
                            _report.Trans_Detail_2016.AddRange(newTransDetail2016);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2017)
                    {
                        trans_detail_2017 = _report.Trans_Detail_2017.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2017.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                if (!newTransDetail2017.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                                {
                                    var iTrans = new Trans_Detail_2017
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year,
                                        Line_No = item.LineNumber
                                    };

                                    newTransDetail2017.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2017.Any())
                        {
                            _report.Trans_Detail_2017.AddRange(newTransDetail2017);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2018)
                    {
                        trans_detail_2018 = _report.Trans_Detail_2018.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2018.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                if (!newTransDetail2018.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                                {
                                    var iTrans = new Trans_Detail_2018
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year,
                                        Line_No = item.LineNumber
                                    };

                                    newTransDetail2018.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2018.Any())
                        {
                            _report.Trans_Detail_2018.AddRange(newTransDetail2018);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2019)
                    {
                        trans_detail_2019 = _report.Trans_Detail_2019.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2019.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                if (!newTransDetail2019.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                                {
                                    var iTrans = new Trans_Detail_2019
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year,
                                        Line_No = item.LineNumber
                                    };

                                    newTransDetail2019.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2019.Any())
                        {
                            _report.Trans_Detail_2019.AddRange(newTransDetail2019);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2020)
                    {
                        trans_detail_2020 = _report.Trans_Detail_2020.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2020.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                if (!newTransDetail2020.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                                {
                                    var iTrans = new Trans_Detail_2020
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year,
                                        Line_No = item.LineNumber
                                    };

                                    newTransDetail2020.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2020.Any())
                        {
                            _report.Trans_Detail_2020.AddRange(newTransDetail2020);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2021)
                    {
                        trans_detail_2021 = _report.Trans_Detail_2021.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2021.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                if (!newTransDetail2021.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                                {
                                    var iTrans = new Trans_Detail_2021
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year,
                                        Line_No = item.LineNumber
                                    };

                                    newTransDetail2021.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2021.Any())
                        {
                            _report.Trans_Detail_2021.AddRange(newTransDetail2021);
                            _report.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "true";
        }

        // In Use
        public SalesReportDashboard GetSalesRecordDashboardTrans(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            try
            {
                if (fromDate == null) fromDate = DateTime.Now;
                if (toDate == null) toDate = DateTime.Now;

                var fromHoDate = fromDate.Value.Date;
                var toHoDate = toDate.Value.Date;

                var transactions = GetSalesTransaction(fromDate, toDate, locationArray, voucherTypesArray).SalesReportItems.Where(x => x.GroupCD != 329).ToList();

                return new SalesReportDashboard
                {
                    SRItemsTrans = transactions
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetSalesDashboardTransTest(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray, string productStringArray)
        {
            string dividedEntryCount = "";
            var count = 0;
            try
            {
                if (fromDate == null) fromDate = DateTime.Now;
                if (toDate == null) toDate = DateTime.Now;

                var fromHoDate = fromDate.Value.Date;
                var toHoDate = toDate.Value.Date;

                var transactions = GetSalesTransaction(fromDate, toDate, locationArray, voucherTypesArray).SalesReportItems;

                var entryIdArray = new List<long>();
                entryIdArray.AddRange(from item in transactions select item.EntryId);

                var dividedEntry = SplitList(entryIdArray, 5000);

                dividedEntryCount = dividedEntry.Count().ToString();

                long[] productIdArray = null;
                if (!string.IsNullOrEmpty(productStringArray))
                {
                    var productIdStringArray = productStringArray.Split(',');
                    productIdArray = Array.ConvertAll(productIdStringArray, s => long.Parse(s));
                }

                var transactionDetails = new List<Trans_Detail_2020>();


                foreach (var entries in dividedEntry)
                {
                    if (productIdArray != null && productIdArray.Length > 0)
                        transactionDetails.AddRange(_report.Trans_Detail_2020.Where(x => entries.Contains(x.EntryId.Value) && productIdArray.Contains(x.ProdId.Value)).ToList());
                    else
                        transactionDetails.AddRange(_report.Trans_Detail_2020.Where(x => entries.Contains(x.EntryId.Value)).ToList());
                    count += 1;
                }

                return "transaction was passed";

            }
            catch (Exception ex)
            {
                return ex.Message + "DEntry Count : " + dividedEntryCount + " and Count : " + count.ToString();
            }
        }

        public string GetSalesMonth(int year, int month, int from, int to)
        {
            try
            {
                // var daysInMonth = DateTime.DaysInMonth(2020, month);
                var fromDate = new DateTime(year, month, from, 00, 00, 00);
                var toDate = new DateTime(year, month, to, 23, 59, 59);

                var salesReportItems = new List<SalesReportItem>();

                var transactions = GetTransactions(fromDate, toDate, fromDate, toDate).ToList();

                foreach (var transaction in transactions)
                {
                    var salesReportItem = new SalesReportItem
                    {
                        EntryId = transaction.Entry_Id,
                        Location = transaction.SM_Location.L_Short_Name,
                        LocationId = transaction.SM_Location.Locat_Cd,
                        Year = transaction.Voucher_Date.Year,
                        Date = transaction.Voucher_Date.Date,
                        InvDateTime = transaction.Voucher_Date,
                        Salesman = transaction.SM_SALESMAN.L_Sman_Name,
                        Voucher = transaction.ICS_Transaction_Types.L_Voucher_Name,
                        VoucherId = transaction.ICS_Transaction_Types.Voucher_Type,
                        InvoiceNumber = transaction.Voucher_No,
                        GroupCD = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.Group_Cd : 0,
                        CustomerId = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.Ldgr_Cd : 0,
                        CustomerName = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.L_Ldgr_Name : "",
                        CustomerNameAr = transaction.GL_Ledger2 != null ? transaction.GL_Ledger2.A_Ldgr_Name : "",

                        AmountRecieved = transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2037
                                        ? -transaction.Voucher_Amt_FC
                                        : transaction.Voucher_Amt_FC,

                        Discount = transaction.Discount_Amt_FC,

                        SalesReturn = transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2037
                                        ? -transaction.Net_Amt_FC
                                        : 0,

                        NetAmount = transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                    transaction.ICS_Transaction_Types.Voucher_Type == 2037
                                        ? -transaction.Net_Amt_FC
                                        : transaction.Net_Amt_FC,

                        Cash = (transaction.ICS_Transaction_Types.Voucher_Type == 201 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2021 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2022 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2025 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2026 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2030) &&
                                string.IsNullOrEmpty(transaction.Credit_Card_Type)
                                    ? transaction.Net_Amt_FC
                                    : (transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2037) &&
                                       string.IsNullOrEmpty(transaction.Credit_Card_Type)
                                            ? -transaction.Net_Amt_FC
                                            : 0,

                        Knet = (transaction.ICS_Transaction_Types.Voucher_Type == 201 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2021 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2022 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2025 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2026 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2030) &&
                               !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                               (transaction.Credit_Card_Type == "K-Net" || transaction.Credit_Card_Type == "Knet")
                                    ? transaction.Net_Amt_FC
                                    : (transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                       transaction.ICS_Transaction_Types.Voucher_Type == 2037) &&
                                       !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                                       (transaction.Credit_Card_Type == "K-Net" || transaction.Credit_Card_Type == "Knet")
                                                 ? -transaction.Net_Amt_FC
                                                 : 0,

                        CreditCard = (transaction.ICS_Transaction_Types.Voucher_Type == 201 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2021 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2022 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2025 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2026 ||
                                transaction.ICS_Transaction_Types.Voucher_Type == 2030) &&
                                     !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                                     transaction.Credit_Card_Type == "CC"
                                        ? transaction.Net_Amt_FC
                                        : (transaction.ICS_Transaction_Types.Voucher_Type == 202 ||
                                           transaction.ICS_Transaction_Types.Voucher_Type == 2023 ||
                                           transaction.ICS_Transaction_Types.Voucher_Type == 2035 ||
                                           transaction.ICS_Transaction_Types.Voucher_Type == 2036 ||
                                           transaction.ICS_Transaction_Types.Voucher_Type == 2037) &&
                                           !string.IsNullOrEmpty(transaction.Credit_Card_Type) &&
                                           transaction.Credit_Card_Type == "CC"
                                               ? -transaction.Net_Amt_FC
                                               : 0,

                        CreditCardType = transaction.Credit_Card_Type
                    };

                    salesReportItems.Add(salesReportItem);
                }

                foreach (var items in salesReportItems.GroupBy(x => x.Year).OrderBy(x => x.Key))
                {
                    var startDate = items.OrderBy(x => x.Date).FirstOrDefault().Date;
                    var endDate = items.OrderByDescending(x => x.Date).FirstOrDefault().Date;

                    if (items.Key == 2021)
                    {
                        List<Transaction_2021> imillTrans2021 = null;
                        var newTrans2021 = new List<Transaction_2021>();

                        imillTrans2021 = _report.Transaction_2021.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2021.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans2021.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2021
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans2021.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans2021.Any())
                        {
                            _report.Transaction_2021.AddRange(newTrans2021);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2020)
                    {
                        List<Transaction_2020> imillTrans2020 = null;
                        var newTrans200 = new List<Transaction_2020>();

                        imillTrans2020 = _report.Transaction_2020.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2020.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans200.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2020
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans200.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans200.Any())
                        {
                            _report.Transaction_2020.AddRange(newTrans200);
                            _report.SaveChanges();

                            //var jsonPath = HttpContext.Current.Server.MapPath("~/App_Data/ICS_Transactions_2020.json");
                            //var list = new List<Transaction_2020>();
                            //var serializer = new JsonSerializer();
                            //using (StreamReader file = File.OpenText(jsonPath))
                            //{

                            //    list = (List<Transaction_2020>)serializer.Deserialize(file, typeof(List<Transaction_2020>));

                            //    //    string file = System.IO.File.ReadAllText(jsonPath);
                            //    //var list = JsonConvert.DeserializeObject<List<Transaction_2020>>(file);

                            //    var deleteItems = new List<Transaction_2020>();

                            //    foreach (var item in list)
                            //        if (newTrans200.Any(x => x.EntryId == item.EntryId))
                            //            deleteItems.Add(item);

                            //    if (deleteItems.Any())
                            //        foreach (var rec in deleteItems)
                            //            list.Remove(rec);

                            //    list.AddRange(newTrans200);
                            //var convertedJson = JsonConvert.SerializeObject(list, Formatting.Indented);
                            //File.WriteAllText(jsonPath, convertedJson);
                            // }

                            //if (list.Any())
                            //{
                            //    using (StreamWriter file = File.CreateText(jsonPath))
                            //    {
                            //        serializer.Serialize(file, list);
                            //    }
                            //}
                            //using (StreamWriter file = File.CreateText(HttpContext.Current.Server.MapPath("~/App_Data/ICS_Transactions_2020.json")))
                            //{
                            //    JsonSerializer serializer = new JsonSerializer();
                            //    serializer.Serialize(file, newTrans200);
                            //}
                        }
                    }

                    if (items.Key == 2019)
                    {
                        List<Transaction_2019> imillTrans2019 = null;
                        var newTrans2019 = new List<Transaction_2019>();

                        imillTrans2019 = _report.Transaction_2019.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2019.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans2019.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2019
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans2019.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans2019.Any())
                        {
                            _report.Transaction_2019.AddRange(newTrans2019);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2018)
                    {
                        List<Transaction_2018> imillTrans2018 = null;
                        var newTrans2018 = new List<Transaction_2018>();

                        imillTrans2018 = _report.Transaction_2018.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2018.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans2018.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2018
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans2018.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans2018.Any())
                        {
                            _report.Transaction_2018.AddRange(newTrans2018);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2017)
                    {
                        List<Transaction_2017> imillTrans2017 = null;
                        var newTrans2017 = new List<Transaction_2017>();

                        imillTrans2017 = _report.Transaction_2017.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2017.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans2017.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2017
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans2017.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans2017.Any())
                        {
                            _report.Transaction_2017.AddRange(newTrans2017);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2016)
                    {
                        List<Transaction_2016> imillTrans2016 = null;
                        var newTrans2016 = new List<Transaction_2016>();

                        imillTrans2016 = _report.Transaction_2016.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2016.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans2016.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2016
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans2016.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans2016.Any())
                        {
                            _report.Transaction_2016.AddRange(newTrans2016);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2015)
                    {
                        List<Transaction_2015> imillTrans2015 = null;
                        var newTrans2015 = new List<Transaction_2015>();

                        imillTrans2015 = _report.Transaction_2015.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
                        foreach (var item in items)
                        {
                            if (!imillTrans2015.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            {
                                if (!newTrans2015.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                                {
                                    var iTrans = new Transaction_2015
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTrans2015.Add(iTrans);
                                }
                            }
                        }
                        if (newTrans2015.Any())
                        {
                            _report.Transaction_2015.AddRange(newTrans2015);
                            _report.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
                throw;
            }

            return "true";
        }

        // Changed Here - Shabb - 21-01-2021
        public string GetSalesDetailMonth(int year, int month, int from, int to)
        {
            try
            {
                var fromDate = new DateTime(year, month, from, 00, 00, 00);
                var toDate = new DateTime(year, month, to, 23, 59, 59);

                var transactions = year == 2020
                                        ? GetSalesTransaction(fromDate, toDate, "", "").SalesReportItems
                                        : year == 2021
                                            ? GetSalesTransaction2021(fromDate, toDate, "", "").SalesReportItems
                                            : year == 2019
                                                ? GetSalesTransaction2019(fromDate, toDate, "", "").SalesReportItems
                                                : year == 2018
                                                    ? GetSalesTransaction2018(fromDate, toDate, "", "").SalesReportItems
                                                    : year == 2017
                                                        ? GetSalesTransaction2017(fromDate, toDate, "", "").SalesReportItems
                                                            : year == 2016
                                                            ? GetSalesTransaction2016(fromDate, toDate, "", "").SalesReportItems
                                                            : GetSalesTransaction2015(fromDate, toDate, "", "").SalesReportItems;

                var entryIdArray = new List<string>();
                entryIdArray.AddRange(from item in transactions select item.EntryId.ToString());

                var entryIdString = entryIdArray != null && entryIdArray.Count > 0 ? string.Join(",", entryIdArray) : "";
                long[] entryIds = Array.ConvertAll(entryIdArray.ToArray(), s => long.Parse(s));
                // var transactionDetails = _context.spICSTransDetail_GetAll(entryIdString, "").ToList();
                var transactionDetails = _context.ICS_Transaction_Details.Where(x => entryIds.Contains(x.Entry_Id)).ToList();

                var dbItems = _context.ICS_Item.ToList();
                var itemUnits = _context.ICS_Unit.ToList();
                var itemUnitDetails = _context.ICS_Item_Unit_Details.ToList();

                var salesReportItems = new List<SalesReportItem>();

                foreach (var detail in transactionDetails)
                {
                    if (detail.Line_No >= 0)
                    {
                        var trans = transactions.FirstOrDefault(x => x.EntryId == detail.Entry_Id);
                        var IUD = itemUnitDetails.FirstOrDefault(x => x.Unit_Entry_Id == detail.Unit_Entry_ID);
                        var salesReportItem = new SalesReportItem
                        {
                            EntryId = detail.Entry_Id,
                            Location = trans.Location,
                            LocationId = trans.LocationId,
                            InvDateTime = trans.InvDateTime,
                            Salesman = trans.Salesman,
                            Voucher = trans.Voucher,
                            VoucherId = trans.VoucherId,
                            InvoiceNumber = trans.InvoiceNumber,
                            GroupCD = trans.GroupCD,
                            CustomerId = trans.CustomerId,
                            CustomerName = trans.CustomerName,
                            CustomerNameAr = trans.CustomerNameAr,
                            //ProdId = detail.ProdId,
                            //ProductNameEn = detail.ProdEn,
                            //ProductNameAr = detail.ProdAr,
                            //BaseQuantity = detail.IUDBaseQty,
                            //BaseUnit = detail.BaseUnit,
                            //BaseUnitId = detail.BaseUnitId,
                            //SellQuantity = detail.Qty,
                            //SellUnit = detail.SellUnit,
                            //SellUnitId = detail.SellUnitId,
                            //LineNumber = detail.LineNumber,

                            ProdId = detail.Prod_Cd,
                            ProductNameEn = dbItems.FirstOrDefault(x => x.Prod_Cd == detail.Prod_Cd).L_Prod_Name,
                            ProductNameAr = dbItems.FirstOrDefault(x => x.Prod_Cd == detail.Prod_Cd).A_Prod_Name,
                            BaseQuantity = IUD.Base_Qty,
                            BaseUnit = itemUnits.FirstOrDefault(x => x.Unit_Cd == IUD.Base_Unit_Cd).L_Unit_Name,
                            BaseUnitId = IUD.Base_Unit_Cd,
                            SellQuantity = detail.Qty,
                            SellUnit = itemUnits.FirstOrDefault(x => x.Unit_Cd == IUD.Alt_Unit_Cd).L_Unit_Name,
                            SellUnitId = IUD.Alt_Unit_Cd,
                            LineNumber = detail.Line_No,


                            Discount = trans.VoucherId == 202 ||
                                   trans.VoucherId == 2023 ||
                                   trans.VoucherId == 2035 ||
                                   trans.VoucherId == 2036 ||
                                   trans.VoucherId == 2037
                                    ? -detail.FC_Prod_Dis
                                    : detail.FC_Prod_Dis,
                            Amount = trans.VoucherId == 202 ||
                                   trans.VoucherId == 2023 ||
                                   trans.VoucherId == 2035 ||
                                   trans.VoucherId == 2036 ||
                                   trans.VoucherId == 2037
                                    ? -detail.FC_Amount
                                    : detail.FC_Amount,
                            Year = trans.InvDateTime.Year,
                            Date = trans.InvDateTime.Date
                        };

                        salesReportItems.Add(salesReportItem);
                    }
                }

                foreach (var items in salesReportItems.GroupBy(x => x.Year).OrderBy(x => x.Key))
                {
                    var startDate = items.OrderBy(x => x.InvDateTime).FirstOrDefault().Date;
                    var endDate = items.OrderByDescending(x => x.InvDateTime).FirstOrDefault().Date;

                    if (items.Key == 2021)
                    {
                        List<Trans_Detail_2021> trans_detail_2021 = null;
                        var newTransDetail2021 = new List<Trans_Detail_2021>();

                        trans_detail_2021 = _report.Trans_Detail_2021.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            //if (!trans_detail_2021.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            //{
                            //    if (!newTransDetail2021.Any(x => x.Date == item.Date && x.InvDateTime == item.InvDateTime && x.EntryId == item.EntryId && x.InvoiceNumber == item.InvoiceNumber && x.LocationId == item.LocationId))
                            //    {

                            if (!trans_detail_2021.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber && x.InvDateTime == item.InvDateTime))
                            {
                                if (!newTransDetail2021.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber && x.InvDateTime == item.InvDateTime))
                                {
                                    var iTrans = new Trans_Detail_2021
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year,
                                        Line_No = item.LineNumber
                                    };

                                    newTransDetail2021.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2021.Any())
                        {
                            _report.Trans_Detail_2021.AddRange(newTransDetail2021);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2020)
                    {
                        List<Trans_Detail_2020> trans_detail_2020 = null;
                        var newTransDetail2020 = new List<Trans_Detail_2020>();

                        trans_detail_2020 = _report.Trans_Detail_2020.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2020.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                if (!newTransDetail2020.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                                {
                                    var iTrans = new Trans_Detail_2020
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year
                                    };

                                    newTransDetail2020.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2020.Any())
                        {
                            _report.Trans_Detail_2020.AddRange(newTransDetail2020);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2019)
                    {
                        List<Trans_Detail_2019> trans_detail_2019 = null;
                        var newTransDetail2019 = new List<Trans_Detail_2019>();

                        trans_detail_2019 = _report.Trans_Detail_2019.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2019.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                if (!newTransDetail2019.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                                {
                                    var iTrans = new Trans_Detail_2019
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTransDetail2019.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2019.Any())
                        {
                            _report.Trans_Detail_2019.AddRange(newTransDetail2019);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2018)
                    {
                        List<Trans_Detail_2018> trans_detail_2018 = null;
                        var newTransDetail2018 = new List<Trans_Detail_2018>();

                        trans_detail_2018 = _report.Trans_Detail_2018.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2018.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                if (!newTransDetail2018.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                                {
                                    var iTrans = new Trans_Detail_2018
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTransDetail2018.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2018.Any())
                        {
                            _report.Trans_Detail_2018.AddRange(newTransDetail2018);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2017)
                    {
                        List<Trans_Detail_2017> trans_detail_2017 = null;
                        var newTransDetail2017 = new List<Trans_Detail_2017>();

                        trans_detail_2017 = _report.Trans_Detail_2017.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2017.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                if (!newTransDetail2017.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                                {
                                    var iTrans = new Trans_Detail_2017
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTransDetail2017.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2017.Any())
                        {
                            _report.Trans_Detail_2017.AddRange(newTransDetail2017);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2016)
                    {
                        List<Trans_Detail_2016> trans_detail_2016 = null;
                        var newTransDetail2016 = new List<Trans_Detail_2016>();

                        trans_detail_2016 = _report.Trans_Detail_2016.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2016.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                if (!newTransDetail2016.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                                {
                                    var iTrans = new Trans_Detail_2016
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTransDetail2016.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2016.Any())
                        {
                            _report.Trans_Detail_2016.AddRange(newTransDetail2016);
                            _report.SaveChanges();
                        }
                    }

                    if (items.Key == 2015)
                    {
                        List<Trans_Detail_2015> trans_detail_2015 = null;
                        var newTransDetail2015 = new List<Trans_Detail_2015>();

                        trans_detail_2015 = _report.Trans_Detail_2015.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

                        foreach (var item in items)
                        {
                            if (!trans_detail_2015.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                            {
                                if (!newTransDetail2015.Any(x => x.Line_No == item.LineNumber && x.InvoiceNumber == item.InvoiceNumber))
                                {
                                    var iTrans = new Trans_Detail_2015
                                    {
                                        Amount = item.Amount,
                                        AmountRecieved = item.AmountRecieved,
                                        BaseQuantity = item.BaseQuantity,
                                        BaseUnit = item.BaseUnit,
                                        BaseUnitId = item.BaseUnitId,
                                        Cash = item.Cash,
                                        CreditCard = item.CreditCard,
                                        CreditCardType = item.CreditCardType,
                                        CustomerId = item.CustomerId,
                                        CustomerName = item.CustomerName,
                                        CustomerNameAr = item.CustomerNameAr,
                                        Date = item.Date,
                                        Discount = item.Discount,
                                        EntryId = item.EntryId,
                                        GroupCD = item.GroupCD,
                                        InvDateTime = item.InvDateTime,
                                        InvoiceNumber = item.InvoiceNumber,
                                        Knet = item.Knet,
                                        Location = item.Location,
                                        LocationId = item.LocationId,
                                        NetAmount = item.NetAmount,
                                        ProdId = item.ProdId,
                                        ProductNameAr = item.ProductNameAr,
                                        ProductNameEn = item.ProductNameEn,
                                        Salesman = item.Salesman,
                                        SalesReturn = item.SalesReturn,
                                        SellQuantity = item.SellQuantity,
                                        SellUnit = item.SellUnit,
                                        SellUnitId = item.SellUnitId,
                                        UnitPrice = item.UnitPrice,
                                        Voucher = item.Voucher,
                                        VoucherId = item.VoucherId,
                                        Year = item.Year

                                    };

                                    newTransDetail2015.Add(iTrans);
                                }
                            }
                        }
                        if (newTransDetail2015.Any())
                        {
                            _report.Trans_Detail_2015.AddRange(newTransDetail2015);
                            _report.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "true";
        }

        public SalesReportViewModel GetSalesTransaction2015(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            if (fromDate == null) fromDate = DateTime.Now;
            if (toDate == null) toDate = DateTime.Now;

            var salesReportItems = new List<SalesReportItem>();

            var fromHoDate = fromDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? fromDate.Value.Date.AddDays(1)
                : fromDate.Value.Date;

            var toHoDate = toDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(1)
                : new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(-1);

            if (toHoDate < fromHoDate)
                toHoDate = fromHoDate.AddDays(1).AddMinutes(-1);


            var transactions = _report.Transaction_2015.Where(x => (x.InvDateTime >= fromDate &&
                                                                          x.InvDateTime <= toDate &&
                                                                          x.LocationId != 1) ||
                                                                          (x.InvDateTime >= fromHoDate &&
                                                                          x.InvDateTime <= toHoDate &&
                                                                          x.LocationId == 1)).ToList();

            if (!string.IsNullOrEmpty(locationArray))
            {
                var locationIds = (from id in locationArray.Split(',')
                                   select short.Parse(id)).ToList();
                transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value)).ToList();
            }

            if (!string.IsNullOrEmpty(voucherTypesArray))
            {
                var voucherIds = (from id in voucherTypesArray.Split(',')
                                  select short.Parse(id)).ToList();
                transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value)).ToList();
            }

            foreach (var transaction in transactions)
            {
                var calcDiscount = transaction.VoucherId.Value == 202 ||
                                               transaction.VoucherId.Value == 2023 ||
                                               transaction.VoucherId.Value == 2035 ||
                                               transaction.VoucherId.Value == 2036 ||
                                               transaction.VoucherId.Value == 2037
                                               ? transaction.Discount.Value
                                               : -transaction.Discount.Value;

                var salesReportItem = new SalesReportItem
                {
                    Amount = transaction.Amount.Value,
                    AmountRecieved = transaction.AmountRecieved,
                    BaseQuantity = transaction.BaseQuantity.Value,
                    BaseUnit = transaction.BaseUnit,
                    BaseUnitId = transaction.BaseUnitId.Value,
                    Cash = transaction.Cash.Value,
                    CreditCard = transaction.CreditCard.Value,
                    CreditCardType = transaction.CreditCardType,
                    CustomerId = transaction.CustomerId.Value,
                    CustomerName = transaction.CustomerName,
                    CustomerNameAr = transaction.CustomerNameAr,
                    Date = transaction.Date.Value,
                    Discount = calcDiscount,
                    EntryId = transaction.EntryId.Value,
                    GroupCD = transaction.GroupCD.Value,
                    InvDateTime = transaction.InvDateTime.Value,
                    InvoiceNumber = transaction.InvoiceNumber.Value,
                    Knet = transaction.Knet.Value,
                    Location = transaction.Location,
                    LocationId = transaction.LocationId.Value,
                    NetAmount = transaction.NetAmount.Value,
                    ProdId = transaction.ProdId.Value,
                    ProductNameAr = transaction.ProductNameAr,
                    ProductNameEn = transaction.ProductNameEn,
                    Salesman = transaction.Salesman,
                    SalesReturn = transaction.SalesReturn.Value,
                    SellQuantity = transaction.SellQuantity.Value,
                    SellUnit = transaction.SellUnit,
                    SellUnitId = transaction.SellUnitId.Value,
                    UnitPrice = transaction.UnitPrice.Value,
                    Voucher = transaction.Voucher,
                    VoucherId = transaction.VoucherId.Value,
                    Year = transaction.Year.Value
                };

                salesReportItems.Add(salesReportItem);
            }

            return new SalesReportViewModel
            {
                SalesReportItems = salesReportItems
            };
        }

        public SalesReportViewModel GetSalesTransaction2016(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            if (fromDate == null) fromDate = DateTime.Now;
            if (toDate == null) toDate = DateTime.Now;

            var salesReportItems = new List<SalesReportItem>();

            var fromHoDate = fromDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? fromDate.Value.Date.AddDays(1)
                : fromDate.Value.Date;

            var toHoDate = toDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(1)
                : new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(-1);

            if (toHoDate < fromHoDate)
                toHoDate = fromHoDate.AddDays(1).AddMinutes(-1);


            var transactions = _report.Transaction_2016.Where(x => (x.InvDateTime >= fromDate &&
                                                                          x.InvDateTime <= toDate &&
                                                                          x.LocationId != 1) ||
                                                                          (x.InvDateTime >= fromHoDate &&
                                                                          x.InvDateTime <= toHoDate &&
                                                                          x.LocationId == 1)).ToList();

            if (!string.IsNullOrEmpty(locationArray))
            {
                var locationIds = (from id in locationArray.Split(',')
                                   select short.Parse(id)).ToList();
                transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value)).ToList();
            }

            if (!string.IsNullOrEmpty(voucherTypesArray))
            {
                var voucherIds = (from id in voucherTypesArray.Split(',')
                                  select short.Parse(id)).ToList();
                transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value)).ToList();
            }

            foreach (var transaction in transactions)
            {
                var calcDiscount = transaction.VoucherId.Value == 202 ||
                                               transaction.VoucherId.Value == 2023 ||
                                               transaction.VoucherId.Value == 2035 ||
                                               transaction.VoucherId.Value == 2036 ||
                                               transaction.VoucherId.Value == 2037
                                               ? transaction.Discount.Value
                                               : -transaction.Discount.Value;

                var salesReportItem = new SalesReportItem
                {
                    Amount = transaction.Amount.Value,
                    AmountRecieved = transaction.AmountRecieved,
                    BaseQuantity = transaction.BaseQuantity.Value,
                    BaseUnit = transaction.BaseUnit,
                    BaseUnitId = transaction.BaseUnitId.Value,
                    Cash = transaction.Cash.Value,
                    CreditCard = transaction.CreditCard.Value,
                    CreditCardType = transaction.CreditCardType,
                    CustomerId = transaction.CustomerId.Value,
                    CustomerName = transaction.CustomerName,
                    CustomerNameAr = transaction.CustomerNameAr,
                    Date = transaction.Date.Value,
                    Discount = calcDiscount,
                    EntryId = transaction.EntryId.Value,
                    GroupCD = transaction.GroupCD.Value,
                    InvDateTime = transaction.InvDateTime.Value,
                    InvoiceNumber = transaction.InvoiceNumber.Value,
                    Knet = transaction.Knet.Value,
                    Location = transaction.Location,
                    LocationId = transaction.LocationId.Value,
                    NetAmount = transaction.NetAmount.Value,
                    ProdId = transaction.ProdId.Value,
                    ProductNameAr = transaction.ProductNameAr,
                    ProductNameEn = transaction.ProductNameEn,
                    Salesman = transaction.Salesman,
                    SalesReturn = transaction.SalesReturn.Value,
                    SellQuantity = transaction.SellQuantity.Value,
                    SellUnit = transaction.SellUnit,
                    SellUnitId = transaction.SellUnitId.Value,
                    UnitPrice = transaction.UnitPrice.Value,
                    Voucher = transaction.Voucher,
                    VoucherId = transaction.VoucherId.Value,
                    Year = transaction.Year.Value
                };

                salesReportItems.Add(salesReportItem);
            }

            return new SalesReportViewModel
            {
                SalesReportItems = salesReportItems
            };
        }

        public SalesReportViewModel GetSalesTransaction2017(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            if (fromDate == null) fromDate = DateTime.Now;
            if (toDate == null) toDate = DateTime.Now;

            var salesReportItems = new List<SalesReportItem>();

            var fromHoDate = fromDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? fromDate.Value.Date.AddDays(1)
                : fromDate.Value.Date;

            var toHoDate = toDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(1)
                : new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(-1);

            if (toHoDate < fromHoDate)
                toHoDate = fromHoDate.AddDays(1).AddMinutes(-1);


            var transactions = _report.Transaction_2017.Where(x => (x.InvDateTime >= fromDate &&
                                                                          x.InvDateTime <= toDate &&
                                                                          x.LocationId != 1) ||
                                                                          (x.InvDateTime >= fromHoDate &&
                                                                          x.InvDateTime <= toHoDate &&
                                                                          x.LocationId == 1)).ToList();

            if (!string.IsNullOrEmpty(locationArray))
            {
                var locationIds = (from id in locationArray.Split(',')
                                   select short.Parse(id)).ToList();
                transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value)).ToList();
            }

            if (!string.IsNullOrEmpty(voucherTypesArray))
            {
                var voucherIds = (from id in voucherTypesArray.Split(',')
                                  select short.Parse(id)).ToList();
                transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value)).ToList();
            }

            foreach (var transaction in transactions)
            {
                var calcDiscount = transaction.VoucherId.Value == 202 ||
                                               transaction.VoucherId.Value == 2023 ||
                                               transaction.VoucherId.Value == 2035 ||
                                               transaction.VoucherId.Value == 2036 ||
                                               transaction.VoucherId.Value == 2037
                                               ? transaction.Discount.Value
                                               : -transaction.Discount.Value;

                var salesReportItem = new SalesReportItem
                {
                    Amount = transaction.Amount.Value,
                    AmountRecieved = transaction.AmountRecieved,
                    BaseQuantity = transaction.BaseQuantity.Value,
                    BaseUnit = transaction.BaseUnit,
                    BaseUnitId = transaction.BaseUnitId.Value,
                    Cash = transaction.Cash.Value,
                    CreditCard = transaction.CreditCard.Value,
                    CreditCardType = transaction.CreditCardType,
                    CustomerId = transaction.CustomerId.Value,
                    CustomerName = transaction.CustomerName,
                    CustomerNameAr = transaction.CustomerNameAr,
                    Date = transaction.Date.Value,
                    Discount = calcDiscount,
                    EntryId = transaction.EntryId.Value,
                    GroupCD = transaction.GroupCD.Value,
                    InvDateTime = transaction.InvDateTime.Value,
                    InvoiceNumber = transaction.InvoiceNumber.Value,
                    Knet = transaction.Knet.Value,
                    Location = transaction.Location,
                    LocationId = transaction.LocationId.Value,
                    NetAmount = transaction.NetAmount.Value,
                    ProdId = transaction.ProdId.Value,
                    ProductNameAr = transaction.ProductNameAr,
                    ProductNameEn = transaction.ProductNameEn,
                    Salesman = transaction.Salesman,
                    SalesReturn = transaction.SalesReturn.Value,
                    SellQuantity = transaction.SellQuantity.Value,
                    SellUnit = transaction.SellUnit,
                    SellUnitId = transaction.SellUnitId.Value,
                    UnitPrice = transaction.UnitPrice.Value,
                    Voucher = transaction.Voucher,
                    VoucherId = transaction.VoucherId.Value,
                    Year = transaction.Year.Value
                };

                salesReportItems.Add(salesReportItem);
            }

            return new SalesReportViewModel
            {
                SalesReportItems = salesReportItems
            };
        }

        public SalesReportViewModel GetSalesTransaction2018(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            if (fromDate == null) fromDate = DateTime.Now;
            if (toDate == null) toDate = DateTime.Now;

            var salesReportItems = new List<SalesReportItem>();

            var fromHoDate = fromDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? fromDate.Value.Date.AddDays(1)
                : fromDate.Value.Date;

            var toHoDate = toDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(1)
                : new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(-1);

            if (toHoDate < fromHoDate)
                toHoDate = fromHoDate.AddDays(1).AddMinutes(-1);


            var transactions = _report.Transaction_2018.Where(x => (x.InvDateTime >= fromDate &&
                                                                          x.InvDateTime <= toDate &&
                                                                          x.LocationId != 1) ||
                                                                          (x.InvDateTime >= fromHoDate &&
                                                                          x.InvDateTime <= toHoDate &&
                                                                          x.LocationId == 1)).ToList();

            if (!string.IsNullOrEmpty(locationArray))
            {
                var locationIds = (from id in locationArray.Split(',')
                                   select short.Parse(id)).ToList();
                transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value)).ToList();
            }

            if (!string.IsNullOrEmpty(voucherTypesArray))
            {
                var voucherIds = (from id in voucherTypesArray.Split(',')
                                  select short.Parse(id)).ToList();
                transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value)).ToList();
            }

            foreach (var transaction in transactions)
            {
                var calcDiscount = transaction.VoucherId.Value == 202 ||
                                               transaction.VoucherId.Value == 2023 ||
                                               transaction.VoucherId.Value == 2035 ||
                                               transaction.VoucherId.Value == 2036 ||
                                               transaction.VoucherId.Value == 2037
                                               ? transaction.Discount.Value
                                               : -transaction.Discount.Value;

                var salesReportItem = new SalesReportItem
                {
                    Amount = transaction.Amount.Value,
                    AmountRecieved = transaction.AmountRecieved,
                    BaseQuantity = transaction.BaseQuantity.Value,
                    BaseUnit = transaction.BaseUnit,
                    BaseUnitId = transaction.BaseUnitId.Value,
                    Cash = transaction.Cash.Value,
                    CreditCard = transaction.CreditCard.Value,
                    CreditCardType = transaction.CreditCardType,
                    CustomerId = transaction.CustomerId.Value,
                    CustomerName = transaction.CustomerName,
                    CustomerNameAr = transaction.CustomerNameAr,
                    Date = transaction.Date.Value,
                    Discount = calcDiscount,
                    EntryId = transaction.EntryId.Value,
                    GroupCD = transaction.GroupCD.Value,
                    InvDateTime = transaction.InvDateTime.Value,
                    InvoiceNumber = transaction.InvoiceNumber.Value,
                    Knet = transaction.Knet.Value,
                    Location = transaction.Location,
                    LocationId = transaction.LocationId.Value,
                    NetAmount = transaction.NetAmount.Value,
                    ProdId = transaction.ProdId.Value,
                    ProductNameAr = transaction.ProductNameAr,
                    ProductNameEn = transaction.ProductNameEn,
                    Salesman = transaction.Salesman,
                    SalesReturn = transaction.SalesReturn.Value,
                    SellQuantity = transaction.SellQuantity.Value,
                    SellUnit = transaction.SellUnit,
                    SellUnitId = transaction.SellUnitId.Value,
                    UnitPrice = transaction.UnitPrice.Value,
                    Voucher = transaction.Voucher,
                    VoucherId = transaction.VoucherId.Value,
                    Year = transaction.Year.Value
                };

                salesReportItems.Add(salesReportItem);
            }

            return new SalesReportViewModel
            {
                SalesReportItems = salesReportItems
            };
        }

        public SalesReportViewModel GetSalesTransaction2019(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            if (fromDate == null) fromDate = DateTime.Now;
            if (toDate == null) toDate = DateTime.Now;

            var salesReportItems = new List<SalesReportItem>();

            var fromHoDate = fromDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? fromDate.Value.Date.AddDays(1)
                : fromDate.Value.Date;

            var toHoDate = toDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(1)
                : new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(-1);

            if (toHoDate < fromHoDate)
                toHoDate = fromHoDate.AddDays(1).AddMinutes(-1);


            var transactions = _report.Transaction_2019.Where(x => (x.InvDateTime >= fromDate &&
                                                                          x.InvDateTime <= toDate &&
                                                                          x.LocationId != 1) ||
                                                                          (x.InvDateTime >= fromHoDate &&
                                                                          x.InvDateTime <= toHoDate &&
                                                                          x.LocationId == 1)).ToList();

            if (!string.IsNullOrEmpty(locationArray))
            {
                var locationIds = (from id in locationArray.Split(',')
                                   select short.Parse(id)).ToList();
                transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value)).ToList();
            }

            if (!string.IsNullOrEmpty(voucherTypesArray))
            {
                var voucherIds = (from id in voucherTypesArray.Split(',')
                                  select short.Parse(id)).ToList();
                transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value)).ToList();
            }

            foreach (var transaction in transactions)
            {
                var calcDiscount = transaction.VoucherId.Value == 202 ||
                                               transaction.VoucherId.Value == 2023 ||
                                               transaction.VoucherId.Value == 2035 ||
                                               transaction.VoucherId.Value == 2036 ||
                                               transaction.VoucherId.Value == 2037
                                               ? transaction.Discount.Value
                                               : -transaction.Discount.Value;

                var salesReportItem = new SalesReportItem
                {
                    Amount = transaction.Amount.Value,
                    AmountRecieved = transaction.AmountRecieved,
                    BaseQuantity = transaction.BaseQuantity.Value,
                    BaseUnit = transaction.BaseUnit,
                    BaseUnitId = transaction.BaseUnitId.Value,
                    Cash = transaction.Cash.Value,
                    CreditCard = transaction.CreditCard.Value,
                    CreditCardType = transaction.CreditCardType,
                    CustomerId = transaction.CustomerId.Value,
                    CustomerName = transaction.CustomerName,
                    CustomerNameAr = transaction.CustomerNameAr,
                    Date = transaction.Date.Value,
                    Discount = calcDiscount,
                    EntryId = transaction.EntryId.Value,
                    GroupCD = transaction.GroupCD.Value,
                    InvDateTime = transaction.InvDateTime.Value,
                    InvoiceNumber = transaction.InvoiceNumber.Value,
                    Knet = transaction.Knet.Value,
                    Location = transaction.Location,
                    LocationId = transaction.LocationId.Value,
                    NetAmount = transaction.NetAmount.Value,
                    ProdId = transaction.ProdId.Value,
                    ProductNameAr = transaction.ProductNameAr,
                    ProductNameEn = transaction.ProductNameEn,
                    Salesman = transaction.Salesman,
                    SalesReturn = transaction.SalesReturn.Value,
                    SellQuantity = transaction.SellQuantity.Value,
                    SellUnit = transaction.SellUnit,
                    SellUnitId = transaction.SellUnitId.Value,
                    UnitPrice = transaction.UnitPrice.Value,
                    Voucher = transaction.Voucher,
                    VoucherId = transaction.VoucherId.Value,
                    Year = transaction.Year.Value
                };

                salesReportItems.Add(salesReportItem);
            }

            return new SalesReportViewModel
            {
                SalesReportItems = salesReportItems
            };
        }

        public SalesReportViewModel GetSalesTransaction2021(DateTime? fromDate, DateTime? toDate, string locationArray, string voucherTypesArray)
        {
            if (fromDate == null) fromDate = DateTime.Now;
            if (toDate == null) toDate = DateTime.Now;

            var salesReportItems = new List<SalesReportItem>();

            var fromHoDate = fromDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? fromDate.Value.Date.AddDays(1)
                : fromDate.Value.Date;

            var toHoDate = toDate.Value.TimeOfDay >= new TimeSpan(06, 00, 00)
                ? new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(1)
                : new DateTime(toDate.Value.Date.Year, toDate.Value.Date.Month, toDate.Value.Date.Day, 23, 59, 00).AddDays(-1);

            if (toHoDate < fromHoDate)
                toHoDate = fromHoDate.AddDays(1).AddMinutes(-1);


            var transactions = _report.Transaction_2021.Where(x => (x.InvDateTime >= fromDate &&
                                                                          x.InvDateTime <= toDate &&
                                                                          x.LocationId != 1) ||
                                                                          (x.InvDateTime >= fromHoDate &&
                                                                          x.InvDateTime <= toHoDate &&
                                                                          x.LocationId == 1)).ToList();

            if (!string.IsNullOrEmpty(locationArray))
            {
                var locationIds = (from id in locationArray.Split(',')
                                   select short.Parse(id)).ToList();
                transactions = transactions.Where(x => locationIds.Contains(x.LocationId.Value)).ToList();
            }

            if (!string.IsNullOrEmpty(voucherTypesArray))
            {
                var voucherIds = (from id in voucherTypesArray.Split(',')
                                  select short.Parse(id)).ToList();
                transactions = transactions.Where(x => voucherIds.Contains(x.VoucherId.Value)).ToList();
            }

            foreach (var transaction in transactions)
            {
                var calcDiscount = transaction.VoucherId.Value == 202 ||
                                               transaction.VoucherId.Value == 2023 ||
                                               transaction.VoucherId.Value == 2035 ||
                                               transaction.VoucherId.Value == 2036 ||
                                               transaction.VoucherId.Value == 2037
                                               ? transaction.Discount.Value
                                               : -transaction.Discount.Value;

                var salesReportItem = new SalesReportItem
                {
                    Amount = transaction.Amount.Value,
                    AmountRecieved = transaction.AmountRecieved,
                    BaseQuantity = transaction.BaseQuantity.Value,
                    BaseUnit = transaction.BaseUnit,
                    BaseUnitId = transaction.BaseUnitId.Value,
                    Cash = transaction.Cash.Value,
                    CreditCard = transaction.CreditCard.Value,
                    CreditCardType = transaction.CreditCardType,
                    CustomerId = transaction.CustomerId.Value,
                    CustomerName = transaction.CustomerName,
                    CustomerNameAr = transaction.CustomerNameAr,
                    Date = transaction.Date.Value,
                    Discount = calcDiscount,
                    EntryId = transaction.EntryId.Value,
                    GroupCD = transaction.GroupCD.Value,
                    InvDateTime = transaction.InvDateTime.Value,
                    InvoiceNumber = transaction.InvoiceNumber.Value,
                    Knet = transaction.Knet.Value,
                    Location = transaction.Location,
                    LocationId = transaction.LocationId.Value,
                    NetAmount = transaction.NetAmount.Value,
                    ProdId = transaction.ProdId.Value,
                    ProductNameAr = transaction.ProductNameAr,
                    ProductNameEn = transaction.ProductNameEn,
                    Salesman = transaction.Salesman,
                    SalesReturn = transaction.SalesReturn.Value,
                    SellQuantity = transaction.SellQuantity.Value,
                    SellUnit = transaction.SellUnit,
                    SellUnitId = transaction.SellUnitId.Value,
                    UnitPrice = transaction.UnitPrice.Value,
                    Voucher = transaction.Voucher,
                    VoucherId = transaction.VoucherId.Value,
                    Year = transaction.Year.Value
                };

                salesReportItems.Add(salesReportItem);
            }

            return new SalesReportViewModel
            {
                SalesReportItems = salesReportItems
            };
        }

        public List<DailyConsumptionVM> GetDailyConsumptionTrans(DateTime? fromDate, DateTime? toDate)
        {
            var salesDetails = GetSalesDashboardTransaction(fromDate, toDate, "", "", "", true).SRItemsTransDetails.Where(x => x.LocationId == 1);

            var dailyConsumptions = new List<DailyConsumptionVM>();

            var ledgerIds = new int[] {53917, 53331, 53334, 53649, 53333, 53357, 53593, 53818,
                             53739, 53877, 53899, 54273, 53756, 54625, 54561, 54536,
                             54631, 54342, 54827, 54901, 54967, 54015, 54096, 54103,
                             54133, 53332, 54732};

            foreach (var item in salesDetails.GroupBy(x => x.ProdId))
            {
                var branchLedgers = item.Where(x => ledgerIds.ToList().Contains(x.CustomerId.Value));

                var sellBranchQtyKg = branchLedgers.Where(x => x.BaseUnitId == 40).Sum(x => x.BaseQuantity * x.SellQuantity);
                var sellBranchQtyGm = branchLedgers.Where(x => x.BaseUnitId == 42).Sum(x => (x.BaseQuantity * x.SellQuantity) / 1000);
                var totalBranchSellKgQty = sellBranchQtyKg + sellBranchQtyGm;

                var totalBranchSellQty = branchLedgers.Where(x => x.BaseUnitId != 40 &&
                                                                  x.BaseUnitId != 42 &&
                                                                  x.ProdId != 19595).Sum(x => x.SellQuantity);

                var sellQtyKg = item.Where(x => x.BaseUnitId == 40).Sum(x => x.BaseQuantity * x.SellQuantity);
                var sellQtyGm = item.Where(x => x.BaseUnitId == 42).Sum(x => (x.BaseQuantity * x.SellQuantity) / 1000);
                var totalSellKgQty = sellQtyKg + sellQtyGm;

                var totalSellQty = item.Where(x => x.BaseUnitId != 40 &&
                                                   x.BaseUnitId != 42 &&
                                                   x.ProdId != 19595).Sum(x => x.SellQuantity);

                var creditVoucher = item.Where(x => x.VoucherId == 2021);
                var sellCreditQtyKg = creditVoucher.Where(x => x.BaseUnitId == 40).Sum(x => x.BaseQuantity * x.SellQuantity);
                var sellCreditQtyGm = creditVoucher.Where(x => x.BaseUnitId == 42).Sum(x => (x.BaseQuantity * x.SellQuantity) / 1000);
                var totalSellCreditKgQty = sellCreditQtyKg + sellCreditQtyGm;

                var totalCreditSellQty = creditVoucher.Where(x => x.BaseUnitId != 40 &&
                                                   x.BaseUnitId != 42 &&
                                                   x.ProdId != 19595).Sum(x => x.SellQuantity);

                var cashVoucher = item.Where(x => x.VoucherId == 2022);
                var sellCashQtyKg = cashVoucher.Where(x => x.BaseUnitId == 40).Sum(x => x.BaseQuantity * x.SellQuantity);
                var sellCashQtyGm = cashVoucher.Where(x => x.BaseUnitId == 42).Sum(x => (x.BaseQuantity * x.SellQuantity) / 1000);
                var totalSellCashKgQty = sellCashQtyKg + sellCashQtyGm;

                var totalCashSellQty = cashVoucher.Where(x => x.BaseUnitId != 40 &&
                                                   x.BaseUnitId != 42 &&
                                                   x.ProdId != 19595).Sum(x => x.SellQuantity);

                decimal totalBranchQty = totalBranchSellKgQty != 0 ? totalBranchSellKgQty.Value : totalBranchSellQty.Value;
                decimal creditQty = totalSellCreditKgQty != 0 ? totalSellCreditKgQty.Value : totalCreditSellQty.Value;
                decimal cashQty = totalSellCashKgQty != 0 ? totalSellCashKgQty.Value : totalCashSellQty.Value;

                dailyConsumptions.Add(new DailyConsumptionVM
                {
                    Oid = item.FirstOrDefault().Oid,
                    ItemOid = item.Key,
                    ItemNameEn = item.FirstOrDefault().ProductNameEn,
                    ItemNameAr = item.FirstOrDefault().ProductNameAr,
                    BaseUnit = totalSellKgQty != 0 ? "Kg" : item.FirstOrDefault().BaseUnit,
                    TotalBranchQty = Math.Abs(totalBranchQty - creditQty),
                    CreditQty = creditQty,
                    CashQty = cashQty,
                    TotalQty = creditQty + cashQty
                    //TotalQty = totalSellKgQty != 0 ? totalSellKgQty : totalSellQty
                });
            }

            return dailyConsumptions;
        }

        public void GetReportByItemGroup(DateTime? fromDate, DateTime? toDate)
        {
            var salesDetails = GetSalesDetailTransaction(fromDate, toDate, "", "", "");
            // prod_cd is prodId
            // get item list and its group from item table and group table

            var salesProdGroup = salesDetails.GroupBy(x => x.ProdId);
            var itemGroups = _context.ICS_Item_Group.ToList();
            var items = _context.ICS_Item.ToList();

            var salesDetailsByItemGroups = (from item in salesProdGroup.Where(x => x.Key.HasValue)
                                            let salesItem = items.FirstOrDefault(x => x.Prod_Cd == item.Key.Value)
                                            let itemGroup = itemGroups.FirstOrDefault(x => x.Group_Cd == salesItem.Group_Cd)
                                            select new SalesItemGroup
                                            {
                                                ProdId = salesItem.Prod_Cd,
                                                ProdNameEn = salesItem.L_Prod_Name,
                                                ProdNameAr = salesItem.A_Prod_Name,
                                                GroupCd = itemGroup.Group_Cd,
                                                ParentGroupCd = itemGroup.M_Group_Cd,
                                                GroupNameEn = itemGroup.L_Group_Name,
                                                GroupNameAr = itemGroup.A_Group_Name,
                                                TotalAmount = item.Sum(x => x.Amount)
                                            }).GroupBy(x => x.GroupCd).ToList();


            var salesItemGroups = salesDetailsByItemGroups.Select(x => new SalesItemGroup
            {
                ProdId = x.FirstOrDefault().ProdId,
                ProdNameEn = x.FirstOrDefault().ProdNameEn,
                ProdNameAr = x.FirstOrDefault().ProdNameAr,
                GroupCd = x.Key,
                ParentGroupCd = x.FirstOrDefault().ParentGroupCd,
                GroupNameEn = x.FirstOrDefault().GroupNameEn,
                GroupNameAr = x.FirstOrDefault().GroupNameAr,
                TotalAmount = x.Sum(a => a.TotalAmount)
            }).ToList();

            foreach (var parent in itemGroups)
            {
                if (!salesItemGroups.Any(x => x.GroupCd == parent.Group_Cd))
                {
                    salesItemGroups.Add(new SalesItemGroup
                    {
                        GroupCd = parent.Group_Cd,
                        ParentGroupCd = parent.M_Group_Cd,
                        GroupNameEn = parent.L_Group_Name,
                        GroupNameAr = parent.A_Group_Name,
                        TotalAmount = 0
                    });
                }
            }

            var ignoreList = new List<int>();

            var salesByItemGroup = new List<SalesByItemGroupResponse>();

            foreach (var group in salesItemGroups)
            {
                var rootId = group.GroupCd;
                var mainId = 0;
                decimal totalAmount = 0;

                if (group.ParentGroupCd != 1)
                {
                    while (rootId != 1)
                    {
                        if (!ignoreList.Any(x => x == rootId))
                        {
                            var result = GetParentDetails(rootId, salesItemGroups);

                            ignoreList.Add(rootId);
                            ignoreList.AddRange(result.GroupCdToIgnore);
                            rootId = result.ParentGroupCd;
                            mainId = result.GroupCd;
                            totalAmount += result.Amount.Value;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    ignoreList.Add(rootId);
                    totalAmount = group.TotalAmount.Value;
                    mainId = rootId;
                }

                var mainItemGroup = itemGroups.FirstOrDefault(x => x.Group_Cd == mainId);

                if (mainItemGroup != null)
                {
                    salesByItemGroup.Add(new SalesByItemGroupResponse
                    {
                        GroupCd = mainItemGroup.Group_Cd,
                        ParentGroupCd = mainItemGroup.M_Group_Cd,
                        GroupNameEn = mainItemGroup.L_Group_Name,
                        GroupNameAr = mainItemGroup.A_Group_Name,
                        Amount = totalAmount
                    });
                }
            }

            var test = salesByItemGroup.Where(x => x.Amount != 0);

            //foreach(var groupItem in salesDetailsByItemGroups)
            //{
            //    var itemGroupByParent = groupItem.GroupBy(x => x.ParentGroupCd);
            //    var amount = itemGroupByParent.Sum(x => x.Sum(a => a.TotalAmount));
            //    var groupCd = itemGroupByParent.FirstOrDefault().Key;

            //    while(groupCd != 1)
            //    {
            //        var itemGroupParent = salesDetailsByItemGroups.Where(x => x.Key == groupCd).GroupBy(a => a.FirstOrDefault().ParentGroupCd);
            //        amount += itemGroupParent.Sum(x => x.Sum(a => a.Sum(b => b.TotalAmount)));
            //        groupC  d = itemGroupParent.FirstOrDefault().Key;
            //    }
            //}

        }

        public SalesByItemGroupResponse GetParentDetails(int groupCd, List<SalesItemGroup> salesItemGroups)
        {
            var selectedGroup = salesItemGroups.FirstOrDefault(x => x.GroupCd == groupCd);
            var resGroupCd = selectedGroup.ParentGroupCd;

            var listofGroupCds = new List<int>();

            var groupItems = salesItemGroups.Where(x => x.ParentGroupCd == resGroupCd);
            var groupCdList = groupItems.Select(x => x.GroupCd);
            var totalAmount = groupItems.Sum(x => x.TotalAmount);

            if (resGroupCd != 1 && groupCdList.Any())
                listofGroupCds.AddRange(groupCdList);

            return new SalesByItemGroupResponse
            {
                Amount = totalAmount,
                GroupCd = groupCd,
                ParentGroupCd = resGroupCd,
                GroupCdToIgnore = listofGroupCds
            };

            //return new SalesByItemGroupResponse
            //{
            //    Amount = 0,
            //    GroupCd = resGroupCd,
            //    GroupCdToIgnore = listofGroupCds
            //};
        }

        public SalesLocationTrendsViewModel GetMonthlyReportLocationWise(string locationsString, int year)
        {
            var fromDate = new DateTime(year, 1, 1, 3, 0, 0);
            var toDate = new DateTime(year, 12, 31, 2, 59, 00);
            var salesReportItems = GetSalesTransaction(fromDate, toDate, locationsString, "").SalesReportItems.Where(x => x.GroupCD != 329);

            var salesLocationMonthlyItems = new List<SalesLocationTrendsItem>();

            if (locationsString != "")
            {
                var locationListIds = new List<int>();
                var locationStringArray = locationsString.Split(',');

                locationListIds.AddRange(from item in locationStringArray
                                         let id = int.Parse(item)
                                         select id);

                var locations = _locationRepository.GetLocations().LocationItems.Where(x => locationListIds.Contains(x.LocationId));

                foreach (var location in locations)
                {
                    var currentStartDate = new DateTime(year, 1, 1, 3, 0, 0);
                    var currentEndDate = new DateTime(year, 1, 31, 2, 59, 00);

                    for (int i = 1; i <= 12; i++)
                    {
                        if (i != 1)
                            currentStartDate = new DateTime(year, i, 1, 3, 0, 0);

                        var lastDateInMonth = DateTime.DaysInMonth(currentStartDate.Year, currentStartDate.Month);
                        currentEndDate = new DateTime(year, currentStartDate.Month, lastDateInMonth, 2, 59, 00);

                        var salesItems = salesReportItems.Where(x => x.InvDateTime >= currentStartDate &&
                                                                     x.InvDateTime <= currentEndDate &&
                                                                     x.LocationId == location.LocationId);

                        var salesLocationMonthlyItem = new SalesLocationTrendsItem
                        {
                            LocationId = location.LocationId,
                            Location = location.Name,
                            MonthNumber = i,
                            Month = currentStartDate.ToString("MMM"),
                            Amount = salesItems.Sum(a => a.NetAmount)
                        };

                        //if (salesLocationMonthlyItem.Amount > 0)
                        salesLocationMonthlyItems.Add(salesLocationMonthlyItem);
                    }
                }
            }

            return new SalesLocationTrendsViewModel
            {
                SalesLocationTrendsItems = salesLocationMonthlyItems
            };

        }

        public SalesLocationTrendsViewModel GetYearlyReportLocationWise(string locationsString)
        {
            var fromDate = new DateTime(2015, 1, 1, 3, 0, 0);
            var toDate = new DateTime(DateTime.Now.Year, 12, 31, 2, 59, 00);
            var salesReportItems = GetSalesTransaction(fromDate, toDate, locationsString, "").SalesReportItems.Where(x => x.GroupCD != 329);

            var salesLocationYearlyItems = new List<SalesLocationTrendsItem>();

            if (locationsString != "")
            {
                var locationListIds = new List<int>();
                var locationStringArray = locationsString.Split(',');

                locationListIds.AddRange(from item in locationStringArray
                                         let id = int.Parse(item)
                                         select id);

                var locations = _locationRepository.GetLocations().LocationItems.Where(x => locationListIds.Contains(x.LocationId));

                foreach (var location in locations)
                {
                    var startYear = new DateTime(2015, 1, 1, 3, 0, 0);

                    for (int i = startYear.Year; i <= DateTime.Now.Year;)
                    {
                        var currentStartDate = new DateTime(i, 1, 1, 3, 0, 0);
                        var currentEndDate = new DateTime(i, 12, 31, 2, 59, 00);

                        var salesItems = salesReportItems.Where(x => x.InvDateTime >= currentStartDate &&
                                                                     x.InvDateTime <= currentEndDate &&
                                                                     x.LocationId == location.LocationId);

                        var salesLocationYearlyItem = new SalesLocationTrendsItem
                        {
                            LocationId = location.LocationId,
                            Location = location.Name,
                            Year = i,
                            Amount = salesItems.Sum(a => a.NetAmount)
                        };

                        salesLocationYearlyItems.Add(salesLocationYearlyItem);

                        startYear = startYear.AddYears(1);
                        i = startYear.Year;
                    }
                }
            }

            return new SalesLocationTrendsViewModel
            {
                SalesLocationTrendsItems = salesLocationYearlyItems
            };
        }

        public SalesLocationTrendsViewModel GetMonthYearReportLocationWise(string locationsString, int month)
        {
            var fromDate = new DateTime(2015, 1, 1, 3, 0, 0);
            var toDate = new DateTime(DateTime.Now.Year, 12, 31, 2, 59, 00);
            var salesReportItems = GetSalesTransaction(fromDate, toDate, locationsString, "").SalesReportItems.Where(x => x.GroupCD != 329);

            var salesLocationYearlyItems = new List<SalesLocationTrendsItem>();

            if (locationsString != "")
            {
                var locationListIds = new List<int>();
                var locationStringArray = locationsString.Split(',');

                locationListIds.AddRange(from item in locationStringArray
                                         let id = int.Parse(item)
                                         select id);

                var locations = _locationRepository.GetLocations().LocationItems.Where(x => locationListIds.Contains(x.LocationId));

                foreach (var location in locations)
                {
                    var startYear = new DateTime(2015, 1, 1, 3, 0, 0);

                    for (int i = startYear.Year; i <= DateTime.Now.Year;)
                    {
                        var currentStartDate = new DateTime(i, month, 1, 3, 0, 0);

                        var lastDateInMonth = DateTime.DaysInMonth(i, month);
                        var currentEndDate = new DateTime(i, month, lastDateInMonth, 2, 59, 00);

                        var salesItems = salesReportItems.Where(x => x.InvDateTime >= currentStartDate &&
                                                                     x.InvDateTime <= currentEndDate &&
                                                                     x.LocationId == location.LocationId);

                        var salesLocationYearlyItem = new SalesLocationTrendsItem
                        {
                            LocationId = location.LocationId,
                            Location = location.Name,
                            Year = i,
                            Amount = salesItems.Sum(a => a.NetAmount)
                        };

                        salesLocationYearlyItems.Add(salesLocationYearlyItem);

                        startYear = startYear.AddYears(1);
                        i = startYear.Year;
                    }
                }
            }

            return new SalesLocationTrendsViewModel
            {
                SalesLocationTrendsItems = salesLocationYearlyItems
            };
        }

        public SalesLocationTrendsViewModel GetWeeklyReportLocationWise(string locationsString, DateTime? fromDate, DateTime? toDate)
        {
            var today = DateTime.Now;
            var lastWeek = today.AddDays(-14);

            fromDate = fromDate ?? new DateTime(lastWeek.Year, lastWeek.Month, lastWeek.Day, 3, 0, 0);
            toDate = toDate ?? new DateTime(today.Year, today.Month, today.Day, 2, 59, 0);

            var salesReportItems = GetSalesTransaction(fromDate, toDate, locationsString, "").SalesReportItems.Where(x => x.GroupCD != 329);

            var salesLocationWeeklyItems = new List<SalesLocationTrendsItem>();

            if (locationsString != "")
            {
                var locationListIds = new List<int>();
                var locationStringArray = locationsString.Split(',');

                locationListIds.AddRange(from item in locationStringArray
                                         let id = int.Parse(item)
                                         select id);

                var locations = _locationRepository.GetLocations().LocationItems.Where(x => locationListIds.Contains(x.LocationId));

                foreach (var location in locations)
                {
                    var weekrange = WeekDays(fromDate.Value, toDate.Value);

                    foreach (var weeks in weekrange.GroupBy(x => x.Week))
                    {
                        DateTime startDate;
                        DateTime endDate;

                        if (weeks.Count() > 1)
                        {
                            startDate = weeks.OrderBy(x => x.StartDate).FirstOrDefault().StartDate;
                            endDate = weeks.OrderByDescending(x => x.EndDate).FirstOrDefault().EndDate;
                        }
                        else
                        {
                            startDate = weeks.FirstOrDefault().StartDate;
                            endDate = weeks.FirstOrDefault().EndDate;
                        }

                        var salesItems = salesReportItems.Where(x => x.InvDateTime >= startDate &&
                                                                     x.InvDateTime <= endDate &&
                                                                     x.LocationId == location.LocationId);

                        var salesLocationWeeklyItem = new SalesLocationTrendsItem
                        {
                            LocationId = location.LocationId,
                            Location = location.Name,
                            Week = weeks.Key,
                            WeekText = $"From : {startDate.ToString("dd/MM/yyyy")} | To : {endDate.ToString("dd/MM/yyyy")}",
                            Amount = salesItems.Sum(a => a.NetAmount),
                            WeekStartDate = startDate,
                            WeekEndDate = endDate
                        };

                        salesLocationWeeklyItems.Add(salesLocationWeeklyItem);

                    }
                }
            }

            return new SalesLocationTrendsViewModel
            {
                SalesLocationTrendsItems = salesLocationWeeklyItems
            };
        }

        private List<WeekRange> WeekDays(DateTime startDate, DateTime endDate)
        {
            DateTime startDateToCheck = startDate;
            DateTime dateToCheck = startDate;
            DateTime dateRangeBegin = dateToCheck;
            DateTime dateRangeEnd = endDate;

            List<WeekRange> weekRangeList = new List<WeekRange>();
            WeekRange weekRange = new WeekRange();


            while ((startDateToCheck.Year <= endDate.Year) && (startDateToCheck.Month <= endDate.Month) && dateToCheck <= endDate)
            {
                int week = 0;

                while (startDateToCheck.Month == dateToCheck.Month && dateToCheck <= endDate)
                {
                    week = week + 1;
                    dateRangeBegin = dateToCheck.AddDays(-(int)dateToCheck.DayOfWeek);
                    dateRangeEnd = dateToCheck.AddDays(6 - (int)dateToCheck.DayOfWeek);

                    if ((dateRangeBegin.Date < dateToCheck) && (dateRangeBegin.Date.Month != dateToCheck.Month))
                    {
                        dateRangeBegin = new DateTime(dateToCheck.Year, dateToCheck.Month, dateToCheck.Day);
                    }

                    if ((dateRangeEnd.Date > dateToCheck) && (dateRangeEnd.Date.Month != dateToCheck.Month))
                    {
                        DateTime dtTo = new DateTime(dateToCheck.Year, dateToCheck.Month, 1);
                        dtTo = dtTo.AddMonths(1);
                        dateRangeEnd = dtTo.AddDays(-(dtTo.Day));
                    }
                    if (dateRangeEnd.Date > endDate)
                    {
                        dateRangeEnd = new DateTime(dateRangeEnd.Year, dateRangeEnd.Month, endDate.Day);
                    }
                    weekRange = new WeekRange
                    {
                        StartDate = dateRangeBegin,
                        EndDate = dateRangeEnd,
                        Range = dateRangeBegin.Date.ToShortDateString() + '-' + dateRangeEnd.Date.ToShortDateString(),
                        Month = dateToCheck.Month,
                        Year = dateToCheck.Year,
                        Week = week
                    };
                    weekRangeList.Add(weekRange);
                    dateToCheck = dateRangeEnd.AddDays(1);
                }
                startDateToCheck = startDateToCheck.AddMonths(1);
            }

            return weekRangeList;
        }

        public SalesLocationTrendsViewModel GetMonthlyReportItemWise(string productString, int year)
        {
            var fromDate = new DateTime(year, 1, 1, 3, 0, 0);
            var toDate = new DateTime(year, 12, 31, 2, 59, 00);

            var salesReportItems = GetSalesDetailTransaction(fromDate, toDate, "", "", productString);

            var salesLocationMonthlyItems = new List<SalesLocationTrendsItem>();

            if (productString != "")
            {
                var itemListIds = new List<long>();
                var itemStringArray = productString.Split(',');

                itemListIds.AddRange(from item in itemStringArray
                                     let id = long.Parse(item)
                                     select id);

                var items = _productRepository.GetAllProducts().Items.Where(x => itemListIds.Contains(x.ProductId));

                foreach (var item in items)
                {
                    var currentStartDate = fromDate;
                    var currentEndDate = toDate;

                    for (int i = 1; i <= 12; i++)
                    {
                        if (i != 1)
                            currentStartDate = new DateTime(year, i, 1, 3, 0, 0);

                        var lastDateInMonth = DateTime.DaysInMonth(currentStartDate.Year, currentStartDate.Month);
                        currentEndDate = new DateTime(year, currentStartDate.Month, lastDateInMonth, 2, 59, 00);

                        var salesItems = salesReportItems.Where(x => x.InvDateTime >= currentStartDate &&
                                                                     x.InvDateTime <= currentEndDate &&
                                                                     x.ProdId == item.ProductId);

                        var salesLocationMonthlyItem = new SalesLocationTrendsItem
                        {
                            ProductId = item.ProductId,
                            ProductName = item.Name,
                            MonthNumber = i,
                            Month = currentStartDate.ToString("MMM"),
                            Amount = salesItems.Sum(a => a.Amount),
                            GroupCd = item.GroupCd
                        };

                        //if (salesLocationMonthlyItem.Amount > 0)
                        salesLocationMonthlyItems.Add(salesLocationMonthlyItem);
                    }
                }
            }

            return new SalesLocationTrendsViewModel
            {
                SalesLocationTrendsItems = salesLocationMonthlyItems
            };
        }

        public SalesLocationTrendsViewModel GetWeeklyReportItemWise(string productString, DateTime? fromDate, DateTime? toDate)
        {
            var today = DateTime.Now;
            var lastWeek = today.AddDays(-14);

            fromDate = fromDate ?? new DateTime(lastWeek.Year, lastWeek.Month, lastWeek.Day, 3, 0, 0);
            toDate = toDate ?? new DateTime(today.Year, today.Month, today.Day, 2, 59, 0);

            var salesReportItems = GetSalesDetailTransaction(fromDate, toDate, "", "", productString);

            var salesLocationWeeklyItems = new List<SalesLocationTrendsItem>();

            if (productString != "")
            {
                var itemListIds = new List<long>();
                var itemStringArray = productString.Split(',');

                itemListIds.AddRange(from item in itemStringArray
                                     let id = long.Parse(item)
                                     select id);

                var items = _productRepository.GetAllProducts().Items.Where(x => itemListIds.Contains(x.ProductId));

                foreach (var item in items)
                {
                    var weekrange = WeekDays(fromDate.Value, toDate.Value);

                    foreach (var weeks in weekrange.GroupBy(x => x.Week))
                    {
                        DateTime startDate;
                        DateTime endDate;

                        if (weeks.Count() > 1)
                        {
                            startDate = weeks.OrderBy(x => x.StartDate).FirstOrDefault().StartDate;
                            endDate = weeks.OrderByDescending(x => x.EndDate).FirstOrDefault().EndDate;
                        }
                        else
                        {
                            startDate = weeks.FirstOrDefault().StartDate;
                            endDate = weeks.FirstOrDefault().EndDate;
                        }

                        var salesItems = salesReportItems.Where(x => x.InvDateTime >= startDate &&
                                                                     x.InvDateTime <= endDate &&
                                                                     x.ProdId == item.ProductId);

                        var salesLocationWeeklyItem = new SalesLocationTrendsItem
                        {
                            ProductId = item.ProductId,
                            ProductName = item.Name,
                            Week = weeks.Key,
                            WeekText = $"From : {startDate.ToString("dd/MM/yyyy")} | To : {endDate.ToString("dd/MM/yyyy")}",
                            Amount = salesItems.Sum(a => a.Amount),
                            WeekStartDate = startDate,
                            WeekEndDate = endDate,
                            GroupCd = item.GroupCd
                        };

                        salesLocationWeeklyItems.Add(salesLocationWeeklyItem);

                    }
                }
            }

            return new SalesLocationTrendsViewModel
            {
                SalesLocationTrendsItems = salesLocationWeeklyItems
            };
        }

        public SalesLocationTrendsViewModel GetYearlyReportItemWise(string productString)
        {
            var fromDate = new DateTime(2015, 1, 1, 3, 0, 0);
            var toDate = new DateTime(DateTime.Now.Year, 12, 31, 2, 59, 00);

            var salesReportItems = GetSalesDetailTransaction(fromDate, toDate, "", "", productString);

            var salesLocationYearlyItems = new List<SalesLocationTrendsItem>();

            if (productString != "")
            {
                var itemListIds = new List<long>();
                var itemStringArray = productString.Split(',');

                itemListIds.AddRange(from item in itemStringArray
                                     let id = long.Parse(item)
                                     select id);

                var items = _productRepository.GetAllProducts().Items.Where(x => itemListIds.Contains(x.ProductId));

                foreach (var item in items)
                {
                    var startYear = new DateTime(2015, 1, 1, 3, 0, 0);

                    for (int i = startYear.Year; i <= DateTime.Now.Year;)
                    {
                        var currentStartDate = new DateTime(i, 1, 1, 3, 0, 0);
                        var currentEndDate = new DateTime(i, 12, 31, 2, 59, 00);

                        var salesItems = salesReportItems.Where(x => x.InvDateTime >= currentStartDate &&
                                                                     x.InvDateTime <= currentEndDate &&
                                                                     x.ProdId == item.ProductId);

                        var salesLocationYearlyItem = new SalesLocationTrendsItem
                        {
                            ProductId = item.ProductId,
                            ProductName = item.Name,
                            GroupCd = item.GroupCd,
                            Year = i,
                            Amount = salesItems.Sum(a => a.Amount)
                        };

                        salesLocationYearlyItems.Add(salesLocationYearlyItem);

                        startYear = startYear.AddYears(1);
                        i = startYear.Year;
                    }
                }
            }

            return new SalesLocationTrendsViewModel
            {
                SalesLocationTrendsItems = salesLocationYearlyItems
            };
        }

        public SalesLocationTrendsViewModel GetMonthYearReportItemWise(string productString, int month)
        {
            var fromDate = new DateTime(2015, 1, 1, 3, 0, 0);
            var toDate = new DateTime(DateTime.Now.Year, 12, 31, 2, 59, 00);
            var salesReportItems = GetSalesDetailTransaction(fromDate, toDate, "", "", productString);

            var salesLocationYearlyItems = new List<SalesLocationTrendsItem>();

            if (productString != "")
            {
                var itemListIds = new List<long>();
                var itemStringArray = productString.Split(',');

                itemListIds.AddRange(from item in itemStringArray
                                     let id = long.Parse(item)
                                     select id);

                var items = _productRepository.GetAllProducts().Items.Where(x => itemListIds.Contains(x.ProductId));

                foreach (var item in items)
                {
                    var startYear = new DateTime(2015, 1, 1, 3, 0, 0);

                    for (int i = startYear.Year; i <= DateTime.Now.Year;)
                    {
                        var currentStartDate = new DateTime(i, month, 1, 3, 0, 0);

                        var lastDateInMonth = DateTime.DaysInMonth(i, month);
                        var currentEndDate = new DateTime(i, month, lastDateInMonth, 2, 59, 00);

                        var salesItems = salesReportItems.Where(x => x.InvDateTime >= currentStartDate &&
                                                                     x.InvDateTime <= currentEndDate &&
                                                                     x.ProdId == item.ProductId);

                        var salesLocationYearlyItem = new SalesLocationTrendsItem
                        {
                            ProductId = item.ProductId,
                            ProductName = item.Name,
                            Year = i,
                            Amount = salesItems.Sum(a => a.Amount)
                        };

                        salesLocationYearlyItems.Add(salesLocationYearlyItem);

                        startYear = startYear.AddYears(1);
                        i = startYear.Year;
                    }
                }
            }

            return new SalesLocationTrendsViewModel
            {
                SalesLocationTrendsItems = salesLocationYearlyItems
            };
        }
    }
}