using JF.ComponentModel;
using JF.DataBased.Context;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace JF.DataBased.Repository
{
    public abstract class RepositoryBase<TDbContext> : IRepository where TDbContext : class, IDbContext
    {
        #region private variables

        protected Hashtable childRepositories;

        #endregion

        #region contructors

        public RepositoryBase(TDbContext dbContext)
        {
            this.DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.childRepositories = Hashtable.Synchronized(new Hashtable());
        }

        #endregion

        #region properties

        /// <summary>
        /// ORM类型
        /// </summary>
        public ORMType OrmType
        {
            get
            {
                ORMType type = default(ORMType);

                if (DbContext is EFDbContext)
                {
                    type = ORMType.EF;
                }
                else if (DbContext is DapperDbContext)
                {
                    type = ORMType.Dapper;
                }

                return type;
            }
        }

        /// <summary>
        /// 上下文对象。
        /// </summary>
        public TDbContext DbContext { get; }

        IDbContext IRepository.DbContext => this.DbContext;

        #endregion

        #region IRepository Support

        public abstract IQueryable<T> All<T>() where T : DataEntity;
        public abstract int Delete<T>(T entity, bool delay = false) where T : DataEntity;
        public abstract int Delete<T>(Expression<Func<T, bool>> conditions, bool delay = false) where T : DataEntity;
        public virtual int ExecuteSqlCommand(string sql)
        {
            return ExecuteSqlCommand(sql, null, null);
        }
        public abstract int ExecuteSqlCommand(string sql, object paramters = null, IDbTransaction transaction = null);
        public abstract IEnumerable<T> Query<T>(string sql, object paramters = null) where T : class, new();
        public abstract T Find<T>(params object[] keyValues) where T : DataEntity;
        public abstract IEnumerable<T> Search<T>(string sql, object paramters = null) where T : DataEntity, new();
        public abstract IEnumerable<T> Search<T>(Expression<Func<T, bool>> conditions = null) where T : DataEntity;
        public abstract IEnumerable<T> Search<T, S>(Expression<Func<T, bool>> conditions, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount) where T : DataEntity;
        public abstract T FirstOrDefault<T>(Expression<Func<T, bool>> conditions) where T : DataEntity;

        public abstract bool Exists<T>(Expression<Func<T, bool>> conditions) where T : DataEntity;

        public abstract int Insert<T>(T entity, bool delay = false) where T : DataEntity;

        public abstract int Update<T>(T entity, bool delay = false) where T : DataEntity;

        public virtual int SaveChanges()
        {
            return this.DbContext.SaveChanges();
        }

        /// <summary>
        /// 获取实际的子仓类实例。
        /// </summary>
        /// <typeparam name="TDataEntity"></typeparam>
        /// <typeparam name="TChildRepositoryResult"></typeparam>
        /// <returns></returns>
        public virtual TChildRepositoryResult GetChild<TDataEntity, TChildRepositoryResult>()
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
        public virtual IChildRepository<T> GetChild<T>() where T : DataEntity
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
        public virtual bool TryGetChild<T>(out IChildRepository<T> repository) where T : DataEntity
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
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                this.childRepositories?.Clear();

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~IService() {
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

        #region private functions

        /// <summary>
        /// 创建一个默认子仓实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected virtual IChildRepository<T> CreateChildRepository<T>() where T : DataEntity
        {
            IChildRepository<T> repository = default(IChildRepository<T>);

            var typed = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(t => t.IsClass && t.GetInterfaces().Contains(typeof(IChildRepository<T>))))
                .FirstOrDefault();

            if (typed != null)
            {
                repository = Activator.CreateInstance(typed, new object[] { DbContext }) as IChildRepository<T>;
            }

            return repository;
        }

        #endregion
    }
}
