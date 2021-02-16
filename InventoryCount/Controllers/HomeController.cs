using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using InventoryCount.Models;
using InventoryCount.ViewModels;
using Newtonsoft.Json;

namespace InventoryCount.Controllers
{
    public class HomeController : Controller
    {

        public IMILLEntities db = new IMILLEntities();
        // public ImillInvCountEntities _context = new ImillInvCountEntities();

        public ActionResult Index()
        {

            //var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;

            //var con = new SQLiteConnection(cs);
            //con.Open();
            //var cmd = new SQLiteCommand(con);

            //var items = db.ICS_Item.ToList().Select(x => new Item
            //{
            //    PartNumber = x.Part_No,
            //    ProdCd = x.Prod_Cd,
            //    NameEn = x.L_Prod_Name,
            //    NameAr = x.A_Prod_Name,
            //    SalesRate = x.Sales_Rate
            //});

            //foreach (var item in items)
            //{

            //    cmd.CommandText = "INSERT INTO Item(PartNumber, ProdCd, NameEn, NameAr, Sales_Rate) VALUES(" +
            //               "'" + item.PartNumber + "'," +
            //               "'" + item.ProdCd + "'," +
            //               "'" + item.NameEn + "'," +
            //               "'" + item.NameAr + "'," +
            //               "'" + item.SalesRate + "'" +
            //               ")";

            //    cmd.ExecuteNonQuery();
            //}

            ViewBag.Items = GetItems();
            ViewBag.Locations = GetLocations();
            ViewBag.date = DateTime.Now.Date;
            return View();
        }

        [HttpPost]
        public ContentResult GetBarcodeValue(string barcode)
        {
            var source = new ScanResponseViewModel();
            if (!string.IsNullOrEmpty(barcode))
            {
                var identifier = barcode.Substring(0, 2);
                var partNumber = "";
                var checkDigit = "";
                decimal _weight = 0;
                var item = new ItemViewModel();
                item = null;

                if (!string.IsNullOrEmpty(identifier) && int.Parse(identifier) == 90)
                {
                    partNumber = barcode.Substring(2, 4);
                    var weight = barcode.Substring(6, 6);
                    checkDigit = barcode.Substring(12, 1);

                    if (!string.IsNullOrEmpty(weight))
                        _weight = (decimal)((double)int.Parse(weight)/1000);

                    item = GetItems().FirstOrDefault(x => x.PartNo == partNumber);

                    if (item == null)
                        item = GetItems().FirstOrDefault(x => x.PartNo == barcode);
                }

                if (item == null)
                    item = GetItems().FirstOrDefault(x => x.PartNo == barcode);


                if (item == null)
                {
                    source.Oid = 0;
                    source.Weight = _weight;
                    source.CheckDigit = checkDigit;
                    source.Identifier = identifier;
                    source.ItemNameEn = "Not Found";
                    source.ItemNameAr = "Not Found";
                    source.PartNumber = partNumber;
                    source.SalesRate = 0;
                }
                else
                {
                    source.Oid = item.ProdCd;
                    source.Weight = _weight;
                    source.CheckDigit = checkDigit;
                    source.Identifier = identifier;
                    source.ItemNameEn = item.NameEn;
                    source.ItemNameAr = item.NameAr;
                    source.PartNumber = item.PartNo;
                    source.SalesRate = item.SalesRate;
                }
            }

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(source),
                ContentType = "application/json"
            };

            return result;
        }

