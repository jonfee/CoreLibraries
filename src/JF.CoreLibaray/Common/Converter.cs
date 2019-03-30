using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using Newtonsoft.Json;

#if NET452 || NET46
#else
using System.Collections.Concurrent;
#endif

namespace JF.Common
{
    /// <summary>
    /// 提供将一种数据类型与其他各种表示形式相互转换的转换器。
    /// </summary>
    public static class Converter
    {
        #region 类型转换

        public static T ConvertValue<T>(object value)
        {
            return (T)ConvertValue(value, typeof(T), () => default(T));
        }

        public static T ConvertValue<T>(object value, T defaultValue)
        {
            return (T)ConvertValue(value, typeof(T), () => defaultValue);
        }

        public static T ConvertValue<T>(object value, Func<object> defaultValueThunk)
        {
            return (T)ConvertValue(value, typeof(T), defaultValueThunk);
        }

        public static object ConvertValue(object value, Type conversionType)
        {
            return ConvertValue(value, conversionType, () => GetDefaultValue(conversionType));
        }

        public static object ConvertValue(object value, Type conversionType, object defaultValue)
        {
            return ConvertValue(value, conversionType, () => defaultValue);
        }

        public static object ConvertValue(object value, Type conversionType, Func<object> defaultValueThunk)
        {
            if (defaultValueThunk == null)
            {
                throw new ArgumentNullException("defaultValueThunk");
            }

            if (conversionType == null)
            {
                return value;
            }

            if (value == null || Converter.IsDBNull(value))
            {
                if (conversionType == typeof(DBNull))
                {
                    return DBNull.Value;
                }
                else
                {
                    return defaultValueThunk();
                }
            }

            Type type = conversionType;

            if (conversionType.IsGenericType && (!conversionType.IsGenericTypeDefinition) && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = conversionType.GetGenericArguments()[0];
            }

            if (type == value.GetType() || type.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            try
            {
                //获取目标类型的转换器
                var converter = GetTypeConverter(type);

                //判断目标类型转换器是否支持从源类型进行转换
                if (converter != null && converter.CanConvertFrom(value.GetType()))
                    return converter.ConvertFrom(value);

                //处理字典序列化的情况
                if (typeof(IDictionary).IsAssignableFrom(value.GetType()) && !typeof(IDictionary).IsAssignableFrom(conversionType))
                    return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value), conversionType);

                return System.Convert.ChangeType(value, type);
            }
            catch
            {
                return defaultValueThunk();
            }
        }

        public static bool TryConvertValue<T>(object value, out T result)
        {
            bool b = true;

            result = (T)ConvertValue(value, typeof(T), () =>
            {
                b = false;
                return default(T);
            });

            return b;
        }

        public static bool TryConvertValue(object value, Type conversionType, out object result)
        {
            result = ConvertValue(value, conversionType, () => typeof(Converter));

            if (object.ReferenceEquals(result, typeof(Converter)))
            {
                result = null;
                return false;
            }

            return true;
        }

        #endregion

        #region 类型判断

        public static bool IsDBNull(object value)
        {
#if NET452 || NET46 || NET46
			return System.Convert.IsDBNull(value);
#else
            return value == DBNull.Value;
#endif
        }

        #endregion

        #region 获取转换器

#if NET452 || NET46
        private static int _initialized;
#else
        private static readonly ConcurrentDictionary<Type, TypeConverter> _converters = new ConcurrentDictionary<Type, TypeConverter>();
#endif

