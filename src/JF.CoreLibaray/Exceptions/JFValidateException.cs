using System;
using System.Runtime.Serialization;

namespace JF.Exceptions
{
    /// <summary>
    /// 数据验证异常类
    /// </summary>
    public class JFValidateException : JFException
    {
        public JFValidateException() : base() { }


        public JFValidateException(string message) : base(message) { }

        public JFValidateException(string message, Exception innerException) : base(message, innerException) { }


        protected JFValidateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
