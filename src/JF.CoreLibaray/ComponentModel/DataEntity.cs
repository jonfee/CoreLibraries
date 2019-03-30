using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace JF.ComponentModel
{
    /// <summary>
    /// DB数据模型抽象基类，
    /// 主要约束数据对象是否允许进行CURD操作，
    /// 本类已默认实现了对数据模型中<see cref="ValidationAttribute"/>特性属性的验证。
    /// </summary>
    public abstract class DataEntity : ValidationModel, IDataEntity
    {
        /// <summary>
        /// 验证数据对象是否允许插入。
        /// </summary>
        /// <param name="htErrors">out 错误信息</param>
        /// <returns></returns>
        public virtual bool CanInsert(out Hashtable htErrors)
        {
            return IsValid(out htErrors);
        }

        /// <summary>
        /// 数据对象是否允许插入。
        /// </summary>
        /// <returns></returns>
        public virtual bool CanInsert()
        {
            return IsValid(out Hashtable errors);
        }

        /// <summary>
        /// 验证数据对象是否允许更新。
        /// </summary>
        /// <param name="htErrors">out 错误信息</param>
        /// <returns></returns>
        public virtual bool CanUpdate(out Hashtable htErrors)
        {
            return IsValid(out htErrors);
        }

        /// <summary>
        /// 验证数据对象是否允许更新。
        /// </summary>
        /// <returns></returns>
        public virtual bool CanUpdate()
        {
            return IsValid(out Hashtable errors);
        }

        /// <summary>
        /// 验证数据对象是否允许删除。
        /// </summary>
        /// <returns></returns>
        public virtual bool CanDelete(out Hashtable htErrors)
        {
            return (htErrors = null) == null;
        }

        /// <summary>
        /// 验证数据对象是否允许删除。
        /// </summary>
        /// <returns></returns>
        public virtual bool CanDelete()
        {
            return true;
        }
    }
}
