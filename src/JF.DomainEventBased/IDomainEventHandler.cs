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
        /// <param name="command">派生自<see cref="IDomainEvent"/>的命令实例</param>
        /// <returns></returns>
        bool Validate(TEvent @event);
    }

    public abstract class DomainEventHandler<TEvent> : IDomainEventHandler<TEvent> where TEvent : IDomainEvent
    {
        public abstract void Handle(TEvent @event);

        public virtual bool Validate(TEvent @event)
        {
            return @event != null;
        }
    }
}
