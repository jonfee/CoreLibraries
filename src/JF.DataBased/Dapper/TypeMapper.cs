using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dapper
{
    /// <summary>
    /// Type Mapper
    /// </summary>
    public static class TypeMapper
    {
        private static List<Type> typed = new List<Type>();

        /// <summary>
        /// 注入到Dapper.SqlMapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void InjectToSqlMapper<T>() where T : class
        {
            var type = typeof(T);

            type.InjectToSqlMapper();
        }

        /// <summary>
        /// 注入到Dapper.SqlMapper
        /// </summary>
        /// <param name="type"></param>
        public static void InjectToSqlMapper(this Type type)
        {
            if (type.ContainsGenericParameters) return;
            if (string.IsNullOrWhiteSpace(type.Namespace)) return;
            if (typed.Contains(type)) return;

            var mapper = (SqlMapper.ITypeMap)Activator.CreateInstance(typeof(ColumnAttributeTypeMapper<>).MakeGenericType(type));

            if (mapper != null) SqlMapper.SetTypeMap(type, mapper);

            typed.Add(type);
        }

        /// <summary>
        /// 将程序集中对应的类型注入到Dapper.SqlMapper，以用于Query查询映射。
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="namespaceStart"></param>
        public static void Initialize(Assembly assembly, string namespaceStart = null)
        {
            var types = from type in assembly.GetTypes()
                        where type.IsClass && !type.ContainsGenericParameters && !string.IsNullOrWhiteSpace(type.Namespace)
                        select type;

            if (!string.IsNullOrWhiteSpace(namespaceStart))
            {
                types = types.Where(type => type.Namespace.StartsWith(@namespaceStart));
            }

            types.ToList().ForEach(type =>
            {
                type.InjectToSqlMapper();
            });
        }
    }
}