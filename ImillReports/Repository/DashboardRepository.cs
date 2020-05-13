using ImillReports.Contracts;
using ImillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ISalesReportRepository _salesReportRepository;
        private readonly ILocationRepository _locationRepository;

        public DashboardRepository(
            ISalesReportRepository salesReportRepository,
            ILocationRepository locationRepository)
        {
            _salesReportRepository = salesReportRepository;
            _locationRepository = locationRepository;
        }
        public SalesOfMonthViewModel GetSalesOfMonth(DateTime? fromDate, DateTime? toDate)
        {
            //1 - Total Sales(Returns Deducted) - Excluding HO and ARD (HO : 1 and ARD : 84)
            //2 - Total Sales(Returns Deducted) - HO
            //3 - Total Sales(Returns Deducted) - HO - Cash Corporate
            //4 - Total Sales(Returns Deducted) - HO - Credit Corporate
            //5 - Total Returns - HO(Cash & Corporate)
            //6 - Total Returns - Only Branches(Excluding HO and ARD)

            var salesOfMonth = _salesReportRepository.GetSalesReport(fromDate, toDate, "", "");
            var salesDetailsOfMonth = _salesReportRepository.GetSalesDetailReport(fromDate, toDate, "", "","");
            var locations = _locationRepository.GetLocations();


            var salesMonthItems = new List<SalesMonthItem>();

            foreach (var location in locations.LocationItems.Where(x => x.LocationId != 1 && x.LocationId != 84))
            {
                var salesMonthItem = new SalesMonthItem
                {
                    Label = location.ShortName,
                    Data = salesOfMonth.SalesReportItems.Where(x => x.LocationId == location.LocationId).Sum(a => a.NetAmount)
                };

                salesMonthItems.Add(salesMonthItem);
            }

            var totalAmount = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(x => x.Amount);

            var salesDetails = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).GroupBy(x => x.Amount)
                .SelectMany(g => g.Select((j,i) => new { j.ProductNameEn, j.ProductNameAr, j.Amount, j.Location, j.LocationId, rn = i + 1 }));

            var top5ProductsByAmount = new List<Product>(); 

            foreach (var item in salesDetails.OrderByDescending(a => a.Amount).Take(5))
            {
                var product = new Product
                {
                    Name = item.ProductNameEn,
                    NameAr = item.ProductNameAr,
                    Amount = item.Amount,
                    Percentage = 100 / totalAmount * item.Amount,
                    Location = item.Location,
                };

                top5ProductsByAmount.Add(product);
            }

            var totalAmountHo = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1).Sum(x => x.Amount);

            var salesDetailsHo = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1).GroupBy(x => x.Amount)
                .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.Amount, j.Location, j.LocationId, rn = i + 1 }));

            var top5HoProductsByAmount = new List<Product>();

            foreach (var item in salesDetailsHo.OrderByDescending(a => a.Amount).Take(5))
            {
                var product = new Product
                {
                    Name = item.ProductNameEn,
                    NameAr = item.ProductNameAr,
                    Amount = item.Amount,
                    Percentage = 100 / totalAmountHo * item.Amount,
                    Location = item.Location,
                };

                top5HoProductsByAmount.Add(product);
            }

            var totalSellKgQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.SellUnitId == 40).Sum(x => x.SellQuantity);

            var salesDetailsByKg = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.SellUnitId == 40).GroupBy(x => x.SellQuantity)
                .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.SellUnit, j.SellQuantity, j.Location, j.LocationId, rn = i + 1 }));

            var top5ProductsByKg = new List<Product>();

            foreach (var item in salesDetailsByKg.OrderByDescending(a => a.SellQuantity).Take(5))
            {
                var product = new Product
                {
                    Name = item.ProductNameEn,
                    NameAr = item.ProductNameAr,
                    SellQuantity = item.SellQuantity,
                    Percentage = 100 / totalSellKgQty * item.SellQuantity,
                    Location = item.Location
                };

                top5ProductsByKg.Add(product);
            }


            var totalHoSellKgQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1 && x.SellUnitId == 40).Sum(x => x.SellQuantity);

            var salesDetailsHoByKg = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1 && x.SellUnitId == 40).GroupBy(x => x.SellQuantity)
                .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.SellUnit, j.SellQuantity, j.Location, j.LocationId, rn = i + 1 }));

            var top5ProductsHoByKg = new List<Product>();

            foreach (var item in salesDetailsHoByKg.OrderByDescending(a => a.SellQuantity).Take(5))
            {
                var product = new Product
                {
                    Name = item.ProductNameEn,
                    NameAr = item.ProductNameAr,
                    SellQuantity = item.SellQuantity,
                    Percentage = 100 / totalHoSellKgQty * item.SellQuantity,
                    Location = item.Location
                };

                top5ProductsHoByKg.Add(product);
            }


            var totalSellQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(x => x.SellQuantity);

            var salesDetailsByQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).GroupBy(x => x.SellQuantity)
                .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.SellUnit, j.SellQuantity, j.Location, j.LocationId, rn = i + 1 }));

            var top5ProductsByQty = new List<Product>();

            foreach (var item in salesDetailsByQty.OrderByDescending(a => a.SellQuantity).Take(5))
            {
                var product = new Product
                {
                    Name = item.ProductNameEn,
                    NameAr = item.ProductNameAr,
                    SellQuantity = item.SellQuantity,
                    Percentage = 100 / totalSellQty * item.SellQuantity,
                    Location = item.Location
                };

                top5ProductsByQty.Add(product);
            }


            var totalSellHoQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1).Sum(x => x.SellQuantity);

            var salesDetailsHoByQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1).GroupBy(x => x.SellQuantity)
                .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.SellUnit, j.SellQuantity, j.Location, j.LocationId, rn = i + 1 }));

            var top5ProductsHoByQty = new List<Product>();

            foreach (var item in salesDetailsHoByQty.OrderByDescending(a => a.SellQuantity).Take(5))
            {
                var product = new Product
                {
                    Name = item.ProductNameEn,
                    NameAr = item.ProductNameAr,
                    SellQuantity = item.SellQuantity,
                    Percentage = 100 / totalSellHoQty * item.SellQuantity,
                    Location = item.Location
                };

                top5ProductsHoByQty.Add(product);
            }



            return new SalesOfMonthViewModel
            {
                SalesMonthItems = salesMonthItems,

                TotalSales = salesOfMonth.SalesReportItems.Where(x => x.LocationId != 84).Sum(a => a.NetAmount),

                TotalBranchSales = salesOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(a => a.NetAmount),
                TotalBranchCash = salesOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(a => a.Cash),
                TotalBranchKnet = salesOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(a => a.Knet),
                TotalBranchCC = salesOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(a => a.CreditCard),
                TotalBranchCarraige = salesOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2025).Sum(a => a.NetAmount),
                TotalBranchOnline = salesOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2026).Sum(a => a.NetAmount),
                SalesReturnBranches = salesOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(a => a.SalesReturn),


                TotalHOSales = salesOfMonth.SalesReportItems.Where(x => x.LocationId == 1).Sum(a => a.NetAmount),
                TotalHOSalesCash = salesOfMonth.SalesReportItems.Where(x => x.VoucherId == 2022 && x.LocationId == 1).Sum(a => a.Cash),
                TotalHOSalesCredit = salesOfMonth.SalesReportItems.Where(x => x.VoucherId == 2021 && x.LocationId == 1).Sum(a => a.NetAmount),
                SalesReturnHO = salesOfMonth.SalesReportItems.Where(x => x.LocationId == 1).Sum(a => a.SalesReturn),

                Top5ProductsByAmount = top5ProductsByAmount,
                Top5HoProductsByAmount = top5HoProductsByAmount,
                Top5ProductsByKg = top5ProductsByKg,
                Top5ProductsHoByKg = top5ProductsHoByKg,
                Top5ProductsByQty = top5ProductsByQty,
                Top5ProductsHoByQty = top5ProductsHoByQty
            };

        }
    }
}