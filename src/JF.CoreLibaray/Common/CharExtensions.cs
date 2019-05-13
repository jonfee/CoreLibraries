using System;
using System.Text;

namespace JF.Common
{
    /// <summary>
    /// 字符类型扩展
    /// </summary>
    public static class CharExtensions
    {
        const string cnPuncation = "。？！，、；：‘’“”（）〔〕【】「」『』—…–．《》〈〉";

        /// <summary>
        /// 是否为中文符号（汉字及标点符号）
        /// </summary>
        /// <param name="char"></param>
        /// <returns></returns>
        public static bool IsChineseSymbols(this char @char)
        {
            // 汉字
            if (@char >= 0x4e00 && @char <= 0x9fbb) return true;
            
            // 中文标点符号
            return cnPuncation.Contains(@char.ToString());
        }

        /// <summary>
        /// 转换为Unicode编码的字符串
        /// </summary>
        /// <param name="char"></param>
        /// <returns></returns>
        public static string ToUnicode(this char @char)
        {
            var buffer = Encoding.Unicode.GetBytes(@char.ToString());

            return String.Format("\\u{0:X2}{1:X2}", buffer[1], buffer[0]).ToLower();
        }
    }
}
