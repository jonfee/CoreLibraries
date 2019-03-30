using JF.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace JF.ComponentModel
{
    /// <summary>
    /// 有需要验证的类型抽象类
    /// </summary>
    public abstract class ValidationModel
    {
        /// <summary>
        /// 验证对象是否有效。
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid()
        {
            return IsValid(out Hashtable errors);
        }

        /// <summary>
        /// 验证数据模型，当验证失败时抛出异常。
        /// </summary>
        public virtual void ThrowExceptionWhenValidationFailed()
        {
            IsValid(out JFValidateException exception);

            if (exception != null) throw exception;
        }

        /// <summary>
        /// 验证对象是否有效
        /// </summary>
        /// <param name="exception">如果验证失败，则输出未通过验证的属性名及错误信息的异常消息。</param>
        /// <returns></returns>
        public virtual bool IsValid(out JFValidateException exception)
        {
            exception = null;
            bool success = IsValid(out Hashtable errors);

            if (!success)
            {
                string jsonErrors = JsonConvert.SerializeObject(errors);
                exception = new JFValidateException(jsonErrors);
            }

            return success;
        }

        /// <summary>
        /// 验证对象是否有效。
        /// </summary>
        /// <param name="errors">如果验证失败，则输出未通过验证的属性名及错误信息。</param>
        /// <returns></returns>
        public virtual bool IsValid(out Hashtable errors)
        {
            errors = Validate();

            return errors.Keys.Count == 0;
        }

        /// <summary>
        /// 自定义验证。
        /// 请以属性名及对应的错误信息List输出。
        /// </summary>
        /// <returns></returns>
        protected virtual Hashtable CustomValidate()
        {
            return null;
        }

        /// <summary>
        /// 验证当前对象中所有带<see cref="ValidationAttribute"/>特性的属性是否验证有效。
        /// </summary>
        /// <returns></returns>
        protected Hashtable Validate()
        {
            // 属性特性验证
            var htErrors = FeatureAttributiesValidate();

            // 自定义验证
            var cusErrors = CustomValidate();

            // 合并验证结果
            if (cusErrors != null && cusErrors.Keys.Count > 0)
            {
                foreach (var key in cusErrors.Keys)
                {
                    if (htErrors.ContainsKey(key))
                    {
                        var cusItemErrors = cusErrors[key] as List<string>;
                        var htItemErrors = htErrors[key] as List<string>;

                        htItemErrors.AddRange(cusItemErrors);
                    }
                }
            }

            return htErrors;
        }

        /// <summary>
        /// 属性特性验证
        /// </summary>
        /// <returns></returns>
        private Hashtable FeatureAttributiesValidate()
        {
            var htErrors = new Hashtable();
            List<string> errors = null;

            PropertyInfo[] properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);


            foreach (PropertyInfo pi in properties)
            {
                Attribute[] attrs = Attribute.GetCustomAttributes(pi, typeof(ValidationAttribute), false);

                if (attrs == null || attrs.Length < 1) continue;

                object value = pi.GetValue(this);

                errors = new List<string>();

                foreach (Attribute attr in attrs)
                {
                    ValidationAttribute validateAttr = attr as ValidationAttribute;

                    if (validateAttr == null) continue;

                    if (!validateAttr.IsValid(value))
                    {
                        errors.Add(validateAttr.ErrorMessage);
                    }
                }

                if (errors.Any()) htErrors.Add(pi.Name, errors);
            }

            return htErrors;
        }
    }
    
}
