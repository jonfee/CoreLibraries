namespace JF.DomainEventBased
{
    /// <summary>
    /// 命令处理程序抽象基类
    /// </summary>
    /// <typeparam name="TCommand">输入命令类型</typeparam>
    public abstract class DomainCommandHandler<TCommand> : JF.EventBus.CommandHandler<TCommand> where TCommand : IDomainCommand
    {
    }
}
