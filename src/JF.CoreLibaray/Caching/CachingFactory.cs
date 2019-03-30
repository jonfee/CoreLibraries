using System;
using System.Collections;
using System.Collections.Generic;

namespace JF.Caching
{
    /// <summary>
    /// 缓存工厂类
    /// </summary>
    public static class CachingFactory
    {
        public static Hashtable caches = Hashtable.Synchronized(new Hashtable());

        #region Cache Instance

        /// <summary>
        /// 获取缓存单例。
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public static ICache<TModel> GetInstance<TModel>()
            where TModel : class, ICacheModel
        {
            Type typedModel = typeof(TModel);

            ICache<TModel> cache = default(ICache<TModel>);

            if (caches.ContainsKey(typedModel)) cache = caches[typedModel] as ICache<TModel>;

            return cache;
        }

        #endregion

        #region Create Factory

        /// <summary>
        /// 创建一个内存缓存
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="expireAfterMinutes"></param>
        /// <param name="initFunc"></param>
        /// <returns></returns>
        public static ICache<TModel> GetOrCreateMemoryCache<TModel>(
            long expireAfterMinutes = -1,
            Func<IEnumerable<TModel>> initFunc = null)
            where TModel : class, ICacheModel
        {
            Type typedModel = typeof(TModel);
            ICache<TModel> cache = null;

            if (caches.ContainsKey(typedModel))
            {
                cache = caches[typedModel] as ICache<TModel>;
            }
            else
            {
                cache= CreateCache(CacheMode.Memory, expireAfterMinutes, initFunc);
                caches.Add(typedModel, cache);
            }

            return cache;
        }

        /// <summary>
        /// 创建一个缓存实例
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="mode">缓存模式</param>
        /// <param name="expireAfterMinutes">过期时间（单位：分钟）</param>
        /// <param name="initFunc">初始化缓存数据的委托事件</param>
        /// <returns></returns>
        private static ICache<TModel> CreateCache<TModel>(
            CacheMode mode = CacheMode.Memory,
            long expireAfterMinutes = -1,
            Func<IEnumerable<TModel>> initFunc = null)
            where TModel : class, ICacheModel
        {
            ICache<TModel> cache = null;

            switch (mode)
            {
                case CacheMode.Memory:
                default:
                    cache = new MemoryCache<TModel>(expireAfterMinutes, initFunc);
                    break;
            }

            return cache;
        }

        #endregion
    }
}
