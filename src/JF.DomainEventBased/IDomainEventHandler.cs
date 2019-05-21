using JF.DomainEventBased.DomainModel;

namespace JF.DomainEventBased
{
    /// <summary>
    /// 事件处理程序接口
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event"></param>
        void Handle(TEvent @event);

        /// <summary>
        /// 验证事件是否有效
        /// </summary>
        /// <param name="@event">派生自<see cref="IDomainEvent"/>的事件实例</param>
        /// <returns></returns>
        bool Validate(TEvent @event);
    }

    /// <summary>
    /// 领域事件处理程序接口
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public abstract class DomainEventHandler<TEvent> : IDomainEventHandler<TEvent> where TEvent : IDomainEvent
    {
        /// <summary>
        /// 处理执行
        /// </summary>
        /// <param name="event"></param>
        public abstract void Handle(TEvent @event);

        /// <summary>
        /// 验证事件是否有效。
        /// 基类中仅检测了事件是否为null,为null时返回false,非null返回true。
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public virtual bool Validate(TEvent @event)
        {
            return @event != null;
        }
    }
}
