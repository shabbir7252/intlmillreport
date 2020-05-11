using ImillReports.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImillReports.Controllers
{
    [Authorize]
    public class SalesmanController : Controller
    {
        private readonly ISalesmanRepository _salesmanRepository;
        public SalesmanController(ISalesmanRepository salesmanRepository)
        {
            _salesmanRepository = salesmanRepository;
        }

        public SalesmanController() { }
        public JsonResult GetSalesmans()
        {
            var salesmanViewModel = _salesmanRepository.GetSalesmans();
            var json = JsonConvert.SerializeObject(salesmanViewModel.SalesmanItems);
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}