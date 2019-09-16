using System.Linq;

namespace JF.DomainEventBased
{
    /// <summary>
    /// 领域服务命令接口
    /// </summary>
    public interface IDomainCommand : JF.EventBus.ICommand
    {
    }
}
