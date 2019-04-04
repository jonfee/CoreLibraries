using System;
using System.Linq.Expressions;

namespace JF.DataBased.Core
{
    /// <summary>
    /// SQL生成类
    /// </summary>
    public class SqlGenerate
    {
        /// <summary>
        /// 生成SQL-Where语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">表达式</param>
        /// <param name="databaseType">数据类型（用于字段是否加引号）</param>
        /// <returns></returns>
        public static string GetWhereByLambda<T>(Expression<Func<T, bool>> expression, DataBaseType databaseType = DataBaseType.SqlServer)
        {
            bool withQuotationMarks = GetWithQuotationMarks(databaseType);
            ConditionBuilder conditionBuilder = new ConditionBuilder();
            conditionBuilder.WithQuotationMarks = withQuotationMarks; //字段是否加引号（PostGreSql,Oracle）
            conditionBuilder.DataBaseType = databaseType;
            conditionBuilder.Build(expression);

            object ce = null;
            for (int i = 0; i < conditionBuilder.Arguments.Count; i++)
            {
                ce = conditionBuilder.Arguments[i];
                if (ce == null)
                {
                    conditionBuilder.Arguments[i] = DBNull.Value;
                    continue;
                }
                if (ce is string || ce is char)
                {
                    bool isQuote = ce.ToString().ToLower().Trim().IndexOf(@"in(") == 0 ||
                        ce.ToString().ToLower().Trim().IndexOf(@"not in(") == 0 ||
                        ce.ToString().ToLower().Trim().IndexOf(@" like '") == 0 ||
                        ce.ToString().ToLower().Trim().IndexOf(@"not like") == 0;
                    conditionBuilder.Arguments[i] = string.Format(" {1}{0}{2} ", ce.ToString(), isQuote ? "" : "'", isQuote ? "" : "'");
                    continue;
                }
                if (ce is int || ce is long || ce is short || ce is decimal || ce is double || ce is float || ce is bool || ce is byte || ce is sbyte || ce is ValueType)
                {
                    conditionBuilder.Arguments[i] = ce.ToString();
                    continue;
                }
                conditionBuilder.Arguments[i] = string.Format("'{0}'", ce.ToString());
            }
            return string.Format(conditionBuilder.Condition, conditionBuilder.Arguments.ToArray());
        }

        /// <summary>
        /// 获取是否字段加双引号
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        private static bool GetWithQuotationMarks(DataBaseType databaseType)
        {
            bool result = false;

            switch (databaseType)
            {
                case DataBaseType.PostgreSql:
                case DataBaseType.Oracle:
                    result = true;
                    break;
            }

            return result;
        }
    }
}
