using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dapper
{
    public class TableMetadata
    {
        public string TableName { get; set; }

        public IDictionary<string, string> PropertyColumnMaps { get; set; }

        public IDictionary<string, string> ColumnPropertyMaps { get; set; }

        public string[] KeyProperties { get; set; }

        public string[] ComputedProperties { get; set; }

        public bool HasKey { get; set; }

        public bool HasCompositeKey { get; set; }

        public bool HasIdentityKey { get; set; }


        public static TableMetadata From(Type entityType)
        {
            var tableAttr = entityType.GetTypeInfo().GetCustomAttribute<TableAttribute>();

            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null);

            var columnsInfo = GetColulmnsInfo(properties);

            if (columnsInfo.Count(c => c.IsKey) == 0)
            {
                var candidateKey = columnsInfo.FirstOrDefault(c => c.Property.Name.ToLower().Equals("id"))
                                    ?? columnsInfo.FirstOrDefault(c => c.Property.Name.ToLower().Equals($"{entityType.Name.ToLower()}id"));

                if (candidateKey != null)
                {
                    candidateKey.IsKey = true;
                    if (candidateKey.Property.PropertyType == typeof(int)
                        && candidateKey.Property.GetCustomAttribute<DatabaseGeneratedAttribute>() == null)
                    {
                        candidateKey.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;
                    }
                }
            }

            return new TableMetadata
            {
                TableName = string.IsNullOrWhiteSpace(tableAttr?.Name)
                            ? entityType.Name
                            : tableAttr.Name,
                HasKey = columnsInfo.Count(c => c.IsKey) > 0,
                HasCompositeKey = columnsInfo.Count(c => c.IsKey) > 1,
                HasIdentityKey = columnsInfo.Count(c => c.IsKey) == 1 && columnsInfo.Count(c => c.IsKey && c.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity) == 1,
                KeyProperties = columnsInfo.Where(c => c.IsKey).Select(c => c.Property.Name).ToArray(),
                ComputedProperties = columnsInfo.Where(c => c.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed).Select(c => c.Property.Name).ToArray(),
                PropertyColumnMaps = columnsInfo.ToDictionary(c => c.Property.Name, c => c.ColumnName),
                ColumnPropertyMaps = columnsInfo.ToDictionary(c => c.ColumnName, c => c.Property.Name)
            };
        }


        private static List<ColumnInfo> GetColulmnsInfo(IEnumerable<PropertyInfo> properties)
        {
            if (properties == null) return null;

            var columnsInfo = new List<ColumnInfo>();

            foreach (var property in properties)
            {
                var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
                var keyAttr = property.GetCustomAttribute<KeyAttribute>();
                var databaseGeneratedAttr = property.GetCustomAttribute<DatabaseGeneratedAttribute>();

                columnsInfo.Add(new ColumnInfo
                {
                    Property = property,
                    ColumnName = string.IsNullOrWhiteSpace(columnAttr?.Name)
                                ? property.Name
                                : columnAttr.Name,
                    IsKey = keyAttr != null,
                    DatabaseGeneratedOption = databaseGeneratedAttr == null
                                            ? DatabaseGeneratedOption.None
                                            : databaseGeneratedAttr.DatabaseGeneratedOption
                });
            }

            return columnsInfo;
        }
    }
}
