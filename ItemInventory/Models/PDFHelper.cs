﻿using iTextSharp.text.pdf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ItemInventory.Models
{
    public class PDFHelper
    {
        public static Dictionary<string, string> GetFormFieldNames(string pdfPath)
        {
            var fields = new Dictionary<string, string>();

            var reader = new PdfReader(pdfPath);
            foreach(var entry in reader.AcroFields.Fields)
            {
                fields.Add(entry.Key.ToString(), string.Empty);
            }

            reader.Close();

            return fields;
        }

        public static byte[] GeneratePDF(string pdfPath, Dictionary<string, string> formFieldMap)
        {
            var output = new MemoryStream();
            var reader = new PdfReader(pdfPath);
            var stamper = new PdfStamper(reader, output);
            var formFields = stamper.AcroFields;

            foreach (var fieldName in formFieldMap.Keys)
                formFields.SetField(fieldName, formFieldMap[fieldName]);

            stamper.FormFlattening = true;
            stamper.Close();
            reader.Close();

            return output.ToArray();
        }


        public static string GetExportValue(AcroFields.Item item)
        {
            var valueDict = item.GetValue(0);
            var appearanceDict = valueDict.GetAsDict(PdfName.AP);

            if (appearanceDict != null)
            {
                var normalAppearances = appearanceDict.GetAsDict(PdfName.N);
                if (normalAppearances != null)
                {
                    foreach (var curKey in normalAppearances.Keys)
                        if (!PdfName.OFF.Equals(curKey))
                            return curKey.ToString().Substring(1); // string will have a leading '/' character, so remove it!
                }
            }

            var curVal = valueDict.GetAsName(PdfName.AS);
            if (curVal != null)
                return curVal.ToString().Substring(1);
            else
                return string.Empty;
        }

        public static void ReturnPDF(byte[] contents)
        {
            ReturnPDF(contents, null);
        }

        public static void ReturnPDF(byte[] contents, string attachmentFilename)
        {
            var response = HttpContext.Current.Response;

            if (!string.IsNullOrEmpty(attachmentFilename))
                response.AddHeader("Content-Disposition", "attachment; filename=" + attachmentFilename);

            response.ContentType = "application/pdf";
            response.BinaryWrite(contents);
            response.End();
        }
    }
}