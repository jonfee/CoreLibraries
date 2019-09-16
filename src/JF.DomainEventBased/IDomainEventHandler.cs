namespace JF.DomainEventBased
{
    /// <summary>
    /// 领域事件处理程序接口
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public abstract class DomainEventHandler<TEvent> : JF.EventBus.EventHandler<TEvent> where TEvent : IDomainEvent
    {
    }
}
