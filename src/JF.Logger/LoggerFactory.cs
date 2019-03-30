using System;

namespace JF.Logger
{
    /// <summary>
    /// 日志记录器工厂
    /// </summary>
    public sealed class LoggerFactory
    {
        /// <summary>
        /// 创建一个日志记录器
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static ILogger CreateLogger(ILogOptions options)
        {
            ILogger logger = null;

            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options is FileLogOptions)
            {
                logger = CreateFileLogger(options as FileLogOptions);
            }

            return logger;
        }

        /// <summary>
        /// 创建一个文件日志记录器
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static FileLogger CreateFileLogger(FileLogOptions options = null)
        {
            return new FileLogger(options ?? new FileLogOptions());
        }
    }
}
