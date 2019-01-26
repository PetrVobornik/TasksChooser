﻿using System;
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
            if (element.HasElements)
                using (var reader = element.CreateReader())
                {
                    reader.MoveToContent();
                    return reader.ReadInnerXml();
                }
            else
                return element.Value;
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
            var sha = new SHA1Managed();
            var hash = sha.ComputeHash(byty);
            byte b = hash[0];
            b ^= hash[1];
            var bytes = new byte[] { hash.Xor(0, 4), hash.Xor(5, 9), hash.Xor(10, 14), hash.Xor(15, 19), };
            return BitConverter.ToInt32(bytes, 0);
            // TODO z nějketách bytů či bitů poskládat výsledný int
            //hash
            //return Convert.ToBase64String(hash).TrimEnd('=').Replace("+", "").Replace("/", "");
        }

        public static byte Xor(this byte[] b, int from, int to)
        {
            byte xor = b[from];
            for (int i = from+1; i <= to; i++)
                xor ^= b[i];
            return xor;
        }
    }

}
