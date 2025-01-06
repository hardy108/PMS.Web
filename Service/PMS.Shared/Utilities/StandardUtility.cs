using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;

namespace PMS.Shared.Utilities
{
    public class StandardUtility
    {
        #region Compression
        public static string Compress(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream();
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;
            MemoryStream outStream = new MemoryStream();

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return Convert.ToBase64String(gzBuffer);
        }
        public static string Decompress(string compressedText)
        {
            byte[] gzBuffer = Convert.FromBase64String(compressedText);
            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }
        #endregion

        public static string RomanMonth(DateTime date)
        {
            switch (date.Month)
            {
                case 1:
                    return "I";
                case 2:
                    return "II";
                case 3:
                    return "III";
                case 4:
                    return "IV";
                case 5:
                    return "V";
                case 6:
                    return "VI";
                case 7:
                    return "VII";
                case 8:
                    return "VIII";
                case 9:
                    return "IX";
                case 10:
                    return "X";
                case 11:
                    return "XI";
                case 12:
                    return "XII";
                default:
                    return string.Empty;
            }

        }

        public static T IsNull<T>(T value, T defaultValue)
        {
            if (value == null)
                return defaultValue;
            return value;
        }

        public static bool IsValidEmail(string source)
        {
            return new EmailAddressAttribute().IsValid(source);
        }


        public static string GetRecordStatusDescription(string status)
        {
            switch (status)
            {
                case "A":
                    return "Approved";
                case "C":
                    return "Canceled";
                case "P":
                    return "Proces";
                case "D":
                    return "Deleted";
                default:
                    return status;
            }
        }


        public static IEnumerable<T> GetNewItems<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems)
        {
            if (oldItems == null || !oldItems.Any())
                return newItems;
            return newItems.Where(o => !oldItems.Contains(o));
        }

