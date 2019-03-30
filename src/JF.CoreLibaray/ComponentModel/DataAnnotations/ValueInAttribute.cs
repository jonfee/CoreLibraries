using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace JF.ComponentModel.DataAnnotations
{
    /// <summary>
    /// 自定义值包含验证特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class ValueInAttribute : ValidationAttribute
    {
        private IEnumerable<object> values;

        public ValueInAttribute(IEnumerable<object> values)
        {
            this.values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public override bool IsValid(object value)
        {
            return values.Contains(value);
        }
    }
}
