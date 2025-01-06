using System;
using System.Collections;
using System.Reflection;

namespace PMS.Shared.Utilities
{
    #region Class StringEnum

    public class StringEnum
    {
        #region Instance implementation

        private readonly Type _enumType;
        private static readonly Hashtable _stringValues = new Hashtable();

        public StringEnum(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException(String.Format("Supplied type must be an Enum.  Type was {0}", enumType));

            _enumType = enumType;
        }

        public string GetStringValue(string valueName)
        {
            string stringValue;
            try
            {
                var enumType = (Enum)Enum.Parse(_enumType, valueName);
                stringValue = GetStringValue(enumType);
            }
            catch
            {
                throw new Exception("Enum error.");
            }

            return stringValue;
        }

        public Array GetStringValues()
        {
            var values = new ArrayList();
            foreach (var fi in _enumType.GetFields())
            {
                var attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attrs != null)
                    if (attrs.Length > 0)
                        values.Add(attrs[0].Value);
            }

            return values.ToArray();
        }

        public IList GetListValues()
        {
            var underlyingType = Enum.GetUnderlyingType(_enumType);
            var values = new ArrayList();
            foreach (var fi in _enumType.GetFields())
            {
                var attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attrs != null)
                    if (attrs.Length > 0)
                        values.Add(new DictionaryEntry(Convert.ChangeType(Enum.Parse(_enumType, fi.Name), underlyingType), attrs[0].Value));
            }

            return values;
        }

        public bool IsStringDefined(string stringValue)
        {
            return Parse(_enumType, stringValue) != null;
        }

        public bool IsStringDefined(string stringValue, bool ignoreCase)
        {
            return Parse(_enumType, stringValue, ignoreCase) != null;
        }

        public Type EnumType
        {
            get { return _enumType; }
        }

        #endregion

        #region Static implementation

        public static string GetStringValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            if (_stringValues.ContainsKey(value))
                output = ((StringValueAttribute)_stringValues[value]).Value;
            else
            {
                FieldInfo fi = type.GetField(value.ToString());
                var attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attrs != null)
                    if (attrs.Length > 0)
                    {
                        _stringValues.Add(value, attrs[0]);
                        output = attrs[0].Value;
                    }
            }
            return output;

        }

        public static object Parse(Type type, string stringValue)
        {
            return Parse(type, stringValue, false);
        }

        public static object Parse(Type type, string stringValue, bool ignoreCase)
        {
            object output = null;
            string enumStringValue = null;

            if (!type.IsEnum)
                throw new ArgumentException(String.Format("Supplied type must be an Enum.  Type was {0}", type.ToString()));

            foreach (FieldInfo fi in type.GetFields())
            {
                var attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attrs != null)
                    if (attrs.Length > 0)
                        enumStringValue = attrs[0].Value;

                if (string.Compare(enumStringValue, stringValue, ignoreCase) == 0)
                {
                    output = Enum.Parse(type, fi.Name);
                    break;
                }
            }

            return output;
        }

        public static bool IsStringDefined(Type enumType, string stringValue)
        {
            return Parse(enumType, stringValue) != null;
        }

        public static bool IsStringDefined(Type enumType, string stringValue, bool ignoreCase)
        {
            return Parse(enumType, stringValue, ignoreCase) != null;
        }

        #endregion
    }

    #endregion

    #region Class StringValueAttribute

    public class StringValueAttribute : Attribute
    {
        private readonly string _value;

        public StringValueAttribute(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }

    #endregion
}
