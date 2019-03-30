using System;
using System.Runtime.Serialization;

namespace JF.Exceptions
{
    /// <summary>
    /// 业务处理或规则异常
    /// </summary>
    public class JFBusinessException : JFException
    {
        public JFBusinessException() : base() { }


        public JFBusinessException(string message) : base(message) { }

        public JFBusinessException(string message, Exception innerException) : base(message, innerException) { }


        protected JFBusinessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
