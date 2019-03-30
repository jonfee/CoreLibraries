namespace JF.DomainEventBased
{
    /// <summary>
    /// 领域服务接口
    /// </summary>
    public interface IDomainService { }

    /// <summary>
    /// 领域服务接口
    /// </summary>
    public interface IDomainService<IUnitOfWork> : IDomainService { }
}
