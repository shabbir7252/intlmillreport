using System;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using Cash_Register.Contracts;

namespace Cash_Register.Controllers
{
    public class HomeController : Controller
    {
        private readonly DateTime sessionExpiryPeriod = DateTime.Now.AddDays(60);
        private readonly ILocationRepository _locationRepository;
        
        public HomeController(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }
        [HttpGet]
        public ActionResult Index()
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
            return View();
        }

        [HttpPost]
        public string Index(int? pin)
        {
            ViewBag.emptyPin = false;
            if (pin == null)
            {
                // ViewBag.emptyPin = true;
                return "Pin is empty";
            }

            var locations = _locationRepository.GetLocations();

            if (!locations.Any(x => x.Pin == pin))
            {
                var message = "Pin does not match. Please try again!";

                Response.Cookies["login"].Expires = DateTime.Now.AddDays(-1);
                Response.Cookies["CRLocationEn"].Expires = DateTime.Now.AddDays(-1);
                Response.Cookies["CRLocationAr"].Expires = DateTime.Now.AddDays(-1);
                Response.Cookies["locationId"].Expires = DateTime.Now.AddDays(-1);
                return message;
            }

            var loginCookie = new HttpCookie("login")
            {
                Value = "true",
                Expires = sessionExpiryPeriod
            };
            Response.Cookies.Add(loginCookie);

            var location = locations.FirstOrDefault(x => x.Pin == pin);

            Response.Cookies["locationId"].Value = location.LocatCd.ToString();
            Response.Cookies["locationId"].Expires = sessionExpiryPeriod;
            
            Response.Cookies["CRLocationEn"].Value = location.NameEn;
            Response.Cookies["CRLocationEn"].Expires = sessionExpiryPeriod;

            Response.Cookies["CRLocationAr"].Value = Server.UrlEncode(locations.FirstOrDefault(x => x.Pin == pin).NameAr);
            Response.Cookies["CRLocationAr"].Expires = sessionExpiryPeriod;
            
            return "success";
            // return RedirectToAction("Index", "CashRegister");
        }

        public ActionResult logoff()
        {
            Response.Cookies["login"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["CRLocationEn"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["CRLocationAr"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["locationId"].Expires = DateTime.Now.AddDays(-1);
            TempData["Message"] = "You have successfully logged out";
            return RedirectToAction("Index");
        }
    }
}