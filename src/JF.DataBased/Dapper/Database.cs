﻿using JF.DataBased.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;

namespace Dapper
{
    /// <summary>
    /// 针对于Dapper的一个数据库操作类。
    /// </summary>
    /// <typeparam name="TDatabase"></typeparam>
    public class Database<TDatabase> : IDisposable
       where TDatabase : Database<TDatabase>
    {
        #region private variables
        private static Action<TDatabase> tableConstructor;
        ConcurrentDictionary<Type, dynamic> typedTables;
        #endregion

        #region contructors

        public Database()
        {
            this.typedTables = new ConcurrentDictionary<Type, dynamic>();
        }

        #endregion

        #region properties

        /// <summary>
        /// IDbConnection
        /// </summary>
        protected IDbConnection Connection { get; set; }

        public IDbTransaction CurrentTransaction { get; set; }

        protected int? CommandTimeout { get; set; }

        #endregion

        #region Database API

        /// <summary>
        /// 获取<see cref="IDbConnection"/>对象实例。
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetDbConnection()
        {
            return this.Connection;
        }

        /// <summary>
        /// 获取一个Table表对象
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            DbSet<TEntity> table = null;
            var entityType = typeof(TEntity);

            if (this.typedTables.TryGetValue(entityType, out var tableObj))
            {
                table = tableObj as DbSet<TEntity>;
            }
            else
            {
                table = new DbSet<TEntity>(this);
                this.typedTables[entityType] = table;
            }

            return table;
        }

