using System;
using System.Collections.Generic;
using System.Text;

namespace JF.DomainEventBased.DomainModel
{
    /// <summary>
    /// 值对象抽象基类
    /// </summary>
    public abstract class ValueObject
    {
        /// <summary>
        /// 获取对象的原子值集合
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<object> GetAtomicValues();

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType()) return false;

            ValueObject other = (ValueObject)obj;
            IEnumerator<object> thisValues = GetAtomicValues().GetEnumerator();
            IEnumerator<object> otherValues = other.GetAtomicValues().GetEnumerator();

            while(thisValues.MoveNext() && otherValues.MoveNext())
            {
                var thisCurrent = thisValues.Current;
                var otherCurrent = otherValues.Current;

                if (ReferenceEquals(thisCurrent, null) ^ ReferenceEquals(otherCurrent, null)) return false;
                if (thisCurrent != null && !thisCurrent.Equals(otherCurrent)) return false;
            }

            return !thisValues.MoveNext() && !otherValues.MoveNext();
        }

        public override int GetHashCode()
        {
            int code = 0;
            var values = GetAtomicValues();
            foreach (var value in values)
            {
                if (value == null) continue;
                code += value.GetHashCode();
            } 
            return code;
        }
    }
}
