using System;
using System.Net;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace JF.ComponentModel
{
	/// <summary>
	/// 提供将 <see cref="System.Net.IPEndPoint"/> 对象与其他各种表示形式相互转换的类型转换器。
	/// </summary>
	public class IPEndPointConverter : TypeConverter
	{
		#region 静态变量

		private static readonly Regex _regex = new Regex(@"(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})(\s*[:#]\s*(?<port>\d{1,8}))?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
		
		#endregion

		#region 重写方法

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			return Parse(value as string);
		}

		#endregion

		#region 静态方法

		public static IPEndPoint Parse(string text)
		{
			if(string.IsNullOrWhiteSpace(text))
				return null;

			var match = _regex.Match(text);

			if(match.Success)
			{
				IPAddress address;

				if(IPAddress.TryParse(match.Groups["ip"].Value, out address))
				{
					int port;
					int.TryParse(match.Groups["port"].Value, out port);

					return new IPEndPoint(address, port);
				}
			}

			return null;
		}

		#endregion
	}
}