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
    /// 使用了<see cref="DapperDbContext"/>数据连接的仓储基类。
    /// </summary>
    internal class DapperRepositoryBase : RepositoryBase<DapperDbContext>
    {
        #region contructors

        public DapperRepositoryBase(DapperDbContext context):base(context)
        {
        }

        #endregion

        #region public behavious

        public override IQueryable<T> All<T>()
        {
            return DbContext.Set<T>().All().AsQueryable();
        }

        public override void Update<T>(T entity)
        {
            if (!entity.CanUpdate(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            DbContext.Set<T>().Update(entity);
        }

        public override void Insert<T>(T entity)
        {
            if (!entity.CanInsert(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            DbContext.Set<T>().Insert(entity);
        }

        public override void Delete<T>(T entity)
        {
            if (!entity.CanDelete(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            DbContext.Set<T>().Delete(entity);
        }

        public override void Delete<T>(Expression<Func<T, bool>> conditions)
        {
            var list = Search<T>(conditions);
            foreach (var item in list)
            {
                Delete<T>(item);
            }
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

        public override IEnumerable<T> Search<T>(string sql, object paramters)
        {
            return DbContext.Set<T>().Search(sql, paramters);
        }

        public override List<T> Search<T>(Expression<Func<T, bool>> conditions = null)
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

        public override List<T> Search<T, S>(Expression<Func<T, bool>> conditions, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount)
        {
            var queryList = conditions == null
                ? All<T>()
                : DbContext.Set<T>().Search(conditions);

            totalCount = queryList.Count();

            return queryList.AsQueryable().OrderByDescending(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public override int ExecuteSqlCommand(string sql)
        {
            return DbContext.ExecuteSqlCommand(sql);
        }

        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        public override int ExecuteSqlCommand(string sql, params object[] paramters)
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
