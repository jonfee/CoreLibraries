﻿using JF.ComponentModel;
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
    public class ChildRepositoryFacede<TEntity> : IChildRepository<TEntity> where TEntity : DataEntity,new()
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

        public int Delete(TEntity entity)
        {
           return repository.Delete(entity);
        }

        public int Delete(Expression<Func<TEntity, bool>> conditions)
        {
           return repository.Delete(conditions);
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

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> conditions)
        {
            return repository.FirstOrDefault(conditions);
        }

        public bool Exists(Expression<Func<TEntity, bool>> conditions)
        {
            return repository.Exists(conditions);
        }

        public TEntity Find(params object[] keyValues)
        {
            return repository.Find(keyValues);
        }

        public int Insert(TEntity entity)
        {
           return repository.Insert(entity);
        }

        public virtual IEnumerable<TEntity> Search(string sql, object paramters)
        {
            return repository.Search(sql, paramters);
        }

        public List<TEntity> Search(Expression<Func<TEntity, bool>> conditions = null)
        {
            return repository.Search(conditions);
        }

        public List<TEntity> Search<S>(Expression<Func<TEntity, bool>> conditions, Expression<Func<TEntity, S>> orderBy, int pageSize, int pageIndex, out int totalCount)
        {
            return repository.Search(conditions, orderBy, pageSize, pageIndex, out totalCount);
        }

        public int Update(TEntity entity)
        {
           return repository.Update(entity);
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
