using JF.ComponentModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JF.DataBased.Context
{
    /// <summary>
    /// 数据库上下文接口
    /// </summary>
    public interface IDbContext : IDisposable
    {
        /// <summary>
        /// 获取当前连接<see cref="IDbConnection"/>对象
        /// </summary>
        IDbConnection Connection { get; }

        #region 数据读取/写入相关接口

        void AddRange(IEnumerable<object> entities);

        void AddRange(params object[] entities);

        void RemoveRange(IEnumerable<object> entities);

        void RemoveRange(params object[] entities);

        void UpdateRange(IEnumerable<object> entities);

        void UpdateRange(params object[] entities);

        int ExecuteSqlCommand(string sql, params object[] paramters);

        IEnumerable<T> Query<T>(string sql, params object[] paramters) where T : class, new();

        IEnumerable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class;

        IEnumerable<T> ProcQuery<T>(string procName, params object[] paramters) where T : class, new();

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">
        /// Indicates whether Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges
        /// is called after the changes have been sent successfully to the database.
        /// </param>
        /// <returns></returns>
        int SaveChanges(bool acceptAllChangesOnSuccess);

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        int SaveChanges();

        /// <summary>
        ///  Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">Indicates whether Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges
        ///  is called after the changes have been sent successfully to the database.
        /// </param>
        /// <param name="cancellationToken">
        /// A System.Threading.CancellationToken to observe while waiting for the task to complete.
        /// </param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">
        /// A System.Threading.CancellationToken to observe while waiting for the task to
        /// complete.
        /// </param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        #endregion
    }
}
