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
    public interface IDomainObject<TDomainObjectID> :IDisposable, IEquatable<IDomainObject<TDomainObjectID>>
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
        public virtual TDomainObjectID ID { get; set; }

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

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~DomainObject() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
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
