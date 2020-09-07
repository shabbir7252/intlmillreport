using ImillReports.Models;
using ImillReports.Contracts;
using ImillReports.ViewModels;
using System.Collections.Generic;

namespace ImillReports.Repository
{
    public class SalesmanRepository : ISalesmanRepository
    {
        private readonly IMILLEntities _context;
        public SalesmanRepository(IMILLEntities context) => _context = context;

        public SalesmanViewModel GetSalesmans()
        {
            var salesmans = _context.SM_SALESMAN;

            var salesmanItems = new List<SalesmanItem>();

            foreach (var salesman in salesmans)
            {
                var salesmanItem = new SalesmanItem
                {
                   SalesmanId = salesman.Sman_Cd,
                   Name = salesman.L_Sman_Name,
                   NameAr = salesman.A_Sman_Name
                };

                salesmanItems.Add(salesmanItem);
            }

            return new SalesmanViewModel
            {
                SalesmanItems = salesmanItems
            };
        }
    }
}