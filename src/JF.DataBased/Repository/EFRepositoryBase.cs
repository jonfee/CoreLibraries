using JF.DataBased.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JF.DataBased.Repository
{
    /// <summary>
    /// 使用了<see cref="EFDbContext"/>数据连接的仓储基类。
    /// </summary>
    internal class EFRepositoryBase : RepositoryBase<EFDbContext>
    {
        #region contructors

        public EFRepositoryBase(EFDbContext context) : base(context)
        {

        }

        #endregion
        
        #region public behavious

        public override IQueryable<T> All<T>()
        {
            return DbContext.Set<T>().AsNoTracking();
        }

        public override void Update<T>(T entity)
        {
            if (!entity.CanUpdate(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            var entry = DbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                DbContext.Set<T>().Attach(entity);
            }
            entry.State = EntityState.Modified;
        }

        public override void Insert<T>(T entity)
        {
            if (!entity.CanInsert(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            DbContext.Set<T>().Add(entity);
        }

        public override void Delete<T>(T entity)
        {
            if (!entity.CanDelete(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            var entry = DbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                DbContext.Set<T>().Attach(entity);
            }
            entry.State = EntityState.Deleted;
            DbContext.Set<T>().Remove(entity);
        }

        public override void Delete<T>(Expression<Func<T, bool>> conditions)
        {
            var list = Find<T>(conditions);
            foreach (var item in list)
            {
                Delete<T>(item);
            }
        }

        public override T Get<T>(Expression<Func<T, bool>> conditions)
        {
            return All<T>().FirstOrDefault(conditions);
        }

        public override List<T> Find<T>(Expression<Func<T, bool>> conditions = null)
        {
            if (conditions != null)
            {
                return All<T>().Where(conditions).ToList();
            }
            else
            {
                return All<T>().ToList();
            }
        }

        public override List<T> Find<T, S>(Expression<Func<T, bool>> conditions, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount)
        {
            var queryList = conditions == null ?
                All<T>() :
                All<T>().Where(conditions);

            totalCount = queryList.Count();

            return queryList.OrderByDescending(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public override int ExecuteSqlCommand(string sql)
        {
            return DbContext.Database.ExecuteSqlCommand(sql);
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
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                this.childRepositories.Clear();

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~RepositoryBase() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public override void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
