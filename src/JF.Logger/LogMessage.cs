using System;

namespace JF.Logger
{
    /// <summary>
    /// 日志消息
    /// </summary>
    public class LogMessage
    {
        /// <summary>
        /// 日志等级
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// 日志标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 日志详情
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// 应用程序（软件）名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 应用程序（软件）编号
        /// </summary>
        public string AppCode { get; set; }

        /// <summary>
        /// 软件版本
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// 操作系统版本
        /// </summary>
        public string OSVersion { get; set; }

        /// <summary>
        /// 操作系统位数
        /// </summary>
        public int OSBit { get; set; }

        /// <summary>
        /// 日志记录时间
        /// </summary>
        public DateTime LogTime { get; set; }
    }
}
