namespace JF.Logger
{
    /// <summary>
    /// 日志等级
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 一般性信息
        /// </summary>
        INFO = 1,
        /// <summary>
        /// 调试信息
        /// </summary>
        DEBUG = 2,
        /// <summary>
        /// 警告信息
        /// </summary>
        WARN = 4,
        /// <summary>
        /// 错误信息
        /// </summary>
        ERROR = 8,
        /// <summary>
        /// 严重问题
        /// </summary>
        FATAL = 16
    }
}
