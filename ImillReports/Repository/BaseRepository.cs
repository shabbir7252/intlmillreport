using System.IO;
using System.Web;
using Newtonsoft.Json;
using ImillReports.Contracts;
using ImillReports.ViewModels;
using System.Collections.Generic;
using static ImillReports.Repository.LocationRepository;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace ImillReports.Repository
{
    public class BaseRepository : IBaseRepository
    {
        public List<int> GetLocationIds(LocationType locationType)
        {
            if (locationType == LocationType.Coops)
            {
                return new List<int>{
                55, 57, 58, 59, 60, 61, 62, 64, 74, 77, 80 };
            }
            else
            {
                return new List<int>{
                54, 56, 63, 65, 66, 68, 73, 76, 78, 81, 82, 83
                };
            }
        }

        public List<SalesReportType> GetSalesReportType()
        {
            return new List<SalesReportType>
            {
                 new SalesReportType
                {
                    Id = 0,
                    Name = "Select Report Type",
                    NameAr = "Select Report Type"
                },
                new SalesReportType
                {
                    Id = 1,
                    Name = "Amount",
                    NameAr = "Amount"
                },
                new SalesReportType
                {
                    Id = 2,
                    Name = "Transaction Count",
                    NameAr = "Transaction Count"
                }
            };
        }

        private void ReadAndDeleteColumnChooserValue(List<ColumnChooserItem> columnChooserItems, string pageName, string userId, string jsonPath)
        {
            foreach (var item in columnChooserItems)
            {
                var fileStream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read);

                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    var columnChooserFile = streamReader.ReadToEnd();
                    var list = JsonConvert.DeserializeObject<List<ColumnChooserItem>>(columnChooserFile);
                    if (list != null)
                    {
                        foreach (var listItem in list)
                        {
                            if (listItem.FieldName == item.FieldName &&
                                listItem.FieldValue == item.FieldValue &&
                                listItem.PageName == pageName &&
                                listItem.UserId == userId)
                            {
                                list.Remove(item);
                                var convertedJson = JsonConvert.SerializeObject(list, Formatting.Indented);
                                File.WriteAllText(jsonPath, convertedJson);
                            }
                        }
                    }
                }

                fileStream.Close();
                fileStream.Dispose();
            }
        }

        public bool SaveColumnChooser(List<ColumnChooserItem> columnChooserItems, string pageName, string userId)
        {
            if (columnChooserItems == null) return false;

            var jsonPath = HttpContext.Current.Server.MapPath("~/App_Data/column_chooser.json");

            using (var fileStream = new FileStream(jsonPath, FileMode.Open, FileAccess.ReadWrite))
            {
                var streamReader = new StreamReader(fileStream, Encoding.UTF8);
                var columnChooserFile = streamReader.ReadToEnd();
                var list = JsonConvert.DeserializeObject<List<ColumnChooserItem>>(columnChooserFile);
                foreach (var item in columnChooserItems)
                {
                    if (list != null)
                    {
                        foreach (var listItem in list)
                        {
                            if (listItem.FieldName == item.FieldName &&
                                listItem.PageName == pageName &&
                                listItem.UserId == userId)
                            {
                                listItem.FieldValue = item.FieldValue;
                            }
                        }
                    }
                }

                streamReader.Close();
                var convertedJson = JsonConvert.SerializeObject(list, Formatting.Indented);
                File.WriteAllText(jsonPath, convertedJson);
                fileStream.Close();
            }

            //using (var file = File.CreateText(jsonPath))
            //{
            //    var columnChooserItemList = new List<ColumnChooserItem>();
            //    foreach (var item in columnChooserItems)
            //    {
            //        var columnChooserItem = new ColumnChooserItem
            //        {
            //            FieldName = item.FieldName,
            //            FieldValue = item.FieldValue,
            //            PageName = pageName,
            //            UserId = userId
            //        };

            //        columnChooserItemList.Add(columnChooserItem);
            //    }

            //    JsonSerializer serializer = new JsonSerializer();
            //    serializer.Serialize(file, columnChooserItemList);
            //    file.Close();
            //}

            //using (var file = File.CreateText(jsonPath))
            //{
            //    foreach (var item in columnChooserItems)
            //    {
            //        var columnChooserItem = new ColumnChooserItem
            //        {
            //            FieldName = item.FieldName,
            //            FieldValue = item.FieldValue,
            //            PageName = pageName,
            //            UserId = userId
            //        };

            //        columnChooserItemList.Add(columnChooserItem);
            //    }

            //    JsonSerializer serializer = new JsonSerializer();
            //    serializer.Serialize(file, columnChooserItemList);
            //}

            return true;
        }

        public bool CheckDate(string date)
        {
            try
            {
                var dt = DateTime.Parse(date);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}