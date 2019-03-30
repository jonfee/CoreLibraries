using JF.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JF.EFContextBased
{
    /// <summary>
    /// 仓储基类。
    /// </summary>
    public class RepositoryBase : IRepository
    {
        #region private variables

        private Hashtable childRepositories;

        #endregion

        #region contructors

        public RepositoryBase(JFDbContext context)
        {
            this.DbContext = context ?? throw new ArgumentNullException(nameof(context));
            this.childRepositories = Hashtable.Synchronized(new Hashtable());
        }

        #endregion

        #region properties

        /// <summary>
        /// 上下文对象。
        /// </summary>
        public JFDbContext DbContext { get; }

        #endregion

        #region public behavious

        public virtual IQueryable<T> All<T>()
            where T : DataEntity
        {
            return DbContext.Set<T>().AsNoTracking();
        }

        public virtual void Update<T>(T entity)
            where T : DataEntity
        {
            if (!entity.CanUpdate(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            var entry = DbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                DbContext.Set<T>().Attach(entity);
            }
            entry.State = EntityState.Modified;
        }

        public virtual void Insert<T>(T entity)
            where T : DataEntity
        {
            if (!entity.CanInsert(out Hashtable errors)) throw new Exception(JsonConvert.SerializeObject(errors));

            DbContext.Set<T>().Add(entity);
        }

        public virtual void Delete<T>(T entity)
            where T : DataEntity
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

        public virtual void Delete<T>(Expression<Func<T, bool>> conditions)
            where T : DataEntity
        {
            var list = Find<T>(conditions);
            foreach (var item in list)
            {
                Delete<T>(item);
            }
        }

        public virtual T Get<T>(Expression<Func<T, bool>> conditions)
            where T : DataEntity
        {
            return All<T>().FirstOrDefault(conditions);
        }

        public virtual List<T> Find<T>(Expression<Func<T, bool>> conditions = null)
            where T : DataEntity
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

        public virtual List<T> Find<T, S>(Expression<Func<T, bool>> conditions, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount)
            where T : DataEntity
        {
            var queryList = conditions == null ?
                All<T>() :
                All<T>().Where(conditions);

            totalCount = queryList.Count();

            return queryList.OrderByDescending(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        public virtual List<T> FromSql<T>(string sql)
            where T : DataEntity
        {
            return DbContext.Set<T>().FromSql(sql).ToList();
        }

        public virtual int ExecuteSqlCommand(string sql)
        {
            return DbContext.Database.ExecuteSqlCommand(sql);
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
            TChildRepositoryResult realRepository = default(TChildRepositoryResult);

            try
            {
                if (TryGetChild<TDataEntity>(out var repository))
                {
                    if (repository is TChildRepositoryResult)
                    {
                        realRepository = repository as TChildRepositoryResult;
                    }
                }
            }
            catch
            {
                realRepository = default(TChildRepositoryResult);
            }

            return realRepository;
        }

        /// <summary>
        /// 获取指定的子仓。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IChildRepository<T> GetChild<T>() where T : DataEntity
        {
            if (!TryGetChild<T>(out var repository))
            {
                repository = null;
            }

            return repository;
        }

        /// <summary>
        /// 获取指定的子仓
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <returns></returns>
        public bool TryGetChild<T>(out IChildRepository<T> repository) where T : DataEntity
        {
            repository = null;

            try
            {
                var entityType = typeof(T);

                if (childRepositories.ContainsKey(entityType))
                {
                    repository = childRepositories[entityType] as IChildRepository<T>;
                }
                else
                {
                    repository = CreateChildRepository<T>();

                    childRepositories.Add(entityType, repository);
                }
            }
            catch
            {
                repository = null;
            }

            return repository != null;
        }


        public virtual int SaveChanges()
        {
            return DbContext.SaveChanges();
        }

        /// <summary>
        /// 创建一个默认子仓实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private IChildRepository<T> CreateChildRepository<T>() where T : DataEntity
        {
            IChildRepository<T> repository = default(IChildRepository<T>);
            
            var typed = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(t =>t.IsClass && t.GetInterfaces().Contains(typeof(IChildRepository<T>))))
                .FirstOrDefault();

            if (typed != null)
            {
                repository = Activator.CreateInstance(typed, new object[] { DbContext }) as IChildRepository<T>;
            }

            return repository;
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
