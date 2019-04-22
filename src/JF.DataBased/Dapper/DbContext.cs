using JF.Common;
using JF.DataBased;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper
{
    /// <summary>
    /// 使用Dapper作为ORM时的数据库上下文
    /// </summary>
    public class DbContext : DatabaseFacade
    {
        #region private variables

        private DatabaseFacade _dataBase;

        private List<object> _addedEntities;

        private List<object> _updatedEntities;

        private List<object> _deletedEntities;

        #endregion

        #region contructors

        /// <summary>
        /// 实例化一个对象实例。
        /// </summary>
        public DbContext() : this(null) { }

        /// <summary>
        /// 实例化一个对象实例，提供实例化参数信息。
        /// </summary>
        /// <param name="options"></param>
        public DbContext(DbConnectOptions options)
        {
            OnConfiguring(options);

            _addedEntities = new List<object>();
            _updatedEntities = new List<object>();
            _deletedEntities = new List<object>();
        }

        #endregion

        #region protperties

        /// <summary>
        /// Provides access to database related information and operations for this context.
        /// </summary>
        public virtual DatabaseFacade Database
        {
            get
            {
                CheckDisposed();

                return _dataBase ?? (_dataBase = new DatabaseFacade(this));
            }
        }

        #endregion

        /// <summary>
        /// 加载配置
        /// </summary>
        protected virtual void OnConfiguring(DbConnectOptions options)
        {
            if (Connection == null)
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
        }

        #region IDbContext Support

        public void AddRange(IEnumerable<object> entities)
        {
            AddRange(entities);
        }

        public void AddRange(params object[] entities)
        {
            if (entities == null) return;

            foreach (var entity in entities)
            {
                _addedEntities.Add(entity);
            }
        }

        public void RemoveRange(IEnumerable<object> entities)
        {
            RemoveRange(entities);
        }

        public void RemoveRange(params object[] entities)
        {
            if (entities == null) return;

            foreach (var entity in entities)
            {
                _deletedEntities.Add(entity);
            }
        }

        public void UpdateRange(IEnumerable<object> entities)
        {
            UpdateRange(entities);
        }

        public void UpdateRange(params object[] entities)
        {
            if (entities == null) return;

            foreach (var entity in entities)
            {
                _updatedEntities.Add(entity);
            }
        }

        public int ExecuteSqlCommand(string sql, params object[] paramters)
        {
            return this.Database.ExecuteSqlCommand(sql, paramters);
        }

        public IEnumerable<T> Query<T>(string sql, params object[] paramters) where T : class, new()
        {
            return this.Database.Query<T>(sql, paramters).ToList();
        }

        public IEnumerable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class
        {
            return this.Set<TEntity>().Search(expression);
        }

        public virtual IEnumerable<T> ProcQuery<T>(string procName, params object[] paramters) where T : class, new()
        {
            return this.Database.ProcQuery<T>(procName, paramters).ToList();
        }

        public int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return SaveChangesAsync(acceptAllChangesOnSuccess).Result;
        }

        public int SaveChanges()
        {
            return SaveChanges(false);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SaveChangesAsync(false, cancellationToken);
        }

        public async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run<int>(() =>
             {
                 int count = 0;

                 try
                 {
                     var databaseType = this.Database.GetType();

                     using (var transaction = Database.BeginTransaction())
                     {
                         foreach (var entity in _addedEntities)
                         {
                             count += TryExecute(entity, "Insert");
                         }

                         foreach (var entity in _updatedEntities)
                         {
                             count += TryExecute(entity, "Update");
                         }

                         foreach (var entity in _deletedEntities)
                         {
                             count += TryExecute(entity, "Delete");
                         }
                     }
                 }
                 catch { }

                 return count;
             });
        }

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="methodName">执行方法，仅限：Insert,Update,Delete</param>
        /// <returns></returns>
        private int TryExecute(object entity, string methodName)
        {
            int count = 0;

            try
            {
                var setMethod = Database.GetType().GetMethod("Set").MakeGenericMethod(entity.GetType());
                var set = setMethod?.Invoke(Database, null);

                // Type.GetType($"Dapper.Database.Table<{entity.GetType()}>");
                Type setType = typeof(DbSet<>).GetGenericTypeDefinition().MakeGenericType(entity.GetType());

                var exeMethod = new DynamicMethod(methodName, typeof(int), new Type[] { entity.GetType() }, setType.Module);
                var rst = exeMethod.Invoke(set, new[] { entity });

                if (rst != null)
                {
                    count = rst.ConvertTo<int>(0);
                }
            }
            catch { }

            return count;
        }

        [DebuggerStepThrough]
        private void CheckDisposed()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().Name);
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
                    this.Connection?.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~DapperDbContext() {
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
