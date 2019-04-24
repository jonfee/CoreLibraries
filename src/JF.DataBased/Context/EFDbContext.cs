using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

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
        public EFDbContext(DbContextOptions<EFDbContext> options) : base(options) { }

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

        public int ExecuteSqlCommand(string sql, params object[] paramters)
        {
            return this.Database.ExecuteSqlCommand(sql, paramters);
        }

        public IEnumerable<T> Query<T>(string sql, params object[] paramters) where T : class, new()
        {
            return this.Database.SqlQuery<T>(sql,paramters).ToList();
        }

        public IEnumerable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class
        {
            return this.Set<TEntity>().AsNoTracking().Where(expression);
        }
        
        public virtual IEnumerable<T> ProcQuery<T>(string procName, params object[] paramters) where T : class, new()
        {
            return this.Database.ProcdureQuery<T>(procName, paramters).ToList();
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
            if (string.IsNullOrWhiteSpace(this.options?.ConnectionString))
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
                optionsBuilder.UseMySql(this.options.ConnectionString);
            }
        }
    }
}
