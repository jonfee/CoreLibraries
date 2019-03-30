using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace JF.Caching
{
    /// <summary>
    /// 缓存接口
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public interface ICache<TModel> where TModel : ICacheModel
    {
        /// <summary>
        /// 获取缓存对象集合
        /// </summary>
        IEnumerable<TModel> Values();

        /// <summary>
        /// 重置或恢复到初始状态
        /// </summary>
        /// <param name="items"></param>
        void Restore(IEnumerable<TModel> items);

        /// <summary>
        /// 重置或恢复到初始状态
        /// </summary>
        void Restore();

        /// <summary>
        /// 向缓存中添加缓存项
        /// </summary>
        /// <param name="item"></param>
        void Add(TModel item);

        /// <summary>
        /// 批量向缓存中添加缓存项
        /// </summary>
        /// <param name="items"></param>
        void Add(IEnumerable<TModel> items);

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="item"></param>
        void Update(TModel item);

        /// <summary>
        /// 批量更新缓存
        /// </summary>
        /// <param name="items"></param>
        void Update(IEnumerable<TModel> items);

        /// <summary>
        /// 添加或更新缓存
        /// </summary>
        /// <param name="item"></param>
        void AddOrUpdate(TModel item);

        /// <summary>
        /// 批量添加或更新缓存
        /// </summary>
        /// <param name="items"></param>
        void AddOrUpdate(IEnumerable<TModel> items);

        /// <summary>
        /// 移除缓存项
        /// </summary>
        /// <param name="item"></param>
        void RemoveAt(TModel item);

        /// <summary>
        /// 移除指定缓存Key的缓存数据
        /// </summary>
        /// <param name="key"></param>
        void RemoveAt(object key);

        /// <summary>
        /// 移除指定缓存Key的缓存数据
        /// </summary>
        /// <param name="key"></param>
        void RemoveAt(string key);

        /// <summary>
        /// 批量移除指定缓存Key的缓存数据
        /// </summary>
        /// <param name="keys"></param>
        void RemoveAt(IEnumerable<string> keys);

        /// <summary>
        /// 根据缓存Key获取缓存数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TModel FindAt(object key);

        /// <summary>
        /// 根据缓存Key获取缓存数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TModel FindAt(string key);

        /// <summary>
        /// 根据缓存Keys获取缓存数据集合
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        IEnumerable<TModel> FindAt(IEnumerable<string> keys);

        /// <summary>
        /// 按条件查询
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        IEnumerable<TModel> SelectFor(Expression<Func<TModel, Boolean>> expression);

        /// <summary>
        /// 检测缓存是否存在
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Exists(TModel item);

        /// <summary>
        /// 检测缓存是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Exists(object key);

        /// <summary>
        /// 检测缓存是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Exists(string key);

        /// <summary>
        /// 检测缓存Key是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ExistsKey(object key);

        /// <summary>
        /// 检测缓存Key是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ExistsKey(string key);

        /// <summary>
        /// 清除缓存
        /// </summary>
        void Clear();

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TModel this[string key] { get; }

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TModel this[object key] { get; }
    }
}