        public static IEnumerable<T> GetDeletedItems<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems)
        {
            if (newItems == null || !newItems.Any())
                return oldItems;
            return oldItems.Where(o => !newItems.Contains(o));            
        }

        public static IEnumerable<T> GetUpdatedItems<T>(IEnumerable<T> newItems, IEnumerable<T> existingItems)
        {
            if (newItems == null || !newItems.Any() || existingItems == null || !existingItems.Any())
                return new List<T>();

            List<T> updatedItems =
            (
                from a in newItems
                join b in existingItems on a equals b
                select a
            ).ToList();

            return updatedItems;
        }

        public static List<T> GetUpdatedItems<T>(List<T> newItems, List<T> existingItems, out List<T> insertedItems, out List<T> deletedItems)
        {
            insertedItems = new List<T>();
            deletedItems = new List<T>();
            if (newItems == null || !newItems.Any())
            {
                deletedItems = existingItems;
                return new List<T>();
            }

            if (existingItems == null || !existingItems.Any())
            {
                insertedItems = newItems;
                return new List<T>();
            }

            List<T> updatedItems =
            (
                from a in newItems
                join b in existingItems on a equals b
                select a
            ).ToList();

            insertedItems = newItems.Where(d => !updatedItems.Contains(d)).ToList();
            deletedItems = existingItems.Where(d => !updatedItems.Contains(d)).ToList();
            return updatedItems;
        }

        public static void NullDateRangeToday(ref DateTime startDate, ref DateTime endDate )
        {
            DateTime nullDate = new DateTime();
            if (startDate == nullDate && endDate == nullDate)
            {
                startDate = DateTime.Today;
                endDate = DateTime.Today;
            }
            else if (startDate == nullDate)
                startDate = endDate;
            else if (endDate == nullDate)
                endDate = startDate;
        }

        public static void NullDateRangeThisMonth(ref DateTime startDate, ref DateTime endDate)
        {
            DateTime nullDate = new DateTime();
            if (startDate == nullDate && endDate == nullDate)
            {
                startDate = DateTime.Today.AddDays(1 - DateTime.Today.Day);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }
            else if (startDate == nullDate)
                startDate = endDate;
            else if (endDate == nullDate)
                endDate = startDate;
        }

        public static void NullDateRangeThisYear(ref DateTime startDate, ref DateTime endDate)
        {
            DateTime nullDate = new DateTime();
            if (startDate == nullDate && endDate == nullDate)
            {
                startDate = new DateTime(DateTime.Today.Year, 1, 1);
                endDate = new DateTime(DateTime.Today.Year, 12, 31);
            }
            else if (startDate == nullDate)
                startDate = endDate;
            else if (endDate == nullDate)
                endDate = startDate;
        }

        public static void NullDateRange(ref DateTime startDate, ref DateTime endDate)
        {
            DateTime nullDate = new DateTime();
            if (startDate == nullDate && endDate == nullDate)
            {
                startDate = new DateTime(1900,1,1);
                endDate = new DateTime(9999, 12, 31);
            }
            else if (startDate == nullDate)
                startDate = endDate;
            else if (endDate == nullDate)
                endDate = startDate;
        }

        public static bool DateBetween(DateTime check,DateTime min, DateTime max)
        {
            if (max<min)
            {
                DateTime swap = max;
                max = min;
                min = swap;
            }
            if (check < min || check > max)
                return false;
            return true;
        }

        #region Conversion
        public const string DecimalFormat = "{0:0.00}";
        public const string DateFormat = "MM/dd/yyyy";
        public const string DateFormatDB = "yyyy-MM-dd";
        public const string TimeFormat = "HH:mm";
        public const string TimeFormatLong = "HH:mm:ss";
        public static string FormatDecimal(object value)
        {
            return string.Format(DecimalFormat, value);
        }
        public static bool IsDate(object value)
        {
            if (value == null) return false;
            if (value.ToString().Trim() == string.Empty) return false;
            try
            {
                DateTime tmp = DateTime.Parse(value.ToString());
                return true;
            }
            catch
            { return false; }
        }
        public static bool IsNumeric(string value)
        {

            try
            {
                decimal val = Convert.ToDecimal(value);
                return true;
            }
            catch { return false; }
        }
        public static T NullValue<T>(T value, T newValue)
        {
            if (value == null)
                return (newValue);
            return (value);

        }
        public static int StringToInt(string value)
        {
            try
            {
                return (Convert.ToInt32(value));
            }
            catch { return (0); }
        }
        public static int StringToInt(string value, out bool isError)
        {
            isError = false;
            try
            {
                return (Convert.ToInt32(value));
            }
            catch
            {
                isError = true;
                return (0);
            }
        }

        public static long StringToLong(string value)
        {
            try
            {
                return (Convert.ToInt64(value));
            }
            catch { return (0); }
        }
        public static long StringToLong(string value, out bool isError)
        {
            isError = false;
            try
            {
                return (Convert.ToInt64(value));
            }
            catch
            {
                isError = true;
                return (0);
            }
        }
        public static decimal ToDecimal(object obj)
        {
            decimal result = 0;
            try
            {
                result = Convert.ToDecimal(obj);
            }
            catch { }
            return result;
        }
        public static double ToDouble(object obj)
        {
            double result = 0;
            try
            {
                result = Convert.ToDouble(obj);
            }
            catch { }
            return result;
        }
        public static long ToLong(object obj)
        {
            long result = 0;
            try
            {
                result = Convert.ToInt64(obj);
            }
            catch { }
            return result;
        }
        public static int ToInt(object obj)
        {

            int result = 0;
            try
            {
                result = Convert.ToInt32(obj);
            }
            catch { }
            return result;
        }
        public static DateTime ToDateTime(object obj)
        {
            DateTime result = DateTime.Now;
            try
            {
                result = Convert.ToDateTime(obj);
            }
            catch { }
            return result;
        }

        public static double StringToNumeric(string value)
        {
            try
            {
                return (Convert.ToDouble(value));
            }
            catch { return (0); }
        }
        public static double StringToNumeric(string value, out bool isError)
        {
            isError = false;
            try
            {
                return (Convert.ToDouble(value));
            }
            catch
            {
                isError = true;
                return (0);
            }
        }

        public static decimal StringToDecimal(string value)
        {
            try
            {
                return (Convert.ToDecimal(value));
            }
            catch { return (0); }
        }
        public static decimal StringToDecimal(string value, out bool isError)
        {
            isError = false;
            try
            {
                return (Convert.ToDecimal(value));
            }
            catch
            {
                isError = true;
                return (0);
            }
        }

        public static DateTime StringToDate(string value)
        {
            bool isError = false;
            return (StringToDate(value, out isError));
        }
        public static DateTime StringToDate(string value, string format)
        {
            bool isError = false;
            return (StringToDate(value, format, out isError));
        }
        public static DateTime StringToDate(string value, string format, out bool isError)
        {
            isError = false;
            try
            {

                if (format != string.Empty)
                    return (DateTime.ParseExact(value, format, CultureInfo.InvariantCulture));
                return (Convert.ToDateTime(value));
            }
            catch
            {
                isError = true;
                return (System.DateTime.Now);
            }
        }
        public static DateTime StringToDate(string value, out bool isError)
        {
            return (StringToDate(value, string.Empty, out isError));
        }
        public static bool StringToBool(string value)
        {
            value = value.Trim().ToLower();
            if (value == "true" || value == "yes" || StringToNumeric(value) != 0)
                return (true);
            return (false);
        }

        public static bool IsNumericString(string text)
        {
            if (text.Any(c => c < '0' || c > '9'))
                return false;
            return true;
        }

        public static bool IsEmptyList<T>(IEnumerable<T> list) where T:class
        {
            return (list == null || list.Count() <= 0);
                
        }

        public static bool NetWorkPing(string ipAddess, int maxTrial)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;


            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            for(int i=0;i<maxTrial;i++)
            {
                PingReply reply = pingSender.Send(ipAddess, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                    return true;
            }
            return false;
        }
        public static bool NetWorkPing(string ipAddess)
        {
            return NetWorkPing(ipAddess, 3);
            
        }
        #endregion
    }
}
