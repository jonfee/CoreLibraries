﻿using JF.ComponentModel;
using JF.DataBased.Context;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JF.DataBased.Repository
{
    /// <summary>
    /// 使用了<see cref="DapperDbContext"/>数据连接的子仓抽象基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DapperChildRepositoryBase<T> : IChildRepository<T> where T : DataEntity
    {
        #region private variables

        protected readonly DapperDbContext dbContext;

        #endregion

        public DapperChildRepositoryBase(DapperDbContext context)
        {
            this.dbContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual IQueryable<T> All()
        {
            return dbContext.Set<T>().Search().AsQueryable();
        }

        public virtual int Update(T entity)
        {
            if (!entity.CanUpdate(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            dbContext.Set<T>().Update(entity);

            return dbContext.SaveChanges();
        }

        public virtual int Insert(T entity)
        {
            if (!entity.CanInsert(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            dbContext.Set<T>().Insert(entity);

            return dbContext.SaveChanges();
        }

        public virtual int Delete(T entity)
        {
            if (!entity.CanDelete(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            dbContext.Set<T>().Delete(entity);

            return dbContext.SaveChanges();
        }

        public virtual int Delete(Expression<Func<T, bool>> conditions)
        {
            Delete(conditions);

            return dbContext.SaveChanges();
        }

        public virtual T FirstOrDefault(Expression<Func<T, bool>> conditions)
        {
            return dbContext.Set<T>().FirstOrDefault(conditions);
        }

        public virtual bool Exists(Expression<Func<T, bool>> conditions)
        {
            return dbContext.Set<T>().Exists(conditions);
        }

        public virtual T Find(params object[] keyValues)
        {
            return dbContext.Set<T>().Find(keyValues.FirstOrDefault());
        }

        public virtual List<T> Search(Expression<Func<T, bool>> conditions = null)
        {
            if (conditions != null)
            {
                return dbContext.Set<T>().Search(conditions).ToList();
            }
            else
            {
                return All().ToList();
            }
        }

        public virtual IEnumerable<T> Search(string sql, object paramters)
        {
            return dbContext.Database.Query<T>(sql, paramters);
        }

        public virtual IEnumerable<T> Search(string where = null, object param = null, string ordering = null)
        {
            return dbContext.Set<T>().Search(where, param, ordering);
        }

        public virtual List<T> Search<S>(Expression<Func<T, bool>> conditions, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount)
        {
            var queryList = conditions == null
                ? All()
                : dbContext.Set<T>().Search(conditions);

            totalCount = queryList.Count();

            return queryList.AsQueryable().OrderByDescending(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public virtual int ExecuteSqlCommand(string sql)
        {
            return dbContext.ExecuteSqlCommand(sql);
        }

        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        public int ExecuteSqlCommand(string sql, params object[] paramters)
        {
            return dbContext.ExecuteSqlCommand(sql,paramters);
        }

        #region IDisposable Support
        protected bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~ChildRepositoryBase() {
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
