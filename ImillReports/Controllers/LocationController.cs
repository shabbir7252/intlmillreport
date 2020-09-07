using System.Web.Mvc;
using Newtonsoft.Json;
using ImillReports.Contracts;

namespace ImillReports.Controllers
{
    [Authorize]
    public class LocationController : Controller
    {
        private readonly ILocationRepository _locationRepository;
        public LocationController(ILocationRepository locationRepository) =>
            _locationRepository = locationRepository;

        public LocationController() { }

        public JsonResult GetLocations()
        {
            var locationViewModel = _locationRepository.GetLocations();
            var json = JsonConvert.SerializeObject(locationViewModel.LocationItems);
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}