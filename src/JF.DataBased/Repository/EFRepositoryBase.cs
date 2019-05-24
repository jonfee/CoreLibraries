using JF.DataBased.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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

        public override int Update<T>(T entity, bool delay = false)
        {
            if (!entity.CanUpdate(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            var entry = DbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                DbContext.Set<T>().Attach(entity);
            }
            entry.State = EntityState.Modified;

            return delay ? 0 : DbContext.SaveChanges();
        }

        public override int Insert<T>(T entity, bool delay = false)
        {
            if (!entity.CanInsert(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            DbContext.Set<T>().Add(entity);

            return delay ? 0 : DbContext.SaveChanges();
        }

        public override int Delete<T>(T entity, bool delay = false)
        {
            if (!entity.CanDelete(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            var entry = DbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                DbContext.Set<T>().Attach(entity);
            }
            entry.State = EntityState.Deleted;
            DbContext.Set<T>().Remove(entity);

            return delay ? 0 : DbContext.SaveChanges();
        }

        public override int Delete<T>(Expression<Func<T, bool>> conditions, bool delay = false)
        {
            int count = 0;
            var list = Search<T>(conditions);
            foreach (var item in list)
            {
                count += Delete<T>(item, delay);
            }

            return count;
        }

        public override T FirstOrDefault<T>(Expression<Func<T, bool>> conditions)
        {
            return All<T>().FirstOrDefault(conditions);
        }

        public override bool Exists<T>(Expression<Func<T, bool>> conditions)
        {
            return All<T>().Count(conditions) > 0;
        }

        public override T Find<T>(params object[] keyValues)
        {
            return DbContext.Set<T>().Find(keyValues);
        }

        public override IEnumerable<T> Search<T>(string sql, object paramters = null)
        {
            return DbContext.Query<T>(sql, paramters);
        }

        public override IEnumerable<T> Search<T>(Expression<Func<T, bool>> conditions = null)
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

        public override IEnumerable<T> Search<T, S>(Expression<Func<T, bool>> conditions, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount)
        {
            var queryList = conditions == null ?
                All<T>() :
                All<T>().Where(conditions);

            totalCount = queryList.Count();

            return queryList.OrderByDescending(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public override IEnumerable<T> Query<T>(string sql, object paramters = null)
        {
            return DbContext.Query<T>(sql, paramters);
        }

        public override int ExecuteSqlCommand(string sql)
        {
            return DbContext.Database.ExecuteSqlCommand(sql);
        }

        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramters"></param>
        /// <param name="transaction">暂无意义，请忽略。</param>
        /// <returns></returns>
        public override int ExecuteSqlCommand(string sql, object paramters = null, IDbTransaction transaction = null)
        {
            return DbContext.ExecuteSqlCommand(sql, paramters);
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

        #endregion
    }
}
