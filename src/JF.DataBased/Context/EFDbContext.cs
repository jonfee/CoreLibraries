using Microsoft.EntityFrameworkCore;
using System;
using System.Data;

namespace JF.DataBased.Context
{
    /// <summary>
    /// 使用EF做为ORM时的数据库上下文
    /// </summary>
    public class EFDbContext : DbContext, IDbContext
    {
        #region private variables

        private DbConnectOptions options;

        #endregion

        #region contructors

        /// <summary>
        /// 实例化一个对象实例。
        /// </summary>
        public EFDbContext() : base() { }

        /// <summary>
        /// 实例化一个对象实例，提供实例化参数信息。
        /// </summary>
        /// <param name="options"></param>
        public EFDbContext(DbContextOptions options) : base(options) { }

        /// <summary>
        /// 实例化一个对象实例，提供实例化参数信息。
        /// </summary>
        /// <param name="options"></param>
        public EFDbContext(DbConnectOptions options) : base()
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            if (this.options.SqlType == default(DataBaseType)) throw new ArgumentOutOfRangeException(nameof(this.options.SqlType), "不是有效的数据库类型");
            if (string.IsNullOrEmpty(options.ConnectionString)) throw new ArgumentNullException(nameof(options.ConnectionString));
        }

        #endregion

        #region properties

        /// <summary>
        /// 数据库连接对象
        /// </summary>
        IDbConnection IDbContext.Connection => this.Database?.GetDbConnection();

        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (string.IsNullOrWhiteSpace(this.options.ConnectionString))
            {
                base.OnConfiguring(optionsBuilder);
                return;
            }

            if (this.options.SqlType == DataBaseType.SqlServer)
            {
                optionsBuilder.UseSqlServer(this.options.ConnectionString, options => options.EnableRetryOnFailure());

            }
            else if (options.SqlType == DataBaseType.MySql)
            {
                optionsBuilder.UseMySQL(this.options.ConnectionString);
            }
        }
    }
}
