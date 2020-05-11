using IntlmillReports.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntlmillReports.Controllers
{
    [Authorize]
    public class LocationController : Controller
    {
        private readonly ILocationRepository _locationRepository;
        public LocationController(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public LocationController() { }

        public JsonResult GetLocations()
        {
            var locationViewModel = _locationRepository.GetLocations();
            var json = JsonConvert.SerializeObject(locationViewModel.LocationItems);
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}