using JF.ComponentModel;
using JF.DataBased.Repository;
using System;
using System.Collections.Generic;

namespace JF.DataBased
{
    /// <summary>
    /// 工作单元接口
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 需要新增的数据对象
        /// </summary>
        Dictionary<dynamic, IRepository> AddedEntities { get; }

        /// <summary>
        /// 需要更新的数据对象
        /// </summary>
        Dictionary<dynamic, IRepository> UpdatedEntities { get; }

        /// <summary>
        /// 需要删除的数据对象
        /// </summary>
        Dictionary<dynamic, IRepository> DeletedEntities { get; }

        /// <summary>
        /// 需要执行的SQL命令行
        /// </summary>
        Dictionary<string, IRepository> SqlCommands { get; }

        /// <summary>
        /// 注册一个新的数据对象插入。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="repository"></param>
        void RegisteAdded<TEntity>(TEntity entity, IRepository repository)
            where TEntity : DataEntity;

        /// <summary>
        /// 注册一个新的数据对象更新。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="repository"></param>
        void RegisteUpdated<TEntity>(TEntity entity, IRepository repository)
            where TEntity : DataEntity;

        /// <summary>
        /// 注册一个新的数据对象删除。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="repository"></param>
        void RegisteDeleted<TEntity>(TEntity entity, IRepository repositoryt)
            where TEntity : DataEntity;

        /// <summary>
        /// 注册一个新的SQL命令。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="repository"></param>
        void RegisteCommand(string sql, IRepository repository);

        /// <summary>
        /// 提交事务，
        /// 并提供一个事务执行成功后的回调程序。
        /// </summary>
        /// <remarks>
        /// MySql目前版本支持同一个连接字符串内多个连接实例的事务（且一个实例SaveChanges()后，必须释放，否则有并发问题。），
        /// 不支持不同连接字符串以及不同数据库之间的事务。
        /// </remarks>
        /// <param name="callback">事务执行成功后的回调程序。</param>
        /// <param name="timeoutSeconds">事务超时时间(单位：秒)。</param>
        int Commit(Action callback = null, int timeoutSeconds = 60);
    }
}
