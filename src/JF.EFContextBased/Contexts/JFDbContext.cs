using Microsoft.EntityFrameworkCore;
using System;

namespace JF.EFContextBased
{
    /// <summary>
    /// AMS默认数据上下文
    /// </summary>
    public abstract class JFDbContext : DbContext
    {
        private EnumSqlType sqlType;
        private string connectionString;

        #region contructors

        public JFDbContext() : base() { }

        public JFDbContext(DbContextOptions options) : base(options) { }

        public JFDbContext(EnumSqlType sqlType, string connectionString)
            : this(new DbOptions { SqlType = sqlType, ConnectionString = connectionString })
        { }

        public JFDbContext(DbOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrEmpty(options.ConnectionString)) throw new ArgumentNullException(nameof(options.ConnectionString));

            this.sqlType = options.SqlType;
            this.connectionString = options.ConnectionString;
        }

        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                base.OnConfiguring(optionsBuilder);
                return;
            }

            if (sqlType == default(EnumSqlType)) sqlType = EnumSqlType.SqlServer;

            if (sqlType == EnumSqlType.SqlServer)
            {
                optionsBuilder.UseSqlServer(this.connectionString, options => options.EnableRetryOnFailure());
            }
            else if (sqlType == EnumSqlType.MySql)
            {
                optionsBuilder.UseMySQL(this.connectionString);
            }
        }
    }
}
