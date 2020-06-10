using ImillReports.Contracts;
using ImillReports.ViewModels;
using System.Collections.Generic;
using static ImillReports.Repository.LocationRepository;

namespace ImillReports.Repository
{
    public class BaseRepository : IBaseRepository
    {
        public List<int> GetLocationIds(LocationType locationType)
        {
            if (locationType == LocationType.Coops)
            {
                return new List<int>{
                55, 57, 58, 59, 60, 61, 62, 64, 74, 77, 80 };
            }
            else
            {
                return new List<int>{
                54, 56, 63, 65, 66, 68, 73, 76, 78, 81, 82, 83
                };
            }
        }

        public List<SalesReportType> GetSalesReportType()
        {
            return new List<SalesReportType>
            {
                 new SalesReportType
                {
                    Id = 0,
                    Name = "Select Report Type",
                    NameAr = "Select Report Type"
                },
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