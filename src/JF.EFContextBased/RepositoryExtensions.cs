using JF.ComponentModel;

namespace JF.EFContextBased
{
    /// <summary>
    /// 子仓服务扩展类
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// 将<see cref="IChildRepository{T}"/>对象转化为指定的类型。
        /// </summary>
        /// <typeparam name="TChildRepositoryResult"></typeparam>
        /// <typeparam name="TDataEntity"></typeparam>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static TChildRepositoryResult ConvertFor<TDataEntity, TChildRepositoryResult>(this IChildRepository<TDataEntity> repository)
            where TDataEntity : DataEntity
            where TChildRepositoryResult : class
        {
            TChildRepositoryResult realRepository = default(TChildRepositoryResult);

            try
            {
                if (repository is TChildRepositoryResult)
                {
                    realRepository = repository as TChildRepositoryResult;
                }
            }
            catch
            {
                realRepository = default(TChildRepositoryResult);
            }

            return realRepository;
        }
    }
}
