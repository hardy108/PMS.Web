
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PMS.Shared.Utilities
{
    public static class DateTimeExtensions
    {
        public static DateTime MonthStartDate(this DateTime date)
        {
            return date.AddDays(1 - date.Day).Date;
        }

        public static DateTime YearStartDate(this DateTime date)
        {
            return date.AddDays(1 - date.Day).AddMonths(1 - date.Month).Date;
        }
    }

    public static class ArrayExtensions
    {
        public static string ToPlainString(this Array array, string separator)
        {
            if (array == null)
                return string.Empty;
            if (array.Length <= 0)
                return string.Empty;
            if (string.IsNullOrWhiteSpace(separator))
                separator = ",";
            string text = string.Empty;
            foreach (string x in array)
            {
                text += x + separator;
            }
            return text.Substring(0, text.Length - separator.Length);
        }

        public static string[] SplitToArray(this string text, string separator)
        {
            string[] stringArray = text.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            return stringArray;
        }
    }

    public static class DynamicExtensions
    {
        public static void AddProperty(this ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

        public static object GetProperty(this ExpandoObject expando, string propertyName)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                return expandoDict[propertyName];
            return null;
        }
    }

    public static class ObjectExtensions
    {
        public static void CopyFrom<T>(this T obj, IFormCollection webForm,List<string> fields) where T : class
        {

            if (obj == null)
                obj = (T)Activator.CreateInstance(typeof(T));
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (!fields.Contains(property.Name))
                    continue;

                string formValue = webForm[property.Name];
                if (string.IsNullOrWhiteSpace(formValue))
                    continue;

                obj.SetPropertyValue(property, formValue);
                fields.Remove(property.Name);
                if (fields.Count <= 0)
                    return;
            }
        }

        public static void CopyFrom<T>(this T obj, IFormCollection webForm) where T:class
        {
            
            if (obj == null)
                obj = (T)Activator.CreateInstance(typeof(T));
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                string formValue = webForm[property.Name];
                if (string.IsNullOrWhiteSpace(formValue))
                    continue;
                
                obj.SetPropertyValue(property, formValue);
            }
        }

        public static void CopyFrom<T>(this T obj, IFormCollection webForm,string modelName,int index) where T:class
        {
            if (obj == null)
                obj = (T)Activator.CreateInstance(typeof(T));

            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                string formValue = webForm[$"{modelName}[{index}][{property.Name}]"];
                if (string.IsNullOrWhiteSpace(formValue))
                    continue;
                obj.SetPropertyValue(property, formValue);
            }
        }


        public static void CopyFrom<T>(this  ICollection<T> list, IFormCollection webForm, string modelName) where T : class
        {
            if (list == null)
                list = new List<T>();

            int listCount = 0;
            if (int.TryParse(webForm[$"{modelName}_COUNT"],out listCount))
            {
                for (int i = 0; i < listCount; i++)
                {
                    T obj = (T)Activator.CreateInstance(typeof(T));
                    obj.CopyFrom<T>(webForm, modelName, i);
                    list.Add(obj);
                }
            }
        }

        

        public static void CopyFrom(this object target, object source)
        {
            CopyPropertiesBetweenObject(source, target);
        }

        public static void CopyTo(this object source, object target)
        {
            CopyPropertiesBetweenObject(source, target);
        }

        private static void CopyPropertiesBetweenObject(object source, object target)
        {
            if (target == null || source == null)
                return;
            PropertyInfo[] targetProperties = target.GetType().GetProperties();
            PropertyInfo[] sourceProperties = source.GetType().GetProperties();

            foreach (var targetProperty in targetProperties)
            {
                foreach (var sourceProperty in sourceProperties)
                {
                    if (targetProperty.Name.Equals(sourceProperty.Name))
                    {
                        try
                        {
                            targetProperty.SetValue(target, sourceProperty.GetValue(source));
                        }
                        catch { }
                        break;
                    }
                }
            }
        }

        public static object GetPropertyValue<T>(this T entity, string name)
        {
            return entity.GetType().GetProperty(name).GetValue(entity, null);
        }

        public static void SetPropertyValue(this object obj, PropertyInfo property, string value)
        {
            try
            {
                if (property.PropertyType == typeof(byte))
                {
                    byte newValue = 0;
                    byte.TryParse(value, out newValue);
                    property.SetValue(obj, newValue);
                    return;
                }

                if (property.PropertyType == typeof(byte?))
                {
                    byte newValue = 0;
                    if (byte.TryParse(value, out newValue))
                        property.SetValue(obj, newValue);
                    else
                        property.SetValue(obj, null);
                    return;
                }


                if (property.PropertyType == typeof(short))
                {
                    short newValue = 0;
                    short.TryParse(value, out newValue);
                    property.SetValue(obj, newValue);
                    return;
                }

                if (property.PropertyType == typeof(short?))
                {
                    short newValue = 0;
                    if (short.TryParse(value, out newValue))
                        property.SetValue(obj, newValue);
                    else
                        property.SetValue(obj, null);
                    return;
                }

                if (property.PropertyType == typeof(int))
                {
                    int newValue = 0;
                    int.TryParse(value, out newValue);
                    property.SetValue(obj, newValue);
                    return;
                }

                if (property.PropertyType == typeof(int?))
                {
                    int newValue = 0;
                    if (int.TryParse(value, out newValue))
                        property.SetValue(obj, newValue);
                    else
                        property.SetValue(obj, null);
                    return;
                }

                if (property.PropertyType == typeof(long))
                {
                    long newValue = 0;
                    long.TryParse(value, out newValue);
                    property.SetValue(obj, newValue);
                    return;
                }

                if (property.PropertyType == typeof(long?))
                {
                    long newValue = 0;
                    if (long.TryParse(value, out newValue))
                        property.SetValue(obj, newValue);
                    else
                        property.SetValue(obj, null);
                    return;
                }

                if (property.PropertyType == typeof(decimal))
                {
                    decimal newValue = 0;
                    decimal.TryParse(value, out newValue);
                    property.SetValue(obj, newValue);
                    return;
                }
                if (property.PropertyType == typeof(decimal?))
                {
                    decimal newValue = 0;
                    if (decimal.TryParse(value, out newValue))
                        property.SetValue(obj, newValue);
                    else
                        property.SetValue(obj, null);
                    return;
                }
                if (property.PropertyType == typeof(float))
                {
                    float newValue = 0;
                    float.TryParse(value, out newValue);
                    property.SetValue(obj, newValue);
                    return;
                }
                if (property.PropertyType == typeof(float?))
                {
                    float newValue = 0;
                    if (float.TryParse(value, out newValue))
                        property.SetValue(obj, newValue);
                    else
                        property.SetValue(obj, null);
                    return;
                }
                if (property.PropertyType == typeof(double))
                {
                    double newValue = 0;
                    double.TryParse(value, out newValue);
                    property.SetValue(obj, newValue);
                    return;
                }
                if (property.PropertyType == typeof(double?))
                {
                    double newValue = 0;
                    if (double.TryParse(value, out newValue))
                        property.SetValue(obj, newValue);
                    else
                        property.SetValue(obj, null);
                    return;
                }

                if (property.PropertyType == typeof(bool))
                {
                    bool newValue = false;
                    bool.TryParse(value, out newValue);
                    property.SetValue(obj, newValue);
                    return;
                }
                if (property.PropertyType == typeof(bool?))
                {
                    bool newValue = false;
                    if (bool.TryParse(value, out newValue))
                        property.SetValue(obj, newValue);
                    else
                        property.SetValue(obj, null);
                    return;
                }

                if (property.PropertyType == typeof(DateTime))
                {
                    DateTime newValue = DateTime.Now;

                    if (DateTime.TryParse(value, out newValue))
                        property.SetValue(obj, newValue);
                    return;
                }

                if (property.PropertyType == typeof(DateTime?))
                {
                    DateTime newValue = DateTime.Now;

                    if (DateTime.TryParse(value, out newValue))
                        property.SetValue(obj, newValue);
                    else
                        property.SetValue(obj, null);
                    return;
                }


                property.SetValue(obj, value);
            }
            catch
            {

            }
        }
    }

    public static class StringExtensions
    {
        public static bool ContainsIgnoreCase(this string source, string toCheck)
        {
            return source?.IndexOf(toCheck,  StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }

    public static class JsExtensions
    {
        public static T Deserialize<T>(this Stream stream) where T : class
        {
            StreamReader sr = new StreamReader(stream);
            using (JsonTextReader reader = new JsonTextReader(sr))
            {
                var x = Newtonsoft.Json.JsonSerializer.Create();
                return x.Deserialize<T>(reader);
            }
        }

        public static T Deserialize<T>(this string jsonString) where T : class
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public static string BooleanStringHtml(bool value)
        {
            if (value)
                return "true";
            return "false";
        }

        public static string Serialize<T>(this T jsonObject) where T : class
        {
            return JsonConvert.SerializeObject(jsonObject);
        }

        public static string SerializeList<T>(this List<T> jsonObjects) where T : class
        {
            if (jsonObjects == null)
                return string.Empty;
            if (!jsonObjects.Any())
                return string.Empty;
            string result = string.Empty;
            jsonObjects.ForEach(d =>
            {
                result += d.Serialize<T>() + ",";
            });
            result = "[" + result.Substring(0, result.Length - 1) + "]";
            return result;
        }
    }
}
    
