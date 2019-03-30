using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JF.Json
{
    /// <summary>
    /// 。
    /// </summary>
    public class JsonSettingsHandler
    {
        #region Default SerializerSettings

        private readonly static object locker = new object();
        private static JsonSettingsHandler current;
        private static JsonSerializerSettings defaultSerializerSettings;

        /// <summary>
        /// 默认配置程序
        /// </summary>
        public static JsonSettingsHandler Default
        {
            get
            {
                if (current == null)
                {
                    lock (locker)
                    {
                        if (current == null)
                        {
                            current = new JsonSettingsHandler();

                            // 时区
                            current.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                            // 时间格式
                            current.SerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                            current.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";

                            // 属性名为驼峰式
                            current.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

                            // 处理长整性数据（js中精度丢失问题）,转换为字符串
                            current.SerializerSettings.Converters.Add(new Int64Convert());
                        }
                    }
                }
                return current;
            }
        }

        /// <summary>
        /// 默认Json序列化/反序列化处理配置。
        /// 1、采用Utc时区；
        /// 2、使用微软时间格式化，"yyyy-MM-dd HH:mm:ss"；
        /// 3、使用驼峰式属性名；
        /// 4、长整型转化为字符串。
        /// </summary>
        public static JsonSerializerSettings DefaultSerializerSettings
        {
            get
            {
                if (defaultSerializerSettings == null)
                {
                    lock (locker)
                    {
                        if (defaultSerializerSettings==null)
                        {
                            defaultSerializerSettings = Default.SerializerSettings;
                        }
                    }
                }

                return defaultSerializerSettings;
            }
        }

        #endregion

        public JsonSerializerSettings SerializerSettings { get; private set; }

        public JsonSettingsHandler()
        {
            SerializerSettings = new JsonSerializerSettings();
        }

        public JsonSerializerSettings AddConverter<TConverter>(TConverter converter) where TConverter : Newtonsoft.Json.JsonConverter
        {
            if (converter != null)
            {
                this.SerializerSettings.Converters.Add(converter);
            }

            return SerializerSettings;
        }

        public JsonSerializerSettings AddConvert<TConverter>() where TConverter : Newtonsoft.Json.JsonConverter, new()
        {
            SerializerSettings.Converters.Add(new TConverter());

            return SerializerSettings;
        }
    }
}
