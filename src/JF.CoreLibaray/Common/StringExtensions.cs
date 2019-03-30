using System;
using System.Collections.Generic;
using System.Linq;

namespace JF.Common
{
    /// <summary>
    /// 基于<see cref="System.String"/>的扩展类
    /// </summary>
    public static class StringExtensions
    {
        public static readonly string InvalidCharacters = "`~!@#$%^&*()-+={}[]|\\/?:;'\"\t\r\n ";

        #region 实用方法

        /// <summary>
        /// 移除指定索引范围内的字符串，并返回移除后的字符串值。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="startIndex"></param>
        /// <param name="removedString">被移除的字符串</param>
        /// <returns></returns>
        public static string TryRemove(this string text, int startIndex, out string removedString)
        {
            return text.TryRemove(startIndex, -1, out removedString);
        }

        /// <summary>
        /// 移除指定索引范围内的字符串，并返回移除后的字符串值。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <param name="removedString">被移除的字符串</param>
        /// <returns></returns>
        public static string TryRemove(this string text, int startIndex, int count, out string removedString)
        {
            var newTxt = text;
            removedString = string.Empty;

            if (text == null) return text;
            if (text.Length <= startIndex) return text;

            if (startIndex + count >= text.Length) count = text.Length - startIndex - 1;

            if (count > 0)
            {
                removedString = text.Substring(startIndex, count);
                newTxt = text.Remove(startIndex, count);
            }
            else
            {
                removedString = text.Substring(startIndex);
                newTxt = text.Remove(startIndex);
            }

            return newTxt;
        }

        public static bool ContainsCharacters(this string text, string characters)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(characters))
                return false;

