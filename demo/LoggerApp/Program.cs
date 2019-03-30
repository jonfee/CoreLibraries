using JF.Logger;
using System;
using System.Threading.Tasks;

namespace LoggerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ILogger logger = new FileLogger(new FileLogOptions
            {
                FileRoot = "logs",
                Period = 60000,
                StoreMode = FileStoreMode.LevelAfterHalfAnHour
            });

            Task[] tasks = new Task[10];

            for (var i = 0; i < 10; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    GenerateLogs(logger);
                });
            }

            Task.WaitAll(tasks);
        }

        private static void GenerateLogs(ILogger logger)
        {
            while (true)
            {
                var level = RandomLevel();
                var details = GetDetails(level);
                logger.Write(new LogMessage
                {
                    AppName = "测试Logger",
                    AppCode = "Logger",
                    AppVersion = "v1.0.2",
                    Level = level,
                    Details = details.Item2,
                    Title = details.Item1,
                    OSVersion = "Windows NT 10.0",
                    OSBit = 64,
                    LogTime = DateTime.Now
                });

                System.Threading.Thread.Sleep(200);
            }
        }

        private static LogLevel RandomLevel()
        {
            var levels = Enum.GetValues(typeof(LogLevel));
            var idx = new Random(Guid.NewGuid().GetHashCode()).Next(levels.Length);
            return (LogLevel)Enum.Parse(typeof(LogLevel), levels.GetValue(idx).ToString());
        }

        private static Tuple<string, string> GetDetails(LogLevel level)
        {
            string title = "", details = "";

            switch (level)
            {
                case LogLevel.DEBUG:
                    title = "这是一个调试信息。";
                    details = "鞋柜防守打法阿萨法，阿斯，蒂芬工阿斯蒂芬基本面基本的";
                    break;
                case LogLevel.ERROR:
                    var ex = GenerateException();
                    title = ex.Message;
                    details = ex.StackTrace;
                    break;
                case LogLevel.FATAL:
                    title = "这是一个非常严重的问题。";
                    details = "要不是潘金莲推开窗户，也不会被正好经过的西门庆看到，要不是大郎没有去卖烧饼，潘金莲也不敢让西门庆勾搭上。";
                    break;
                case LogLevel.INFO:
                    title = "192.168.0.12 帅小伙 对用户密码进行重置。";
                    details = "调用API: http://www.baidu.com/api/user/password 请求方式: POST 请求参数：{userid:123,password:'123456'}";
                    break;
                case LogLevel.WARN:
                    title = "这是一个警告信息。";
                    details = "请这位小姐矜持一点。";
                    break;
            }

            return new Tuple<string, string>(title, details);
        }

        private static Exception GenerateException()
        {
            try
            {
                var b = 0;
                var a = 2 / b;
            }
            catch (Exception ex)
            {
                return ex;
            }
            return null;
        }
    }
}
