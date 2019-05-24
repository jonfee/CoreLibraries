using JF.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JF.DataBased.Repository
{
    /// <summary>
    /// 子仓接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChildRepository<T> : IDisposable where T : DataEntity
    {
        /// <summary>
        /// 获取当前指定类型的所有数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQueryable<T> All();

        /// <summary>
        /// 根据主键获取实体数据。
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        T Find(params object[] keyValues);

        /// <summary>
        /// 根据条件返回匹配条件的第一条数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        /// <returns></returns>
        T FirstOrDefault(Expression<Func<T, bool>> conditions);

        /// <summary>
        /// 检测是否存在符合条件的数据。
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        bool Exists(Expression<Func<T, bool>> conditions);

        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        int Insert(T entity);

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        int Update(T entity);

        /// <summary>
        /// 删除指定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        int Delete(T entity);

        /// <summary>
        /// 按条件删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        int Delete(Expression<Func<T, bool>> conditions);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        IEnumerable<T> Search(string sql, object paramters);

        /// <summary>
        /// 根据条件查找
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        /// <returns></returns>
        List<T> Search(Expression<Func<T, bool>> conditions = null);

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
        List<T> Search<S>(Expression<Func<T, bool>> conditions, Expression<Func<T, S>> orderBy, int pageSize, int pageIndex, out int totalCount);

        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        int ExecuteSqlCommand(string sql);

        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        int ExecuteSqlCommand(string sql, params object[] paramters);
    }
}
