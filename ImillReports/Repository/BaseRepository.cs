using ImillReports.Contracts;
using ImillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.Repository
{
    public class BaseRepository : IBaseRepository
    {
        public List<SalesReportType> GetSalesReportType()
        {
            return new List<SalesReportType>
            {
                new SalesReportType
                {
                    Id = 1,
                    Name = "Amount",
                    NameAr = "Amount"
                },
                new SalesReportType
                {
                    Id = 2,
                    Name = "Transaction Count",
                    NameAr = "Transaction Count"
                }
            };
        }
    }
}