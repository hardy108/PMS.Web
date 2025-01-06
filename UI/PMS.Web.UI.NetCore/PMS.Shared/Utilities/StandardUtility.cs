using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
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



        #endregion
    }
}
