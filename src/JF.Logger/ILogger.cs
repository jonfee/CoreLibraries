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

        /// <summary>
        /// 信息日志写入
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        void Info(string title, string content);

        /// <summary>
        /// 警告信息写入
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        void Waring(string title, string content);

        /// <summary>
        /// 致命错误写入
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        void Fatal(string title, string content);

        /// <summary>
        /// 调试信息写入
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        void Debug(string title, string content);

        /// <summary>
        /// 错误信息写入
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        void Error(string title, string content);
    }
}
