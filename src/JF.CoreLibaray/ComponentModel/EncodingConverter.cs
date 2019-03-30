using System;
using System.Text;

namespace JF.ComponentModel
{
    public class EncodingConverter : System.ComponentModel.TypeConverter
	{
		public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
		{
			if(sourceType.IsPrimitive || sourceType == typeof(string) || sourceType == typeof(decimal))
				return true;

			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, Type destinationType)
		{
			if(destinationType.IsPrimitive || destinationType == typeof(string) || destinationType == typeof(decimal))
				return true;

			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if(value == null)
				return Encoding.UTF8;

			if(value.GetType() == typeof(string))
			{
				switch(((string)value).ToLowerInvariant())
				{
					case "utf8":
					case "utf-8":
						return Encoding.UTF8;
					case "utf7":
					case "utf-7":
						return Encoding.UTF7;
					case "utf32":
						return Encoding.UTF32;
					case "unicode":
						return Encoding.Unicode;
					case "ascii":
						return Encoding.ASCII;
					case "bigend":
					case "bigendian":
						return Encoding.BigEndianUnicode;
					default:
						return Encoding.GetEncoding((string)value);
				}
			}
			else if(value.GetType().IsPrimitive || value.GetType() == typeof(decimal))
			{
				return Encoding.GetEncoding((int)System.Convert.ChangeType(value, typeof(int)));
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			var encoding = value as Encoding;

			if(destinationType == typeof(string))
				return encoding == null ? "utf-8" : encoding.EncodingName;

			if(value.GetType().IsPrimitive || value.GetType() == typeof(decimal))
				return encoding == null ? Encoding.UTF8.CodePage : encoding.CodePage;

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