        private static TypeConverter GetTypeConverter(Type type)
        {
#if NET452 || NET46

            if (_initialized == 0)
			{
				var initialized = System.Threading.Interlocked.CompareExchange(ref _initialized, 1, 0);

				if(initialized == 0)
				{
					TypeDescriptor.AddAttributes(typeof(System.Boolean), new Attribute[] { new TypeConverterAttribute(typeof(BooleanConverter))});
					TypeDescriptor.AddAttributes(typeof(System.Enum), new Attribute[]{new TypeConverterAttribute(typeof(EnumConverter))});
					TypeDescriptor.AddAttributes(typeof(System.Guid), new Attribute[]{new TypeConverterAttribute(typeof(GuidConverter))});
					TypeDescriptor.AddAttributes(typeof(Encoding), new Attribute[]{new TypeConverterAttribute(typeof(ComponentModel.EncodingConverter))});
					TypeDescriptor.AddAttributes(typeof(System.Net.IPEndPoint), new Attribute[]{new TypeConverterAttribute(typeof(ComponentModel.IPEndPointConverter))});
				}
			}

			return TypeDescriptor.GetConverter(type);

#else

            if (_converters.ContainsKey(type))
            {
                return _converters[type];
            }

            if (type.GetTypeInfo().IsEnum)
            {
                _converters[type] = new EnumConverter(type);
            }
            else if (type == typeof(Boolean))
            {
                _converters[type] = new BooleanConverter();
            }
            else if (type == typeof(Guid))
            {
                _converters[type] = new GuidConverter();
            }
            else if (type == typeof(Encoding))
            {
                _converters[type] = new ComponentModel.EncodingConverter();
            }
            else if (type == typeof(System.Net.IPEndPoint))
            {
                _converters[type] = new ComponentModel.IPEndPointConverter();
            }
            else
            {
                _converters[type] = TypeDescriptor.GetConverter(type);
            }

            return _converters[type];

#endif
        }

        #endregion

        #region 取默认值

        public static object GetDefaultValue(Type type)
        {
            if (type == typeof(DBNull))
            {
                return DBNull.Value;
            }

            if (type == null || type.IsClass || type.IsInterface || type == typeof(Nullable<>))
            {
                return null;
            }

            if (type.IsEnum)
            {
                var attribute = (DefaultValueAttribute)type.GetCustomAttribute(typeof(DefaultValueAttribute), true);

                if (attribute != null)
                {
                    return attribute.Value;
                }

                Array values = Enum.GetValues(type);

                if (values.Length > 0)
                {
                    return values.GetValue(0);
                }
            }

            return Activator.CreateInstance(type);
        }

        #endregion

        #region 字节文本

        /// <summary>
        /// 将指定的字节数组转换为其用十六进制数字编码的等效字符串表示形式。
        /// </summary>
        /// <param name="buffer">一个 8 位无符号字节数组。</param>
        /// <returns>参数中元素的字符串表示形式，以十六进制文本表示。</returns>
        public static string ToHexString(byte[] buffer)
        {
            return ToHexString(buffer, '\0');
        }

