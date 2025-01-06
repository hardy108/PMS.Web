using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PMS.Web.UI.Code
{
    public static class JsHelper
    {
        public static string CurrentDate(string format)
        {
            try
            {
                return DateTime.Now.ToString(format);
            }
            catch { return string.Empty; }
        }
        public static string CurrentMonthStart(string format)
        {
            try
            {
                return DateTime.Now.AddDays(1 - DateTime.Today.Day).ToString(format);
            }
            catch { return string.Empty; }
        }
        public static string CurrentYearStart(string format)
        {
            try
            {
                DateTime date = DateTime.Now.AddDays(1 - DateTime.Today.Day);
                return date.AddMonths(1 - date.Month).ToString(format);
            }
            catch { return string.Empty; }
        }

        public static string GetDefaultFilterString(string menuID)
        {
            switch (menuID)
            {
                case "APPRVINBOX":
                    return "DocType In ('EMPREGIST','EMPCHANGE')";
                default:
                    return "1=2";
            }
        }

    }

    public static class GeneralHelpers
    {
        

        public static string BooleanStringHtml(bool value)
        {
            if (value)
                return "true";
            return "false";
        }

    }

    
        
}