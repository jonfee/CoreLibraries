using JF.DataBased;
using System;

namespace JF.DomainEventBased.DomainModel
{
    /// <summary>
    /// 领域对象 接口约束
    /// </summary>
    public interface IDomainObject : IDomainObject<object>
    {
    }

    /// <summary>
    /// 自定义领域对象标识ID类型的接口约束
    /// </summary>
    /// <typeparam name="TDomainObjectID"></typeparam>
    public interface IDomainObject<TDomainObjectID> : IEquatable<IDomainObject<TDomainObjectID>>
    {
        TDomainObjectID ID { get; set; }
    }

    /// <summary>
    /// 聚合根接口
    /// </summary>
    public interface IAggregateRoot<TUnitOfWork> : IAggregateRoot<object, TUnitOfWork>
        where TUnitOfWork : class, IUnitOfWork, new()
    {
    }

    /// <summary>
    /// 聚合根接口
    /// </summary>
    /// <typeparam name="TDomainObjectID"></typeparam>
    public interface IAggregateRoot<TDomainObjectID, TUnitOfWork> : IDomainObject<TDomainObjectID>
        where TUnitOfWork : class, IUnitOfWork, new()
    {
        TUnitOfWork Worker { get; }
    }

    /// <summary>
    /// 领域对象 抽象基类
    /// </summary>
    public abstract class DomainObject : DomainObject<object>
    {
        protected DomainObject(object domainObjectID) : base(domainObjectID)
        {
        }
    }

    /// <summary>
    /// 领域对象 抽象基类
    /// </summary>
    /// <typeparam name="TDomainObjectID"></typeparam>
    public abstract class DomainObject<TDomainObjectID> : IDomainObject<TDomainObjectID>
    {
        public DomainObject(TDomainObjectID id)
        {
            this.ID = id;
        }

        /// <summary>
        /// 领域对象ID
        /// </summary>
        public TDomainObjectID ID { get; set; }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public bool Equals(IDomainObject<TDomainObjectID> other)
        {
            return other != null && other.GetHashCode() == this.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is DomainObject<TDomainObjectID>)
            {
                return Equals(obj as DomainObject<TDomainObjectID>);
            }

            return false;
        }

        public static bool operator ==(DomainObject<TDomainObjectID> left, DomainObject<TDomainObjectID> right)
        {
            if (left == null || right == null) return false;

            return left.GetHashCode() == right.GetHashCode();
        }

        public static bool operator !=(DomainObject<TDomainObjectID> left, DomainObject<TDomainObjectID> right)
        {
            if (left == null || right == null) return true;

            return left.GetHashCode() != right.GetHashCode();
        }
    }

    /// <summary>
    /// 领域对象聚合根 抽象基类
    /// </summary>
    /// <typeparam name="TUnitOfWork">工作单元类型</typeparam>
    public abstract class AggregateRoot<TUnitOfWork> : AggregateRoot<object, TUnitOfWork>
        where TUnitOfWork : class, IUnitOfWork, new()
    {
        public AggregateRoot(object id) : base(id) { }
    }

    /// <summary>
    /// 领域对象聚合根 抽象基类
    /// </summary>
    /// <typeparam name="TDomainObjectID">领域对象ID类型</typeparam>
    /// <typeparam name="TUnitOfWork">工作单元类型</typeparam>
    public abstract class AggregateRoot<TDomainObjectID, TUnitOfWork> : DomainObject<TDomainObjectID>, IAggregateRoot<TDomainObjectID, TUnitOfWork>
        where TUnitOfWork : class, IUnitOfWork, new()
    {
        public TUnitOfWork Worker { get; protected set; }

        public AggregateRoot() : this(default(TDomainObjectID)) { }

        public AggregateRoot(TDomainObjectID id) : base(id)
        {
            Worker = new TUnitOfWork();
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public bool Equals(IDomainObject<TDomainObjectID> other)
        {
            return other != null && other.GetHashCode() == this.GetHashCode();
        }
    }
}
