using System;
using System.Runtime.Serialization;

namespace JF.Exceptions
{
    /// <summary>
    /// 访问权限异常
    /// </summary>
    public class JFAuthorizationException : JFException
    {
        public JFAuthorizationException() : base() { }


        public JFAuthorizationException(string message) : base(message) { }

        public JFAuthorizationException(string message, Exception innerException) : base(message, innerException) { }


        protected JFAuthorizationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
