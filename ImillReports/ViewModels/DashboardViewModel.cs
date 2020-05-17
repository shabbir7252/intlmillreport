using ImillReports.Models;
using System.Collections.Generic;

namespace ImillReports.ViewModels
{
    public class DashboardViewModel
    {
        public Chart ChartData { get; set; }
        public string TotalBranchSales { get; set; }
        public string TotalHOSales { get; internal set; }
        public string TotalHOSalesCash { get; internal set; }
        public string TotalHOSalesCredit { get; internal set; }
        public string SalesReturnHO { get; set; }
        public string SalesReturnBranches { get; set; }
    }

    public class SalesOfMonthViewModel
    {
        // Total Branch Sales
        public decimal? TotalBranchSales { get; set; }

        // Total Cash Sales(Branches)
        public decimal? TotalBranchCash { get; internal set; }

        // Total Knet Sales(Branches)
        public decimal? TotalBranchKnet { get; internal set; }

        // Total Credit Card Sales(Branches)
        public decimal? TotalBranchCC { get; internal set; }

        // Total Carraige Sales(Branches)
        public decimal? TotalBranchCarraige { get; internal set; }

        // Total Online Sales(Branches)
        public decimal? TotalBranchOnline { get; internal set; }

        // Total Returns(Branches)
        public decimal? SalesReturnBranches { get; set; }

        // Total HO Sales
        public decimal? TotalHOSales { get; set; }

        // Total HO Sales(Cash)
        public decimal? TotalHOSalesCash { get; set; }

        // Total HO Sales(Credit)
        public decimal? TotalHOSalesCredit { get; set; }

        // Total Returns(HO)
        public decimal? SalesReturnHO { get; set; }

        public List<SalesMonthItem> SalesMonthItems { get; set; }
        public decimal? TotalSales { get; internal set; }

        public List<Product> Top5ProductsByAmount { get; set; }
        public List<Product> Top5HoProductsByAmount { get; internal set; }
        public List<Product> Top5ProductsByKg { get; internal set; }
        public List<Product> Top5ProductsHoByKg { get; internal set; }
        public List<Product> Top5ProductsByQty { get; internal set; }
        public List<Product> Top5ProductsHoByQty { get; internal set; }
        public List<Product> Top5ProdDetailsByAmount { get; internal set; }
    }

    public class Product
    {
        public string Name { get; set; }
        public string NameAr { get; internal set; }
        public decimal? Amount { get; internal set; }
        public string Location { get; internal set; }
        public decimal Percentage { get; internal set; }
        public decimal SellQuantity { get; internal set; }
        public long ProductId { get; internal set; }
        public List<ProductDetail> ProductDetails { get; set; }
    }

    public class ProductDetail
    {
        public string Name { get; set; }
        public string NameAr { get; internal set; }
        public decimal? Amount { get; internal set; }
        public string Location { get; internal set; }
        public decimal Percentage { get; internal set; }
        public decimal SellQuantity { get; internal set; }
        public long ProductId { get; internal set; }
    }

    public class SalesMonthItem
    {
        public string Label { get; set; }
        public decimal? Data { get; set; }
    }

    public class BranchSalesDashboardViewModel
    {
        public string TotalBranchSales { get; internal set; }
        public string TotalCashSales { get; internal set; }
        public string TotalKnetSales { get; internal set; }
        public string TotalCreditCardSales { get; internal set; }
        public string TotalCarraigeSales { get; internal set; }
        public string TotalOnlineSales { get; internal set; }
        public string SalesReturnBranches { get; internal set; }
        public string TotalSales { get; internal set; }
        public string TotalHOSales { get; internal set; }
        public string TotalHOSalesCash { get; internal set; }
        public string TotalHOSalesCredit { get; internal set; }
        public string SalesReturnHO { get; internal set; }
    }
}