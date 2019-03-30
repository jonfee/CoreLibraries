using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JF.Caching
{
    /// <summary>
    /// 缓存类
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class MemoryCache<TModel> : ICache<TModel> where TModel : class, ICacheModel
    {
        #region private variables

        static object locker = new object();
        ConcurrentDictionary<string, TModel> data;
        long expiredAfterMinutes = -1;
        // 过期时间，为null时表示永久有效
        DateTime? expireTime;
        // 重置或恢复到初始状态的委托
        Func<IEnumerable<TModel>> restoreFunc;
        // 过期处理定时器
        System.Threading.Timer eTimer;

        #endregion

        #region contructors

        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="expiredAfterMinutes">在指定的时间（分钟）后失效,小于1分钟时表示永久有效。</param>
        /// <param name="initFunc">初始化缓存数据的委托事件。</param>
        public MemoryCache(long expiredAfterMinutes = -1, Func<IEnumerable<TModel>> initFunc = null)
        {
            this.data = new ConcurrentDictionary<string, TModel>();
            this.expiredAfterMinutes = expiredAfterMinutes;
            if (expiredAfterMinutes > 0)
            {
                expireTime = DateTime.Now.AddMinutes(expiredAfterMinutes);
            }

            this.restoreFunc = initFunc;

            Restore();
        }

        #endregion

        #region public indexer

        public TModel this[object key]
        {
            get
            {
                var _key = key != null ? key.ToString() : string.Empty;

                return this[_key];
            }
        }

        public TModel this[string key]
        {
            get
            {
                return data.ContainsKey(key) ? data[key] as TModel : default(TModel);
            }
        }

        #endregion

        #region public bahavious

        public IEnumerable<TModel> Values()
        {
            return data.Values.ToList();
        }

        #region clear

        public virtual void Clear()
        {
            data.Clear();
        }

        #endregion

        #region add or update

        public virtual void Add(TModel item)
        {
            if (item == null) return;

            Add(new[] { item });
        }

        public virtual void Add(IEnumerable<TModel> items)
        {
            if (items == null || !items.Any()) return;

            lock (locker)
            {
                foreach (var item in items)
                {
                    if (!data.ContainsKey(item.Key))
                    {
                        data.TryAdd(item.Key, item);
                    }
                }
            }
        }

        public virtual void Update(TModel item)
        {
            if (item == null) return;

            lock (locker)
            {
                if (ExistsKey(item.Key))
                {
                    data[item.Key] = item;
                }
            }
        }

        public virtual void Update(IEnumerable<TModel> items)
        {
            foreach (var item in items)
            {
                Update(item);
            }
        }

        public virtual void AddOrUpdate(TModel item)
        {
            if (item == null) return;

            if (ExistsKey(item.Key))
            {
                Update(item);
            }
            else
            {
                Add(item);
            }
        }

        public virtual void AddOrUpdate(IEnumerable<TModel> items)
        {
            foreach (var item in items)
            {
                AddOrUpdate(item);
            }
        }

        #endregion

        #region find

        public virtual TModel FindAt(object key)
        {
            return FindAt(key?.ToString());
        }

        public virtual TModel FindAt(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return default(TModel);

            return data.ContainsKey(key) ? data[key] as TModel : default(TModel);
        }

        public virtual IEnumerable<TModel> FindAt(IEnumerable<string> keys)
        {
            List<TModel> items = new List<TModel>();

            if (keys != null && keys.Any())
            {
                foreach (var key in keys)
                {
                    if (data.ContainsKey(key))
                    {
                        items.Add(data[key] as TModel);
                    }
                }
            }

            return items;
        }

        public IEnumerable<TModel> SelectFor(Expression<Func<TModel, Boolean>> expression)
        {
            return Values().Where(expression.Compile());
        }

        #endregion

        #region restore

        public virtual void Restore(IEnumerable<TModel> items)
        {
            Clear();

            // 销毁定时器
            if (eTimer != null)
            {
                eTimer.Dispose();
            }

            Add(items);

            expireTime = expiredAfterMinutes < 1
                            ? default(DateTime?)
                            : DateTime.Now.AddMinutes(expiredAfterMinutes);

            // 重设定时器
            if (expireTime.HasValue)
            {
                eTimer = new System.Threading.Timer((state) =>
                {
                    Restore();
                }, null, (long)TimeSpan.FromMinutes(expiredAfterMinutes).TotalMilliseconds, -1);
            }
        }

        public virtual void Restore()
        {
            var items = restoreFunc?.Invoke();

            Restore(items);
        }

        #endregion

        #region remove

        public virtual void RemoveAt(TModel item)
        {
            if (item != null)
            {
                RemoveAt(item.Key);
            }
        }

        public virtual void RemoveAt(object key)
        {
            RemoveAt(key?.ToString());
        }

        public virtual void RemoveAt(string key)
        {
            RemoveAt(new[] { key });
        }

        public virtual void RemoveAt(IEnumerable<string> keys)
        {
            if (keys == null || !keys.Any()) return;

            lock (locker)
            {
                foreach (var key in keys)
                {
                    if (string.IsNullOrWhiteSpace(key)) continue;

                    if (data.ContainsKey(key))
                    {
                        data.TryRemove(key, out var value);
                    }
                }
            }
        }

        #endregion

        #region exists

        public virtual bool Exists(TModel item)
        {
            if (item == null) return false;

            return this[item.Key] != null;
        }

        public virtual bool Exists(object key)
        {
            return Exists(key?.ToString());
        }

        public virtual bool Exists(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;

            return this[key] != null;
        }

        public virtual bool ExistsKey(object key)
        {
            return Exists(key?.ToString());
        }

        public virtual bool ExistsKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;

            return data.ContainsKey(key);
        }

        #endregion

        #endregion
    }
}
