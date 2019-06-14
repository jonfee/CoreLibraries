using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace JF.Common
{
    /// <summary>
    /// 数字类型扩展
    /// </summary>
    public static class NumberExtensions
    {
        private static Regex regex = new Regex(@"^(?<flag>[+-]?)(?<zs>\d+)(?<xs>\.\d+)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 把 Number 转换为指定小数位数的字符串。
        /// </summary>
        /// <param name="val"></param>
        /// <param name="digits">保留的最大小数位，当小数位均为0时，将忽略小数位。</param>
        /// <param name="midpoint">舍入模式。</param>
        /// <returns></returns>
        public static string ToFixed(this decimal val, int digits = 2, MidpointRounding midpoint = MidpointRounding.AwayFromZero)
        {
            var num = (double)val;

            return num.ToFixed(digits, midpoint);
        }

        /// <summary>
        /// 把 Number 转换为指定小数位数的字符串。
        /// </summary>
        /// <param name="val"></param>
        /// <param name="digits">保留的最大小数位，当小数位均为0时，将忽略小数位。</param>
        /// <param name="midpoint">舍入模式。</param>
        /// <returns></returns>
        public static string ToFixed(this float val, int digits = 2, MidpointRounding midpoint = MidpointRounding.AwayFromZero)
        {
            var num = (double)val;

            return num.ToFixed(digits, midpoint);
        }

        /// <summary>
        /// 把 Number 转换为指定小数位数的字符串。
        /// </summary>
        /// <param name="val"></param>
        /// <param name="digits">保留的最大小数位，当小数位均为0时，将忽略小数位。</param>
        /// <param name="midpoint">舍入模式。</param>
        /// <returns></returns>
        public static string ToFixed(this double val, int digits = 2, MidpointRounding midpoint = MidpointRounding.AwayFromZero)
        {
            val = Math.Round(val, digits, midpoint);

            var str = val.ToString();

            var match = regex.Match(str);

            if (val % 1 == 0)
            {
                str = $"{match.Groups["flag"]}{match.Groups["zs"]}";
            }

            return str;
        }
    }
}