        public virtual IDbTransaction BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            return CurrentTransaction = Connection.BeginTransaction();
        }

        /// <summary>
        /// 事务提交
        /// </summary>
        public virtual void CommitTransaction()
        {
            CurrentTransaction.Commit();
            CurrentTransaction = null;
        }

        /// <summary>
        /// 事务回滚
        /// </summary>
        public virtual void RollbackTransaction()
        {
            CurrentTransaction.Rollback();
            CurrentTransaction = null;
        }

        public virtual int ExecuteSqlCommand(string sql, object param = null, IDbTransaction transaction = null)
        {
            return Connection.Execute(sql, param, transaction, CommandTimeout);
        }

        public virtual IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true)
        {
            typeof(T).InjectToSqlMapper();

            DynamicParameters parameters = new DynamicParameters(param);

            return Connection.Query<T>(sql,
                                        parameters,
                                        transaction,
                                        buffered: buffered,
                                        commandTimeout: CommandTimeout);
        }

        public virtual IEnumerable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class
        {
            return Set<TEntity>().Search(expression);
        }


        public virtual IEnumerable<T> ProcQuery<T>(string procName, object param = null, bool buffered = true)
        {
            typeof(T).InjectToSqlMapper();
            DynamicParameters parameters = new DynamicParameters(param);
            return Connection.Query<T>(procName,
                                        parameters,
                                        buffered: buffered,
                                        commandTimeout: CommandTimeout,
                                        commandType: CommandType.StoredProcedure);
        }
        #endregion

        #region Table<TEntity>

        public class DbSet<TEntity> where TEntity : class
        {
            private Database<TDatabase> _database;
            private TableMetadata _metadata;

            public DbSet(Database<TDatabase> database)
            {
                _database = database;
                Init();
            }

            private void Init()
            {
                _metadata = GetTableMetadata();
                typeof(TEntity).InjectToSqlMapper();
            }

            private static readonly ConcurrentDictionary<Type, TableMetadata> TableMetadataCache = new ConcurrentDictionary<Type, TableMetadata>();
            private static TableMetadata GetTableMetadata()
            {
                var entityType = typeof(TEntity);
                if (!TableMetadataCache.TryGetValue(entityType, out var metadata))
                {
                    metadata = TableMetadata.From(entityType);
                    TableMetadataCache[entityType] = metadata;
                }

                return metadata;
            }

            static readonly ConcurrentDictionary<Type, string> TableNameCache = new ConcurrentDictionary<Type, string>();
            private static string GetTableName()
            {
                if (!TableNameCache.TryGetValue(typeof(TEntity), out var tableName))
                {
                    var tableAttr = typeof(TEntity).GetTypeInfo().GetCustomAttribute<TableAttribute>();
                    tableName = tableAttr == null || string.IsNullOrWhiteSpace(tableAttr.Name)
                                        ? typeof(TEntity).Name
                                        : tableAttr.Name;

                    TableNameCache[typeof(TEntity)] = tableName;
                }

                return tableName;
            }

            static readonly ConcurrentDictionary<Type, List<ColumnInfo>> ColumnsInfoCache = new ConcurrentDictionary<Type, List<ColumnInfo>>();
            private static List<ColumnInfo> GetColumnsInfo()
            {
                if (!ColumnsInfoCache.TryGetValue(typeof(TEntity), out var columnsInfo))
                {
                    var columnProperties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null);

                    ColumnsInfoCache[typeof(TEntity)] = columnsInfo;
                }

                return columnsInfo;
            }

            /// <summary>
            /// 插入数据
            /// </summary>
            /// <param name="entity"></param>
            /// <returns></returns>
            public int Insert(TEntity entity)
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));

                var paramList = GetInsertParamNames(entity, this._metadata);

                string columns = string.Join(", ", paramList.Select(paramName => _metadata.PropertyColumnMaps[paramName]));
                string values = string.Join(", ", paramList.Select(paramName => $"@{paramName}"));
                string sql = $"insert into {_metadata.TableName} ({columns}) values ({values})";

                return _database.ExecuteSqlCommand(sql, entity);
            }

            public int Update(TEntity entity)
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));

                if (!_metadata.HasKey)
                {
                    throw new InvalidOperationException($"No primary key specified for entity '{typeof(TEntity).FullName}'.");
                }

                var paramList = GetUpdateParamNames(entity, this._metadata);

                if (_metadata.HasKey)
                {
                    foreach (string key in _metadata.KeyProperties)
                    {
                        paramList.Remove(key);
                    }
                }

                foreach (var computedProperty in _metadata.ComputedProperties)
                {
                    paramList.Remove(computedProperty);
                }

                StringBuilder builder = new StringBuilder($"update {_metadata.TableName}").Append(" set ");
                builder.AppendLine(string.Join(", ", paramList.Select(paramName => $"{_metadata.PropertyColumnMaps[paramName]} = @{paramName}")));
                builder.Append("where ").Append(string.Join(" AND ", _metadata.KeyProperties.Select(key => $"{_metadata.PropertyColumnMaps[key]} = @{key}")));

                DynamicParameters param = new DynamicParameters(entity);

                //if (_metadata.HasCompositeKey)
                //{
                //    param.AddDynamicParams(id);
                //}
                //else
                //{
                //    param.Add(_metadata.KeyProperties[0], id);
                //}

                return _database.ExecuteSqlCommand(builder.ToString(), param);
            }

            ///// <summary>
            ///// 
            ///// </summary>
            ///// <param name="id"></param>
            ///// <param name="data"></param>
            ///// <returns></returns>
            //public int Update(object id, object data)
            //{
            //    if (!_metadata.HasKey)
            //    {
            //        throw new InvalidOperationException($"No primary key specified for entity '{typeof(TEntity).FullName}'.");
            //    }

            //    var paramList = GetParamNames(data);
            //    if (_metadata.HasKey)
            //    {
            //        foreach (string key in _metadata.KeyProperties)
            //        {
            //            paramList.Remove(key);
            //        }
            //    }

            //    foreach (var computedProperty in _metadata.ComputedProperties)
            //    {
            //        paramList.Remove(computedProperty);
            //    }

            //    StringBuilder builder = new StringBuilder($"update {_metadata.TableName}").Append(" set ");
            //    builder.AppendLine(string.Join(", ", paramList.Select(paramName => $"{_metadata.PropertyColumnMaps[paramName]} = @{paramName}")));
            //    builder.Append("where ").Append(string.Join(" AND ", _metadata.KeyProperties.Select(key => $"{_metadata.PropertyColumnMaps[key]} = @{key}")));

            //    DynamicParameters param = new DynamicParameters(data);
            //    if (_metadata.HasCompositeKey)
            //    {
            //        param.AddDynamicParams(id);
            //    }
            //    else
            //    {
            //        param.Add(_metadata.KeyProperties[0], id);
            //    }

            //    return _database.Execute(builder.ToString(), param);
            //}

            public int Delete(TEntity entity)
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));

                WhereByEntity(entity, out string clause);

                StringBuilder builder = new StringBuilder($"delete from {_metadata.TableName}");
                builder.Append(" where ").Append(clause);

                DynamicParameters param = new DynamicParameters(entity);

                return _database.ExecuteSqlCommand(builder.ToString(), param);
            }

            public int Delete(object id)
            {
                WhereById(id, out string clause, out object param);

                StringBuilder builder = new StringBuilder($"delete from {_metadata.TableName}");
                builder.Append(" where ").Append(clause);

                return _database.ExecuteSqlCommand(builder.ToString(), param);
            }

            public int Delete(Expression<Func<TEntity, bool>> expression)
            {
                StringBuilder builder = new StringBuilder($"delete from {_metadata.TableName}");
                builder.Append(ResolveWhereTsql(expression));

                return _database.ExecuteSqlCommand(builder.ToString());
            }

            public TEntity Find(object id)
            {
                WhereById(id, out string clause, out object param);

                //StringBuilder builder = new StringBuilder("select ");
                //builder.Append(string.Join(", ", _metadata.ColumnPropertyMaps.Select(item => $"{item.Key} AS {item.Value}")));
                //builder.AppendLine($" from {_metadata.TableName}");
                //builder.Append("where ").Append(clause);

                StringBuilder builder = new StringBuilder($"select * from {_metadata.TableName} ");
                builder.Append("where ").Append(clause);

                return _database.Query<TEntity>(builder.ToString(), param).SingleOrDefault();
            }

            public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> expression)
            {
                //StringBuilder builder = new StringBuilder("select ");
                //builder.Append(string.Join(", ", _metadata.ColumnPropertyMaps.Select(item => $"{item.Key} AS {item.Value}")));
                //builder.AppendLine($" from {_metadata.TableName}");
                //builder.Append(ResolveWehreTsql(expression));

                StringBuilder builder = new StringBuilder($"select * from {_metadata.TableName} ");
                builder.Append(ResolveWhereTsql(expression));

                return _database.Query<TEntity>(builder.ToString()).FirstOrDefault();
            }

            public bool Exists(Expression<Func<TEntity, bool>> expression)
            {
                StringBuilder builder = new StringBuilder($"select count(1) from {_metadata.TableName}");
                builder.Append(ResolveWhereTsql(expression));

                var aa = _database.Query<int>(builder.ToString()).FirstOrDefault();

                return aa > 0;
            }

            public IEnumerable<TEntity> All()
            {
                //StringBuilder builder = new StringBuilder("select ");
                //builder.Append(string.Join(", ", _metadata.ColumnPropertyMaps.Select(item => $"{item.Key} AS {item.Value}")));
                //builder.AppendLine($" from {_metadata.TableName}");

                StringBuilder builder = new StringBuilder($"select * from {_metadata.TableName}");

                return _database.Query<TEntity>(builder.ToString());
            }

            public IEnumerable<TEntity> Search<S>(int pageIndex, int pageSize, out int totalCount, Expression<Func<TEntity, bool>> query, Expression<Func<TEntity, S>> orderBy, bool orderAsc = true)
            {
                if (pageIndex < 1) pageIndex = 1;
                if (pageSize < 1) pageSize = 1;
                string where = ResolveWhereTsql(query);
                string orders = ResolveOrderByTsql(orderBy, orderAsc);
                int limitStart = (pageIndex - 1) * pageSize;

                if (!string.IsNullOrWhiteSpace(where)) where = $" where {where}";
                if (!string.IsNullOrWhiteSpace(orders)) orders = $" order by {orders}";

                StringBuilder dataSqlBuilder = new StringBuilder($"select * from {_metadata.TableName}{where}{orders} limit {limitStart},{limitStart + pageSize}");
                StringBuilder countSqlBuilder = new StringBuilder($"select count(1) AS Count from {_metadata.TableName}{where}");

                IEnumerable<dynamic> list = _database.Connection.Query(countSqlBuilder.ToString());
                totalCount = list?.FirstOrDefault()?.Count;
                return _database.Query<TEntity>(dataSqlBuilder.ToString());
            }

            public IEnumerable<TEntity> Search(string where = null, object param = null, string ordering = null)
            {
                //StringBuilder builder = new StringBuilder("select ");
                //builder.Append(string.Join(", ", _metadata.ColumnPropertyMaps.Select(item => $"{item.Key} AS {item.Value}")));
                //builder.AppendLine($" from {_metadata.TableName}");

                StringBuilder builder = new StringBuilder($"select * from {_metadata.TableName}");

                if (!string.IsNullOrWhiteSpace(where))
                {
                    builder.Append(" where ").AppendLine(ReplacePropertyNameWithColumnName(where));
                }
                if (!string.IsNullOrWhiteSpace(ordering))
                {
                    builder.Append(" order by ").AppendLine(ReplacePropertyNameWithColumnName(ordering));
                }

                return _database.Query<TEntity>(builder.ToString(), param);
            }

            public IEnumerable<TEntity> Search(Expression<Func<TEntity, bool>> expression)
            {
                //StringBuilder builder = new StringBuilder("select ");
                //builder.Append(string.Join(", ", _metadata.ColumnPropertyMaps.Select(item => $"{item.Key} AS {item.Value}")));
                //builder.Append($" from {_metadata.TableName}");
                //builder.Append(ResolveWehreTsql(expression));

                StringBuilder builder = new StringBuilder($"select * from {_metadata.TableName}");
                builder.Append(ResolveWhereTsql(expression));

                return _database.Query<TEntity>(sql: builder.ToString());
            }

            public IEnumerable<TEntity> Search(string sql, object paramters)
            {
                return _database.Query<TEntity>(sql, paramters);
            }

            #region pvivate functions

            private string ResolveWhereTsql(Expression<Func<TEntity, bool>> expression)
            {
                string whereTsql = string.Empty;
                if (expression != null)
                {
                    var where = SqlGenerate.GetWhereByLambda(expression);

                    if (!string.IsNullOrEmpty(where))
                    {
                        whereTsql = $" where {ReplacePropertyNameWithColumnName(where)}";
                    }
                }
                return whereTsql;
            }

            /// <summary>
            /// 解析Order by语句
            /// </summary>
            /// <param name="expression"></param>
            /// <param name="asc">是否为升序，true为升序，false为降序</param>
            /// <returns></returns>
            private string ResolveOrderByTsql<S>(Expression<Func<TEntity, S>> expression, bool asc = true)
            {
                List<string> orderTsqls = new List<string>();
                if (expression != null)
                {
                    var parameters = expression.Parameters;

                    foreach (var para in parameters)
                    {
                        orderTsqls.Add($"{para.Name} {(asc ? "ASC" : "DESC")}");
                    }
                }
                return string.Join(",", orderTsqls);
            }

            private string ReplacePropertyNameWithColumnName(string clause)
            {
                string s = clause;

                foreach (var map in _metadata.PropertyColumnMaps)
                {
                    Regex rx = new Regex($"(?<!@){map.Key}", RegexOptions.IgnoreCase); // replace property name which not follow with '@' to the corresponding column name
                    s = rx.Replace(s, map.Value);
                }

                return s;
            }

            private void WhereByEntity(TEntity entity, out string clause)
            {
                clause = string.Join(" AND ", _metadata.KeyProperties.Select(key => $"{_metadata.PropertyColumnMaps[key]} = @{key}"));
            }

            private void WhereById(object id, out string clause, out object param)
            {
                DynamicParameters parameter;
                if (_metadata.HasCompositeKey)
                {
                    parameter = new DynamicParameters(id);
                }
                else
                {
                    parameter = new DynamicParameters();
                    parameter.Add(_metadata.KeyProperties[0], id);
                }

                clause = string.Join(" AND ", _metadata.KeyProperties.Select(key => $"{_metadata.PropertyColumnMaps[key]} = @{key}"));
                param = parameter;
            }

            //static ConcurrentDictionary<Type, List<string>> paramNameCache = new ConcurrentDictionary<Type, List<string>>();

            //internal static List<string> GetParamNames(object o)
            //{
            //    var parameters = o as DynamicParameters;
            //    if (parameters != null)
            //    {
            //        return parameters.ParameterNames.ToList();
            //    }

            //    List<string> paramNames;
            //    if (!paramNameCache.TryGetValue(o.GetType(), out paramNames))
            //    {
            //        paramNames = new List<string>();
            //        foreach (var prop in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetGetMethod(false) != null))
            //        {
            //            if (prop.GetCustomAttribute<NotMappedAttribute>() == null)
            //            {
            //                paramNames.Add(prop.Name);
            //            }
            //        }
            //        paramNameCache[o.GetType()] = paramNames;
            //    }
            //    return paramNames;
            //}

            static ConcurrentDictionary<Type, List<string>> updateParamNameCache = new ConcurrentDictionary<Type, List<string>>();
            internal static List<string> GetUpdateParamNames(object o, TableMetadata metadata)
            {
                var parameters = o as DynamicParameters;
                if (parameters != null)
                {
                    return parameters.ParameterNames.ToList();
                }

                List<string> paramNames = null;
                if (!updateParamNameCache.TryGetValue(o.GetType(), out paramNames))
                {
                    paramNames = new List<string>();
                    foreach (var prop in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetGetMethod(false) != null))
                    {
                        if (prop.GetCustomAttribute<NotMappedAttribute>() == null)
                        {
                            paramNames.Add(prop.Name);
                        }
                    }

                    if (metadata.HasKey)
                    {
                        foreach (string key in metadata.KeyProperties)
                        {
                            if (!paramNames.Contains(key)) continue;
                            paramNames.Remove(key);
                        }
                    }

                    foreach (var computedProperty in metadata.ComputedProperties)
                    {
                        if (!paramNames.Contains(computedProperty)) continue;
                        paramNames.Remove(computedProperty);
                    }

                    updateParamNameCache[o.GetType()] = paramNames;
                }
                return paramNames;
            }

            static ConcurrentDictionary<Type, List<string>> insertParamNameCache = new ConcurrentDictionary<Type, List<string>>();
            internal static List<string> GetInsertParamNames(object o, TableMetadata metadata)
            {
                var parameters = o as DynamicParameters;
                if (parameters != null)
                {
                    return parameters.ParameterNames.ToList();
                }

                List<string> paramNames = null;
                if (!insertParamNameCache.TryGetValue(o.GetType(), out paramNames))
                {
                    paramNames = new List<string>();
                    foreach (var prop in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetGetMethod(false) != null))
                    {
                        if (prop.GetCustomAttribute<NotMappedAttribute>() == null)
                        {
                            paramNames.Add(prop.Name);
                        }
                    }

                    if (metadata.HasIdentityKey && 
                        paramNames.Contains(metadata.KeyProperties[0]))
                    {
                        paramNames.Remove(metadata.KeyProperties[0]);
                    }

                    foreach (var computedProperty in metadata.ComputedProperties)
                    {
                        if (!paramNames.Contains(computedProperty)) continue;
                        paramNames.Remove(computedProperty);
                    }

                    insertParamNameCache[o.GetType()] = paramNames;
                }
                return paramNames;
            }

            #endregion
        }

        #endregion

        protected void InitDatabase(IDbConnection connection, int? commandTimeout = null)
        {
            this.Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.CommandTimeout = commandTimeout;
            this.typedTables = new ConcurrentDictionary<Type, object>();

            if (tableConstructor == null)
            {
                tableConstructor = CreateTableConstructorForTable();
            }

            tableConstructor(this as TDatabase);
        }

        private Action<TDatabase> CreateTableConstructorForTable()
        {
            return CreateTableConstructor(typeof(DbSet<>));
        }

        private Action<TDatabase> CreateTableConstructor(Type tableType)
        {
            var dm = new DynamicMethod("ConstructInstances", null, new[] { typeof(TDatabase) }, true);
            var il = dm.GetILGenerator();

            var setters = GetType().GetProperties()
                .Where(p => p.PropertyType.GetTypeInfo().IsGenericType && tableType == p.PropertyType.GetTypeInfo().GetGenericTypeDefinition())
                .Select(p => Tuple.Create(
                        p.GetSetMethod(true),
                        p.PropertyType.GetConstructor(new[] { typeof(TDatabase) }),
                        p.Name,
                        p.DeclaringType
                 ));

            foreach (var setter in setters)
            {
                il.Emit(OpCodes.Ldarg_0);
                // [db]

                il.Emit(OpCodes.Newobj, setter.Item2);
                // [table]

                var table = il.DeclareLocal(setter.Item2.DeclaringType);
                il.Emit(OpCodes.Stloc, table);
                // []

                il.Emit(OpCodes.Ldarg_0);
                // [db]

                il.Emit(OpCodes.Castclass, setter.Item4);
                // [db cast to container]

                il.Emit(OpCodes.Ldloc, table);
                // [db cast to container, table]

                il.Emit(OpCodes.Callvirt, setter.Item1);
                // []
            }

            il.Emit(OpCodes.Ret);
            return (Action<TDatabase>)dm.CreateDelegate(typeof(Action<TDatabase>));
        }

        public void Dispose()
        {
            if (Connection.State != ConnectionState.Closed)
            {
                CurrentTransaction?.Rollback();

                Connection.Close();
                Connection.Dispose();
                Connection = null;
            }
        }
    }
}
