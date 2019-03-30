using System;
using System.Runtime.Serialization;

namespace JF.Exceptions
{
    /// <summary>
    /// 自定义异常基类
    /// </summary>
    public class JFException : Exception
    {
        public JFException() : base() { }


        public JFException(string message) : base(message) { }

        public JFException(string message, Exception innerException) : base(message, innerException) { }


        protected JFException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
