using JF.ComponentModel;
using JF.DataBased.Context;
using JF.DataBased.Repository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;

namespace JF.DataBased
{
    /// <summary>
    /// 工作单元抽象基类
    /// </summary>
    public abstract class UnitOfWorkBase : IUnitOfWork
    {
        #region private variables

        /// <summary>
        /// 仓储服务集合
        /// </summary>
        private Hashtable repositories;

        #endregion

        #region Public Properties

        /// <summary>
        /// 获取默认且唯一的仓储服务,
        /// 如果工作单元中存在多们仓储服务时，将返回NULL。
        /// </summary>
        protected IRepository DefaultRepository
        {
            get
            {
                IRepository repository = null;

                if (this.repositories.Count != 1) throw new IndexOutOfRangeException("当前工作单元不存在唯一的仓储服务。");

                foreach (var value in this.repositories.Values)
                {
                    repository = value as IRepository;
                    break;
                }

                return repository;
            }
        }

        /// <summary>
        /// 需要新增的数据对象
        /// </summary>
        public Dictionary<DataEntity, IRepository> AddedEntities { get; }

        /// <summary>
        /// 需要更新的数据对象
        /// </summary>
        public Dictionary<DataEntity, IRepository> UpdatedEntities { get; }

        /// <summary>
        /// 需要删除的数据对象
        /// </summary>
        public Dictionary<DataEntity, IRepository> DeletedEntities { get; }

        /// <summary>
        /// 需要执行的SQL命令行
        /// </summary>
        public Dictionary<string, IRepository> SqlCommands { get; }

        /// <summary>
        /// 当前工作单元中待处理的数据数量
        /// </summary>
        public int PendingNumber => this.AddedEntities.Count + this.UpdatedEntities.Count + this.DeletedEntities.Count + SqlCommands.Count;

        #endregion

        #region contructors

        /// <summary>
        /// 初始化一个<see cref="UnitOfWorkBase"/>对象实例。
        /// </summary>
        public UnitOfWorkBase() : this(null) { }

        /// <summary>
        /// 初始化一个<see cref="UnitOfWorkBase"/>对象实例。
        /// </summary>
        /// <param name="repository">仓储实例对象。</param>
        public UnitOfWorkBase(IRepository repository)
        {
            this.repositories = Hashtable.Synchronized(new Hashtable());
            this.AddedEntities = new Dictionary<DataEntity, IRepository>();
            this.UpdatedEntities = new Dictionary<DataEntity, IRepository>();
            this.DeletedEntities = new Dictionary<DataEntity, IRepository>();
            this.SqlCommands = new Dictionary<string, IRepository>();

            if (repository != null)
            {
                AddRepository(repository);
            }
        }

        #endregion

        #region public behavious

        /// <summary>
        /// 注册一个新的数据对象插入。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        public void RegisteAdded<TEntity>(TEntity entity) where TEntity : DataEntity
        {
            this.RegisteAdded(entity, this.DefaultRepository);
        }

        /// <summary>
        /// 注册一个新的数据对象更新。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        public void RegisteUpdated<TEntity>(TEntity entity) where TEntity : DataEntity
        {
            this.RegisteUpdated(entity, this.DefaultRepository);
        }

        /// <summary>
        /// 注册一个新的数据对象删除。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        public void RegisteDeleted<TEntity>(TEntity entity) where TEntity : DataEntity
        {
            this.RegisteDeleted(entity, this.DefaultRepository);
        }

        /// <summary>
        /// 注册一个新的SQL命令。
        /// </summary>
        /// <param name="sql"></param>
        public void RegisteCommand(string sql)
        {
            this.RegisteCommand(sql, this.DefaultRepository);
        }

        public void RegisteAdded<TEntity>(TEntity entity, IRepository repository) where TEntity : DataEntity
        {
            this.AddedEntities.Add(entity, repository);
            this.AddRepository(repository);
        }

        public void RegisteUpdated<TEntity>(TEntity entity, IRepository repository) where TEntity : DataEntity
        {
            this.UpdatedEntities.Add(entity, repository);
            this.AddRepository(repository);
        }

        public void RegisteDeleted<TEntity>(TEntity entity, IRepository repository) where TEntity : DataEntity
        {
            this.DeletedEntities.Add(entity, repository);
            this.AddRepository(repository);
        }

        public void RegisteCommand(string sql, IRepository repository)
        {
            this.SqlCommands.Add(sql, repository);
            this.AddRepository(repository);
        }

        /// <summary>
        /// 添加仓储服务
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <param name="repository"></param>
        protected virtual void AddRepository<TRepository>(TRepository repository) where TRepository : class, IRepository
        {
            var type = typeof(TRepository);

            if (!this.repositories.ContainsKey(type.Name))
            {
                this.repositories.Add(type.Name, repository);
            }
        }

        /// <summary>
        /// 获取仓储服务对象。
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <returns></returns>
        protected TRepository GetRepository<TRepository>() where TRepository : class, IRepository
        {
            var repository = default(TRepository);

            var type = typeof(TRepository);

            if (!this.repositories.ContainsKey(type.Name))
            {
                repository = this.repositories[type.Name] as TRepository;
            }

            return repository;
        }

        #endregion

        #region transaction

        /// <summary>
        /// 提交事务。
        /// 提供一个事务执行成功后的回调程序。
        /// </summary>
        /// <remarks>
        /// MySql目前版本支持同一个连接字符串内多个连接实例的事务（且一个实例SaveChanges()后，必须释放，否则有并发问题。），
        /// 不支持不同连接字符串以及不同数据库之间的事务。
        /// </remarks>
        /// <param name="callback">事务执行成功后的回调程序。</param>
        /// <param name="timeoutSeconds">事务超时时间(单位：秒)。</param>
        public void Commit(Action callback = null, int timeoutSeconds = 60)
        {
            if (timeoutSeconds < 1) timeoutSeconds = 10;
            TransactionOptions options = new TransactionOptions();
            options.Timeout = new TimeSpan(0, 0, timeoutSeconds);

            using (TransactionScope trans = new TransactionScope(TransactionScopeOption.Required, options))
            {
                // 处理新增
                foreach (var kv in this.AddedEntities)
                {
                    kv.Value.Insert(kv.Key);
                }

                // 处理更新
                foreach (var kv in this.UpdatedEntities)
                {
                    kv.Value.Update(kv.Key);
                }

                // 处理删除
                foreach (var kv in this.DeletedEntities)
                {
                    kv.Value.Delete(kv.Key);
                }

                // 处理命令行
                foreach (var kv in this.SqlCommands)
                {
                    kv.Value.ExecuteSqlCommand(kv.Key);
                }

                // 执行SaveChanges()
                foreach (IRepository repository in this.repositories.Values)
                {
                    repository.DbContext.SaveChanges();
                }

                trans.Complete();

                // 清空当前事务已处理的工作项
                this.ClearWorks();

                callback?.Invoke();
            }
        }

        #endregion

        #region private behavious

        /// <summary>
        /// 清空当前的工作项
        /// </summary>
        private void ClearWorks()
        {
            this.AddedEntities.Clear();
            this.UpdatedEntities.Clear();
            this.DeletedEntities.Clear();
            this.SqlCommands.Clear();
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
                    foreach (KeyValuePair<IDbContext, IRepository> kv in this.repositories)
                    {
                        kv.Key?.Dispose();
                        kv.Value?.Dispose();
                    }
                    this.repositories.Clear();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                ClearWorks();

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~UnitWorkBase() {
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

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