        /// <summary>
        /// 将指定的字节数组转换为其用十六进制数字编码的等效字符串表示形式。参数指定是否在返回值中插入分隔符。
        /// </summary>
        /// <param name="buffer">一个 8 位无符号字节数组。</param>
        /// <param name="separator">每字节对应的十六进制文本中间的分隔符。</param>
        /// <returns>参数中元素的字符串表示形式，以十六进制文本表示。</returns>
        public static string ToHexString(byte[] buffer, char separator)
        {
            if (buffer == null || buffer.Length < 1)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(buffer.Length * 2);

            for (int i = 0; i < buffer.Length; i++)
            {
                builder.AppendFormat("{0:X2}", buffer[i]);

                if (separator != '\0' && i < buffer.Length - 1)
                {
                    builder.Append(separator);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// 将指定的十六进制格式的字符串转换为等效的字节数组。
        /// </summary>
        /// <param name="text">要转换的十六进制格式的字符串。</param>
        /// <returns>与<paramref name="text"/>等效的字节数组。</returns>
        /// <exception cref="System.FormatException"><paramref name="text"/>参数中含有非空白字符。</exception>
        /// <remarks>该方法的实现始终忽略<paramref name="text"/>参数中的空白字符。</remarks>
        public static byte[] FromHexString(string text)
        {
            return FromHexString(text, '\0', true);
        }

        /// <summary>
        /// 将指定的十六进制格式的字符串转换为等效的字节数组。
        /// </summary>
        /// <param name="text">要转换的十六进制格式的字符串。</param>
        /// <param name="separator">要过滤掉的分隔符字符。</param>
        /// <returns>与<paramref name="text"/>等效的字节数组。</returns>
        /// <exception cref="System.FormatException"><paramref name="text"/>参数中含有非空白字符或非指定的分隔符。</exception>
        /// <remarks>该方法的实现始终忽略<paramref name="text"/>参数中的空白字符。</remarks>
        public static byte[] FromHexString(string text, char separator)
        {
            return FromHexString(text, separator, true);
        }

        /// <summary>
        /// 将指定的十六进制格式的字符串转换为等效的字节数组。
        /// </summary>
        /// <param name="text">要转换的十六进制格式的字符串。</param>
        /// <param name="separator">要过滤掉的分隔符字符。</param>
        /// <param name="throwExceptionOnFormat">指定当输入文本中含有非法字符时是否抛出<seealso cref="System.FormatException"/>异常。</param>
        /// <returns>与<paramref name="text"/>等效的字节数组。</returns>
        /// <exception cref="System.FormatException">当<paramref name="throwExceptionOnFormat"/>参数为真，并且<paramref name="text"/>参数中含有非空白字符或非指定的分隔符。</exception>
        /// <remarks>该方法的实现始终忽略<paramref name="text"/>参数中的空白字符。</remarks>
        public static byte[] FromHexString(string text, char separator, bool throwExceptionOnFormat)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new byte[0];
            }

            int index = 0;
            char[] buffer = new char[2];
            List<byte> result = new List<byte>();

            foreach (char character in text)
            {
                if (char.IsWhiteSpace(character) || character == separator)
                {
                    continue;
                }

                buffer[index++] = character;
                if (index == buffer.Length)
                {
                    index = 0;
                    byte value = 0;

                    if (TryParseHex(buffer, out value))
                    {
                        result.Add(value);
                    }
                    else
                    {
                        if (throwExceptionOnFormat)
                        {
                            throw new FormatException();
                        }
                        else
                        {
                            return new byte[0];
                        }
                    }
                }
            }

            return result.ToArray();
        }

        public static bool TryParseHex(char[] characters, out byte value)
        {
            long number;

            if (TryParseHex(characters, out number))
            {
                if (number >= byte.MinValue && number <= byte.MaxValue)
                {
                    value = (byte)number;
                    return true;
                }
            }

            value = 0;
            return false;
        }

        public static bool TryParseHex(char[] characters, out short value)
        {
            long number;

            if (TryParseHex(characters, out number))
            {
                if (number >= short.MinValue && number <= short.MaxValue)
                {
                    value = (short)number;
                    return true;
                }
            }

            value = 0;
            return false;
        }

        public static bool TryParseHex(char[] characters, out int value)
        {
            long number;

            if (TryParseHex(characters, out number))
            {
                if (number >= int.MinValue && number <= int.MaxValue)
                {
                    value = (int)number;
                    return true;
                }
            }

            value = 0;
            return false;
        }

        public static bool TryParseHex(char[] characters, out long value)
        {
            value = 0;

            if (characters == null)
            {
                return false;
            }

            int count = 0;
            byte[] digits = new byte[characters.Length];

            foreach (char character in characters)
            {
                if (char.IsWhiteSpace(character))
                {
                    continue;
                }

                if (character >= '0' && character <= '9')
                {
                    digits[count++] = (byte)(character - '0');
                }
                else if (character >= 'A' && character <= 'F')
                {
                    digits[count++] = (byte)((character - 'A') + 10);
                }
                else if (character >= 'a' && character <= 'f')
                {
                    digits[count++] = (byte)((character - 'a') + 10);
                }
                else
                {
                    return false;
                }
            }

            long number = 0;

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    number += digits[i] * (long)Math.Pow(16, count - i - 1);
                }
            }

