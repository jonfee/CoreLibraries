using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;

namespace JF.DataBased.Context
{
    /// <summary>
    /// 使用Dapper做为ORM时的数据库上下文
    /// </summary>
    public class DapperDbContext : DbContext, IDbContext
    {
        #region private variables
        

        #endregion

        #region contructors

        /// <summary>
        /// 实例化一个对象实例。
        /// </summary>
        public DapperDbContext() : base() { }

        /// <summary>
        /// 实例化一个对象实例，提供实例化参数信息。
        /// </summary>
        /// <param name="options"></param>
        public DapperDbContext(DbConnectOptions options) : base(options)
        {
        }

        #endregion

        #region properties

        /// <summary>
        /// 数据库连接对象
        /// </summary>
        IDbConnection IDbContext.Connection => this.Database?.GetDbConnection();

        #endregion

        #region 

        protected override void OnConfiguring(DbConnectOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.SqlType == default(DataBaseType)) throw new ArgumentOutOfRangeException(nameof(options.SqlType));
            if (string.IsNullOrEmpty(options.ConnectionString)) throw new ArgumentNullException(nameof(options.ConnectionString));

            switch (options.SqlType)
            {
                case DataBaseType.MySql:
                    this.Connection = new MySqlConnection(options.ConnectionString);
                    break;
                case DataBaseType.SqlServer:
                    this.Connection = new SqlConnection(options.ConnectionString);
                    break;
            }
        }

        #endregion
    }
}
