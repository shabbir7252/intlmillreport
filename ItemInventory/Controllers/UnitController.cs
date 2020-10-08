using System;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using ItemInventory.Models;
using ItemInventory.ViewModels;
using System.Collections.Generic;

namespace ItemInventory.Controllers
{
    public class UnitController : Controller
    {
        public ImillItemInventoryEntities db = new ImillItemInventoryEntities();

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.DataSource = db.Units.Where(x => !x.IsDeleted.Value || x.IsDeleted == null).ToList();
            return View();
        }

        [HttpPost]
        public ContentResult SaveUnit(string nameEn, string nameAr)
        {
            var source = new ItemResponseViewmodel();
            try
            {
                if (string.IsNullOrEmpty(nameEn) || string.IsNullOrEmpty(nameAr))
                {
                    source.ReponseId = 0;
                    source.Message = "Name is invalid or empty";
                }
                else
                {
                    var unit = new Unit
                    {
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now,
                        NameEn = nameEn,
                        NameAr = nameAr,
                        IsDeleted = false
                    };

                    db.Units.Add(unit);
                    db.SaveChanges();

                    source.ReponseId = 1;
                    source.Message = "Record Saved Successfully!";
                }
            }
            catch (Exception)
            {
                source.ReponseId = 2;
                source.Message = "Error occurred while saving";
            }

            var result = new ContentResult
            {

                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpPost]
        public ContentResult Delete(List<int> verifiedIds)
        {
            var source = new ItemResponseViewmodel();

            try
            {
                if (!verifiedIds.Any())
                {
                    source.ReponseId = 0;
                    source.Message = "Selection is Empty";
                }
                else
                {
                    var units = db.Units.Where(x => verifiedIds.Contains(x.Oid));
                    foreach (var rec in units)
                        rec.IsDeleted = true;

                    db.SaveChanges();
                    source.ReponseId = 1;
                    source.Message = "Record Deleted Successfully!";
                }
            }
            catch (Exception)
            {
                source.ReponseId = 2;
                source.Message = "Error occurred while deleting";
            }
            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }
    }
}