using Dapper;
using JF.DataBased.Context;
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
    /// 使用了<see cref="DapperDbContext"/>数据连接的仓储基类。
    /// </summary>
    internal class DapperRepositoryBase : RepositoryBase<DapperDbContext>
    {
        #region contructors

        public DapperRepositoryBase(DapperDbContext context) : base(context)
        {
        }

        #endregion

        #region public behavious

        public override IQueryable<T> All<T>()
        {
            return DbContext.Set<T>().All().AsQueryable();
        }

        public override int Update<T>(T entity, bool delay = false)
        {
            if (!entity.CanUpdate(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            return DbContext.Set<T>().Update(entity);
        }

        public override int Insert<T>(T entity, bool delay = false)
        {
            if (!entity.CanInsert(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            return DbContext.Set<T>().Insert(entity);
        }

        public override int Delete<T>(T entity, bool delay = false)
        {
            if (!entity.CanDelete(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            return DbContext.Set<T>().Delete(entity);
        }

        public override int Delete<T>(Expression<Func<T, bool>> conditions, bool delay = false)
        {
            int count = 0;
            var list = Search<T>(conditions);
            foreach (var item in list)
            {
                count += Delete<T>(item);
            }
            return count;
        }

        public override T FirstOrDefault<T>(Expression<Func<T, bool>> conditions)
        {
            return DbContext.Set<T>().FirstOrDefault(conditions);
        }

        public override bool Exists<T>(Expression<Func<T, bool>> conditions)
        {
            return DbContext.Set<T>().Exists(conditions);
        }

        public override T Find<T>(params object[] keyValues)
        {
            return DbContext.Set<T>().Find(keyValues.FirstOrDefault());
        }

        public override IEnumerable<T> Search<T>(string sql, object paramters = null)
        {
            return DbContext.Set<T>().Search(sql, paramters);
        }

        public override IEnumerable<T> Search<T>(Expression<Func<T, bool>> conditions = null)
        {
            if (conditions != null)
            {
                return DbContext.Set<T>().Search(conditions).ToList();
            }
            else
            {
                return All<T>().ToList();
            }
        }

        public override IEnumerable<T> Search<T, S>(Expression<Func<T, bool>> conditions, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount)
        {
            return DbContext.Set<T>().Search(pageIndex, pageSize, out totalCount, conditions, orderBy, true);
        }

        public override IEnumerable<T> Query<T>(string sql, object paramters = null)
        {
            return DbContext.Database.Query<T>(sql, paramters);
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
        /// <param name="transaction"></param>
        /// <returns></returns>
        public override int ExecuteSqlCommand(string sql, object paramters = null, IDbTransaction transaction = null)
        {
            return DbContext.Database.ExecuteSqlCommand(sql, paramters,transaction);
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
