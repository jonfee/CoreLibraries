namespace JF.DataBased
{
    /// <summary>
    /// 数据库信息配置参数
    /// </summary>
    public class DbOptions
    {
        /// <summary>
        /// 数据库类型，枚举：<see cref="DataBaseType"/>
        /// </summary>
        public DataBaseType SqlType { get; set; }

        /// <summary>
        /// ORM方式，枚举：<see cref="ORMType"/>
        /// </summary>
        public ORMType OrmType { get; set; }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
    }

    /// <summary>
    /// 数据库连接参数
    /// </summary>
    public class DbConnectOptions
    {
        /// <summary>
        /// 数据库类型，枚举：<see cref="DataBaseType"/>
        /// </summary>
        public DataBaseType SqlType { get; set; }
        
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 将<see cref="DbOptions"/>类型隐式转换为新的<see cref="DbConnectOptions"/>对象实例并返回。
        /// </summary>
        /// <param name="option"></param>
        public static implicit operator DbConnectOptions(DbOptions option)
        {
            if (option == null) return null;

            return new DbConnectOptions
            {
                SqlType = option.SqlType,
                ConnectionString = option.ConnectionString
            };
        }
    }
}
