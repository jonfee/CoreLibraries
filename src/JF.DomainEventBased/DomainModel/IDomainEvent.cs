using System.Collections.Generic;
using System.Linq;

namespace JF.DomainEventBased.DomainModel
{
    /// <summary>
    /// 领域事件接口
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// 事件处理结果
        /// </summary>
        IList<object> Results { get; }

        /// <summary>
        /// 获取匹配的指定类型的第一个结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetTypedResult<T>();

        /// <summary>
        /// 获取指定类型的结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IList<T> GetTypedResults<T>();

        /// <summary>
        /// 添加结果
        /// </summary>
        /// <param name="result"></param>
        void AddResult(object result);

        /// <summary>
        /// 批量添加结果
        /// </summary>
        /// <param name="results"></param>
        void AddResults(IEnumerable<object> results);

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="sync">是否同步执行</param>
        void Publish(bool sync);
    }

    /// <summary>
    /// 领域事件抽象基类
    /// </summary>
    public abstract class DomainEventBase : IDomainEvent
    {
        public DomainEventBase()
        {
            Results = new List<object>();
        }

        /// <summary>
        /// 事件结果
        /// </summary>
        public IList<object> Results { get; private set; }

        /// <summary>
        /// 添加事件结果
        /// </summary>
        /// <param name="result"></param>
        public void AddResult(object result)
        {
            if (result != null)
            {
                Results.Add(result);
            }
        }

        /// <summary>
        /// 批量添加事件结果
        /// </summary>
        /// <param name="results"></param>
        public void AddResults(IEnumerable<object> results)
        {
            if (results == null) return;

            foreach(var rst in results)
            {
                AddResult(rst);
            }
        }

        /// <summary>
        /// 获取指定类型的默认结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetTypedResult<T>()
        {
            var filteredResults = GetTypedResults<T>();
            if (filteredResults.Count > 0)
            {
                return filteredResults[0];
            }
            return default(T);
        }

        /// <summary>
        /// 获取指定类型的结果集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IList<T> GetTypedResults<T>()
        {
            return Results.OfType<T>().ToList();
        }

        /// <summary>
        /// 事件发布
        /// </summary>
        /// <param name="sync">是否同步执行,True时，将立即执行事件广播</param>
        public virtual void Publish(bool sync = true)
        {
            if (sync)
            {
                EventBus.Current.Process(this);
            }
            else
            {
                EventBus.Current.Publish(this);
            }
        }
    }
}
