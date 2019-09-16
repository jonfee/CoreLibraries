namespace JF.EventBus
{
    /// <summary>
    /// 命令处理程序接口
    /// </summary>
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// 验证命令是否有效
        /// </summary>
        /// <param name="command">派生自<see cref="ICommand"/>的命令实例</param>
        /// <returns></returns>
        bool Validate(TCommand command);

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="command">派生自<see cref="ICommand"/>的命令实例</param>
        /// <returns></returns>
        object Execute(TCommand command);
    }

    /// <summary>
    /// 命令处理程序抽象基类
    /// </summary>
    /// <typeparam name="TCommand">输入命令类型</typeparam>
    public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public abstract object Execute(TCommand command);

        public virtual bool Validate(TCommand command)
        {
            return command != null;
        }
    }
}
