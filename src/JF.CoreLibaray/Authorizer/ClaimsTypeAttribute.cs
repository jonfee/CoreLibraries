using System;

namespace JF.Authorizer
{
    /// <summary>
    /// 自定义用于Claims的特性
    /// </summary>
    internal class ClaimsTypeAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
