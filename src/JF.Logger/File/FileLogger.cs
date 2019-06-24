using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JF.Logger
{
    /// <summary>
    /// 文件日志记录器。
    /// 目前默认日志文件格式为TXT文件。
    /// </summary>
    public class FileLogger : ILogger
    {
        #region private variables
        static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();
        private FileLogOptions options;

        /// <summary>
        /// 当前待处理日志集合，以存储文件名为Key。
        /// </summary>
        private ConcurrentDictionary<string, List<LogMessage>> fileLogs;

        /// <summary>
        /// 日志消费定时器。
        /// </summary>
        private System.Threading.Timer consumeTimer;

        #endregion

        #region contructors

        /// <summary>
        /// 初始化一个<see cref="FileLogger"/>对象实例。
        /// </summary>
        /// <param name="options"></param>
        public FileLogger(FileLogOptions options)
        {
            this.options = options;
            this.fileLogs = new ConcurrentDictionary<string, List<LogMessage>>();

            // 定时消费日志并保存
            consumeTimer = new System.Threading.Timer(
                new System.Threading.TimerCallback(DoWorking),
                null,
                this.options.Period,
                this.options.Period);
        }

        #endregion

        #region Interface Implementation

        /// <summary>
        /// 信息日志写入
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public virtual void Info(string title, string content)
        {
            Write(LogLevel.INFO, title, content);
        }

        /// <summary>
        /// 警告信息写入
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public virtual void Waring(string title, string content)
        {
            Write(LogLevel.WARN,title,content);
        }

        /// <summary>
        /// 错误信息写入
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public virtual void Error(string title, string content)
        {
            Write(LogLevel.ERROR, title, content);
        }

        /// <summary>
        /// 调试信息写入
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public virtual void Debug(string title, string content)
        {
            Write(LogLevel.DEBUG, title, content);
        }

        /// <summary>
        /// 致命错误信息写入
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public virtual void Fatal(string title, string content)
        {
            Write(LogLevel.FATAL, title, content);
        }

        public virtual void Write(LogLevel level,string title,string content)
        {
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(content)) return;

            var message = new LogMessage
            {
                AppCode = string.Empty,
                AppName = string.Empty,
                AppVersion = string.Empty,
                OSBit = Environment.Is64BitOperatingSystem ? 64 : 32,
                OSVersion = Environment.OSVersion.VersionString,
                Level = level,
                Title = title,
                Details = content,
                LogTime = DateTime.Now
            };

            Write(message);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="message"></param>
        public void Write(LogMessage message)
        {
            if (message == null) return;

            string fileName = GetStoreFileName(message);

            if (this.fileLogs.ContainsKey(fileName))
            {
                this.fileLogs.TryGetValue(fileName, out var logs);
                logs.Add(message);
            }
            else
            {
                this.fileLogs.TryAdd(fileName, new List<LogMessage> { message });
            }
        }

        #endregion

        #region private behavious

        /// <summary>
        /// 获取当前<paramref name="message"/>对应的绝对文件名称。
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string GetStoreFileName(LogMessage message)
        {
            string baseRoot = AppDomain.CurrentDomain.BaseDirectory;
            string level = message.Level.ToString();
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString().PadLeft(2, '0');
            string day = DateTime.Now.Day.ToString().PadLeft(2, '0');
            string hour = DateTime.Now.Hour.ToString().PadLeft(2, '0');
            string minute = DateTime.Now.Minute / 30 == 0 ? "00" : "30";

            string fileRoot = Path.Combine(baseRoot, this.options.FileRoot, level, year, month);


            switch (this.options.StoreMode)
            {
                case FileStoreMode.LevelAfterHour:
                    fileRoot = Path.Combine(fileRoot, day, hour);
                    break;
                case FileStoreMode.LevelAfterHalfAnHour:
                    fileRoot = Path.Combine(fileRoot, day, hour + minute);
                    break;
                case FileStoreMode.LevelAfterDay:
                default:
                    fileRoot = Path.Combine(fileRoot, day);
                    break;
            }

            return $"{fileRoot}.txt";
        }

        /// <summary>
        /// 开始工作。
        /// 定时消费一个时间内的日志。
        /// </summary>
        private void DoWorking(object state)
        {
            if (this.fileLogs.Count < 1) return;

            var fileName = this.fileLogs.Keys.ElementAt(0);

            if (this.fileLogs.TryRemove(fileName, out var logs))
            {
                SaveAsync(fileName, logs).Wait();
            }
        }

        /// <summary>
        /// 保存日志到文件
        /// </summary>
        /// <param name="fileName">存储的文件名称</param>
        /// <param name="logs">需要存储的日志信息</param>
        /// <returns></returns>
        private async Task SaveAsync(string fileName, List<LogMessage> logs)
        {
            StringBuilder data = new StringBuilder();

            foreach (var log in logs.OrderBy(p => p.LogTime))
            {
                data.Append($"\r\n【{log.Level} # {log.LogTime.ToString()} # {log.Title}】");
                data.Append($"\r\nEnvironment：{log.AppName}/{log.AppCode} # 版本:{log.AppVersion} # 操作系统:{log.OSVersion} {log.OSBit}");
                data.Append(log.Details);
            }

            try
            {
                var dir = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                using (var sw = new StreamWriter(fs))
                {
                    await sw.WriteAsync(data.ToString());
                    await sw.FlushAsync();
                    sw.Close();
                    fs.Close();
                }
            }
            catch
            {
                // No Code
            }
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    this.consumeTimer.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                this.fileLogs.Clear();

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~FileLogger() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
