using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IntlmillReports.ViewModels
{
    public class SalesReportViewModel
    {
        public List<SalesReportItem> SalesReportItems { get; set; }
    }

    public class SalesReportItem
    {
        public string Location { get; set; }
        public DateTime InvDateTime { get; set; }
        public string Salesman { get; set; }
        public string Voucher { get; set; }
        public long InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNameAr { get; set; }
        public decimal? AmountRecieved { get; set; }
        public decimal? Discount { get; set; }
        public decimal? NetAmount { get; set; }
        public string CreditCardType { get; set; }
        public decimal? Cash { get; set; }
        public decimal? Knet { get; set; }
        public decimal? CreditCard { get; set; }
        public int GroupCD { get; set; }

    }
}