namespace JF.Logger
{
    /// <summary>
    /// 文件日志参数配置类
    /// </summary>
    public class FileLogOptions : ILogOptions
    {
        /// <summary>
        /// 在应用程序根目录+当前<see cref="FileRoot"/>目录下写入日志
        /// </summary>
        public string FileRoot { get; set; } = "logs";

        /// <summary>
        /// 日志文件分片存储模式，
        /// 可选项：
        /// <see cref="FileStoreMode.Normal"/> | <see cref="FileStoreMode.LevelAfterDay"/> | <see cref="FileStoreMode.LevelAfterHour"/> | <see cref="FileStoreMode.LevelAfterHalfAnHour"/>
        /// </summary>
        public FileStoreMode StoreMode { get; set; } = FileStoreMode.Normal;

        /// <summary>
        /// 日志存储周期，单位：毫秒。
        /// </summary>
        public int Period { get; set; } = 60000;

        /// <summary>
        /// 日志输出格式，可选项：<see cref="LogFormat.Default"/> | <see cref="LogFormat.Json"/>
        /// </summary>
        public LogFormat Format { get; set; } = LogFormat.Json;
    }

    /// <summary>
    /// 日志存储格式
    /// </summary>
    public enum LogFormat
    {
        /// <summary>
        /// 默认
        /// </summary>
        Default = 0,
        /// <summary>
        /// Json
        /// </summary>
        Json = 1
    }

    /// <summary>
    /// 日志文件分片存储模式
    /// </summary>
    public enum FileStoreMode
    {
        /// <summary>
        /// 默认模式（<see cref="LevelAfterDay"/>按等级后每天存储）
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 按等级后每天存储
        /// </summary>
        LevelAfterDay = 1,

        /// <summary>
        /// 按等级后每小时存储
        /// </summary>
        LevelAfterHour = 2,

        /// <summary>
        /// 按等级后每半小时
        /// </summary>
        LevelAfterHalfAnHour = 4
    }
}
