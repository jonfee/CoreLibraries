using System;

namespace JF.Logger
{
    /// <summary>
    /// 日志记录器接口
    /// </summary>
    public interface ILogger : IDisposable
    {
        /// <summary>
        /// 日志写入
        /// </summary>
        /// <param name="message"></param>
        void Write(LogMessage message);
    }
}
