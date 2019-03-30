namespace JF.EFContextBased
{
    /// <summary>
    /// 数据库连接配置参数
    /// </summary>
    public class DbOptions
    {
        /// <summary>
        /// 数据库类型，枚举：<see cref="EnumSqlType"/>
        /// </summary>
        public EnumSqlType SqlType { get; set; }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
