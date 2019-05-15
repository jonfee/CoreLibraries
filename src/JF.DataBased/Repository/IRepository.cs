using JF.ComponentModel;
using JF.DataBased.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JF.DataBased.Repository
{
    /// <summary>
    /// 主仓储服务接口。
    /// </summary>
    public interface IRepository : IDisposable
    {
        /// <summary>
        /// 数据库上下文对象
        /// </summary>
        IDbContext DbContext { get; }

        /// <summary>
        /// 获取当前指定类型的所有数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQueryable<T> All<T>()
            where T : DataEntity;

        /// <summary>
        /// 根据条件返回匹配条件的第一条数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        /// <returns></returns>
        T FirstOrDefault<T>(Expression<Func<T, bool>> conditions)
            where T : DataEntity;

        /// <summary>
        /// 检测是否存在符合条件的数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        /// <returns></returns>
        bool Exists<T>(Expression<Func<T, bool>> conditions)
            where T : DataEntity;

        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        void Insert<T>(T entity)
            where T : DataEntity;

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        void Update<T>(T entity)
            where T : DataEntity;

        /// <summary>
        /// 删除指定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        void Delete<T>(T entity)
            where T : DataEntity;

        /// <summary>
        /// 按条件删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        void Delete<T>(Expression<Func<T, bool>> conditions)
            where T : DataEntity;

        /// <summary>
        /// 根据主键获取实体数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        T Find<T>(params object[] keyValues) where T : DataEntity;

        /// <summary>
        /// 根据条件查找
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        /// <returns></returns>
        List<T> Search<T>(Expression<Func<T, bool>> conditions = null)
            where T : DataEntity;

        /// <summary>
        /// 根据条件分页查找
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="conditions"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        List<T> Search<T, S>(Expression<Func<T, bool>> conditions, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount)
            where T : DataEntity;

        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        int ExecuteSqlCommand(string sql);

        /// <summary>
        /// 保存更改
        /// </summary>
        /// <returns></returns>
        int SaveChanges();

        #region 子仓操作

        /// <summary>
        /// 获取实际的子仓类实例。
        /// </summary>
        /// <typeparam name="TDataEntity"></typeparam>
        /// <typeparam name="TChildRepositoryResult"></typeparam>
        /// <returns></returns>
        TChildRepositoryResult GetChild<TDataEntity, TChildRepositoryResult>()
            where TDataEntity : DataEntity
            where TChildRepositoryResult : class, IChildRepository<TDataEntity>;

        /// <summary>
        /// 获取子仓
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IChildRepository<T> GetChild<T>() where T : DataEntity;

        /// <summary>
        /// 获取子仓。
        /// 建议使用享元模式实现子仓。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool TryGetChild<T>(out IChildRepository<T> repository) where T : DataEntity;

        #endregion
    }
}
