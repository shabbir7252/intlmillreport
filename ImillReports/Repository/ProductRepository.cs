using ImillReports.Contracts;
using ImillReports.Models;
using ImillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMILLEntities _context;

        public ProductRepository(IMILLEntities context)
        {
            _context = context;
        }

        public ProductViewModel GetAllProducts()
        {
            var items = _context.ICS_Item.Where(a => !a.Discontinued);

            var prodItems = from item in items
                             let prodItem = new Item
                             {
                                 ProductId = item.Prod_Cd,
                                 Name = item.L_Prod_Name,
                                 NameAr = item.A_Prod_Name
                             }
                             select prodItem;

            return new ProductViewModel
            {
                Items = prodItems.ToList()
            };
        }
    }
}