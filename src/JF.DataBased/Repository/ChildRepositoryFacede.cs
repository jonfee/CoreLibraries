using JF.ComponentModel;
using JF.DataBased.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JF.DataBased.Repository
{
    /// <summary>
    /// 子仓外观类
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    public class ChildRepositoryFacede<TEntity> : IChildRepository<TEntity> where TEntity : DataEntity
    {
        #region private variables

        private readonly IChildRepository<TEntity> repository;

        #endregion

        #region contructors

        public ChildRepositoryFacede(IDbContext dbContext)
        {
            if (dbContext is EFDbContext)
            {
                repository = new EFChildRepositoryBase<TEntity>(dbContext as EFDbContext);
            }
            else if (dbContext is DapperDbContext)
            {
                repository = new DapperChildRepositoryBase<TEntity>(dbContext as DapperDbContext);
            }
            else
            {
                throw new Exception("不是有效的DbContext类型。");
            }
        }

        #endregion

        #region IClildRepotitory Support

        public IQueryable<TEntity> All()
        {
            return repository.All();
        }

        public void Delete(TEntity entity)
        {
            repository.Delete(entity);
        }

        public void Delete(Expression<Func<TEntity, bool>> conditions)
        {
            repository.Delete(conditions);
        }

        public int ExecuteSqlCommand(string sql)
        {
            return repository.ExecuteSqlCommand(sql);
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> conditions)
        {
            return repository.FirstOrDefault(conditions);
        }

        public TEntity Find(params object[] keyValues)
        {
            return repository.Find(keyValues);
        }

        public void Insert(TEntity entity)
        {
            repository.Insert(entity);
        }

        public List<TEntity> Search(Expression<Func<TEntity, bool>> conditions = null)
        {
            return repository.Search(conditions);
        }

        public List<TEntity> Search<S>(Expression<Func<TEntity, bool>> conditions, Expression<Func<TEntity, S>> orderBy, int pageSize, int pageIndex, out int totalCount)
        {
            return repository.Search(conditions, orderBy, pageSize, pageIndex, out totalCount);
        }

        public void Update(TEntity entity)
        {
            repository.Update(entity);
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
        // ~ChildRepositoryFacede() {
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