            value = number;
            return true;
        }

        #endregion

        #region 对象解析

        #region 获取方法

        public static Type GetMemberType(object target, string text)
        {
            if (target == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return target.GetType();
            }

            var type = target.GetType();

            var parts = text.Split(new char[]
            {
                '.'
            }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var member = GetMember(type, part, (BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static), true);

                if (member == null)
                {
                    throw new ArgumentException(string.Format("The '{0}' member is not existed in the '{1}' type, the original text is '{2}'.", part, type.FullName, text));
                }

                if (member.IsField())
                {
                    type = ((FieldInfo)member).FieldType;
                }
                else if (member.IsProperty())
                {
                    type = ((PropertyInfo)member).PropertyType;
                }
            }

            return type;
        }

        public static bool TryGetMemberType(object target, string text, out Type memberType)
        {
            memberType = null;

            if (target == null)
            {
                return false;
            }

            memberType = target.GetType();

            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            var parts = text.Split(new char[]
            {
                '.'
            }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var member = GetMember(memberType, part, (BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static), true);

                if (member == null)
                {
                    return false;
                }

                if (member.IsField())
                {
                    memberType = ((FieldInfo)member).FieldType;
                }
                else if (member.IsProperty())
                {
                    memberType = ((PropertyInfo)member).PropertyType;
                }
            }

            return true;
        }

        public static object GetValue(object target, string text)
        {
            if (target == null || text == null || text.Length < 1)
            {
                return target;
            }

            return GetValue(target, text.Split('.'), null);
        }

        public static object GetValue(object target, string text, Action<ObjectResolvingContext> resolve)
        {
            if (target == null || text == null || text.Length < 1)
            {
                return target;
            }

            return GetValue(target, text.Split('.'), resolve);
        }

        public static object GetValue(object target, string[] memberNames)
        {
            if (target == null || memberNames == null || memberNames.Length < 1)
            {
                return target;
            }

            return GetValue(target, memberNames, 0, memberNames.Length, null);
        }

        public static object GetValue(object target, string[] memberNames, Action<ObjectResolvingContext> resolve)
        {
            if (target == null || memberNames == null || memberNames.Length < 1)
            {
                return target;
            }

            return GetValue(target, memberNames, 0, memberNames.Length, resolve);
        }

        public static object GetValue(object target, string[] memberNames, int start, int length, Action<ObjectResolvingContext> resolve)
        {
            if (target == null || memberNames == null || memberNames.Length < 1)
            {
                return target;
            }

            if (start < 0 || start >= memberNames.Length)
            {
                throw new ArgumentOutOfRangeException("start");
            }

            //创建解析上下文对象
            ObjectResolvingContext context = new ObjectResolvingContext(target, string.Join(".", memberNames));

            for (int i = 0; i < Math.Min(memberNames.Length - start, length); i++)
            {
                string memberName = memberNames[start + i];

                if (memberName == null || memberName.Trim().Length < 1)
                {
                    continue;
                }

                if (context.Value == null)
                {
                    continue;
                }

                context.Handled = true;
                context.MemberName = memberName;
                context.Container = context.Value;
                context.Value = null;

                //解析对象成员
                if (resolve == null)
                {
                    DefaultResolve(context);
                }
                else
                {
                    resolve(context);

                    if (!context.Handled)
                    {
                        DefaultResolve(context);
                    }
                }

                //如果解析被终止则退出后续解析
                if (context.IsTerminated)
                {
                    break;
                }
            }

            return context.Value;
        }

        #endregion

        #region 设置方法

        public static void SetValue(object target, string text, object value)
        {
            SetValue(target, text, () => value, null);
        }

        public static void SetValue(object target, string text, Func<object> valueThunk)
        {
            SetValue(target, text, valueThunk, null);
        }

        public static void SetValue(object target, string text, object value, Action<ObjectResolvingContext> resolve)
        {
            SetValue(target, text, () => value, resolve);
        }

        public static void SetValue(object target, string text, Func<object> valueThunk, Action<ObjectResolvingContext> resolve)
        {
            if (target == null || text == null || text.Length < 1)
                return;

            SetValue(target, text.Split('.'), valueThunk, resolve);
        }

        public static void SetValue(object target, string[] memberNames, object value)
        {
            SetValue(target, memberNames, () => value, null);
        }

        public static void SetValue(object target, string[] memberNames, Func<object> valueThunk)
        {
            SetValue(target, memberNames, valueThunk, null);
        }

        public static void SetValue(object target, string[] memberNames, object value, Action<ObjectResolvingContext> resolve)
        {
            SetValue(target, memberNames, () => value, resolve);
        }

        public static void SetValue(object target, string[] memberNames, Func<object> valueThunk, Action<ObjectResolvingContext> resolve)
        {
            if (valueThunk == null)
                throw new ArgumentNullException("valueThunk");

            if (target == null || memberNames == null || memberNames.Length < 1)
                return;

            object container = target;

            if (memberNames.Length > 1)
            {
                container = GetValue(target, memberNames, 0, memberNames.Length - 1, resolve);

                if (container == null)
                    throw new InvalidOperationException(string.Format("The '{0}' member value is null of '{1}' type.", string.Join(".", memberNames, 0, memberNames.Length - 1), target.GetType().FullName));
            }

            //创建构件解析上下文对象
            var context = new ObjectResolvingContext(target, container, memberNames[memberNames.Length - 1], valueThunk, string.Join(".", memberNames));

            //调用解析回调方法
            if (resolve == null)
                DefaultResolve(context);
            else
            {
                resolve(context);

                if (!context.Handled)
                    DefaultResolve(context);
            }

            var member = context.Member;

            if (member == null)
                throw new KeyNotFoundException(string.Format("The '{0}' member is not exists in the '{1}' type.", context.MemberName, context.Container.GetType().FullName));

            if (member.IsField())
            {
                ((FieldInfo)member).SetValue(context.Container, ConvertValue(context.Value, ((FieldInfo)member).FieldType));
            }
            else if (member.IsProperty())
            {
                ((PropertyInfo)member).SetValue(context.Container, ConvertValue(context.Value, ((PropertyInfo)member).PropertyType), null);
            }
        }

        #endregion

        #region 私有方法

        private static readonly Action<ObjectResolvingContext> DefaultResolve = (ctx) =>
        {
            if (ctx.Direction == ObjectResolvingDirection.Get)
            {
                ctx.Value = ctx.GetMemberValue();
            }
        };

        private static MemberInfo GetMember(Type type, string name, BindingFlags? binding = null, bool ignoreCase = true)
        {
            object[] index;
            return GetMember(type, name, binding, ignoreCase, out index);
        }

        private static MemberInfo GetMember(Type type, string name, BindingFlags? binding, bool ignoreCase, out object[] index)
        {
            index = null;

            if (type == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (!binding.HasValue)
            {
                binding = (BindingFlags.Public | BindingFlags.Instance);
            }

            var members = type.GetMembers(binding.Value);

            foreach (var member in members)
            {
                if (!member.IsField() && !member.IsProperty())
                {
                    continue;
                }

                string indexer;
                bool isString;

                if (member.IsProperty() && IsIndexer(name, out indexer, out isString))
                {
                    var parameters = ((PropertyInfo)member).GetIndexParameters();
                    object value;

                    if (parameters.Length == 1)
                    {
                        if (isString && parameters[0].ParameterType == typeof(string))
                        {
                            index = new object[]
                            {
                                indexer
                            };
                            return member;
                        }
                        else if (parameters[0].ParameterType != typeof(string) && Converter.TryConvertValue(indexer, parameters[0].ParameterType, out value))
                        {
                            index = new object[]
                            {
                                value
                            };
                            return member;
                        }
                    }
                }
                else
                {
                    if (string.Equals(name, member.Name, (ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)))
                    {
                        return member;
                    }
                }
            }

            return null;
        }

        private static bool IsIndexer(string name, out string indexer, out bool isString)
        {
            isString = false;

            if (name.Length >= 4)
            {
                int endPosition = name.Length - 1;

                if ((name[0] == '[' && name[1] == '"' && name[endPosition - 1] == '"' && name[endPosition] == ']') || (name[0] == '[' && name[1] == '\'' && name[endPosition - 1] == '\'' && name[endPosition] == ']'))
                {
                    isString = true;
                    indexer = name.Substring(2, name.Length - 4);
                    return true;
                }
            }
            else if (name.Length >= 2)
            {
                if (name[0] == '[' && name[name.Length - 1] == ']')
                {
                    indexer = name.Substring(1, name.Length - 2);
                    return true;
                }
            }

            indexer = name;
            return false;
        }

        #endregion

        #region 嵌套子类

        /// <summary>
        /// 表示对象成员的解析方向。
        /// </summary>
        public enum ObjectResolvingDirection
        {
            /// <summary>
            /// 获取对象的成员值。
            /// </summary>
            Get,

            /// <summary>
            /// 设置对象的成员值。
            /// </summary>
            Set,
        }

        /// <summary>
        /// 表示在对象成员解析程序中的操作上下文。
        /// </summary>
#if NET452 || NET46
        public class ObjectResolvingContext : MarshalByRefObject
#else
        public class ObjectResolvingContext
#endif
        {
            #region 私有字段

            private ObjectResolvingDirection _direction;
            private object _target;
            private object _container;
            private MemberInfo _member;
            private string _text;
            private string _memberName;
            private object[] _memberParameters;
            private object _value;
            private int _valueEvaluated;
            private Func<object> _valueThunk;
            private bool _handled;
            private bool _isTerminated;

            #endregion

            #region 公共属性

            /// <summary>
            /// 获取解析过程中当前处理的方向。
            /// </summary>
            public ObjectResolvingDirection Direction
            {
                get
                {
                    return _direction;
                }
            }

            /// <summary>
            /// 获取解析程序的目标根对象。
            /// </summary>
            public object Target
            {
                get
                {
                    return _target;
                }
            }

            /// <summary>
            /// 获取解析过程中当前成员的容器对象。
            /// </summary>
            public object Container
            {
                get
                {
                    return _container;
                }
                internal set
                {
                    _container = value;

                    //必须重置当前的解析成员
                    _member = null;
                }
            }

            /// <summary>
            /// 获取解析的文本参数值。
            /// </summary>
            public string Text
            {
                get
                {
                    return _text;
                }
            }

            /// <summary>
            /// 获取或设置一个操作的值，该属性在不同场景中所表示的含义和可设置性均不同。详情请参考备注。
            /// </summary>
            /// <remarks>
            ///		<para>当<see cref="Direction"/>属性值等于<seealso cref="ObjectResolvingDirection.Get"/>时，表示处理程序所解析出来的成员值。</para>
            ///		<para>当<see cref="Direction"/>属性值等于<seealso cref="ObjectResolvingDirection.Set"/>时，表示是由用户指定要设置的目标值。</para>
            /// </remarks>
            /// <exception cref="System.InvalidOperationException">当<see cref="Direction"/>属性值不等于<seealso cref="ObjectResolvingDirection.Get"/>时激发。</exception>
            public object Value
            {
                get
                {
                    if (System.Threading.Interlocked.CompareExchange(ref _valueEvaluated, 1, 0) == 0)
                        _value = _valueThunk();

                    return _value;
                }
                set
                {
                    _value = value;
                }
            }

            /// <summary>
            /// 获取当前解析的成员名称。
            /// </summary>
            public string MemberName
            {
                get
                {
                    return _memberName;
                }
                internal set
                {
                    _memberName = value;

                    //必须重置当前的解析成员
                    _member = null;
                }
            }

            /// <summary>
            /// 获取当前解析的成员信息。
            /// </summary>
            public MemberInfo Member
            {
                get
                {
                    if (_member == null)
                    {
                        if (_container == null || string.IsNullOrWhiteSpace(_memberName))
                        {
                            return null;
                        }

                        _member = GetMember(_container.GetType(), _memberName, null, true, out _memberParameters);
                    }

                    return _member;
                }
            }

            /// <summary>
            /// 获取当前解析成员的类型。
            /// </summary>
            public Type MemberType
            {
                get
                {
                    var member = this.Member;

                    if (member == null)
                    {
                        return null;
                    }

                    if (member.IsField())
                    {
                        return ((FieldInfo)member).FieldType;
                    }
                    else if (member.IsProperty())
                    {
                        return ((PropertyInfo)member).PropertyType;
                    }
                    else if (member.IsMethod())
                    {
                        return ((MethodInfo)member).ReturnType;
                    }

                    return null;
                }
            }

            /// <summary>
            /// 获取或设置处理完成标记。
            /// </summary>
            /// <remarks>
            ///		<para>如果设置该属性为真(true)，表示自定义解析程序已经完成对当前成员的解析，则表示告知系统不要再对当前成员的进行解析处理了；</para>
            ///		<para>如果设置该属性为假(false)，即默认值。表示自定义自定义解析程序未对当前成员进行解析，则意味将由系统对当前成员进行解析处理。</para>
            /// </remarks>
            public bool Handled
            {
                get
                {
                    return _handled;
                }
                set
                {
                    _handled = value;
                }
            }

            /// <summary>
            /// 获取或设置是否终止标记。
            /// </summary>
            public bool IsTerminated
            {
                get
                {
                    return _isTerminated;
                }
                set
                {
                    _isTerminated = value;
                }
            }

            #endregion

            #region 构造方法

            internal ObjectResolvingContext(object target, string text)
            {
                if (target == null)
                {
                    throw new ArgumentNullException("target");
                }

                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new ArgumentNullException("text");
                }

                _direction = ObjectResolvingDirection.Get;
                _target = target;
                _container = target;
                _value = target;
                _valueEvaluated = 1;
                _text = text;
                _handled = true;
            }

            internal ObjectResolvingContext(object target, object container, string memberName, Func<object> valueThunk, string text)
            {
                if (target == null)
                    throw new ArgumentNullException("target");

                if (string.IsNullOrWhiteSpace(text))
                    throw new ArgumentNullException("text");

                if (valueThunk == null)
                    throw new ArgumentNullException("valueThunk");


                _direction = ObjectResolvingDirection.Set;
                _target = target;
                _container = container;
                _memberName = memberName;
                _valueThunk = valueThunk;
                _text = text;
                _handled = true;
            }

            #endregion

            #region 公共方法

            public object GetMemberValue()
            {
                return this.GetMemberValue(this.Container);
            }

            public object GetMemberValue(object container)
            {
                var member = this.Member;

                if (member == null)
                {
                    if (container == null)
                    {
                        throw new KeyNotFoundException(string.Format("The '{0}' member is not exists for '{1}' text.", this.MemberName, this.Text));
                    }
                    else
                    {
                        throw new KeyNotFoundException(string.Format("The '{0}' member is not exists in the '{1}' type.", this.MemberName, container.GetType().FullName));
                    }
                }

                if (member.IsField())
                {
                    return ((FieldInfo)member).GetValue(container);
                }
                else if (member.IsProperty())
                {
                    return ((PropertyInfo)member).GetValue(container, _memberParameters);
                }
                else if (member.IsMethod())
                {
                    return ((MethodInfo)member).Invoke(container, _memberParameters);
                }

                throw new InvalidOperationException(string.Format("Invalid '{0}' member for '{1}' text.", this.MemberName, this.Text));
            }

            #endregion
        }

        #endregion

        #endregion
    }
}