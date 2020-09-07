using ImillReports.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ImillReports.Repository.LocationRepository;

namespace ImillReports.Contracts
{
    public interface IBaseRepository
    {
        List<SalesReportType> GetSalesReportType();
        List<int> GetLocationIds(LocationType locationType);
        bool SaveColumnChooser(List<ColumnChooserItem> columnChooserItems, string pageName, string userId);
        bool CheckDate(string date);
    }
}
