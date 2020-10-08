using System;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using ItemInventory.Models;
using ItemInventory.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using RandomSolutions;

namespace ItemInventory.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ImillItemInventoryEntities db = new ImillItemInventoryEntities();

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.DataSource = db.Inventories.ToList();
            return View();
        }

        [HttpGet]
        public ActionResult AddItems()
        {
            ViewBag.Items = db.Items.Where(x => (x.NameEn != null || x.NameEn != "") && (x.NameAr != null || x.NameAr != "") && !x.IsDeleted.Value || x.IsDeleted == null);
            ViewBag.Units = db.Units.Where(x => (x.NameEn != null || x.NameEn != "") && (x.NameAr != null || x.NameAr != "") && !x.IsDeleted.Value || x.IsDeleted == null);

            ViewBag.DataSource = new List<ItemViewModel>();

            return View();
        }

        [HttpPost]
        public ContentResult SaveItems(List<ItemViewModel> itemList)
        {
            var source = new ItemResponseViewmodel();
            try
            {
                var provider = CultureInfo.InvariantCulture;
                var format = "dd-M-yyyy";
                var inventory = new List<Inventory>();
                foreach (var item in itemList)
                {
                    var dbItem = db.Items.FirstOrDefault(x => x.Oid == item.ItemId);
                    var dbUnit = db.Units.FirstOrDefault(x => x.Oid == item.UnitId);

                    if (!string.IsNullOrEmpty(item.TransDate) && dbItem != null && dbUnit != null)
                    {
                        inventory.Add(new Inventory
                        {
                            CreatedBy = User.Identity.Name,
                            CreatedOn = DateTime.Now,
                            ItemOid = dbItem.Oid,
                            ItemName = dbItem.NameEn,
                            Quantity = item.Quantity,
                            UnitOid = dbUnit.Oid,
                            UnitName = dbUnit.NameEn,
                            TransDate = (string.IsNullOrEmpty(item.TransDate)
                                            ? DateTime.ParseExact("01-01-1900", format, provider)
                                            : DateTime.ParseExact(item.TransDate, format, provider)).AddMonths(1)
                        });
                    }
                }

                if (inventory.Any())
                {
                    db.Inventories.AddRange(inventory);
                    db.SaveChanges();

                    source.ReponseId = 1;
                    source.Message = "Items added successfully!";

                    var pdf = inventory.ToPdf(scheme =>
                    {
                        scheme.Title = "Item_Inventory";
                        scheme.PageOrientation = ArrayToPdfOrientations.Portrait;
                        scheme.PageFormat = ArrayToPdfFormats.A4;
                        scheme.AddColumn("Trans Date", x => x.TransDate.ToString("dd-MM-yyyy"));
                        scheme.AddColumn("Item Name", x => x.ItemName);
                        scheme.AddColumn("Unit Name", x => x.UnitName);
                        scheme.AddColumn("Quantity", x => x.Quantity);
                    });

                    string path_name = "~/App_Data/Print/";
                    var pdfPath = Path.Combine(Server.MapPath(path_name));
                    // var formFieldMap = PDFHelper.GetFormFieldNames(pdfPath);
                    string file_name_pdf = "Item_Inventory.pdf";

                    System.IO.File.WriteAllBytes(Path.Combine(pdfPath, file_name_pdf), pdf);
                }
                else
                {
                    source.ReponseId = 0;
                    source.Message = "No Item were added in system";
                }
            }
            catch (Exception ex)
            {
                source.ReponseId = 2;
                source.Message = $"Error occurred. {ex.Message}";
            }

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

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
                    var inventories = db.Inventories.Where(x => verifiedIds.Contains(x.Oid));
                    db.Inventories.RemoveRange(inventories);
                    db.SaveChanges();
                    source.ReponseId = 1;
                    source.Message = "Record Deleted Successfully!";
                }
            }
            catch (Exception)
            {
                source.ReponseId = 2;
                source.Message = "Error occurred while Deleting";
            }
            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpGet]
        public ActionResult PrintItems(List<ItemViewModel> itemList)
        {
            string path_name = "~/App_Data/Print/";
            var pdfPath = Path.Combine(Server.MapPath(path_name));
            string[] files = Directory.GetFiles(pdfPath);
            foreach (string file in files)
            {
                PrintPDFs(file);
            }
            return RedirectToAction("Index");
        }

        public static Boolean PrintPDFs(string pdfFileName)
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo.Verb = "print";

                //Get application path will get default application for given file type ("pdf")
                //This will allow you to not care if its adobe reader 10 or adobe acrobat.
                proc.StartInfo.FileName = "C:\\Program Files (x86)\\Adobe\\Acrobat Reader DC\\Reader\\AcroRd32.exe";
                proc.StartInfo.Arguments = string.Format(@"/p /h {0}", pdfFileName);
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;

                proc.Start();
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                if (proc.HasExited == false)
                {
                    proc.WaitForExit(10000);
                }

                proc.EnableRaisingEvents = true;

                proc.Close();
                FindAndKillProcess("AcroRd32");
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static bool FindAndKillProcess(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.StartsWith(name))
                {
                    clsProcess.Kill();
                    return true;
                }
            }
            return false;
        }

        protected void save_pdf()
        {
            string path_name = "~/App_Data/Print/";
            var pdfPath = Path.Combine(Server.MapPath(path_name));
            var formFieldMap = PDFHelper.GetFormFieldNames(pdfPath);

            //string username = "Test";
            //string password = "12345";
            string file_name_pdf = "Test.pdf";

            var pdfContents = PDFHelper.GeneratePDF(pdfPath, formFieldMap);

            System.IO.File.WriteAllBytes(Path.Combine(pdfPath, file_name_pdf), pdfContents);

            WebRequest request = WebRequest.Create(Server.MapPath("~/App_Data/Print/" + pdfContents));
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // request.Credentials = new NetworkCredential(username, password);
            Stream reqStream = request.GetRequestStream();
            reqStream.Close();
        }
    }
}