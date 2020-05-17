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
        private readonly IProductRepository _productRepository;

        public DashboardRepository(
            IProductRepository productRepository,
            ISalesReportRepository salesReportRepository,
            ILocationRepository locationRepository)
        {
            _salesReportRepository = salesReportRepository;
            _locationRepository = locationRepository;
            _productRepository = productRepository;
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
            var salesDetailsOfMonth = _salesReportRepository.GetSalesDetailReport(fromDate, toDate, "", "", "");
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


            var products = _productRepository.GetAllProducts().Items;


            #region Top 5 Product by Amount

            var totalAmount = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(x => x.Amount);

            //var salesDetails = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).GroupBy(x => x.ProdId)
            //    .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.Amount, rn = i + 1 }));

            var salesDetails = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).GroupBy(x => x.ProdId);

            var top5ProductsByAmount = new List<Product>();
            
            foreach (var item in salesDetails.OrderByDescending(a => a.Sum(b => b.Amount)).Take(5))
            {
                var productDetails = new List<ProductDetail>();
                var itemTotalAmount = item.Sum(a => a.Amount);
                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var individualSalesDetails = salesDetailsOfMonth.SalesReportItems.Where(x => x.ProdId == item.Key && x.LocationId != 1 && x.LocationId != 84).GroupBy(a => a.LocationId);

                foreach (var detail in individualSalesDetails.OrderByDescending(a => a.Sum(x => x.Amount)))
                {
                    var detailAmount = detail.Sum(x => x.Amount);
                    var productDetail = new ProductDetail
                    {
                        Amount = detailAmount,
                        Percentage = 100 / totalAmount * detailAmount,
                        Location = locations.LocationItems.FirstOrDefault(x => x.LocationId == detail.Key).Name
                    };

                    productDetails.Add(productDetail);
                }

                var product = new Product
                {
                    ProductId = prod.ProductId,
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    Amount = itemTotalAmount,
                    Percentage = 100 / totalAmount * itemTotalAmount,
                    ProductDetails = productDetails
                };

                top5ProductsByAmount.Add(product);
            }



            var totalAmountHo = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1).Sum(x => x.Amount);

            //var salesDetailsHo = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1).GroupBy(x => x.ProdId)
            //    .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.Amount, rn = i + 1 }));

            var salesDetailsHo = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1).GroupBy(x => x.ProdId);

            var top5HoProductsByAmount = new List<Product>();

            foreach (var item in salesDetailsHo.OrderByDescending(a => a.Sum(b => b.Amount)).Take(5))
            {
                var itemTotalAmount = item.Sum(a => a.Amount);
                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    Amount = itemTotalAmount,
                    Percentage = 100 / totalAmountHo * itemTotalAmount
                };

                top5HoProductsByAmount.Add(product);
            }

            #endregion

            #region Top 5 Product by Kg

            var totalSellKgQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.SellUnitId == 40).Sum(x => x.SellQuantity);

            //var salesDetailsByKg = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.SellUnitId == 40).GroupBy(x => x.SellQuantity)
            //    .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.SellUnit, j.SellQuantity, j.Location, j.LocationId, rn = i + 1 }));

            var salesDetailsByKg = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.SellUnitId == 40).GroupBy(x => x.ProdId);

            var top5ProductsByKg = new List<Product>();

            foreach (var item in salesDetailsByKg.OrderByDescending(a => a.Sum(b => b.SellQuantity)).Take(5))
            {
                var productDetails = new List<ProductDetail>();
                var itemTotalQty = item.Sum(a => a.SellQuantity);
                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var individualSalesDetails = salesDetailsOfMonth.SalesReportItems.Where(x => x.ProdId == item.Key && x.LocationId != 1 && x.LocationId != 84 && x.SellUnitId == 40).GroupBy(a => a.LocationId);

                foreach (var detail in individualSalesDetails.OrderByDescending(a => a.Sum(x => x.SellQuantity)))
                {
                    var detailSellQty = detail.Sum(x => x.SellQuantity);
                    var productDetail = new ProductDetail
                    {
                        SellQuantity = detailSellQty,
                        Percentage = 100 / totalSellKgQty * detailSellQty,
                        Location = locations.LocationItems.FirstOrDefault(x => x.LocationId == detail.Key).Name
                    };

                    productDetails.Add(productDetail);
                }

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    SellQuantity = itemTotalQty,
                    Percentage = 100 / totalSellKgQty * itemTotalQty,
                    ProductDetails = productDetails
                };

                top5ProductsByKg.Add(product);
            }


            var totalHoSellKgQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1 && x.SellUnitId == 40).Sum(x => x.SellQuantity);

            //var salesDetailsHoByKg = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1 && x.SellUnitId == 40).GroupBy(x => x.SellQuantity)
            //    .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.SellUnit, j.SellQuantity, j.Location, j.LocationId, rn = i + 1 }));

            var salesDetailsHoByKg = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1 && x.SellUnitId == 40).GroupBy(x => x.ProdId);

            var top5ProductsHoByKg = new List<Product>();

            foreach (var item in salesDetailsHoByKg.OrderByDescending(a => a.Sum(b => b.SellQuantity)).Take(5))
            {
                var itemTotalQty = item.Sum(a => a.SellQuantity);
                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    SellQuantity = itemTotalQty,
                    Percentage = 100 / totalHoSellKgQty * itemTotalQty
                };

                top5ProductsHoByKg.Add(product);
            }

            #endregion

            #region Top 5 Product by Quantity

            var totalSellQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(x => x.SellQuantity);

            //var salesDetailsByQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).GroupBy(x => x.SellQuantity)
            //    .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.SellUnit, j.SellQuantity, j.Location, j.LocationId, rn = i + 1 }));

            var salesDetailsByQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).GroupBy(x => x.ProdId);

            var top5ProductsByQty = new List<Product>();

            foreach (var item in salesDetailsByQty.OrderByDescending(a => a.Sum(b => b.SellQuantity)).Take(5))
            {
                var productDetails = new List<ProductDetail>();
                var itemTotalQty = item.Sum(a => a.SellQuantity);
                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var individualSalesDetails = salesDetailsOfMonth.SalesReportItems.Where(x => x.ProdId == item.Key && x.LocationId != 1 && x.LocationId != 84).GroupBy(a => a.LocationId);

                foreach (var detail in individualSalesDetails.OrderByDescending(a => a.Sum(x => x.SellQuantity)))
                {
                    var detailSellQty = detail.Sum(x => x.SellQuantity);
                    var productDetail = new ProductDetail
                    {
                        SellQuantity = detailSellQty,
                        Percentage = 100 / totalSellKgQty * detailSellQty,
                        Location = locations.LocationItems.FirstOrDefault(x => x.LocationId == detail.Key).Name
                    };

                    productDetails.Add(productDetail);
                }

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    SellQuantity = itemTotalQty,
                    Percentage = 100 / totalSellQty * itemTotalQty,
                    ProductDetails = productDetails
                };

                top5ProductsByQty.Add(product);
            }


            var totalSellHoQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1).Sum(x => x.SellQuantity);

            //var salesDetailsHoByQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1).GroupBy(x => x.SellQuantity)
            //    .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.SellUnit, j.SellQuantity, j.Location, j.LocationId, rn = i + 1 }));

            var salesDetailsHoByQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId == 1).GroupBy(x => x.ProdId);

            var top5ProductsHoByQty = new List<Product>();

            foreach (var item in salesDetailsHoByQty.OrderByDescending(a => a.Sum(b => b.SellQuantity)).Take(5))
            {
                var itemTotalQty = item.Sum(a => a.SellQuantity);
                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    SellQuantity = itemTotalQty,
                    Percentage = 100 / totalSellHoQty * itemTotalQty
                };

                top5ProductsHoByQty.Add(product);
            }
            #endregion


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
//                 Top5ProdDetailsByAmount = top5ProdDetailsByAmount,

                Top5HoProductsByAmount = top5HoProductsByAmount,
                Top5ProductsByKg = top5ProductsByKg,
                Top5ProductsHoByKg = top5ProductsHoByKg,
                Top5ProductsByQty = top5ProductsByQty,
                Top5ProductsHoByQty = top5ProductsHoByQty
            };

        }
    }
}