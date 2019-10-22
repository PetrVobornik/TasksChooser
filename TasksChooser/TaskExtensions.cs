using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Amporis.TasksChooser
{
    public static class TaskExtensions
    {
        public static string InnerText(this XElement element)
        {
            using (var reader = element.CreateReader())
            {
                reader.MoveToContent();
                return reader.ReadInnerXml();
            }
        }

        public static T GetAtt<T>(this XElement element, XName attributeName, T defaultValue)
            => (T)AttributeValue(typeof(T), element, attributeName, defaultValue);

        public static object AttributeValue(Type type, XElement element, XName attributeName, object defaultValue)
        {
            XAttribute attribute = null;
            if (element != null)
                attribute = element.Attribute(attributeName);
            if (attribute == null || String.IsNullOrEmpty(attribute.Value))
                return defaultValue;
            if (type == typeof(DateTime))
                return (DateTime)attribute;
            return ConvertValue(type, attribute.Value, defaultValue);
        }

        public static T ConvertValue<T>(string value, T defaultValue)
            => (T)ConvertValue(typeof(T), value, defaultValue);

        public static object ConvertValue(Type type, string value, object defaultValue)
        {
            try
            {
                if (String.IsNullOrEmpty(value))
                    return defaultValue;
                if (type == typeof(double))
                    return Convert.ToDouble(value, CultureInfo.InvariantCulture);
                if (type == typeof(decimal))
                    return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                if (type == typeof(short))
                    return Convert.ToInt16(value);
                if (type == typeof(int))
                    return Convert.ToInt32(value);
                if (type == typeof(int?))
                    return (int?)Convert.ToInt32(value);
                if (type == typeof(long))
                    return Convert.ToInt64(value);
                if (type == typeof(long?))
                    return (long?)Convert.ToInt64(value);
                if (type == typeof(DateTime))
                    return DateTime.Parse(value);
                if (type == typeof(bool) || type == typeof(bool?))
                {
                    bool result = (!String.IsNullOrEmpty(value) &&
                        (value == "1" || value.ToLower() == "true"));
                    if (type == typeof(bool?))
                        return (bool?)result;
                    return result;
                }
                if (type.IsSubclassOf(typeof(Enum)))
                    return Enum.Parse(type, value, true);
                return value;
            }
            catch { }
            return defaultValue;
        }


        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
                action(item);
        }

        public static U? Get<T, U>(this Dictionary<T, U> dict, T key) where U : struct
        {
            U val;
            if (dict.TryGetValue(key, out val))
                return val;
            return null;
        }

        public static U GetRef<T, U>(this Dictionary<T, U> dict, T key) where U : class
        {
            U val;
            if (dict.TryGetValue(key, out val))
                return val;
            return null;
        }


        public static string EncodeEscapedHtmlTags(this string s)
        {
            return s
                .Replace(@"\&lt;", "<")
                .Replace(@"\&gt;", ">")
                .Replace(@"\&amp;", "&")
                .Replace(@"\&nbsp;", " ")
                .Replace(@"\&quot;", "\"")
                .Replace(@"\&apos;", "'");
        }

        public static string GetFirstNotNull(params string[] texts)
        {
            foreach (var text in texts)
                if (!String.IsNullOrEmpty(text))
                    return text;
            return String.Empty;
        }


        public static string ArrayToCommaText(this IEnumerable<string> recordsId, string separator = ", ")
        {
            var sbIds = new StringBuilder();
            if (recordsId != null)
                foreach (var id in recordsId)
                    sbIds.Append(id + separator);
            if (sbIds.Length > 1)
                sbIds = sbIds.Remove(sbIds.Length - separator.Length, separator.Length);
            string ids = sbIds.ToString();
            return ids;
        }

        public static short ToBin(this bool b)
            => b ? (short)1 : (short)0;

        public static int GetIntHash(this string text)
        {
            if (String.IsNullOrEmpty(text))
                return 0;
            var byty = Encoding.UTF8.GetBytes(text);
            //var sha = new SHA3Managed(HashBitLength.L256); //  L256=0-31, L512=0-63
            var sha = new SHA1Managed(); 
            var hash = sha.ComputeHash(byty); // 20: 0-19 bytes
            //var bytes = new byte[] { hash.Xor(0, 4), hash.Xor(5, 9), hash.Xor(10, 14), hash.Xor(15, 19), };
            var bytes = new byte[] { hash.XorNthBytes(0, 4), hash.XorNthBytes(1, 4), hash.XorNthBytes(2, 4), hash.XorNthBytes(3, 4), };
            return BitConverter.ToInt32(bytes, 0);
            //hash
            //return Convert.ToBase64String(hash).TrimEnd('=').Replace("+", "").Replace("/", "");
        }

        public static byte XorNthBytes(this byte[] b, int startIndex, int n)
        {
            byte xor = b[startIndex];
            for (int i = startIndex+n; i < b.Length; i += n)
                xor ^= b[i];
            return xor;
        }

        public static byte XorBytes(this byte[] b, params int[] indexes)
        {
            byte xor = b[0];
            for (int i = 1; i < indexes.Length; i++)
                xor ^= b[i];
            return xor;
        }

        public static byte Xor(this byte[] b, int from, int to)
        {
            byte xor = b[from];
            for (int i = from+1; i <= to; i++)
                xor ^= b[i];
            return xor;
        }

        public static int? ToInt(this string text)
        {
            if (!String.IsNullOrEmpty(text))
                if (int.TryParse(text, out int number))
                    return number;
            return null;
        }
    }

}
