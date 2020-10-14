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

            var dashboardTransaction = _salesReportRepository.GetSalesDashboardTransaction(fromDate, toDate, "", "", "", false);

            //var salesOfMonth = _salesReportRepository.GetSalesReport(fromDate, toDate, "", "");
            //var salesDetailsOfMonth = _salesReportRepository.GetSalesDetailReport(fromDate, toDate, "", "", ""); 

            var salesOfMonth = dashboardTransaction.SRItemsTrans;
            var salesDetailsOfMonth = dashboardTransaction.SRItemsTransDetails;

            var locations = _locationRepository.GetLocations();

            var salesMonthItems = new List<SalesMonthItem>();
            var grandTotalOfMonth = salesOfMonth.Sum(a => a.NetAmount);

            foreach (var location in locations.LocationItems.Where(x => x.LocationId != 1 && x.LocationId != 84))
            {
                var locationTotalAmount = salesOfMonth.Where(x => x.LocationId == location.LocationId).Sum(a => a.NetAmount);
                var salesMonthItem = new SalesMonthItem
                {
                    Label = location.ShortName,
                    Data = locationTotalAmount
                };

                if (locationTotalAmount.Value > 0)
                {
                    salesMonthItems.Add(salesMonthItem);
                }
            }


            var products = _productRepository.GetAllProducts().Items;


            #region Top 10 Product by Amount

            #region Branch

            var totalAmount = salesDetailsOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(x => x.Amount);

            var salesDetails = salesDetailsOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84).GroupBy(x => x.ProdId);

            var top5ProductsByAmount = new List<Product>();

            foreach (var item in salesDetails.OrderByDescending(a => a.Sum(b => b.Amount)).Take(10))
            {
                var productDetails = new List<ProductDetail>();
                var itemTotalAmount = item.Sum(a => a.Amount);
                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key && x.LocationId != 1 && x.LocationId != 84).GroupBy(a => a.LocationId);

                foreach (var detail in individualSalesDetails.OrderByDescending(a => a.Sum(x => x.Amount)))
                {
                    var detailAmount = detail.Sum(x => x.Amount);

                    var totalSellAmountInBranch = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                                                x.LocationId != 84 &&
                                                                                                x.LocationId == detail.Key
                                                                                                //&&
                                                                                                //(x.BaseUnitId == 40 ||
                                                                                                //x.BaseUnitId == 42)
                                                                                                ).Sum(b => b.Amount);

                    var productDetail = new ProductDetail
                    {
                        Amount = detailAmount,
                        Location = locations.LocationItems.FirstOrDefault(x => x.LocationId == detail.Key).Name,
                        Percentage = totalAmount.Value != 0 ? 100 / totalAmount.Value * detailAmount.Value : 0,
                        PercentageAllItem = totalSellAmountInBranch.Value != 0 ? 100 / totalSellAmountInBranch.Value * detailAmount.Value : 0
                    };

                    productDetails.Add(productDetail);
                }

                var product = new Product
                {
                    ProductId = prod.ProductId,
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    Amount = itemTotalAmount.Value,
                    Percentage = totalAmount.Value != 0 ? 100 / totalAmount.Value * itemTotalAmount.Value : 0,
                    ProductDetails = productDetails
                };

                top5ProductsByAmount.Add(product);
            }
            #endregion

            #region HO

            var totalAmountHo = salesDetailsOfMonth.Where(x => x.LocationId == 1 || x.LocationId == 84).Sum(x => x.Amount);

            var salesDetailsHo = salesDetailsOfMonth.Where(x => x.LocationId == 1 || x.LocationId == 84).GroupBy(x => x.ProdId);

            var top5HoProductsByAmount = new List<Product>();

            foreach (var item in salesDetailsHo.OrderByDescending(a => a.Sum(b => b.Amount)).Take(10))
            {
                var productDetails = new List<ProductDetail>();
                var itemTotalAmount = item.Sum(a => a.Amount);
                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key && (x.LocationId == 1 || x.LocationId == 84));

                foreach (var detail in individualSalesDetails.OrderByDescending(x => x.Amount))
                {
                    var detailAmount = detail.Amount;

                    var totalSellAmountInHo = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
                                                                                                x.CustomerId == detail.CustomerId).Sum(b => b.Amount);

                    var productDetail = new ProductDetail
                    {
                        Amount = detailAmount,
                        Location = locations.LocationItems.FirstOrDefault(x => x.LocationId == detail.LocationId).Name,
                        CustomerAr = detail.CustomerNameAr,
                        Percentage = totalAmountHo.Value != 0 ? 100 / totalAmountHo.Value * detailAmount.Value : 0,
                        // PercentageAllItem = totalSellAmountInHo != 0 ? 100 / totalSellAmountInHo * detailAmount : 0
                        PercentageAllItem = totalSellAmountInHo.Value != 0 ? 100 / totalSellAmountInHo.Value * detailAmount.Value : 0
                    };

                    productDetails.Add(productDetail);
                }

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    Amount = itemTotalAmount,
                    Percentage = totalAmountHo.Value != 0 ? 100 / totalAmountHo.Value * itemTotalAmount.Value : 0,
                    ProductDetails = productDetails
                };

                top5HoProductsByAmount.Add(product);

            }
            #endregion

            #endregion

            #region Top 10 Product by Kg

            #region Branch

            var sellQtyKg = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                         x.LocationId != 84 &&
                                                                         (x.BaseUnitId == 40)).Sum(x => x.BaseQuantity * x.SellQuantity);

            var sellQtyGm = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                                 x.LocationId != 84 &&
                                                                                 (x.BaseUnitId == 42)).Sum(x => (x.BaseQuantity * x.SellQuantity) / 1000);

            var totalSellQtyKg = sellQtyKg + sellQtyGm;

            //var salesDetailsByKg = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.SellUnitId == 40).GroupBy(x => x.SellQuantity)
            //    .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.SellUnit, j.SellQuantity, j.Location, j.LocationId, rn = i + 1 }));

            var salesDetailsByKg = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                                   x.LocationId != 84 &&
                                                                                   (x.BaseUnitId == 40 ||
                                                                                    x.BaseUnitId == 42)).GroupBy(x => x.ProdId);

            var top5ProductsByKg = new List<Product>();

            foreach (var item in salesDetailsByKg.OrderByDescending(a => a.Sum(b => b.BaseUnitId == 40
                                                                                    ? b.BaseQuantity * b.SellQuantity
                                                                                    : (b.BaseQuantity * b.SellQuantity) / 1000)).Take(10))
            {
                var productDetails = new List<ProductDetail>();
                var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key &&
                                                                                             x.LocationId != 1 &&
                                                                                             x.LocationId != 84 &&
                                                                                             (x.BaseUnitId == 40 ||
                                                                                              x.BaseUnitId == 42)).GroupBy(a => a.LocationId);


                foreach (var detail in individualSalesDetails.OrderByDescending(a => a.Sum(x => x.BaseUnitId == 40
                                                                                                ? x.BaseQuantity * x.SellQuantity
                                                                                                : (x.BaseQuantity * x.SellQuantity) / 1000)))
                {
                    var detailSellQty = detail.Sum(x => x.BaseUnitId == 40
                                                        ? x.BaseQuantity * x.SellQuantity
                                                        : (x.BaseQuantity * x.SellQuantity) / 1000);

                    var totalSellQtyKgInBranch = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                                                 x.LocationId != 84 &&
                                                                                                 x.LocationId == detail.Key &&
                                                                                                 (x.BaseUnitId == 40 ||
                                                                                                 x.BaseUnitId == 42)).Sum(b => b.BaseUnitId == 40
                                                                                                                            ? b.BaseQuantity * b.SellQuantity
                                                                                                                            : (b.BaseQuantity * b.SellQuantity) / 1000);

                    var productDetail = new ProductDetail
                    {
                        Location = locations.LocationItems.FirstOrDefault(x => x.LocationId == detail.Key).Name,
                        SellQuantity = detailSellQty.Value,
                        Percentage = totalSellQtyKg.Value != 0 ? 100 / totalSellQtyKg.Value * detailSellQty.Value : 0,
                        PercentageAllItem = totalSellQtyKgInBranch.Value != 0 ? 100 / totalSellQtyKgInBranch.Value * detailSellQty.Value : 0
                    };

                    productDetails.Add(productDetail);
                }


                var itemTotalQty = item.Sum(x => x.BaseUnitId == 40
                                                        ? x.BaseQuantity * x.SellQuantity
                                                        : (x.BaseQuantity * x.SellQuantity) / 1000);

                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    SellQuantity = itemTotalQty.Value,
                    Percentage = totalSellQtyKg.Value != 0 ? 100 / totalSellQtyKg.Value * itemTotalQty.Value : 0,
                    ProductDetails = productDetails
                };

                top5ProductsByKg.Add(product);
            }
            #endregion

            #region HO

            var hoSellQtyKg = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
                                                                                 (x.BaseUnitId == 40)).Sum(x => x.BaseQuantity * x.SellQuantity);

            var hoSellQtyGm = salesDetailsOfMonth.Where(x => x.LocationId == 1 &&
                                                                              (x.BaseUnitId == 42)).Sum(x => (x.BaseQuantity * x.SellQuantity) / 1000);

            var totalHoSellKgQty = hoSellQtyKg + hoSellQtyGm;

            var salesDetailsHoByKg = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
                                                                                     (x.BaseUnitId == 40 ||
                                                                                     x.BaseUnitId == 42)).GroupBy(x => x.ProdId);

            var top5ProductsHoByKg = new List<Product>();

            foreach (var item in salesDetailsHoByKg.OrderByDescending(a => a.Sum(x => x.BaseUnitId == 40
                                                                                    ? x.BaseQuantity * x.SellQuantity
                                                                                    : x.BaseQuantity * x.SellQuantity / 1000)).Take(10))
            {

                var productDetails = new List<ProductDetail>();
                var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key &&
                                                                                             (x.LocationId == 1 || x.LocationId == 84) &&
                                                                                             (x.BaseUnitId == 40 ||
                                                                                              x.BaseUnitId == 42));

                foreach (var detail in individualSalesDetails.OrderByDescending(a => a.BaseUnitId == 40
                                                                                        ? a.BaseQuantity * a.SellQuantity
                                                                                        : (a.BaseQuantity * a.SellQuantity) / 1000))
                {
                    var detailSellHoQty = detail.BaseUnitId == 40
                                                        ? detail.BaseQuantity * detail.SellQuantity
                                                        : (detail.BaseQuantity * detail.SellQuantity) / 1000;

                    var totalSellQtyKgInHo = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
                                                                                              x.CustomerId == detail.CustomerId &&
                                                                                             (x.BaseUnitId == 40 || x.BaseUnitId == 42))
                                                                                             .Sum(b => b.BaseUnitId == 40
                                                                                                ? b.BaseQuantity * b.SellQuantity
                                                                                                : (b.BaseQuantity * b.SellQuantity) / 1000);

                    var productDetail = new ProductDetail
                    {
                        CustomerAr = detail.CustomerNameAr,
                        SellQuantity = detailSellHoQty.Value,
                        Percentage = totalHoSellKgQty.Value != 0 ? 100 / totalHoSellKgQty.Value * detailSellHoQty.Value : 0,
                        PercentageAllItem = totalSellQtyKgInHo.Value != 0 ? 100 / totalSellQtyKgInHo.Value * detailSellHoQty.Value : 0
                    };

                    productDetails.Add(productDetail);
                }

                var itemTotalQty = item.Sum(x => x.BaseUnitId == 40
                                               ? x.BaseQuantity * x.SellQuantity
                                               : (x.BaseQuantity * x.SellQuantity) / 1000);

                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    SellQuantity = itemTotalQty.Value,
                    Percentage = totalHoSellKgQty.Value != 0 ? 100 / totalHoSellKgQty.Value * itemTotalQty.Value : 0,
                    ProductDetails = productDetails
                };

                top5ProductsHoByKg.Add(product);

            }

            #endregion

            #endregion

            #region Top 10 Product by Quantity

            #region Branch

            var totalSellQty = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                       x.LocationId != 84 &&
                                                                       x.BaseUnitId != 40 &&
                                                                       x.BaseUnitId != 42 &&
                                                                       x.ProdId != 19595).Sum(x => x.BaseQuantity * x.SellQuantity);

            //var salesDetailsByQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).GroupBy(x => x.SellQuantity)
            //    .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.SellUnit, j.SellQuantity, j.Location, j.LocationId, rn = i + 1 }));

            var salesDetailsByQty = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                                    x.LocationId != 84 &&
                                                                                    x.BaseUnitId != 40 &&
                                                                                    x.BaseUnitId != 42 &&
                                                                                    x.ProdId != 19595).GroupBy(x => x.ProdId);

            var top5ProductsByQty = new List<Product>();

            foreach (var item in salesDetailsByQty.OrderByDescending(a => a.Sum(b => b.BaseQuantity * b.SellQuantity)).Take(10))
            {
                var productDetails = new List<ProductDetail>();

                var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key &&
                                                                                             x.ProdId != 19595 &&
                                                                                             x.LocationId != 1 &&
                                                                                             x.LocationId != 84 &&
                                                                                             x.BaseUnitId != 40 &&
                                                                                             x.BaseUnitId != 42).GroupBy(a => a.LocationId);

                foreach (var detail in individualSalesDetails.OrderByDescending(a => a.Sum(x => x.BaseQuantity * x.SellQuantity)))
                {
                    var detailSellQty = detail.Sum(x => x.BaseQuantity * x.SellQuantity);

                    var totalSellQtyInBranch = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                                               x.LocationId != 84 &&
                                                                                               x.ProdId != 19595 &&
                                                                                               x.LocationId == detail.Key &&
                                                                                               (x.BaseUnitId != 40 ||
                                                                                               x.BaseUnitId != 42)).Sum(b => b.BaseQuantity * b.SellQuantity);
                    var productDetail = new ProductDetail
                    {
                        Location = locations.LocationItems.FirstOrDefault(x => x.LocationId == detail.Key).Name,
                        SellQuantity = detailSellQty.Value,
                        Percentage = totalSellQty.Value != 0 ? 100 / totalSellQty.Value * detailSellQty.Value : 0,
                        PercentageAllItem = totalSellQtyInBranch.Value != 0 ? 100 / totalSellQtyInBranch.Value * detailSellQty.Value : 0,
                    };

                    productDetails.Add(productDetail);
                }

                var itemTotalQty = item.Sum(a => a.SellQuantity);
                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    SellQuantity = itemTotalQty.Value,
                    Percentage = totalSellQty.Value != 0 ? 100 / totalSellQty.Value * itemTotalQty.Value : 0,
                    ProductDetails = productDetails
                };

                top5ProductsByQty.Add(product);
            }
            #endregion

            #region HO

            var totalSellHoQty = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
                                                                                     x.BaseUnitId != 40 &&
                                                                                     x.BaseUnitId != 42 &&
                                                                                     x.ProdId != 19595).Sum(x => x.SellQuantity);

            var salesDetailsHoByQty = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
                                                                                      x.BaseUnitId != 40 &&
                                                                                      x.BaseUnitId != 42 &&
                                                                                      x.ProdId != 19595).GroupBy(x => x.ProdId);

            var top5ProductsHoByQty = new List<Product>();

            foreach (var item in salesDetailsHoByQty.OrderByDescending(a => a.Sum(b => b.SellQuantity)).Take(10))
            {
                var productDetails = new List<ProductDetail>();

                var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key &&
                                                                                             x.ProdId != 19595 &&
                                                                                             (x.LocationId == 1) &&
                                                                                             x.BaseUnitId != 40 &&
                                                                                             x.BaseUnitId != 42);

                foreach (var detail in individualSalesDetails.OrderByDescending(x => x.BaseQuantity * x.SellQuantity))
                {
                    var detailSellHoQty = detail.BaseQuantity * detail.SellQuantity;

                    var totalSellQtyInHo = salesDetailsOfMonth.Where(x => x.LocationId == 1 &&
                                                                                           x.ProdId != 19595 &&
                                                                                            x.CustomerId == detail.CustomerId &&
                                                                                           (x.BaseUnitId != 40 || x.BaseUnitId != 42))
                                                                                           .Sum(b => b.BaseQuantity * b.SellQuantity);
                    var productDetail = new ProductDetail
                    {
                        CustomerAr = detail.CustomerNameAr,
                        SellQuantity = detailSellHoQty.Value,
                        Percentage = totalSellHoQty.Value != 0 ? 100 / totalSellHoQty.Value * detailSellHoQty.Value : 0,
                        PercentageAllItem = totalSellQtyInHo.Value != 0 ? 100 / totalSellQtyInHo.Value * detailSellHoQty.Value : 0
                    };

                    productDetails.Add(productDetail);
                }

                var itemTotalQty = item.Sum(a => a.SellQuantity);
                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    SellQuantity = itemTotalQty.Value,
                    Percentage = totalSellHoQty.Value != 0 ? 100 / totalSellHoQty.Value * itemTotalQty.Value : 0,
                    ProductDetails = productDetails
                };

                top5ProductsHoByQty.Add(product);
            }

            #endregion

            #endregion

            var branchSalesTransDetail = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84);
            var branchSTDCount = branchSalesTransDetail.Count();

            var talabatTransCount = branchSalesTransDetail.Count(x => x.VoucherId == 2025);
            var talabatTransSrCount = branchSalesTransDetail.Count(x => x.VoucherId == 2035);
            var talabatTransNetCount = talabatTransCount - talabatTransSrCount;

            var onlineTransCount = branchSalesTransDetail.Count(x => x.VoucherId == 2026);
            var onlineTransSrCount = branchSalesTransDetail.Count(x => x.VoucherId == 2036);
            var onlineTransNetCount = onlineTransCount - onlineTransSrCount;

            var branchTalabatSales = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2025);
            var branchTalabatSalesReturn = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2035);

            var branchTalabatSalesReturnCash = branchTalabatSalesReturn.Sum(x => x.Cash) != null ? (decimal)branchTalabatSalesReturn.Sum(x => x.Cash) : 0;
            var branchTalabatSalesCash = branchTalabatSales.Sum(x => x.Cash) - Math.Abs(branchTalabatSalesReturnCash);

            var branchTalabatSalesReturnKnet = branchTalabatSalesReturn.Sum(x => x.Knet) != null ? (decimal)branchTalabatSalesReturn.Sum(x => x.Knet) : 0;
            var branchTalabatSalesKnet = branchTalabatSales.Sum(x => x.Knet) - Math.Abs(branchTalabatSalesReturnKnet);

            var branchTalabatSalesReturnCc = branchTalabatSalesReturn.Sum(x => x.CreditCard) != null ? (decimal)branchTalabatSalesReturn.Sum(x => x.CreditCard) : 0;
            var branchTalabatSalesCc = branchTalabatSales.Sum(x => x.CreditCard) - Math.Abs(branchTalabatSalesReturnCc);

            var brTalabatSrAmount = branchTalabatSalesReturn.Sum(a => a.NetAmount) ?? 0;
            var brTalabatSalesAmount = branchTalabatSales.Sum(a => a.NetAmount) ?? 0;



            var branchOnlineSales = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2026);
            var branchOnlineSalesReturn = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2036);

            var branchOnlineSalesReturnCash = branchOnlineSalesReturn.Sum(x => x.Cash) != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.Cash) : 0;
            var branchOnlineSalesCash = branchOnlineSales.Sum(x => x.Cash) - Math.Abs(branchOnlineSalesReturnCash);

            var branchOnlineSalesReturnKnet = branchOnlineSalesReturn.Sum(x => x.Knet) != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.Knet) : 0;
            var branchOnlineSalesKnet = branchOnlineSales.Sum(x => x.Knet) - Math.Abs(branchOnlineSalesReturnKnet);

            var branchOnlineSalesReturnCc = branchOnlineSalesReturn.Sum(x => x.CreditCard) != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.CreditCard) : 0;
            var branchOnlineSalesCc = branchOnlineSales.Sum(x => x.CreditCard) - Math.Abs(branchOnlineSalesReturnCc);

            var brOnlineSrNet = branchOnlineSalesReturn.Sum(x => x.NetAmount) ?? 0;
            var branchSalesReturn = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(a => a.SalesReturn) + Math.Abs(brOnlineSrNet) + Math.Abs(brTalabatSrAmount);
            //var brSalesReturnCash = salesOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId == 2023 || x.VoucherId == 202)).Sum(a => a.Cash) ?? 0;
            //var brSalesReturnKnet = salesOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId == 2023 || x.VoucherId == 202)).Sum(a => a.Knet) ?? 0;
            //var brSalesReturnCc = salesOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId == 2023 || x.VoucherId == 202)).Sum(a => a.CreditCard) ?? 0;

            return new SalesOfMonthViewModel
            {
                SalesMonthItems = salesMonthItems,

                TotalSales = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(a => a.NetAmount),
                TotalBranchSales = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId != 2026 || x.VoucherId != 2036)).Sum(a => a.NetAmount),

                TotalBranchCash = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId != 2023 && x.VoucherId != 202)).Sum(a => a.Cash) - branchOnlineSalesCash - branchTalabatSalesCash,
                TotalBranchKnet = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId != 2023 && x.VoucherId != 202)).Sum(a => a.Knet) - branchOnlineSalesKnet - branchTalabatSalesKnet,
                TotalBranchCC = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId != 2023 && x.VoucherId != 202)).Sum(a => a.CreditCard) - branchOnlineSalesCc - branchTalabatSalesCc,
                TotalBranchCarraige = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2025).Sum(a => a.NetAmount),
                TotalBranchCount = branchSTDCount - (talabatTransCount + talabatTransSrCount + onlineTransCount + onlineTransSrCount),

                //TotalBranchOnline = branchOnlineSales.Sum(a => a.NetAmount) - Math.Abs(branchOnlineSalesReturn != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.NetAmount) : 0),
                //TotalBranchOnlineCash = branchOnlineSales.Sum(a => a.Cash) - Math.Abs(branchOnlineSalesReturn != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.Cash) : 0),
                //TotalBranchOnlineKnet = branchOnlineSales.Sum(a => a.Knet) - Math.Abs(branchOnlineSalesReturn != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.Knet) : 0),
                //TotalBranchOnlineCc = branchOnlineSales.Sum(a => a.CreditCard) - Math.Abs(branchOnlineSalesReturn != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.CreditCard) : 0),

                TotalBranchOnline = branchOnlineSales.Sum(a => a.NetAmount),
                TotalBranchOnlineCash = branchOnlineSales.Sum(a => a.Cash),
                TotalBranchOnlineKnet = branchOnlineSales.Sum(a => a.Knet),
                TotalBranchOnlineCc = branchOnlineSales.Sum(a => a.CreditCard),
                TotalBranchOnlineReturn = branchOnlineSalesReturn.Sum(x => x.NetAmount),
                TotalOnlineTransCount = onlineTransNetCount,

                TotalTalabat = brTalabatSalesAmount - Math.Abs(brTalabatSrAmount),
                TalabatTransCount = talabatTransNetCount,

                SalesReturnBranches = branchSalesReturn,

                TotalHOSales = salesOfMonth.Where(x => x.LocationId == 1).Sum(a => a.NetAmount),
                TotalHOSalesCash = salesOfMonth.Where(x => (x.VoucherId == 2022 || x.VoucherId == 2026) && x.LocationId == 1).Sum(a => a.Cash),
                TotalHOSalesCredit = salesOfMonth.Where(x => x.VoucherId == 2021 && x.LocationId == 1).Sum(a => a.NetAmount),
                SalesReturnHO = salesOfMonth.Where(x => x.LocationId == 1).Sum(a => a.SalesReturn),
                TotalHOCount = salesDetailsOfMonth.Count(x => x.LocationId == 1),

                Top5ProductsByAmount = top5ProductsByAmount,

                Top5HoProductsByAmount = top5HoProductsByAmount,
                Top5ProductsByKg = top5ProductsByKg,
                Top5ProductsHoByKg = top5ProductsHoByKg,
                Top5ProductsByQty = top5ProductsByQty,
                Top5ProductsHoByQty = top5ProductsHoByQty
            };

        }


        //public string GetSalesOfMonthTest(DateTime? fromDate, DateTime? toDate)
        //{

        //    try
        //    {
        //        var dashboardTransaction = _salesReportRepository.GetSalesDashboardTransTest(fromDate, toDate, "", "", "");
        //        return "Dashboard transactions : " + dashboardTransaction;
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //    }

        //    //var salesOfMonth = dashboardTransaction.SRItemsTrans;
        //    //var salesDetailsOfMonth = dashboardTransaction.SRItemsTransDetails;

        //    //var locations = _locationRepository.GetLocations();

        //    //var salesMonthItems = new List<SalesMonthItem>();
        //    //var grandTotalOfMonth = salesOfMonth.Sum(a => a.NetAmount);

        //    //foreach (var location in locations.LocationItems.Where(x => x.LocationId != 1 && x.LocationId != 84))
        //    //{
        //    //    var locationTotalAmount = salesOfMonth.Where(x => x.LocationId == location.LocationId).Sum(a => a.NetAmount);
        //    //    var salesMonthItem = new SalesMonthItem
        //    //    {
        //    //        Label = location.ShortName,
        //    //        Data = locationTotalAmount
        //    //    };

        //    //    if (locationTotalAmount.Value > 0)
        //    //    {
        //    //        salesMonthItems.Add(salesMonthItem);
        //    //    }
        //    //}


        //    //var products = _productRepository.GetAllProducts().Items;


        //    //#region Top 10 Product by Amount

        //    //#region Branch

        //    //var totalAmount = salesDetailsOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(x => x.Amount);

        //    //var salesDetails = salesDetailsOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84).GroupBy(x => x.ProdId);

        //    //var top5ProductsByAmount = new List<Product>();

        //    //foreach (var item in salesDetails.OrderByDescending(a => a.Sum(b => b.Amount)).Take(10))
        //    //{
        //    //    var productDetails = new List<ProductDetail>();
        //    //    var itemTotalAmount = item.Sum(a => a.Amount);
        //    //    var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

        //    //    var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key && x.LocationId != 1 && x.LocationId != 84).GroupBy(a => a.LocationId);

        //    //    foreach (var detail in individualSalesDetails.OrderByDescending(a => a.Sum(x => x.Amount)))
        //    //    {
        //    //        var detailAmount = detail.Sum(x => x.Amount);

        //    //        var totalSellAmountInBranch = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
        //    //                                                                                    x.LocationId != 84 &&
        //    //                                                                                    x.LocationId == detail.Key
        //    //                                                                                    //&&
        //    //                                                                                    //(x.BaseUnitId == 40 ||
        //    //                                                                                    //x.BaseUnitId == 42)
        //    //                                                                                    ).Sum(b => b.Amount);

        //    //        var productDetail = new ProductDetail
        //    //        {
        //    //            Amount = detailAmount,
        //    //            Location = locations.LocationItems.FirstOrDefault(x => x.LocationId == detail.Key).Name,
        //    //            Percentage = totalAmount.Value != 0 ? 100 / totalAmount.Value * detailAmount.Value : 0,
        //    //            PercentageAllItem = totalSellAmountInBranch.Value != 0 ? 100 / totalSellAmountInBranch.Value * detailAmount.Value : 0
        //    //        };

        //    //        productDetails.Add(productDetail);
        //    //    }

        //    //    var product = new Product
        //    //    {
        //    //        ProductId = prod.ProductId,
        //    //        Name = prod.Name,
        //    //        NameAr = prod.NameAr,
        //    //        Amount = itemTotalAmount.Value,
        //    //        Percentage = totalAmount.Value != 0 ? 100 / totalAmount.Value * itemTotalAmount.Value : 0,
        //    //        ProductDetails = productDetails
        //    //    };

        //    //    top5ProductsByAmount.Add(product);
        //    //}
        //    //#endregion

        //    //#region HO

        //    //var totalAmountHo = salesDetailsOfMonth.Where(x => x.LocationId == 1 || x.LocationId == 84).Sum(x => x.Amount);

        //    //var salesDetailsHo = salesDetailsOfMonth.Where(x => x.LocationId == 1 || x.LocationId == 84).GroupBy(x => x.ProdId);

        //    //var top5HoProductsByAmount = new List<Product>();

        //    //foreach (var item in salesDetailsHo.OrderByDescending(a => a.Sum(b => b.Amount)).Take(10))
        //    //{
        //    //    var productDetails = new List<ProductDetail>();
        //    //    var itemTotalAmount = item.Sum(a => a.Amount);
        //    //    var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

        //    //    var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key && (x.LocationId == 1 || x.LocationId == 84));

        //    //    foreach (var detail in individualSalesDetails.OrderByDescending(x => x.Amount))
        //    //    {
        //    //        var detailAmount = detail.Amount;

        //    //        var totalSellAmountInHo = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
        //    //                                                                                    x.CustomerId == detail.CustomerId).Sum(b => b.Amount);

        //    //        var productDetail = new ProductDetail
        //    //        {
        //    //            Amount = detailAmount,
        //    //            Location = locations.LocationItems.FirstOrDefault(x => x.LocationId == detail.LocationId).Name,
        //    //            CustomerAr = detail.CustomerNameAr,
        //    //            Percentage = totalAmountHo.Value != 0 ? 100 / totalAmountHo.Value * detailAmount.Value : 0,
        //    //            // PercentageAllItem = totalSellAmountInHo != 0 ? 100 / totalSellAmountInHo * detailAmount : 0
        //    //            PercentageAllItem = totalSellAmountInHo.Value != 0 ? 100 / totalSellAmountInHo.Value * detailAmount.Value : 0
        //    //        };

        //    //        productDetails.Add(productDetail);
        //    //    }

        //    //    var product = new Product
        //    //    {
        //    //        Name = prod.Name,
        //    //        NameAr = prod.NameAr,
        //    //        Amount = itemTotalAmount,
        //    //        Percentage = totalAmountHo.Value != 0 ? 100 / totalAmountHo.Value * itemTotalAmount.Value : 0,
        //    //        ProductDetails = productDetails
        //    //    };

        //    //    top5HoProductsByAmount.Add(product);

        //    //}
        //    //#endregion

        //    //#endregion

        //    //#region Top 10 Product by Kg

        //    //#region Branch

        //    //var sellQtyKg = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
        //    //                                                             x.LocationId != 84 &&
        //    //                                                             (x.BaseUnitId == 40)).Sum(x => x.BaseQuantity * x.SellQuantity);

        //    //var sellQtyGm = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
        //    //                                                                     x.LocationId != 84 &&
        //    //                                                                     (x.BaseUnitId == 42)).Sum(x => (x.BaseQuantity * x.SellQuantity) / 1000);

        //    //var totalSellQtyKg = sellQtyKg + sellQtyGm;

        //    ////var salesDetailsByKg = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.SellUnitId == 40).GroupBy(x => x.SellQuantity)
        //    ////    .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.SellUnit, j.SellQuantity, j.Location, j.LocationId, rn = i + 1 }));

        //    //var salesDetailsByKg = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
        //    //                                                                       x.LocationId != 84 &&
        //    //                                                                       (x.BaseUnitId == 40 ||
        //    //                                                                        x.BaseUnitId == 42)).GroupBy(x => x.ProdId);

        //    //var top5ProductsByKg = new List<Product>();

        //    //foreach (var item in salesDetailsByKg.OrderByDescending(a => a.Sum(b => b.BaseUnitId == 40
        //    //                                                                        ? b.BaseQuantity * b.SellQuantity
        //    //                                                                        : (b.BaseQuantity * b.SellQuantity) / 1000)).Take(10))
        //    //{
        //    //    var productDetails = new List<ProductDetail>();
        //    //    var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key &&
        //    //                                                                                 x.LocationId != 1 &&
        //    //                                                                                 x.LocationId != 84 &&
        //    //                                                                                 (x.BaseUnitId == 40 ||
        //    //                                                                                  x.BaseUnitId == 42)).GroupBy(a => a.LocationId);


        //    //    foreach (var detail in individualSalesDetails.OrderByDescending(a => a.Sum(x => x.BaseUnitId == 40
        //    //                                                                                    ? x.BaseQuantity * x.SellQuantity
        //    //                                                                                    : (x.BaseQuantity * x.SellQuantity) / 1000)))
        //    //    {
        //    //        var detailSellQty = detail.Sum(x => x.BaseUnitId == 40
        //    //                                            ? x.BaseQuantity * x.SellQuantity
        //    //                                            : (x.BaseQuantity * x.SellQuantity) / 1000);

        //    //        var totalSellQtyKgInBranch = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
        //    //                                                                                     x.LocationId != 84 &&
        //    //                                                                                     x.LocationId == detail.Key &&
        //    //                                                                                     (x.BaseUnitId == 40 ||
        //    //                                                                                     x.BaseUnitId == 42)).Sum(b => b.BaseUnitId == 40
        //    //                                                                                                                ? b.BaseQuantity * b.SellQuantity
        //    //                                                                                                                : (b.BaseQuantity * b.SellQuantity) / 1000);

        //    //        var productDetail = new ProductDetail
        //    //        {
        //    //            Location = locations.LocationItems.FirstOrDefault(x => x.LocationId == detail.Key).Name,
        //    //            SellQuantity = detailSellQty.Value,
        //    //            Percentage = totalSellQtyKg.Value != 0 ? 100 / totalSellQtyKg.Value * detailSellQty.Value : 0,
        //    //            PercentageAllItem = totalSellQtyKgInBranch.Value != 0 ? 100 / totalSellQtyKgInBranch.Value * detailSellQty.Value : 0
        //    //        };

        //    //        productDetails.Add(productDetail);
        //    //    }


        //    //    var itemTotalQty = item.Sum(x => x.BaseUnitId == 40
        //    //                                            ? x.BaseQuantity * x.SellQuantity
        //    //                                            : (x.BaseQuantity * x.SellQuantity) / 1000);

        //    //    var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

        //    //    var product = new Product
        //    //    {
        //    //        Name = prod.Name,
        //    //        NameAr = prod.NameAr,
        //    //        SellQuantity = itemTotalQty.Value,
        //    //        Percentage = totalSellQtyKg.Value != 0 ? 100 / totalSellQtyKg.Value * itemTotalQty.Value : 0,
        //    //        ProductDetails = productDetails
        //    //    };

        //    //    top5ProductsByKg.Add(product);
        //    //}
        //    //#endregion

        //    //#region HO

        //    //var hoSellQtyKg = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
        //    //                                                                     (x.BaseUnitId == 40)).Sum(x => x.BaseQuantity * x.SellQuantity);

        //    //var hoSellQtyGm = salesDetailsOfMonth.Where(x => x.LocationId == 1 &&
        //    //                                                                  (x.BaseUnitId == 42)).Sum(x => (x.BaseQuantity * x.SellQuantity) / 1000);

        //    //var totalHoSellKgQty = hoSellQtyKg + hoSellQtyGm;

        //    //var salesDetailsHoByKg = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
        //    //                                                                         (x.BaseUnitId == 40 ||
        //    //                                                                         x.BaseUnitId == 42)).GroupBy(x => x.ProdId);

        //    //var top5ProductsHoByKg = new List<Product>();

        //    //foreach (var item in salesDetailsHoByKg.OrderByDescending(a => a.Sum(x => x.BaseUnitId == 40
        //    //                                                                        ? x.BaseQuantity * x.SellQuantity
        //    //                                                                        : x.BaseQuantity * x.SellQuantity / 1000)).Take(10))
        //    //{

        //    //    var productDetails = new List<ProductDetail>();
        //    //    var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key &&
        //    //                                                                                 (x.LocationId == 1 || x.LocationId == 84) &&
        //    //                                                                                 (x.BaseUnitId == 40 ||
        //    //                                                                                  x.BaseUnitId == 42));

        //    //    foreach (var detail in individualSalesDetails.OrderByDescending(a => a.BaseUnitId == 40
        //    //                                                                            ? a.BaseQuantity * a.SellQuantity
        //    //                                                                            : (a.BaseQuantity * a.SellQuantity) / 1000))
        //    //    {
        //    //        var detailSellHoQty = detail.BaseUnitId == 40
        //    //                                            ? detail.BaseQuantity * detail.SellQuantity
        //    //                                            : (detail.BaseQuantity * detail.SellQuantity) / 1000;

        //    //        var totalSellQtyKgInHo = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
        //    //                                                                                  x.CustomerId == detail.CustomerId &&
        //    //                                                                                 (x.BaseUnitId == 40 || x.BaseUnitId == 42))
        //    //                                                                                 .Sum(b => b.BaseUnitId == 40
        //    //                                                                                    ? b.BaseQuantity * b.SellQuantity
        //    //                                                                                    : (b.BaseQuantity * b.SellQuantity) / 1000);

        //    //        var productDetail = new ProductDetail
        //    //        {
        //    //            CustomerAr = detail.CustomerNameAr,
        //    //            SellQuantity = detailSellHoQty.Value,
        //    //            Percentage = totalHoSellKgQty.Value != 0 ? 100 / totalHoSellKgQty.Value * detailSellHoQty.Value : 0,
        //    //            PercentageAllItem = totalSellQtyKgInHo.Value != 0 ? 100 / totalSellQtyKgInHo.Value * detailSellHoQty.Value : 0
        //    //        };

        //    //        productDetails.Add(productDetail);
        //    //    }

        //    //    var itemTotalQty = item.Sum(x => x.BaseUnitId == 40
        //    //                                   ? x.BaseQuantity * x.SellQuantity
        //    //                                   : (x.BaseQuantity * x.SellQuantity) / 1000);

        //    //    var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

        //    //    var product = new Product
        //    //    {
        //    //        Name = prod.Name,
        //    //        NameAr = prod.NameAr,
        //    //        SellQuantity = itemTotalQty.Value,
        //    //        Percentage = totalHoSellKgQty.Value != 0 ? 100 / totalHoSellKgQty.Value * itemTotalQty.Value : 0,
        //    //        ProductDetails = productDetails
        //    //    };

        //    //    top5ProductsHoByKg.Add(product);

        //    //}

        //    //#endregion

        //    //#endregion

        //    //#region Top 10 Product by Quantity

        //    //#region Branch

        //    //var totalSellQty = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
        //    //                                                           x.LocationId != 84 &&
        //    //                                                           x.BaseUnitId != 40 &&
        //    //                                                           x.BaseUnitId != 42 &&
        //    //                                                           x.ProdId != 19595).Sum(x => x.BaseQuantity * x.SellQuantity);

        //    ////var salesDetailsByQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).GroupBy(x => x.SellQuantity)
        //    ////    .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.SellUnit, j.SellQuantity, j.Location, j.LocationId, rn = i + 1 }));

        //    //var salesDetailsByQty = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
        //    //                                                                        x.LocationId != 84 &&
        //    //                                                                        x.BaseUnitId != 40 &&
        //    //                                                                        x.BaseUnitId != 42 &&
        //    //                                                                        x.ProdId != 19595).GroupBy(x => x.ProdId);

        //    //var top5ProductsByQty = new List<Product>();

        //    //foreach (var item in salesDetailsByQty.OrderByDescending(a => a.Sum(b => b.BaseQuantity * b.SellQuantity)).Take(10))
        //    //{
        //    //    var productDetails = new List<ProductDetail>();

        //    //    var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key &&
        //    //                                                                                 x.ProdId != 19595 &&
        //    //                                                                                 x.LocationId != 1 &&
        //    //                                                                                 x.LocationId != 84 &&
        //    //                                                                                 x.BaseUnitId != 40 &&
        //    //                                                                                 x.BaseUnitId != 42).GroupBy(a => a.LocationId);

        //    //    foreach (var detail in individualSalesDetails.OrderByDescending(a => a.Sum(x => x.BaseQuantity * x.SellQuantity)))
        //    //    {
        //    //        var detailSellQty = detail.Sum(x => x.BaseQuantity * x.SellQuantity);

        //    //        var totalSellQtyInBranch = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
        //    //                                                                                   x.LocationId != 84 &&
        //    //                                                                                   x.ProdId != 19595 &&
        //    //                                                                                   x.LocationId == detail.Key &&
        //    //                                                                                   (x.BaseUnitId != 40 ||
        //    //                                                                                   x.BaseUnitId != 42)).Sum(b => b.BaseQuantity * b.SellQuantity);
        //    //        var productDetail = new ProductDetail
        //    //        {
        //    //            Location = locations.LocationItems.FirstOrDefault(x => x.LocationId == detail.Key).Name,
        //    //            SellQuantity = detailSellQty.Value,
        //    //            Percentage = totalSellQty.Value != 0 ? 100 / totalSellQty.Value * detailSellQty.Value : 0,
        //    //            PercentageAllItem = totalSellQtyInBranch.Value != 0 ? 100 / totalSellQtyInBranch.Value * detailSellQty.Value : 0,
        //    //        };

        //    //        productDetails.Add(productDetail);
        //    //    }

        //    //    var itemTotalQty = item.Sum(a => a.SellQuantity);
        //    //    var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

        //    //    var product = new Product
        //    //    {
        //    //        Name = prod.Name,
        //    //        NameAr = prod.NameAr,
        //    //        SellQuantity = itemTotalQty.Value,
        //    //        Percentage = totalSellQty.Value != 0 ? 100 / totalSellQty.Value * itemTotalQty.Value : 0,
        //    //        ProductDetails = productDetails
        //    //    };

        //    //    top5ProductsByQty.Add(product);
        //    //}
        //    //#endregion

        //    //#region HO

        //    //var totalSellHoQty = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
        //    //                                                                         x.BaseUnitId != 40 &&
        //    //                                                                         x.BaseUnitId != 42 &&
        //    //                                                                         x.ProdId != 19595).Sum(x => x.SellQuantity);

        //    //var salesDetailsHoByQty = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
        //    //                                                                          x.BaseUnitId != 40 &&
        //    //                                                                          x.BaseUnitId != 42 &&
        //    //                                                                          x.ProdId != 19595).GroupBy(x => x.ProdId);

        //    //var top5ProductsHoByQty = new List<Product>();

        //    //foreach (var item in salesDetailsHoByQty.OrderByDescending(a => a.Sum(b => b.SellQuantity)).Take(10))
        //    //{
        //    //    var productDetails = new List<ProductDetail>();

        //    //    var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key &&
        //    //                                                                                 x.ProdId != 19595 &&
        //    //                                                                                 (x.LocationId == 1) &&
        //    //                                                                                 x.BaseUnitId != 40 &&
        //    //                                                                                 x.BaseUnitId != 42);

        //    //    foreach (var detail in individualSalesDetails.OrderByDescending(x => x.BaseQuantity * x.SellQuantity))
        //    //    {
        //    //        var detailSellHoQty = detail.BaseQuantity * detail.SellQuantity;

        //    //        var totalSellQtyInHo = salesDetailsOfMonth.Where(x => x.LocationId == 1 &&
        //    //                                                                               x.ProdId != 19595 &&
        //    //                                                                                x.CustomerId == detail.CustomerId &&
        //    //                                                                               (x.BaseUnitId != 40 || x.BaseUnitId != 42))
        //    //                                                                               .Sum(b => b.BaseQuantity * b.SellQuantity);
        //    //        var productDetail = new ProductDetail
        //    //        {
        //    //            CustomerAr = detail.CustomerNameAr,
        //    //            SellQuantity = detailSellHoQty.Value,
        //    //            Percentage = totalSellHoQty.Value != 0 ? 100 / totalSellHoQty.Value * detailSellHoQty.Value : 0,
        //    //            PercentageAllItem = totalSellQtyInHo.Value != 0 ? 100 / totalSellQtyInHo.Value * detailSellHoQty.Value : 0
        //    //        };

        //    //        productDetails.Add(productDetail);
        //    //    }

        //    //    var itemTotalQty = item.Sum(a => a.SellQuantity);
        //    //    var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

        //    //    var product = new Product
        //    //    {
        //    //        Name = prod.Name,
        //    //        NameAr = prod.NameAr,
        //    //        SellQuantity = itemTotalQty.Value,
        //    //        Percentage = totalSellHoQty.Value != 0 ? 100 / totalSellHoQty.Value * itemTotalQty.Value : 0,
        //    //        ProductDetails = productDetails
        //    //    };

        //    //    top5ProductsHoByQty.Add(product);
        //    //}

        //    //#endregion

        //    //#endregion

        //    //var branchSalesTransDetail = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84);
        //    //var branchSTDCount = branchSalesTransDetail.Count();

        //    //var talabatTransCount = branchSalesTransDetail.Count(x => x.VoucherId == 2025);
        //    //var talabatTransSrCount = branchSalesTransDetail.Count(x => x.VoucherId == 2035);
        //    //var talabatTransNetCount = talabatTransCount - talabatTransSrCount;

        //    //var onlineTransCount = branchSalesTransDetail.Count(x => x.VoucherId == 2026);
        //    //var onlineTransSrCount = branchSalesTransDetail.Count(x => x.VoucherId == 2036);
        //    //var onlineTransNetCount = onlineTransCount - onlineTransSrCount;

        //    //var branchTalabatSales = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2025);
        //    //var branchTalabatSalesReturn = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2035);

        //    //var branchTalabatSalesReturnCash = branchTalabatSalesReturn.Sum(x => x.Cash) != null ? (decimal)branchTalabatSalesReturn.Sum(x => x.Cash) : 0;
        //    //var branchTalabatSalesCash = branchTalabatSales.Sum(x => x.Cash) - Math.Abs(branchTalabatSalesReturnCash);

        //    //var branchTalabatSalesReturnKnet = branchTalabatSalesReturn.Sum(x => x.Knet) != null ? (decimal)branchTalabatSalesReturn.Sum(x => x.Knet) : 0;
        //    //var branchTalabatSalesKnet = branchTalabatSales.Sum(x => x.Knet) - Math.Abs(branchTalabatSalesReturnKnet);

        //    //var branchTalabatSalesReturnCc = branchTalabatSalesReturn.Sum(x => x.CreditCard) != null ? (decimal)branchTalabatSalesReturn.Sum(x => x.CreditCard) : 0;
        //    //var branchTalabatSalesCc = branchTalabatSales.Sum(x => x.CreditCard) - Math.Abs(branchTalabatSalesReturnCc);

        //    //var brTalabatSrAmount = branchTalabatSalesReturn.Sum(a => a.NetAmount) ?? 0;
        //    //var brTalabatSalesAmount = branchTalabatSales.Sum(a => a.NetAmount) ?? 0;



        //    //var branchOnlineSales = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2026);
        //    //var branchOnlineSalesReturn = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2036);

        //    //var branchOnlineSalesReturnCash = branchOnlineSalesReturn.Sum(x => x.Cash) != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.Cash) : 0;
        //    //var branchOnlineSalesCash = branchOnlineSales.Sum(x => x.Cash) - Math.Abs(branchOnlineSalesReturnCash);

        //    //var branchOnlineSalesReturnKnet = branchOnlineSalesReturn.Sum(x => x.Knet) != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.Knet) : 0;
        //    //var branchOnlineSalesKnet = branchOnlineSales.Sum(x => x.Knet) - Math.Abs(branchOnlineSalesReturnKnet);

        //    //var branchOnlineSalesReturnCc = branchOnlineSalesReturn.Sum(x => x.CreditCard) != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.CreditCard) : 0;
        //    //var branchOnlineSalesCc = branchOnlineSales.Sum(x => x.CreditCard) - Math.Abs(branchOnlineSalesReturnCc);

        //    //var brOnlineSrNet = branchOnlineSalesReturn.Sum(x => x.NetAmount) ?? 0;
        //    //var branchSalesReturn = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(a => a.SalesReturn) + Math.Abs(brOnlineSrNet) + Math.Abs(brTalabatSrAmount);
        //    ////var brSalesReturnCash = salesOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId == 2023 || x.VoucherId == 202)).Sum(a => a.Cash) ?? 0;
        //    ////var brSalesReturnKnet = salesOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId == 2023 || x.VoucherId == 202)).Sum(a => a.Knet) ?? 0;
        //    ////var brSalesReturnCc = salesOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId == 2023 || x.VoucherId == 202)).Sum(a => a.CreditCard) ?? 0;

        //    //return new SalesOfMonthViewModel
        //    //{
        //    //    SalesMonthItems = salesMonthItems,

        //    //    TotalSales = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(a => a.NetAmount),
        //    //    TotalBranchSales = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId != 2026 || x.VoucherId != 2036)).Sum(a => a.NetAmount),

        //    //    TotalBranchCash = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId != 2023 && x.VoucherId != 202)).Sum(a => a.Cash) - branchOnlineSalesCash - branchTalabatSalesCash,
        //    //    TotalBranchKnet = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId != 2023 && x.VoucherId != 202)).Sum(a => a.Knet) - branchOnlineSalesKnet - branchTalabatSalesKnet,
        //    //    TotalBranchCC = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId != 2023 && x.VoucherId != 202)).Sum(a => a.CreditCard) - branchOnlineSalesCc - branchTalabatSalesCc,
        //    //    TotalBranchCarraige = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2025).Sum(a => a.NetAmount),
        //    //    TotalBranchCount = branchSTDCount - (talabatTransCount + talabatTransSrCount + onlineTransCount + onlineTransSrCount),

        //    //    //TotalBranchOnline = branchOnlineSales.Sum(a => a.NetAmount) - Math.Abs(branchOnlineSalesReturn != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.NetAmount) : 0),
        //    //    //TotalBranchOnlineCash = branchOnlineSales.Sum(a => a.Cash) - Math.Abs(branchOnlineSalesReturn != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.Cash) : 0),
        //    //    //TotalBranchOnlineKnet = branchOnlineSales.Sum(a => a.Knet) - Math.Abs(branchOnlineSalesReturn != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.Knet) : 0),
        //    //    //TotalBranchOnlineCc = branchOnlineSales.Sum(a => a.CreditCard) - Math.Abs(branchOnlineSalesReturn != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.CreditCard) : 0),

        //    //    TotalBranchOnline = branchOnlineSales.Sum(a => a.NetAmount),
        //    //    TotalBranchOnlineCash = branchOnlineSales.Sum(a => a.Cash),
        //    //    TotalBranchOnlineKnet = branchOnlineSales.Sum(a => a.Knet),
        //    //    TotalBranchOnlineCc = branchOnlineSales.Sum(a => a.CreditCard),
        //    //    TotalBranchOnlineReturn = branchOnlineSalesReturn.Sum(x => x.NetAmount),
        //    //    TotalOnlineTransCount = onlineTransNetCount,

        //    //    TotalTalabat = brTalabatSalesAmount - Math.Abs(brTalabatSrAmount),
        //    //    TalabatTransCount = talabatTransNetCount,

        //    //    SalesReturnBranches = branchSalesReturn,

        //    //    TotalHOSales = salesOfMonth.Where(x => x.LocationId == 1).Sum(a => a.NetAmount),
        //    //    TotalHOSalesCash = salesOfMonth.Where(x => (x.VoucherId == 2022 || x.VoucherId == 2026) && x.LocationId == 1).Sum(a => a.Cash),
        //    //    TotalHOSalesCredit = salesOfMonth.Where(x => x.VoucherId == 2021 && x.LocationId == 1).Sum(a => a.NetAmount),
        //    //    SalesReturnHO = salesOfMonth.Where(x => x.LocationId == 1).Sum(a => a.SalesReturn),
        //    //    TotalHOCount = salesDetailsOfMonth.Count(x => x.LocationId == 1),

        //    //    Top5ProductsByAmount = top5ProductsByAmount,

        //    //    Top5HoProductsByAmount = top5HoProductsByAmount,
        //    //    Top5ProductsByKg = top5ProductsByKg,
        //    //    Top5ProductsHoByKg = top5ProductsHoByKg,
        //    //    Top5ProductsByQty = top5ProductsByQty,
        //    //    Top5ProductsHoByQty = top5ProductsHoByQty
        //    //};

        //}


        public SalesOfMonthViewModel GetSalesRecordOfMonth(DateTime fromDate, DateTime toDate)
        {
            var dashboardTransaction = _salesReportRepository.GetSalesRecordDashboardTrans(fromDate, toDate, "", "");

            var salesOfMonth = dashboardTransaction.SRItemsTrans;

            var locations = _locationRepository.GetLocations();

            var salesMonthItems = new List<SalesMonthItem>();
            var grandTotalOfMonth = salesOfMonth.Sum(a => a.NetAmount);

            foreach (var location in locations.LocationItems.Where(x => x.LocationId != 1 && x.LocationId != 84))
            {
                var locationTotalAmount = salesOfMonth.Where(x => x.LocationId == location.LocationId &&
                                                                  x.VoucherId != 2025 &&
                                                                  x.VoucherId != 2035 &&
                                                                  x.VoucherId != 2026 &&
                                                                  x.VoucherId != 2037 &&
                                                                  x.VoucherId != 2036).Sum(a => a.NetAmount);

                var salesMonthItem = new SalesMonthItem
                {
                    Label = location.ShortName,
                    Data = locationTotalAmount
                };

                if (locationTotalAmount.Value > 0)
                {
                    salesMonthItems.Add(salesMonthItem);
                }
            }

            // var products = _productRepository.GetAllProducts().Items;

            var branchSalesTransDetail = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84);
            var branchSTDCount = branchSalesTransDetail.Count();

            var talabatTransCount = branchSalesTransDetail.Count(x => x.VoucherId == 2025);
            var talabatTransSrCount = branchSalesTransDetail.Count(x => x.VoucherId == 2035);
            var talabatTransNetCount = talabatTransCount - talabatTransSrCount;

            var deliverooTransCount = branchSalesTransDetail.Count(x => x.VoucherId == 2030);
            var deliverooTransSrCount = branchSalesTransDetail.Count(x => x.VoucherId == 2037);
            var deliverooTransNetCount = deliverooTransCount - deliverooTransSrCount;

            var onlineTransCount = branchSalesTransDetail.Count(x => x.VoucherId == 2026);
            var onlineTransSrCount = branchSalesTransDetail.Count(x => x.VoucherId == 2036);
            var onlineTransNetCount = onlineTransCount - onlineTransSrCount;

            #region Talabat

            var branchTalabatSales = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2025);
            var branchTalabatSalesReturn = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2035);

            var branchTalabatSalesReturnCash = branchTalabatSalesReturn.Sum(x => x.Cash) != null ? (decimal)branchTalabatSalesReturn.Sum(x => x.Cash) : 0;
            var branchTalabatSalesCash = branchTalabatSales.Sum(x => x.Cash) - Math.Abs(branchTalabatSalesReturnCash);

            var branchTalabatSalesReturnKnet = branchTalabatSalesReturn.Sum(x => x.Knet) != null ? (decimal)branchTalabatSalesReturn.Sum(x => x.Knet) : 0;
            var branchTalabatSalesKnet = branchTalabatSales.Sum(x => x.Knet) - Math.Abs(branchTalabatSalesReturnKnet);

            var branchTalabatSalesReturnCc = branchTalabatSalesReturn.Sum(x => x.CreditCard) != null ? (decimal)branchTalabatSalesReturn.Sum(x => x.CreditCard) : 0;
            var branchTalabatSalesCc = branchTalabatSales.Sum(x => x.CreditCard) - Math.Abs(branchTalabatSalesReturnCc);

            var brTalabatSrAmount = branchTalabatSalesReturn.Sum(a => a.NetAmount) ?? 0;
            var brTalabatSalesAmount = branchTalabatSales.Sum(a => a.NetAmount) ?? 0;

            #endregion

            #region Deliveroo

            var branchDeliverooSales = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2030);
            var branchDeliverooSalesReturn = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2037);

            var branchDeliverooSalesReturnCash = branchDeliverooSalesReturn.Sum(x => x.Cash) != null ? (decimal)branchDeliverooSalesReturn.Sum(x => x.Cash) : 0;
            var branchDeliverooSalesCash = branchDeliverooSales.Sum(x => x.Cash) - Math.Abs(branchDeliverooSalesReturnCash);

            var branchDeliverooSalesReturnKnet = branchDeliverooSalesReturn.Sum(x => x.Knet) != null ? (decimal)branchDeliverooSalesReturn.Sum(x => x.Knet) : 0;
            var branchDeliverooSalesKnet = branchDeliverooSales.Sum(x => x.Knet) - Math.Abs(branchDeliverooSalesReturnKnet);

            var branchDeliverooSalesReturnCc = branchDeliverooSalesReturn.Sum(x => x.CreditCard) != null ? (decimal)branchDeliverooSalesReturn.Sum(x => x.CreditCard) : 0;
            var branchDeliverooSalesCc = branchDeliverooSales.Sum(x => x.CreditCard) - Math.Abs(branchDeliverooSalesReturnCc);

            var brDeliverooSrAmount = branchDeliverooSalesReturn.Sum(a => a.NetAmount) ?? 0;
            var brDeliverooSalesAmount = branchDeliverooSales.Sum(a => a.NetAmount) ?? 0;

            #endregion

            #region Branch Online Sales 

            var branchOnlineSales = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2026);
            var branchOnlineSalesReturn = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2036);

            var branchOnlineSalesReturnCash = branchOnlineSalesReturn.Sum(x => x.Cash) != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.Cash) : 0;
            var branchOnlineSalesCash = branchOnlineSales.Sum(x => x.Cash) - Math.Abs(branchOnlineSalesReturnCash);

            var branchOnlineSalesReturnKnet = branchOnlineSalesReturn.Sum(x => x.Knet) != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.Knet) : 0;
            var branchOnlineSalesKnet = branchOnlineSales.Sum(x => x.Knet) - Math.Abs(branchOnlineSalesReturnKnet);

            var branchOnlineSalesReturnCc = branchOnlineSalesReturn.Sum(x => x.CreditCard) != null ? (decimal)branchOnlineSalesReturn.Sum(x => x.CreditCard) : 0;
            var branchOnlineSalesCc = branchOnlineSales.Sum(x => x.CreditCard) - Math.Abs(branchOnlineSalesReturnCc);

            var brOnlineSrNet = branchOnlineSalesReturn.Sum(x => x.NetAmount) ?? 0; 

            #endregion

            var branchSalesReturn = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(a => a.SalesReturn) + Math.Abs(brOnlineSrNet)
                                                                                                                             + Math.Abs(brTalabatSrAmount)
                                                                                                                             + Math.Abs(brDeliverooSrAmount);

            return new SalesOfMonthViewModel
            {
                SalesMonthItems = salesMonthItems,

                TotalSales = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(a => a.NetAmount),
                TotalBranchSales = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId != 2026 || x.VoucherId != 2036)).Sum(a => a.NetAmount),

                TotalBranchCash = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId != 2023 && x.VoucherId != 202)).Sum(a => a.Cash) - branchOnlineSalesCash - branchTalabatSalesCash - branchDeliverooSalesCash,
                TotalBranchKnet = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId != 2023 && x.VoucherId != 202)).Sum(a => a.Knet) - branchOnlineSalesKnet - branchTalabatSalesKnet - branchDeliverooSalesKnet,
                TotalBranchCC = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && (x.VoucherId != 2023 && x.VoucherId != 202)).Sum(a => a.CreditCard) - branchOnlineSalesCc - branchTalabatSalesCc - branchDeliverooSalesCc,
                TotalBranchCarraige = salesOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.VoucherId == 2025 && x.VoucherId == 2030).Sum(a => a.NetAmount),
                TotalBranchCount = branchSTDCount - (talabatTransCount + talabatTransSrCount + onlineTransCount + onlineTransSrCount + deliverooTransCount + deliverooTransSrCount),

                TotalBranchOnline = branchOnlineSales.Sum(a => a.NetAmount),
                TotalBranchOnlineCash = branchOnlineSales.Sum(a => a.Cash),
                TotalBranchOnlineKnet = branchOnlineSales.Sum(a => a.Knet),
                TotalBranchOnlineCc = branchOnlineSales.Sum(a => a.CreditCard),
                TotalBranchOnlineReturn = branchOnlineSalesReturn.Sum(x => x.NetAmount),
                TotalOnlineTransCount = onlineTransNetCount,

                TotalTalabat = brTalabatSalesAmount - Math.Abs(brTalabatSrAmount),
                TalabatTransCount = talabatTransNetCount,

                TotalDeliveroo = brDeliverooSalesAmount - Math.Abs(brDeliverooSrAmount),
                DeliverooTransCount = deliverooTransNetCount,

                SalesReturnBranches = branchSalesReturn,

                TotalHOSales = salesOfMonth.Where(x => x.LocationId == 1).Sum(a => a.NetAmount),
                TotalHOSalesCash = salesOfMonth.Where(x => (x.VoucherId == 2022 || x.VoucherId == 2026) && x.LocationId == 1).Sum(a => a.Cash),
                TotalHOSalesCredit = salesOfMonth.Where(x => x.VoucherId == 2021 && x.LocationId == 1).Sum(a => a.NetAmount),
                SalesReturnHO = salesOfMonth.Where(x => x.LocationId == 1).Sum(a => a.SalesReturn)
            };

        }

        public SalesOfMonthViewModel GetSalesRecordDetailOfMonth(DateTime? fromDate, DateTime? toDate)
        {
            //1 - Total Sales(Returns Deducted) - Excluding HO and ARD (HO : 1 and ARD : 84)
            //2 - Total Sales(Returns Deducted) - HO
            //3 - Total Sales(Returns Deducted) - HO - Cash Corporate
            //4 - Total Sales(Returns Deducted) - HO - Credit Corporate
            //5 - Total Returns - HO(Cash & Corporate)
            //6 - Total Returns - Only Branches(Excluding HO and ARD)

            var dashboardTransaction = _salesReportRepository.GetSalesDashboardTransaction(fromDate, toDate, "", "", "", false);
            var salesDetailsOfMonth = dashboardTransaction.SRItemsTransDetails;

            var locations = _locationRepository.GetLocations();
            var products = _productRepository.GetAllProducts().Items;


            #region Top 10 Product by Amount

            #region Branch

            var totalAmount = salesDetailsOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84).Sum(x => x.Amount);

            var salesDetails = salesDetailsOfMonth.Where(x => x.LocationId != 1 && x.LocationId != 84).GroupBy(x => x.ProdId);

            var top5ProductsByAmount = new List<Product>();

            foreach (var item in salesDetails.OrderByDescending(a => a.Sum(b => b.Amount)).Take(10))
            {
                var productDetails = new List<ProductDetail>();
                var itemTotalAmount = item.Sum(a => a.Amount);
                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key && x.LocationId != 1 && x.LocationId != 84).GroupBy(a => a.LocationId);

                foreach (var detail in individualSalesDetails.OrderByDescending(a => a.Sum(x => x.Amount)))
                {
                    var detailAmount = detail.Sum(x => x.Amount);

                    var totalSellAmountInBranch = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                                                x.LocationId != 84 &&
                                                                                                x.LocationId == detail.Key
                                                                                                //&&
                                                                                                //(x.BaseUnitId == 40 ||
                                                                                                //x.BaseUnitId == 42)
                                                                                                ).Sum(b => b.Amount);

                    var productDetail = new ProductDetail
                    {
                        Amount = detailAmount,
                        Location = locations.LocationItems.FirstOrDefault(x => x.LocationId == detail.Key).Name,
                        Percentage = totalAmount.Value != 0 ? 100 / totalAmount.Value * detailAmount.Value : 0,
                        PercentageAllItem = totalSellAmountInBranch.Value != 0 ? 100 / totalSellAmountInBranch.Value * detailAmount.Value : 0
                    };

                    productDetails.Add(productDetail);
                }

                var product = new Product
                {
                    ProductId = prod.ProductId,
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    Amount = itemTotalAmount.Value,
                    Percentage = totalAmount.Value != 0 ? 100 / totalAmount.Value * itemTotalAmount.Value : 0,
                    ProductDetails = productDetails
                };

                top5ProductsByAmount.Add(product);
            }
            #endregion

            #region HO

            var totalAmountHo = salesDetailsOfMonth.Where(x => x.LocationId == 1 || x.LocationId == 84).Sum(x => x.Amount);

            var salesDetailsHo = salesDetailsOfMonth.Where(x => x.LocationId == 1 || x.LocationId == 84).GroupBy(x => x.ProdId);

            var top5HoProductsByAmount = new List<Product>();

            foreach (var item in salesDetailsHo.OrderByDescending(a => a.Sum(b => b.Amount)).Take(10))
            {
                var productDetails = new List<ProductDetail>();
                var itemTotalAmount = item.Sum(a => a.Amount);
                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key && (x.LocationId == 1 || x.LocationId == 84));

                foreach (var detail in individualSalesDetails.GroupBy(a => a.CustomerId).OrderByDescending(x => x.Sum(b => b.Amount)))
                {
                    var detailAmount = detail.Sum(x => x.Amount);

                    var totalSellAmountInHo = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
                                                                                                x.CustomerId == detail.Key).Sum(b => b.Amount);

                    var productDetail = new ProductDetail
                    {
                        Amount = detailAmount,
                        Location = "",
                        CustomerAr = detail.FirstOrDefault().CustomerNameAr,
                        Percentage = totalAmountHo.Value != 0 ? 100 / totalAmountHo.Value * detailAmount.Value : 0,
                        // PercentageAllItem = totalSellAmountInHo != 0 ? 100 / totalSellAmountInHo * detailAmount : 0
                        PercentageAllItem = totalSellAmountInHo.Value != 0 ? 100 / totalSellAmountInHo.Value * detailAmount.Value : 0
                    };

                    productDetails.Add(productDetail);
                }

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    Amount = itemTotalAmount,
                    Percentage = totalAmountHo.Value != 0 ? 100 / totalAmountHo.Value * itemTotalAmount.Value : 0,
                    ProductDetails = productDetails
                };

                top5HoProductsByAmount.Add(product);

            }
            #endregion

            #endregion

            #region Top 10 Product by Kg

            #region Branch

            var sellQtyKg = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                         x.LocationId != 84 &&
                                                                         (x.BaseUnitId == 40)).Sum(x => x.BaseQuantity * x.SellQuantity);

            var sellQtyGm = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                                 x.LocationId != 84 &&
                                                                                 (x.BaseUnitId == 42)).Sum(x => (x.BaseQuantity * x.SellQuantity) / 1000);

            var totalSellQtyKg = sellQtyKg + sellQtyGm;

            //var salesDetailsByKg = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84 && x.SellUnitId == 40).GroupBy(x => x.SellQuantity)
            //    .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.SellUnit, j.SellQuantity, j.Location, j.LocationId, rn = i + 1 }));

            var salesDetailsByKg = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                                   x.LocationId != 84 &&
                                                                                   (x.BaseUnitId == 40 ||
                                                                                    x.BaseUnitId == 42)).GroupBy(x => x.ProdId);

            var top5ProductsByKg = new List<Product>();

            foreach (var item in salesDetailsByKg.OrderByDescending(a => a.Sum(b => b.BaseUnitId == 40
                                                                                    ? b.BaseQuantity * b.SellQuantity
                                                                                    : (b.BaseQuantity * b.SellQuantity) / 1000)).Take(10))
            {
                var productDetails = new List<ProductDetail>();
                var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key &&
                                                                                             x.LocationId != 1 &&
                                                                                             x.LocationId != 84 &&
                                                                                             (x.BaseUnitId == 40 ||
                                                                                              x.BaseUnitId == 42)).GroupBy(a => a.LocationId);


                foreach (var detail in individualSalesDetails.OrderByDescending(a => a.Sum(x => x.BaseUnitId == 40
                                                                                                ? x.BaseQuantity * x.SellQuantity
                                                                                                : (x.BaseQuantity * x.SellQuantity) / 1000)))
                {
                    var detailSellQty = detail.Sum(x => x.BaseUnitId == 40
                                                        ? x.BaseQuantity * x.SellQuantity
                                                        : (x.BaseQuantity * x.SellQuantity) / 1000);

                    var totalSellQtyKgInBranch = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                                                 x.LocationId != 84 &&
                                                                                                 x.LocationId == detail.Key &&
                                                                                                 (x.BaseUnitId == 40 ||
                                                                                                 x.BaseUnitId == 42)).Sum(b => b.BaseUnitId == 40
                                                                                                                            ? b.BaseQuantity * b.SellQuantity
                                                                                                                            : (b.BaseQuantity * b.SellQuantity) / 1000);

                    var productDetail = new ProductDetail
                    {
                        Location = locations.LocationItems.FirstOrDefault(x => x.LocationId == detail.Key).Name,
                        SellQuantity = detailSellQty.Value,
                        Percentage = totalSellQtyKg.Value != 0 ? 100 / totalSellQtyKg.Value * detailSellQty.Value : 0,
                        PercentageAllItem = totalSellQtyKgInBranch.Value != 0 ? 100 / totalSellQtyKgInBranch.Value * detailSellQty.Value : 0
                    };

                    productDetails.Add(productDetail);
                }


                var itemTotalQty = item.Sum(x => x.BaseUnitId == 40
                                                        ? x.BaseQuantity * x.SellQuantity
                                                        : (x.BaseQuantity * x.SellQuantity) / 1000);

                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    SellQuantity = itemTotalQty.Value,
                    Percentage = totalSellQtyKg.Value != 0 ? 100 / totalSellQtyKg.Value * itemTotalQty.Value : 0,
                    ProductDetails = productDetails
                };

                top5ProductsByKg.Add(product);
            }
            #endregion

            #region HO

            var hoSellQtyKg = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
                                                                                 (x.BaseUnitId == 40)).Sum(x => x.BaseQuantity * x.SellQuantity);

            var hoSellQtyGm = salesDetailsOfMonth.Where(x => x.LocationId == 1 &&
                                                                              (x.BaseUnitId == 42)).Sum(x => (x.BaseQuantity * x.SellQuantity) / 1000);

            var totalHoSellKgQty = hoSellQtyKg + hoSellQtyGm;

            var salesDetailsHoByKg = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
                                                                                     (x.BaseUnitId == 40 ||
                                                                                     x.BaseUnitId == 42)).GroupBy(x => x.ProdId);

            var top5ProductsHoByKg = new List<Product>();

            foreach (var item in salesDetailsHoByKg.OrderByDescending(a => a.Sum(x => x.BaseUnitId == 40
                                                                                    ? x.BaseQuantity * x.SellQuantity
                                                                                    : x.BaseQuantity * x.SellQuantity / 1000)).Take(10))
            {

                var productDetails = new List<ProductDetail>();
                var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key &&
                                                                                             (x.LocationId == 1 || x.LocationId == 84) &&
                                                                                             (x.BaseUnitId == 40 ||
                                                                                              x.BaseUnitId == 42));

                foreach (var detail in individualSalesDetails.GroupBy(x => x.CustomerId).OrderByDescending(a => a.Sum(x => x.BaseUnitId == 40
                                                                                         ? x.BaseQuantity * x.SellQuantity
                                                                                         : (x.BaseQuantity * x.SellQuantity) / 1000)))
                {
                    var detailSellHoQty = detail.Sum(x => x.BaseUnitId == 40
                                                        ? x.BaseQuantity * x.SellQuantity
                                                        : (x.BaseQuantity * x.SellQuantity) / 1000);

                    var totalSellQtyKgInHo = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
                                                                                              x.CustomerId == detail.Key &&
                                                                                             (x.BaseUnitId == 40 || x.BaseUnitId == 42))
                                                                                             .Sum(b => b.BaseUnitId == 40
                                                                                                ? b.BaseQuantity * b.SellQuantity
                                                                                                : (b.BaseQuantity * b.SellQuantity) / 1000);

                    var productDetail = new ProductDetail
                    {
                        CustomerAr = detail.FirstOrDefault().CustomerNameAr,
                        SellQuantity = detailSellHoQty.Value,
                        Percentage = totalHoSellKgQty.Value != 0 ? 100 / totalHoSellKgQty.Value * detailSellHoQty.Value : 0,
                        PercentageAllItem = totalSellQtyKgInHo.Value != 0 ? 100 / totalSellQtyKgInHo.Value * detailSellHoQty.Value : 0
                    };

                    productDetails.Add(productDetail);
                }

                var itemTotalQty = item.Sum(x => x.BaseUnitId == 40
                                               ? x.BaseQuantity * x.SellQuantity
                                               : (x.BaseQuantity * x.SellQuantity) / 1000);

                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    SellQuantity = itemTotalQty.Value,
                    Percentage = totalHoSellKgQty.Value != 0 ? 100 / totalHoSellKgQty.Value * itemTotalQty.Value : 0,
                    ProductDetails = productDetails
                };

                top5ProductsHoByKg.Add(product);

            }

            #endregion

            #endregion

            #region Top 10 Product by Quantity

            #region Branch

            var totalSellQty = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                       x.LocationId != 84 &&
                                                                       x.BaseUnitId != 40 &&
                                                                       x.BaseUnitId != 42 &&
                                                                       x.ProdId != 19595).Sum(x => x.BaseQuantity * x.SellQuantity);

            //var salesDetailsByQty = salesDetailsOfMonth.SalesReportItems.Where(x => x.LocationId != 1 && x.LocationId != 84).GroupBy(x => x.SellQuantity)
            //    .SelectMany(g => g.Select((j, i) => new { j.ProductNameEn, j.ProductNameAr, j.SellUnit, j.SellQuantity, j.Location, j.LocationId, rn = i + 1 }));

            var salesDetailsByQty = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                                    x.LocationId != 84 &&
                                                                                    x.BaseUnitId != 40 &&
                                                                                    x.BaseUnitId != 42 &&
                                                                                    x.ProdId != 19595).GroupBy(x => x.ProdId);

            var top5ProductsByQty = new List<Product>();

            foreach (var item in salesDetailsByQty.OrderByDescending(a => a.Sum(b => b.BaseQuantity * b.SellQuantity)).Take(10))
            {
                var productDetails = new List<ProductDetail>();

                var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key &&
                                                                                             x.ProdId != 19595 &&
                                                                                             x.LocationId != 1 &&
                                                                                             x.LocationId != 84 &&
                                                                                             x.BaseUnitId != 40 &&
                                                                                             x.BaseUnitId != 42).GroupBy(a => a.LocationId);

                foreach (var detail in individualSalesDetails.OrderByDescending(a => a.Sum(x => x.BaseQuantity * x.SellQuantity)))
                {
                    var detailSellQty = detail.Sum(x => x.BaseQuantity * x.SellQuantity);

                    var totalSellQtyInBranch = salesDetailsOfMonth.Where(x => x.LocationId != 1 &&
                                                                                               x.LocationId != 84 &&
                                                                                               x.ProdId != 19595 &&
                                                                                               x.LocationId == detail.Key &&
                                                                                               (x.BaseUnitId != 40 ||
                                                                                               x.BaseUnitId != 42)).Sum(b => b.BaseQuantity * b.SellQuantity);
                    var productDetail = new ProductDetail
                    {
                        Location = locations.LocationItems.FirstOrDefault(x => x.LocationId == detail.Key).Name,
                        SellQuantity = detailSellQty.Value,
                        Percentage = totalSellQty.Value != 0 ? 100 / totalSellQty.Value * detailSellQty.Value : 0,
                        PercentageAllItem = totalSellQtyInBranch.Value != 0 ? 100 / totalSellQtyInBranch.Value * detailSellQty.Value : 0,
                    };

                    productDetails.Add(productDetail);
                }

                var itemTotalQty = item.Sum(a => a.SellQuantity);
                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    SellQuantity = itemTotalQty.Value,
                    Percentage = totalSellQty.Value != 0 ? 100 / totalSellQty.Value * itemTotalQty.Value : 0,
                    ProductDetails = productDetails
                };

                top5ProductsByQty.Add(product);
            }
            #endregion

            #region HO

            var totalSellHoQty = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
                                                                                     x.BaseUnitId != 40 &&
                                                                                     x.BaseUnitId != 42 &&
                                                                                     x.ProdId != 19595).Sum(x => x.SellQuantity);

            var salesDetailsHoByQty = salesDetailsOfMonth.Where(x => (x.LocationId == 1 || x.LocationId == 84) &&
                                                                                      x.BaseUnitId != 40 &&
                                                                                      x.BaseUnitId != 42 &&
                                                                                      x.ProdId != 19595).GroupBy(x => x.ProdId);

            var top5ProductsHoByQty = new List<Product>();

            foreach (var item in salesDetailsHoByQty.OrderByDescending(a => a.Sum(b => b.SellQuantity)).Take(10))
            {
                var productDetails = new List<ProductDetail>();

                var individualSalesDetails = salesDetailsOfMonth.Where(x => x.ProdId == item.Key &&
                                                                                             x.ProdId != 19595 &&
                                                                                             (x.LocationId == 1) &&
                                                                                             x.BaseUnitId != 40 &&
                                                                                             x.BaseUnitId != 42);

                foreach (var detail in individualSalesDetails.GroupBy(x => x.CustomerId).OrderByDescending(x => x.Sum(a => a.BaseQuantity * a.SellQuantity)))
                {
                    var detailSellHoQty = detail.Sum(x => x.BaseQuantity * x.SellQuantity);

                    var totalSellQtyInHo = salesDetailsOfMonth.Where(x => x.LocationId == 1 &&
                                                                                           x.ProdId != 19595 &&
                                                                                            x.CustomerId == detail.Key &&
                                                                                           (x.BaseUnitId != 40 || x.BaseUnitId != 42))
                                                                                           .Sum(b => b.BaseQuantity * b.SellQuantity);
                    var productDetail = new ProductDetail
                    {
                        CustomerAr = detail.FirstOrDefault().CustomerNameAr,
                        SellQuantity = detailSellHoQty.Value,
                        Percentage = totalSellHoQty.Value != 0 ? 100 / totalSellHoQty.Value * detailSellHoQty.Value : 0,
                        PercentageAllItem = totalSellQtyInHo.Value != 0 ? 100 / totalSellQtyInHo.Value * detailSellHoQty.Value : 0
                    };

                    productDetails.Add(productDetail);
                }

                var itemTotalQty = item.Sum(a => a.SellQuantity);
                var prod = products.FirstOrDefault(x => x.ProductId == item.Key);

                var product = new Product
                {
                    Name = prod.Name,
                    NameAr = prod.NameAr,
                    SellQuantity = itemTotalQty.Value,
                    Percentage = totalSellHoQty.Value != 0 ? 100 / totalSellHoQty.Value * itemTotalQty.Value : 0,
                    ProductDetails = productDetails
                };

                top5ProductsHoByQty.Add(product);
            }

            #endregion

            #endregion

            return new SalesOfMonthViewModel
            {
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