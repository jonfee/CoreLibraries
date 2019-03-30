using System;
using System.ComponentModel;
using System.Reflection;

namespace JF.Common
{
    /// <summary>
    /// 基于<see cref="System.Object"/>的扩展类
    /// </summary>
    public static class ObjectExtensions
    {
        #region 对象复制

        /// <summary>
        /// 深度复制对象为指定类型<typeparamref name="TResult"/>的新对象
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="obj">源对象</param>
        /// <returns></returns>
        public static TResult Copy<TResult>(this object source) where TResult : class, new()
        {
            var result = new TResult();

            source.CopyTo(result);

            return result;
        }

        /// <summary>
        /// 深度复制数据对象
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static object Copy(this object source)
        {
            object target;
            Type targetType = source.GetType();

            if (targetType.IsValueType
                || typeof(string).IsAssignableFrom(targetType))
            {
                target = source;
            }
            else
            {
                Assembly assembly = Assembly.GetAssembly(targetType);
                target = assembly.CreateInstance(targetType.FullName);
                //target = Activator.CreateInstance(targetType);

                var memberCollection = source.GetType().GetMembers();

                foreach (var member in memberCollection)
                {
                    if (member.MemberType == MemberTypes.Field)
                    {
                        var field = (FieldInfo)member;
                        var fieldValue = field.GetValue(source);

                        if (fieldValue is ICloneable)
                        {
                            field.SetValue(target, (fieldValue as ICloneable).Clone());
                        }
                        else
                        {
                            field.SetValue(target, Copy(fieldValue));
                        }
                    }
                    else if (member.MemberType == MemberTypes.Property)
                    {
                        var property = (PropertyInfo)member;
                        var methodInfo = property.GetSetMethod(false);

                        if (methodInfo != null)
                        {
                            var propertyValue = property.GetValue(source, null);

                            if (propertyValue is ICloneable)
                            {
                                property.SetValue(target, (propertyValue as ICloneable).Clone(), null);
                            }
                            else
                            {
                                property.SetValue(target, Copy(propertyValue), null);
                            }
                        }
                    }
                }
            }

            return target;
        }

        /// <summary>
        /// 将源对象属性复制到目标对象
        /// </summary>
        /// <typeparam name="TResult">目标对象类型</typeparam>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象</param>
        /// <returns></returns>
        public static void CopyTo<TResult>(this object source, out TResult target) where TResult : class, new()
        {
            target = new TResult();

            source.CopyTo(target);
        }

        /// <summary>
        /// 将源对象属性复制到目标对象
        /// </summary>
        /// <typeparam name="TResult">目标对象类型</typeparam>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象</param>
        /// <returns></returns>
        public static void CopyTo<TResult>(this object source, TResult target) where TResult : class, new()
        {
            if (source == null || target == null) return;

            PropertyDescriptorCollection fromProperties = TypeDescriptor.GetProperties(source);
            PropertyDescriptorCollection toProperties = TypeDescriptor.GetProperties(target);

            foreach (PropertyDescriptor fromProperty in fromProperties)
            {
                var toProperty = toProperties.Find(fromProperty.Name, true);

                if (toProperty != null && !toProperty.IsReadOnly)
                {
                    bool isDirectlyAssignable = toProperty.PropertyType.IsAssignableFrom(fromProperty.PropertyType);

                    object fromValue = fromProperty.GetValue(source);

                    if (isDirectlyAssignable || fromValue != null)
                    {
                        var toValue = fromValue.Copy();
                        try
                        {
                            if (fromProperty.PropertyType.IsEnum)
                            {
                                toProperty.SetValue(target, Convert.ChangeType(toValue, toProperty.PropertyType));
                            }
                            else if (fromProperty.PropertyType.IsValueType
                                || typeof(string).IsAssignableFrom(fromProperty.PropertyType))
                            {
                                toProperty.SetValue(target, Convert.ChangeType(toValue.ToString(), toProperty.PropertyType));
                            }
                            else
                            {
                                var toTypeValue = Assembly.GetAssembly(toProperty.PropertyType).CreateInstance(toProperty.PropertyType.FullName);

                                fromValue.CopyTo(toTypeValue);
                                toProperty.SetValue(target, toTypeValue);
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        #endregion

        #region 类型转换

        public static T ConvertTo<T>(this object value)
        {
            return Converter.ConvertValue<T>(value);
        }

        public static T ConvertTo<T>(this object value, T defaultValue)
        {
            return Converter.ConvertValue<T>(value, defaultValue);
        }

        public static T ConvertTo<T>(this object value, Func<object> defaultValueThunk)
        {
            return Converter.ConvertValue<T>(value, defaultValueThunk);
        }

        public static object ConvertTo(this object value, Type conversionType)
        {
            return Converter.ConvertValue(value, conversionType);
        }

        public static object ConvertTo(this object value, Type conversionType, object defaultValue)
        {
            return Converter.ConvertValue(value, conversionType, defaultValue);
        }

        public static object ConvertTo(this object value, Type conversionType, Func<object> defaultValueThunk)
        {
            return Converter.ConvertValue(value, conversionType, defaultValueThunk);
        }

        public static bool TryConvertTo<T>(this object value, out T result)
        {
            return Converter.TryConvertValue<T>(value, out result);
        }

        public static bool TryConvertTo(object value, Type conversionType, out object result)
        {
            return Converter.TryConvertValue(value, conversionType, out result);
        }

        #endregion

        #region 对象解析

        public static Type GetMemberType(this object target, string text)
        {
            return Converter.GetMemberType(target, text);
        }

        public static bool TryGetMemberType(this object target, string text, out Type memberType)
        {
            return Converter.TryGetMemberType(target, text, out memberType);
        }

        public static object GetValue(this object target, string text)
        {
            return Converter.GetValue(target, text);
        }

        public static object GetValue(this object target, string[] memberNames)
        {
            return Converter.GetValue(target, memberNames);
        }

        public static void SetValue(this object target, string text, object value)
        {
            Converter.SetValue(target, text, value);
        }

        public static void SetValue(this object target, string[] memberNames, object value)
        {
            Converter.SetValue(target, memberNames, value);
        }

        #endregion
    }
}
