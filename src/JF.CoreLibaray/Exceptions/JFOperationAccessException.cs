using System;
using System.Runtime.Serialization;

namespace JF.Exceptions
{
    /// <summary>
    /// 操作权限验证异常
    /// </summary>
    public class JFOperationAccessException : JFException
    {
        public JFOperationAccessException() : base() { }


        public JFOperationAccessException(string message) : base(message) { }

        public JFOperationAccessException(string message, Exception innerException) : base(message, innerException) { }


        protected JFOperationAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
