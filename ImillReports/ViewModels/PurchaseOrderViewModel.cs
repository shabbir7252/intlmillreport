using ImillReports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.ViewModels
{
    public class PurchaseOrderViewModel
    {
        public int Oid { get; set; }
        public short LocatCd { get; set; }
        public string LocationNameEn { get; set; }
        public string LocationNameAr { get; set; }
        public DateTime VoucherDate { get; set; }
        public short VoucherType { get; set; }
        public string VoucherNameEn { get; set; }
        public string VoucherNameAr { get; set; }
        public long VoucherNumber { get; set; }
        public decimal? VoucherAmount { get; set; }
        public int LdgrCd { get; set; }
        public long EntryId { get; set; }
        public DateTime UserDateTime { get; set; }

        /// <summary>
        /// 0. Pending 
        /// 1. Approve
        /// 2. Reject
        /// </summary>
        public int LpoStatus { get; set; }
        public string LpoStatusString { get; set; }

        /// <summary>
        /// 0. Pending
        /// 1. Created
        /// </summary>
        public int LpoInvoiceStatus { get; set; }
        public string LpoInvoiceString { get; set; }

        /// <summary>
        /// 0. Pending 
        /// 1. Approve
        /// 2. Reject
        /// </summary>
        public int LpoPaymentStatus { get; set; }
        public string LpoPaymentStatusString { get; set; }

        public string PaymentRemarks { get; set; }
        public string GmComments { get; set; }
        public int CustomerId { get; internal set; }
        public string CustomerName { get; internal set; }
        public string CustomerNameAr { get; internal set; }
        public decimal Discount { get; internal set; }
        public string LpoStatusTransDate { get; internal set; }
        public string LpoPaymentTransDate { get; internal set; }
        public int QAStatus { get; internal set; }
        public string QAStatusString { get; internal set; }
        public string QATransDate { get; internal set; }
        public string QARemarks { get; internal set; }
    }
}