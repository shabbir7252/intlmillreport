using ImillReports.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImillReports.Controllers
{
    public class PdaController : Controller
    {
        private readonly IPdaRepo _pdaRepo;

        public PdaController(IPdaRepo pdaRepo)
        {
            _pdaRepo = pdaRepo;
        }

        [HttpGet]
        public string ValidateTransDetail()
        {
            return $"{_pdaRepo.ValidateTransDetail()} | {DateTime.Now:dd/MMM/yyyy}";
        }
    }
}