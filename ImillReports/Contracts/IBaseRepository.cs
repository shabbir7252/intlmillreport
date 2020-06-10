using ImillReports.ViewModels;
using System.Collections.Generic;
using static ImillReports.Repository.LocationRepository;

namespace ImillReports.Contracts
{
    public interface IBaseRepository
    {
        List<SalesReportType> GetSalesReportType();
        List<int> GetLocationIds(LocationType locationType);
    }
}
