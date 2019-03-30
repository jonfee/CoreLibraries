using JF.DomainEventBased.DomainModel;

namespace JF.DomainEventBased
{
    /// <summary>
    /// 命令处理程序接口
    /// </summary>
    public interface IDomainCommandHandler<in TCommand> where TCommand : IDomainCommand
    {
        /// <summary>
        /// 验证命令是否有效
        /// </summary>
        /// <param name="command">派生自<see cref="IDomainCommand"/>的命令实例</param>
        /// <returns></returns>
        bool Validate(TCommand command);

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="command">派生自<see cref="IDomainCommand"/>的命令实例</param>
        /// <returns></returns>
        object Execute(TCommand command);
    }

    /// <summary>
    /// 命令处理程序抽象基类
    /// </summary>
    /// <typeparam name="TCommand">输入命令类型</typeparam>
    public abstract class DomainCommandHandler<TCommand> : IDomainCommandHandler<TCommand>
        where TCommand : IDomainCommand
    {
        public abstract object Execute(TCommand command);

        public virtual bool Validate(TCommand command)
        {
            return command != null;
        }
    }
}
