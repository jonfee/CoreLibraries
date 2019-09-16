using JF.EventBus.Mapping;
using System.Linq;

namespace JF.EventBus
{
    /// <summary>
    /// 命令接口
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 发送命令，将结果以指定类型返回。
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        TResult Send<TResult>();

        /// <summary>
        /// 发送命令，将结果以object类型返回。
        /// </summary>
        /// <returns></returns>
        object Send();
    }

    public abstract class CommandBase : ICommand
    {
        /// <summary>
        /// 发送命令。
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public TResult Send<TResult>()
        {
            var result = Send();

            return (TResult)result;
        }

        /// <summary>
        /// 发送命令。
        /// </summary>
        /// <returns></returns>
        public object Send()
        {
            object result = null;
            var commandType = this.GetType();
            var genericType = typeof(ICommandHandler<>).MakeGenericType(commandType);

            if (CommandSubscriberTypedMapping.Current.TryGet(commandType, out var handler))
            {
                var method = genericType.GetMethods().FirstOrDefault(m => m.Name == "Execute");

                result = method?.Invoke(handler, new object[] { this });
            }

            return result;
        }
    }
}
