using JF.ComponentModel;
using JF.DataBased.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JF.DataBased.Repository
{
    /// <summary>
    /// 主仓外观类
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    public class RepositoryFacede : IRepository
    {
        #region private variables

        private readonly IRepository repository;

        #endregion

        #region contructors

        public RepositoryFacede(IDbContext dbContext)
        {
            if (dbContext is EFDbContext)
            {
                repository = new EFRepositoryBase(dbContext as EFDbContext);
            }
            else if (dbContext is DapperDbContext)
            {
                repository = new DapperRepositoryBase(dbContext as DapperDbContext);
            }
            else
            {
                throw new Exception("不是有效的DbContext类型。");
            }

            this.DbContext = dbContext;
        }

        #endregion

        #region properties

        public IDbContext DbContext { get; }

        #endregion

        #region IRepository Support

        public IQueryable<T> All<T>() where T : DataEntity
        {
            return repository.All<T>();
        }

        public void Delete<T>(T entity) where T : DataEntity
        {
            repository.Delete(entity);
        }

        public void Delete<T>(Expression<Func<T, bool>> conditions) where T : DataEntity
        {
            repository.Delete(conditions);
        }

        public int ExecuteSqlCommand(string sql)
        {
            return repository.ExecuteSqlCommand(sql);
        }

        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        public int ExecuteSqlCommand(string sql, params object[] paramters)
        {
            return repository.ExecuteSqlCommand(sql, paramters);
        }

        public IEnumerable<T> Query<T>(string sql, params object[] paramters) where T : class, new()
        {
            return repository.Query<T>(sql, paramters);
        }

        public IEnumerable<T> Search<T>(string sql, object paramters) where T : DataEntity, new()
        {
            return repository.Search<T>(sql, paramters);
        }

        public List<T> Search<T>(Expression<Func<T, bool>> conditions = null) where T : DataEntity
        {
            return repository.Search(conditions);
        }

        public List<T> Search<T, S>(Expression<Func<T, bool>> conditions, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount) where T : DataEntity
        {
            return repository.Search(conditions, orderBy, pageSize, pageIndex, out totalCount);
        }

        public T Find<T>(params object[] keyValues) where T : DataEntity
        {
            return repository.Find<T>(keyValues);
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> conditions) where T : DataEntity
        {
            return repository.FirstOrDefault(conditions);
        }

        public bool Exists<T>(Expression<Func<T, bool>> conditions) where T : DataEntity
        {
            return repository.Exists(conditions);
        }

        public void Insert<T>(T entity) where T : DataEntity
        {
            repository.Insert(entity);
        }

        public void Update<T>(T entity) where T : DataEntity
        {
            repository.Update(entity);
        }

        #region 子仓操作

        public bool TryGetChild<T>(out IChildRepository<T> repository) where T : DataEntity
        {
            return this.repository.TryGetChild<T>(out repository);
        }

        public IChildRepository<T> GetChild<T>() where T : DataEntity
        {
            return repository.GetChild<T>();
        }

        /// <summary>
        /// 获取实际的子仓类实例。
        /// </summary>
        /// <typeparam name="TDataEntity"></typeparam>
        /// <typeparam name="TChildRepositoryResult"></typeparam>
        /// <returns></returns>
        public TChildRepositoryResult GetChild<TDataEntity, TChildRepositoryResult>()
            where TDataEntity : DataEntity
            where TChildRepositoryResult : class, IChildRepository<TDataEntity>
        {
            return repository.GetChild<TDataEntity, TChildRepositoryResult>();
        }

        #endregion

        public int SaveChanges()
        {
            return repository.SaveChanges();
        }

        #endregion

        #region IDisposable Support
        protected bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    this.repository?.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~RepositoryFacede() {
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
