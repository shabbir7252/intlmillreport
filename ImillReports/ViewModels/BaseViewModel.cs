using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImillReports.ViewModels
{
    public class BaseViewModel
    {
    }

    public class ColumnChooserItem
    {
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public string PageName { get; set; }
        public string UserId { get; set; }
    }
}