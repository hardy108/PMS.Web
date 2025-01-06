using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PMS.Web.Services.Reports
{
    public class ExcelGenerator
    {
        public static XLWorkbook GetExcelWorkbook<T>(List<T> data)
            where T : class
        {
            return GetExcelWorkbook(data, string.Empty, null);

        }

        public static byte[] GetExcelStream<T>(List<T> data)
            where T : class
        {
            return GetExcelStream(data, string.Empty, null);

        }

        public static XLWorkbook GetExcelWorkbook<T>(List<T> rows, string worksheetName, List<string> excludedFieldNames)
            where T : class
        {
            var wb = new XLWorkbook();

            if (rows == null)
                return wb;

            if (rows.Count <= 0)
                return wb;


            if (string.IsNullOrWhiteSpace(worksheetName))
                worksheetName = "data";

            if (excludedFieldNames == null)
                excludedFieldNames = new List<string>();


            var worksheet = wb.Worksheets.Add(worksheetName);
            var currentRow = 1;
            var firstRow = rows[0];


            PropertyInfo[] fields = firstRow.GetType().GetProperties().Where(d => !excludedFieldNames.Contains(d.Name)).ToArray();




            bool[] isText = new bool[fields.Length];

            int colIndex = 1;
            foreach (var field in fields)
            {
                worksheet.Cell(currentRow, colIndex).Value = field.Name;
                if (field.PropertyType.Name == "String")
                    isText[colIndex - 1] = true;
                colIndex++;
            }



            foreach (var row in rows)
            {
                currentRow++;
                colIndex = 1;
                foreach (var field in fields)
                {
                    var valuex = field.GetValue(row);
                    if (valuex != null)
                    {
                        if (isText[colIndex - 1])
                        {
                            worksheet.Cell(currentRow, colIndex).Value = "'" + valuex.ToString();
                        }
                        else
                            worksheet.Cell(currentRow, colIndex).Value = valuex;
                    }
                    colIndex++;
                }
            }


            return wb;
        }

        public static byte[] GetExcelStream<T>(List<T> rows, string worksheetName, List<string> excludedFieldNames)
            where T : class
        {
            using (var stream = new MemoryStream())
            {
                var wb = GetExcelWorkbook(rows, worksheetName, excludedFieldNames);
                wb.SaveAs(stream);
                return stream.ToArray();
            }
        }


    }
}
