using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Web.Mvc;
using RandomSolutions;
using System.Net.Mail;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using ItemInventory.Models;
using ItemInventory.ViewModels;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Web;

namespace ItemInventory.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ImillItemInventoryEntities db = new ImillItemInventoryEntities();
        public IMILLEntities _context = new IMILLEntities();

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.DataSource = db.Transactions.Where(x => !x.IsDeleted).Select(x => new TransactionViewModel
            {
                Oid = x.Oid,
                ItemCount = x.ItemsCount,
                RequestedBy = db.Users.FirstOrDefault(a => a.Oid == x.RequestedBy).FullnameAr,
                TransactionNumber = x.TransactionNumber,
                TransNum = x.TransNum,
                TransDate = x.TransDate
            }).ToList();

            return View();
        }

        [HttpGet]
        public ActionResult TransactionDetails(int oid)
        {
            var transaction = db.Transactions.FirstOrDefault(x => x.Oid == oid);

            if (transaction != null)
            {
                var transDetails = db.TransactionDetails.Where(x => x.TransactionId == transaction.Oid).ToList();
                if (transDetails.Any())
                {
                    ViewBag.DataSource = transDetails.Select(x => new TransactionDetailVM
                    {
                        Oid = x.Oid,
                        ItemId = x.ItemOid,
                        ItemNameEn = x.ItemNameEn,
                        ItemNameAr = x.ItemNameAr,
                        UnitId = x.UnitOid,
                        UnitNameEn = x.UnitNameEn,
                        UnitNameAr = x.UnitNameAr,
                        Quantity = x.Quantity,
                        TransactionNumber = x.TransactionNumber
                    });

                    ViewBag.TransactionNumber = transaction.TransNum;
                    ViewBag.TransDate = transaction.TransDate.ToShortDateString();
                    ViewBag.Comments = transaction.Comments;


                    var partNumbers = db.ItemPartNumbers.Select(x => x.PartNumber).ToList();

                    var items = _context.ICS_Item.Where(x => partNumbers.Contains(x.Part_No)).Select(a => new ItemsViewModel
                    {
                        Oid = a.Prod_Cd,
                        NameAr = a.A_Prod_Name,
                        NameEn = a.L_Prod_Name,
                        PartNumber = a.Part_No,
                        Unit_Cd = a.Base_Unit_Cd
                    }).ToList();

                    ViewBag.Items = items;

                    var units = db.Units.Select(x => new UnitViewModel
                    {
                        Oid = x.Oid,
                        NameAr = x.NameAr,
                        NameEn = x.NameEn
                    }).ToList();

                    ViewBag.Units = units;



                    return View();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult AddItems()
        {
            var partNumbers = db.ItemPartNumbers.Select(x => x.PartNumber).ToList();

            var items = _context.ICS_Item.Where(x => partNumbers.Contains(x.Part_No)).Select(a => new ItemsViewModel
            {
                Oid = a.Prod_Cd,
                NameAr = a.A_Prod_Name,
                NameEn = a.L_Prod_Name,
                PartNumber = a.Part_No,
                Unit_Cd = a.Base_Unit_Cd
            }).ToList();

            ViewBag.Items = items;

            //var units = _context.ICS_Unit.Select(x => new UnitViewModel
            //{
            //    Oid = x.Unit_Cd,
            //    NameAr = x.A_Unit_Name,
            //    NameEn = x.L_Unit_Name
            //}).ToList();

            var units = db.Units.Where(x => x.IsDeleted.HasValue && !x.IsDeleted.Value);

            ViewBag.Units = units;

            ViewBag.Users = db.Users.Where(x => !x.IsDeleted && x.IsActiveDropDown).ToList();

            ViewBag.DataSource = new List<ItemViewModel>();

            ViewBag.TransDate = DateTime.Now.Date;

            return View();
        }

        public ContentResult UpdateTransaction(List<TransactionDetailVM> transactionDetailVM)
        {
            var source = new ItemResponseViewmodel();
            var today = DateTime.Now;

            try
            {
                var transOid = 0;

                if (transactionDetailVM.Any())
                {
                    foreach (var transDtls in transactionDetailVM)
                    {
                        var transDetails = db.TransactionDetails.FirstOrDefault(x => x.Oid == transDtls.Oid);
                        var item = _context.ICS_Item.FirstOrDefault(x => x.Prod_Cd == transDtls.ItemId);
                        var unit = db.Units.FirstOrDefault(x => x.Oid == transDtls.UnitId);

                        if (transDetails != null)
                        {
                            transDetails.ItemOid = item.Prod_Cd;
                            transDetails.ItemNameEn = item.L_Prod_Name;
                            transDetails.ItemNameAr = item.A_Prod_Name;
                            transDetails.UnitOid = unit.Oid;
                            transDetails.UnitNameEn = unit.NameEn;
                            transDetails.UnitNameAr = unit.NameAr;
                            transDetails.Quantity = transDtls.Quantity;
                            transDetails.UpdatedOn = today;
                            transDetails.UpdatedBy = User.Identity.Name;
                        }

                        transOid = transDetails.TransactionId;
                    }

                    var transaction = db.Transactions.FirstOrDefault(x => x.Oid == transOid);

                    transaction.UpdatedOn = today;
                    transaction.UpdatedBy = User.Identity.Name;

                    db.SaveChanges();

                    var transactionDetails = db.TransactionDetails.Where(x => x.TransactionId == transaction.Oid).ToList();
                    CreateJson(transaction.Oid);
                    PrintAndEmail(transactionDetails, transaction.TransNum, true);

                    source.ReponseId = 1;
                    source.Message = "Item Updated successfully!";

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

        [HttpPost]
        public ContentResult SaveItems(List<ItemViewModel> itemList, int user, string comments)
        {
            var source = new ItemResponseViewmodel();
            try
            {
                // var timeStamp = HiResDateTime.UtcNowTicks;
                var provider = CultureInfo.InvariantCulture;
                var format = "dd-M-yyyy";
                var today = DateTime.Now;
                var transactionDetails = new List<TransactionDetail>();

                var _transNumber = db.Transactions.OrderByDescending(x => x.TransactionNumber).FirstOrDefault()?.TransactionNumber;
                var transNumber = _transNumber.HasValue ? _transNumber.Value + 1 : 1;
                var transaction = new Transaction
                {
                    TransactionNumber = transNumber,
                    TransNum = transNumber.ToString("000")
                };

                foreach (var item in itemList)
                {
                    var dbItem = _context.ICS_Item.FirstOrDefault(x => x.Prod_Cd == item.ItemId);
                    var dbUnit = db.Units.FirstOrDefault(x => x.Oid == item.UnitId);

                    if (!string.IsNullOrEmpty(item.TransDate) && dbItem != null && dbUnit != null && user != 0)
                    {
                        transactionDetails.Add(new TransactionDetail
                        {
                            CreatedBy = User.Identity.Name,
                            CreatedOn = today,
                            ItemOid = dbItem.Prod_Cd,
                            ItemNameEn = dbItem.L_Prod_Name,
                            ItemNameAr = dbItem.A_Prod_Name,
                            Quantity = item.Quantity,
                            UnitOid = dbUnit.Oid,
                            UnitNameEn = dbUnit.NameEn,
                            UnitNameAr = dbUnit.NameAr,
                            TransDate = (string.IsNullOrEmpty(item.TransDate)
                                            ? DateTime.ParseExact("01-01-1900", format, provider)
                                            : DateTime.ParseExact(item.TransDate, format, provider)),
                            TransactionNumber = transNumber,
                            Transaction = transaction,
                            TransactionId = transaction.Oid
                        });
                    }
                }

                if (transactionDetails.Any())
                {
                    transaction.ItemsCount = transactionDetails.Count();
                    transaction.Comments = comments;
                    transaction.RequestedBy = user;
                    transaction.CreatedBy = User.Identity.Name;
                    transaction.CreatedOn = today;
                    transaction.TransDate = transactionDetails.FirstOrDefault().TransDate;

                    db.Transactions.Add(transaction);
                    db.TransactionDetails.AddRange(transactionDetails);
                    db.SaveChanges();

                    source.ReponseId = 1;
                    // source.Message = "Items added successfully!";
                    source.Message = "تم إرسال الطلب";

                    CreateJson(transaction.Oid);
                    PrintAndEmail(transactionDetails, transaction.TransNum, false);
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

        [HttpPost]
        public ContentResult PrintInventory(int oid)
        {
            var status = true;
            var message = "Print Successfull";
            try
            {
                if (oid <= 0)
                {
                    status = false;
                    message = "Id Not Found!";
                }
                else
                {
                    var transaction = db.Transactions.FirstOrDefault(x => x.Oid == oid);

                    if (transaction != null)
                    {
                        var transactionDetails = db.TransactionDetails.Where(x => x.TransactionId == transaction.Oid).ToList();
                        CreateJson(oid);
                        PrintAndEmail(transactionDetails, transaction.TransNum, false);
                    }
                }
            }
            catch (Exception ex)
            {
                status = false;
                message = ex.Message;
            }

            var generalMessage = new GeneralMessage
            {
                Status = status,
                Message = message
            };

            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(generalMessage),
                ContentType = "application/json"
            };
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
                    var transactions = db.Transactions.Where(x => verifiedIds.Contains(x.Oid));
                    foreach (var trans in transactions)
                        trans.IsDeleted = true;

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
            string path_name = "~/Content/Print/";
            var pdfPath = Path.Combine(Server.MapPath(path_name));
            string[] files = Directory.GetFiles(pdfPath);
            foreach (string file in files)
                PrintPDFs(file);

            return RedirectToAction("Index");
        }

        public static bool PrintPDFs(string pdfFileName)
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

        private GeneralMessage SendEmail(string filePath, string transNum, bool? isEditedTrans)
        {
            try
            {
                var users = db.Users.Where(x => !x.IsDeleted && x.IsRegForEmail);
                if (users.Any())
                {

                    if (string.IsNullOrEmpty(filePath))
                        return new GeneralMessage
                        {
                            Status = false,
                            Message = "File path is empty"
                        };

                    var jsonPath = Server.MapPath("~/Content/configuration.json");
                    var file = System.IO.File.ReadAllText(jsonPath);
                    var config = JsonConvert.DeserializeObject<Configuration>(file);

                    if (config == null) return new GeneralMessage
                    {
                        Status = false,
                        Message = "No Configuration Found"
                    };

                    MailMessage mailMessage = new MailMessage
                    {
                        Subject = $"{config.Subject} - Transaction Number : {transNum}",
                        Body = isEditedTrans.HasValue && isEditedTrans.Value ? $"Your order reference {transNum} has been revised. " + config.Body : config.Body,
                        From = new MailAddress(config.FromAddress),
                    };

                    var newAttachment = new Attachment(filePath);
                    mailMessage.Attachments.Add(newAttachment);

                    foreach (var user in users)
                    {
                        mailMessage.To.Add(new MailAddress(user.Email));
                    }

                    SmtpClient smtp = new SmtpClient
                    {
                        Host = config.Host,
                        Port = config.Port,
                        EnableSsl = config.EnableSsl,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(config.FromAddress, config.Password)
                    };

                    smtp.Send(mailMessage);

                    newAttachment.Dispose();

                    return new GeneralMessage
                    {
                        Status = true,
                        Message = "Email Send Successfully"
                    };

                }

                return new GeneralMessage
                {
                    Status = false,
                    Message = "No User Found To Send Email"
                };

            }
            catch (Exception ex)
            {
                return new GeneralMessage
                {
                    Status = true,
                    Message = ex.Message,
                };
            }
        }

        private void PrintAndEmail(List<TransactionDetail> transDetails, string transNum, bool? isEditedTrans)
        {
            FindAndKillProcess("AcroRd32");

            GeneratePdf();

            //var pdf = transDetails.ToPdf(scheme =>
            //{
            //    scheme.Header = $"Transaction Number : {transNumber} \n Requested By : Shabbir \n Comments : {comments}";
            //    scheme.HeaderHeight = 15;
            //    scheme.PageOrientation = ArrayToPdfOrientations.Portrait;
            //    scheme.PageFormat = ArrayToPdfFormats.A4;
            //    scheme.AddColumn("Trans Date", x => x.TransDate.ToString("dd-MM-yyyy"));
            //    scheme.AddColumn("Item Name", x => x.ItemNameEn);
            //    scheme.AddColumn("Unit Name", x => x.UnitNameEn);
            //    scheme.AddColumn("Quantity", x => x.Quantity);
            //});

            var pathName = "~/Content/Print/";
            var path = Path.Combine(Server.MapPath(pathName));
            var fileName = "Material_Requests.pdf";
            var filePath = Path.Combine(path, fileName);
            //System.IO.File.WriteAllBytes(filePath, pdf);


            SendEmail(filePath, transNum, isEditedTrans);
        }

        [AllowAnonymous]
        public ActionResult GetTransactionDetails()
        {
            var transDetails = new List<TransactionDetail>();

            var jsonPath = Server.MapPath("~/Content/TransactionOid.json");
            string file = System.IO.File.ReadAllText(jsonPath);
            var model = JsonConvert.DeserializeObject<TransactionJsonVM>(file);

            var transaction = db.Transactions.FirstOrDefault(x => x.Oid == model.TransactionOid);

            if (transaction != null)
            {
                ViewBag.TransactionNumber = transaction.TransNum;
                ViewBag.RequestedBy = db.Users.FirstOrDefault(x => x.Oid == transaction.RequestedBy).FullnameAr;
                ViewBag.Comments = transaction.Comments;
                ViewBag.TransDate = transaction.TransDate.ToString("dd-MM-yyyy");

                transDetails = db.TransactionDetails.Where(x => x.TransactionId == transaction.Oid).ToList();
            }

            return View(transDetails);
        }

        [AllowAnonymous]
        public void GeneratePdf()
        {
            var actionResult = new Rotativa.ActionAsPdf("GetTransactionDetails")
            {
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                IsLowQuality = false
            };
            var byteArray = actionResult.BuildPdf(ControllerContext);
            var pathName = "~/Content/Print/";
            var path = Path.Combine(Server.MapPath(pathName));
            var fileName = "Material_Requests.pdf";
            var fullPath = Path.Combine(path, fileName);

            var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            fileStream.Write(byteArray, 0, byteArray.Length);
            fileStream.Close();
        }

        public void CreateJson(int oid)
        {
            var jsonPath = Server.MapPath("~/Content/TransactionOid.json");
            var transactionJsonVM = new TransactionJsonVM
            {
                TransactionOid = oid
            };

            var convertedJson = JsonConvert.SerializeObject(transactionJsonVM, Formatting.Indented);
            System.IO.File.WriteAllText(jsonPath, convertedJson);
        }
    }
}