        [HttpPost]
        public ContentResult SaveItems(string date, string location, List<ItemGridRes> itemList)
        {
            var source = new ItemResponseViewmodel();
            try
            {
                var provider = CultureInfo.InvariantCulture;
                var format = "dd-M-yyyy";
                var today = DateTime.Now;
                var transactionDetails = new List<TransactionDetailVM>();

                var _transNumber = GetTransactionNumber();
                var transNumber = _transNumber != 0 ? _transNumber + 1 : 1;

                var transaction = new TransactionViewModel
                {
                    TransactionNumber = transNumber,
                    TransNum = transNumber.ToString("000"),
                    Location = location
                };

                foreach (var item in itemList)
                {
                    var dbItem = GetItems().FirstOrDefault(x => x.PartNo == item.PartNumber);

                    if (!string.IsNullOrEmpty(date))
                    {
                        transactionDetails.Add(new TransactionDetailVM
                        {
                            SerialNo = item.SerialNo,
                            ItemOid = dbItem != null ? dbItem.ProdCd : 0,
                            ItemNameEn = item.ItemNameEn,
                            ItemNameAr = item.ItemNameAr,
                            TransactionId = transNumber,
                            PartNumber = item.PartNumber,
                            TransDate = (string.IsNullOrEmpty(date)
                                            ? DateTime.ParseExact("01-01-1900", format, provider)
                                            : DateTime.ParseExact(date, format, provider)),
                            Weight = item.Weight,
                            SalesRate = item.SalesRate,
                            Total = item.Weight * item.SalesRate
                        });
                    }
                }

                if (transactionDetails.Any())
                {
                    transaction.ItemCount = transactionDetails.Count();
                    transaction.TransDate = transactionDetails.FirstOrDefault().TransDate;

                    //_context.Transactions.Add(transaction);
                    //_context.TransactionDetails.AddRange(transactionDetails);
                    //_context.SaveChanges();

                    SaveTransaction(transaction);
                    SaveTransactionDetails(transactionDetails);

                    source.ReponseId = 1;
                    source.Message = "Items added successfully!";
                    // source.Message = "تم إرسال الطلب";
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
        public ContentResult PrintTransaction(int oid)
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
                    var transaction = GetTransactions().FirstOrDefault(x => x.TransactionNumber == oid);

                    if (transaction != null)
                    {
                        var transactionDetails = GetTransactionsDetails(oid);
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

        [HttpGet]
        public ActionResult Transactions()
        {
            ViewBag.DataSource = GetTransactions();

            return View();
        }

        [HttpGet]
        public ActionResult TransactionDetails(int oid)
        {
            var transaction = GetTransactions().FirstOrDefault(x => x.TransactionNumber == oid);

            if (transaction != null)
            {
                var transDetails = GetTransactionsDetails(oid);
                if (transDetails.Any())
                {
                    //ViewBag.DataSource = transDetails.Select(x => new TransactionDetailVM
                    //{
                    //    Oid = x.Oid,
                    //    PartNumber = x.PartNumber,
                    //    ItemNameEn = x.ItemNameEn,
                    //    ItemNameAr = x.ItemNameAr,
                    //    Weight = x.Weight
                    //});

                    ViewBag.DataSource = transDetails;

                    ViewBag.TransactionNumber = transaction.TransNum;
                    ViewBag.TransDate = transaction.TransDate.ToShortDateString();
                    ViewBag.Location = transaction.Location;

                    return View();
                }
            }

            return RedirectToAction("Index");
        }

        private List<LocationItem> GetLocations()
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
                var con = new SQLiteConnection(cs);
                con.Open();

                var cmd = new SQLiteCommand(con)
                {
                    CommandText = "select * from Location"
                };

                var rdr = cmd.ExecuteReader();
                var locations = new List<LocationItem>();
                while (rdr.Read())
                {
                    locations.Add(
                        new LocationItem
                        {
                            Name = rdr.GetString(0),
                            NameAr = rdr.GetString(1),
                            LocationId = rdr.GetInt32(2),
                            ShortName = rdr.GetString(3)
                        });
                }

                cmd.Dispose();
                con.Close();
                con.Dispose();

                return locations;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private List<ItemViewModel> GetItems()
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
                var con = new SQLiteConnection(cs);
                con.Open();

                var cmd = new SQLiteCommand(con)
                {
                    CommandText = "select * from Item"
                };

                var rdr = cmd.ExecuteReader();
                var items = new List<ItemViewModel>();
                while (rdr.Read())
                {
                    items.Add(
                        new ItemViewModel
                        {
                            PartNo = rdr.GetString(0),
                            ProdCd = rdr.GetInt32(1),
                            NameEn = rdr.GetString(2),
                            NameAr = rdr.GetString(3),
                            SalesRate = rdr.GetDecimal(4)
                        });
                }

                cmd.Dispose();
                con.Close();
                con.Dispose();

                return items;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private int GetTransactionNumber()
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
                var con = new SQLiteConnection(cs);
                con.Open();

                var cmd = new SQLiteCommand(con)
                {
                    CommandText = "select TransactionNumber from [Transaction] Order by TransactionNumber desc LIMIT 1"
                };

                var rdr = cmd.ExecuteReader();
                var transNum = 0;
                while (rdr.Read())
                {
                    transNum = rdr.GetInt32(0);
                }

                cmd.Dispose();
                con.Close();
                con.Dispose();

                return transNum;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private void SaveTransaction(TransactionViewModel transaction)
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con)
                {
                    CommandText = "INSERT INTO [Transaction](TransactionNumber, TransNum, TransDate, ItemsCount, Location) VALUES(" +
                               "'" + transaction.TransactionNumber + "'," +
                               "'" + transaction.TransNum + "'," +
                               "'" + transaction.TransDate + "'," +
                               "'" + transaction.ItemCount + "'," +
                               "'" + transaction.Location + "'" +
                               ")"
                };

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private void SaveTransactionDetails(List<TransactionDetailVM> transactionDetails)
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con);

                foreach (var item in transactionDetails)
                {
                    cmd.CommandText = "INSERT INTO [TransactionDetail](SerialNo, TransactionId, TransDate, PartNumber, ItemNameEn, ItemNameAr, Weight, SalesRate, Total) VALUES(" +
                                   "'" + item.SerialNo + "'," +
                                   "'" + item.TransactionId + "'," +
                                   "'" + item.TransDate + "'," +
                                   "'" + item.PartNumber + "'," +
                                   "'" + item.ItemNameEn + "'," +
                                   "'" + item.ItemNameAr + "'," +
                                   "'" + item.Weight + "'," +
                                   "'" + item.SalesRate + "'," +
                                   "'" + item.Total + "'" +
                                   ")";

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private List<TransactionViewModel> GetTransactions()
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
                var con = new SQLiteConnection(cs);
                con.Open();

                var cmd = new SQLiteCommand(con)
                {
                    CommandText = "Select * from [Transaction]"
                };

                var rdr = cmd.ExecuteReader();
                var transactions = new List<TransactionViewModel>();
                while (rdr.Read())
                {
                    var locationId = rdr.GetString(6);
                    var location = GetLocation(rdr.GetString(6));
                    var date = DateTime.Parse(rdr.GetString(0));
                    transactions.Add(
                        new TransactionViewModel
                        {
                            TransDate = date,
                            TransactionNumber = rdr.GetInt32(1),
                            ItemCount = rdr.GetInt32(2),
                            TransNum = rdr.GetString(5),
                            Location = location
                        });
                }

                cmd.Dispose();
                con.Close();
                con.Dispose();

                return transactions.OrderByDescending(x => x.TransactionNumber).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private List<TransactionDetailVM> GetTransactionsDetails(int oid)
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
                var con = new SQLiteConnection(cs);
                con.Open();

                var cmd = new SQLiteCommand(con)
                {
                    CommandText = $"Select * from [TransactionDetail] where TransactionId = {oid}"
                };

                var rdr = cmd.ExecuteReader();
                var transactionDetails = new List<TransactionDetailVM>();
                while (rdr.Read())
                {
                    var date = DateTime.Parse(rdr.GetString(1));
                    transactionDetails.Add(
                        new TransactionDetailVM
                        {
                            TransactionId = rdr.GetInt32(0),
                            TransDate = date,
                            PartNumber = rdr.GetString(2),
                            Weight = rdr.GetDecimal(3),
                            ItemNameEn = rdr.GetString(4),
                            ItemNameAr = rdr.GetString(5),
                            SalesRate = rdr.GetDecimal(6),
                            Total = Math.Round(rdr.GetDecimal(7), 3),
                            SerialNo = rdr.GetInt32(8)
                        });
                }

                cmd.Dispose();
                con.Close();
                con.Dispose();

                return transactionDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
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

        private void PrintAndEmail(List<TransactionDetailVM> _, string _1, bool? _2)
        {
            FindAndKillProcess("AcroRd32");

            GeneratePdf();

            var pathName = "~/Content/Print/";
            var path = Path.Combine(Server.MapPath(pathName));
            var fileName = "Material_Requests.pdf";
            Path.Combine(path, fileName);
            //System.IO.File.WriteAllBytes(filePath, pdf);


            // SendEmail(filePath, transNum, isEditedTrans);
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
            var fileName = "InventoryCount.pdf";
            var fullPath = Path.Combine(path, fileName);

            var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            fileStream.Write(byteArray, 0, byteArray.Length);
            fileStream.Close();
        }

        [AllowAnonymous]
        public ActionResult GetTransactionDetails()
        {
            var transDetails = new List<TransactionDetailVM>();

            var jsonPath = Server.MapPath("~/Content/TransactionOid.json");
            string file = System.IO.File.ReadAllText(jsonPath);
            var model = JsonConvert.DeserializeObject<TransactionJsonVM>(file);

            var transaction = GetTransactions().FirstOrDefault(x => x.TransactionNumber == model.TransactionOid);

            if (transaction != null)
            {
                ViewBag.TransactionNumber = transaction.TransNum;
                ViewBag.TransDate = transaction.TransDate.ToString("dd-MM-yyyy");
                ViewBag.Location = transaction.Location;

                transDetails = GetTransactionsDetails(model.TransactionOid);
            }

            return View(transDetails);
        }

        [HttpPost]
        public ContentResult UpdateTransDetail(decimal weight, decimal salesRate, string partNumber, string transNumber, int serialNo)
        {
            var source = new ItemResponseViewmodel();

            try
            {
                var oid = int.Parse(transNumber);
                var total = Math.Round((weight * salesRate), 3);
                
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con)
                {
                    CommandText = $"Update TransactionDetail Set Weight = {weight}, SalesRate = {salesRate}, Total = {total} where PartNumber = {partNumber} and TransactionId = {oid} and SerialNo = {serialNo}"
                };

                cmd.ExecuteNonQuery();

                source.ReponseId = 1;
                source.Message = "Record Updated Successfully!";
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

        [HttpPost]
        public ContentResult DeleteTransDetails(string partNumber, string transNumber, int serialNo)
        {
            var source = new ItemResponseViewmodel();
            try
            {
                var oid = int.Parse(transNumber);
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
                var con = new SQLiteConnection(cs);
                con.Open();
                var cmd = new SQLiteCommand(con)
                {
                    CommandText = $"Delete from TransactionDetail where TransactionId = {oid} and PartNumber = '{partNumber}' and SerialNo = {serialNo}"
                };

                cmd.ExecuteNonQuery();

                source.ReponseId = 1;
                source.Message = "Record Updated Successfully!";
                
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

        private string GetLocation(string id)
        {
            try
            { 
                var locat_Cd = int.Parse(id);
                var location = "";
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
                var con = new SQLiteConnection(cs);
                con.Open();

                var cmd = new SQLiteCommand(con)
                {
                    CommandText = $"Select * from Location Where LocatCd = {locat_Cd} LIMIT 1"
                };

                var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    location = rdr.GetString(0);
                }

                cmd.Dispose();
                con.Close();
                con.Dispose();

                return location;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private List<CRLocation> GetLocalLocations()
        {
            try
            {
                var cs = @ConfigurationManager.ConnectionStrings["slconnection"].ConnectionString;
                var con = new SQLiteConnection(cs);
                con.Open();

                var cmd = new SQLiteCommand(con)
                {
                    CommandText = "select * from CR_Location"
                };

                var rdr = cmd.ExecuteReader();
                var cRLocations = new List<CRLocation>();
                while (rdr.Read())
                {
                    cRLocations.Add(
                        new CRLocation
                        {
                            LocatCd = rdr.GetInt16(0),
                            NameEn = rdr.GetString(1),
                            NameAr = rdr.GetString(2),
                            ShortNameEn = rdr.GetString(3),
                            ShortNameAr = rdr.GetString(4),
                            Pin = rdr.GetInt32(5)
                        });
                }

                cmd.Dispose();
                con.Close();
                con.Dispose();

                return cRLocations;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

    }
}