            return ContainsCharacters(text, characters.ToArray());
        }

        public static bool ContainsCharacters(this string text, params char[] characters)
        {
            if (string.IsNullOrEmpty(text) || characters.Length < 1)
                return false;

            foreach (char character in characters)
            {
                foreach (char item in text)
                {
                    if (character == item)
                        return true;
                }
            }

            return false;
        }

        public static string RemoveCharacters(this string text, string invalidCharacters)
        {
            return RemoveCharacters(text, invalidCharacters, 0);
        }

        public static string RemoveCharacters(this string text, char[] invalidCharacters)
        {
            return RemoveCharacters(text, invalidCharacters, 0);
        }

        public static string RemoveCharacters(this string text, string invalidCharacters, int startIndex)
        {
            return RemoveCharacters(text, invalidCharacters, startIndex, -1);
        }

        public static string RemoveCharacters(this string text, char[] invalidCharacters, int startIndex)
        {
            return RemoveCharacters(text, invalidCharacters, startIndex, -1);
        }

        public static string RemoveCharacters(this string text, string invalidCharacters, int startIndex, int count)
        {
            return RemoveCharacters(text, invalidCharacters.ToCharArray(), startIndex, count);
        }

        public static string RemoveCharacters(this string text, char[] invalidCharacters, int startIndex, int count)
        {
            if (string.IsNullOrEmpty(text) || invalidCharacters.Length < 1)
                return text;

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex");

            if (count < 1)
                count = invalidCharacters.Length - startIndex;

            if (startIndex + count > invalidCharacters.Length)
                throw new ArgumentOutOfRangeException("count");

            string result = text;

            for (int i = startIndex; i < startIndex + count; i++)
                result = result.Replace(invalidCharacters[i].ToString(), "");

            return result;
        }

        public static string TrimString(this string text, string trimString)
        {
            return TrimString(text, trimString, StringComparison.OrdinalIgnoreCase);
        }

        public static string TrimString(this string text, string trimString, StringComparison comparisonType)
        {
            return TrimStringEnd(
                TrimStringStart(text, trimString, comparisonType),
                trimString,
                comparisonType);
        }

        public static string TrimString(this string text, string prefix, string suffix)
        {
            return TrimString(text, prefix, suffix, StringComparison.OrdinalIgnoreCase);
        }

        public static string TrimString(this string text, string prefix, string suffix, StringComparison comparisonType)
        {
            return text
                    .TrimStringStart(prefix, comparisonType)
                    .TrimStringEnd(suffix, comparisonType);
        }

        public static string TrimStringEnd(this string text, string trimString)
        {
            return TrimStringEnd(text, trimString, StringComparison.OrdinalIgnoreCase);
        }

        public static string TrimStringEnd(this string text, string trimString, StringComparison comparisonType)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(trimString))
                return text;

            while (text.EndsWith(trimString, comparisonType))
                text = text.Remove(text.Length - trimString.Length);

            return text;
        }

        public static string TrimStringStart(this string text, string trimString)
        {
            return TrimStringStart(text, trimString, StringComparison.OrdinalIgnoreCase);
        }

        public static string TrimStringStart(this string text, string trimString, StringComparison comparisonType)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(trimString))
                return text;

            while (text.StartsWith(trimString, comparisonType))
                text = text.Remove(0, trimString.Length);

            return text;
        }

        public static bool In(this string text, IEnumerable<string> collection, StringComparison comparisonType)
        {
            if (collection == null)
                return false;

            return collection.Any(item => string.Equals(item, text, comparisonType));
        }

        /// <summary>
        /// 反转字符串
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Reverse(this string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            char[] chars = text.ToCharArray();

            int begin = 0;
            int end = chars.Length - 1;
            char tempChar;

            while (begin < end)
            {
                tempChar = chars[begin];
                chars[begin] = chars[end];
                chars[end] = tempChar;

                begin++;
                end--;
            }

            return new string(chars);
        }

        #endregion

        #region 类型转换

        public static bool ToBoolean(this string text)
        {
            return Converter.ConvertValue<bool>(text);
        }

        public static bool ToBoolean(this string text, bool defaultValue)
        {
            return Converter.ConvertValue<bool>(text, defaultValue);
        }

        public static byte ToByte(this string text)
        {
            return Converter.ConvertValue<byte>(text);
        }

        public static byte ToByte(this string text, byte defaultValue)
        {
            return Converter.ConvertValue<byte>(text, defaultValue);
        }

        public static sbyte ToSByte(this string text)
        {
            return Converter.ConvertValue<sbyte>(text);
        }

        public static sbyte ToSByte(this string text, sbyte defaultValue)
        {
            return Converter.ConvertValue<sbyte>(text, defaultValue);
        }

        public static char ToChar(this string text)
        {
            return Converter.ConvertValue<char>(text);
        }

        public static char ToChar(this string text, char defaultValue)
        {
            return Converter.ConvertValue<char>(text, defaultValue);
        }

        public static DateTime ToDateTime(this string text)
        {
            return Converter.ConvertValue<DateTime>(text);
        }

        public static DateTime ToDateTime(this string text, DateTime defaultValue)
        {
            return Converter.ConvertValue<DateTime>(text, defaultValue);
        }

        public static decimal ToDecimal(this string text)
        {
            return Converter.ConvertValue<decimal>(text);
        }

        public static decimal ToDecimal(this string text, decimal defaultValue)
        {
            return Converter.ConvertValue<decimal>(text, defaultValue);
        }

        public static float ToSingle(this string text)
        {
            return Converter.ConvertValue<float>(text);
        }

        public static float ToSingle(this string text, float defaultValue)
        {
            return Converter.ConvertValue<float>(text, defaultValue);
        }

        public static double ToDouble(this string text)
        {
            return Converter.ConvertValue<double>(text);
        }

        public static double ToDouble(this string text, double defaultValue)
        {
            return Converter.ConvertValue<double>(text, defaultValue);
        }

        public static short ToInt16(this string text)
        {
            return Converter.ConvertValue<short>(text);
        }

        public static short ToInt16(this string text, short defaultValue)
        {
            return Converter.ConvertValue<short>(text, defaultValue);
        }

        public static ushort ToUInt16(this string text)
        {
            return Converter.ConvertValue<ushort>(text);
        }

        public static ushort ToUInt16(this string text, ushort defaultValue)
        {
            return Converter.ConvertValue<ushort>(text, defaultValue);
        }

        public static int ToInt32(this string text)
        {
            return Converter.ConvertValue<int>(text);
        }

        public static int ToInt32(this string text, int defaultValue)
        {
            return Converter.ConvertValue<int>(text, defaultValue);
        }

        public static uint ToUInt32(this string text)
        {
            return Converter.ConvertValue<uint>(text);
        }

        public static uint ToUInt32(this string text, uint defaultValue)
        {
            return Converter.ConvertValue<uint>(text, defaultValue);
        }

        public static long ToInt64(this string text)
        {
            return Converter.ConvertValue<long>(text);
        }

        public static long ToInt64(this string text, long defaultValue)
        {
            return Converter.ConvertValue<long>(text, defaultValue);
        }

        public static ulong ToUInt64(this string text)
        {
            return Converter.ConvertValue<ulong>(text);
        }

        public static ulong ToUInt64(this string text, ulong defaultValue)
        {
            return Converter.ConvertValue<ulong>(text, defaultValue);
        }

        #endregion
    }
}