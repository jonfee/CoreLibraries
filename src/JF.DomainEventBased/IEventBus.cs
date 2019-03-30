using JF.DomainEventBased.DomainModel;

namespace JF.DomainEventBased
{
    /// <summary>
    /// 事件总线接口
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="event"></param>
        void Publish(IDomainEvent @event);
    }
}
