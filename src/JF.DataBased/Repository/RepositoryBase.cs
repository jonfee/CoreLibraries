using JF.ComponentModel;
using JF.DataBased.Context;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public abstract void Delete<T>(T entity) where T : DataEntity;
        public abstract void Delete<T>(Expression<Func<T, bool>> conditions) where T : DataEntity;
        public abstract void Dispose();
        public abstract int ExecuteSqlCommand(string sql);
        public abstract T Find<T>(params object[] keyValues) where T : DataEntity;
        public abstract List<T> Search<T>(Expression<Func<T, bool>> conditions = null) where T : DataEntity;
        public abstract List<T> Search<T, S>(Expression<Func<T, bool>> conditions, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount) where T : DataEntity;
        public abstract T FirstOrDefault<T>(Expression<Func<T, bool>> conditions) where T : DataEntity;

        public abstract void Insert<T>(T entity) where T : DataEntity;

        public abstract void Update<T>(T entity) where T : DataEntity;

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
