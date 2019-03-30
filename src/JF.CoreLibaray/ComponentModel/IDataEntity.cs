using System.Collections;

namespace JF.ComponentModel
{
    /// <summary>
    /// DB数据模型接口。
    /// 主要约束数据对象是否允许进行CURD操作。
    /// </summary>
    public interface IDataEntity
    {
        /// <summary>
        /// 验证数据对象是否允许插入。
        /// </summary>
        /// <param name="htErrors">out 错误信息</param>
        /// <returns></returns>
        bool CanInsert(out Hashtable htErrors);

        /// <summary>
        /// 验证数据对象是否允许插入。
        /// </summary>
        /// <returns></returns>
        bool CanInsert();

        /// <summary>
        /// 验证数据对象是否允许更新。
        /// </summary>
        /// <param name="htErrors"></param>
        /// <returns></returns>
        bool CanUpdate(out Hashtable htErrors);

        /// <summary>
        /// 验证数据对象是否允许更新。
        /// </summary>
        /// <returns></returns>
        bool CanUpdate();

        /// <summary>
        /// 验证数据对象是否允许删除。
        /// </summary>
        /// <returns></returns>
        bool CanDelete(out Hashtable htErrors);

        /// <summary>
        /// 验证数据对象是否允许删除。
        /// </summary>
        /// <returns></returns>
        bool CanDelete();
    }
}
