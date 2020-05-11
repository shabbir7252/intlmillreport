using IntlmillReports.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntlmillReports.Contracts
{
    public interface ISalesReportRepository
    {
        SalesReportViewModel GetSalesReport(DateTime? fromDate, DateTime? toDate, int? locationId);
    }
}
