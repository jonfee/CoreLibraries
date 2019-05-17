using JF.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace JF.DataBased.Core
{
    /// <summary>
    /// 脚本生成实体
    /// </summary>
    internal sealed class ConditionBuilder : ExpressionVisitor
    {
        /// <summary>
        /// 字段是否加引号
        /// </summary>
        private bool withQuotationMarks = false;
        private List<object> arguments;
        private Stack<string> conditionParts;

        /// <summary>
        /// 加双引号
        /// </summary>
        /// <param name="str">字串</param>
        /// <returns></returns>
        private string AddQuotationMarks(string str)
        {
            if (str.IsEmpty() && withQuotationMarks)
                return "\"" + str.Trim() + "\"";
            return str;
        }

        /// <summary>
        /// 执行生成
        /// </summary>
        /// <param name="expression"></param>
        public void Build(Expression expression)
        {
            PartialEvaluator evaluator = new PartialEvaluator();
            Expression evaluatedExpression = evaluator.Eval(expression);
            this.arguments = new List<object>();
            this.conditionParts = new Stack<string>();
            this.Visit(evaluatedExpression);
        }

        protected sealed override Expression VisitBinary(BinaryExpression b)
        {
            if (b == null)
                return b;
            string opr;
            switch (b.NodeType)
            {
                case ExpressionType.Equal:
                    opr = "=";
                    break;
                case ExpressionType.NotEqual:
                    opr = "<>";
                    break;
                case ExpressionType.GreaterThan:
                    opr = ">";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    opr = ">=";
                    break;
                case ExpressionType.LessThan:
                    opr = "<";
                    break;
                case ExpressionType.LessThanOrEqual:
                    opr = "<=";
                    break;
                case ExpressionType.AndAlso:
                    opr = "and";
                    break;
                case ExpressionType.OrElse:
                    opr = "or";
                    break;
                case ExpressionType.Add:
                    opr = "+";
                    break;
                case ExpressionType.Subtract:
                    opr = "-";
                    break;
                case ExpressionType.Multiply:
                    opr = "*";
                    break;
                case ExpressionType.Divide:
                    opr = "/";
                    break;
                default:
                    throw new NotSupportedException(b.NodeType + "is not supported.");
            }

            this.Visit(b.Left);
            this.Visit(b.Right);
            string right = this.conditionParts.Pop();
            string left = this.conditionParts.Pop();
            string condition = String.Format("({0} {1} {2})", left, opr, right);
            this.conditionParts.Push(condition);
            return b;
        }

        protected sealed override Expression VisitConstant(ConstantExpression c)
        {
            if (c == null)
                return c;
            this.arguments.Add(c.Value);
            this.conditionParts.Push(String.Format("{{{0}}}", this.arguments.Count - 1));
            return c;
        }

        protected sealed override Expression VisitMember(MemberExpression m)
        {
            if (m == null)
                return m;
            PropertyInfo propertyInfo = m.Member as PropertyInfo;
            if (propertyInfo == null)
                return m;
            this.conditionParts.Push(String.Format(" {0} ", AddQuotationMarks(propertyInfo.Name)));
            return m;
        }

        private string BinarExpressionProvider(Expression left, Expression right, ExpressionType type)
        {
            string stringInfo = "(";
            //先处理左边
            stringInfo += ExpressionRouter(left);
            stringInfo += ExpressionTypeCast(type);
            //再处理右边
            string tmpStr = ExpressionRouter(right);
            if (tmpStr.SameAs("null"))
            {
                if (stringInfo.EndsWith(" ="))
                    stringInfo = stringInfo.Substring(0, stringInfo.Length - 1) + " is null";
                else if (stringInfo.EndsWith("<>"))
                    stringInfo = stringInfo.Substring(0, stringInfo.Length - 1) + " is not null";
            }
            else
                stringInfo += tmpStr;
            return stringInfo += ")";
        }

        private string ExpressionRouter(Expression exp)
        {
            if (exp is BinaryExpression)
            {
                BinaryExpression be = ((BinaryExpression)exp);
                return BinarExpressionProvider(be.Left, be.Right, be.NodeType);
            }
            if (exp is MemberExpression)
            {
                MemberExpression me = ((MemberExpression)exp);
                return me.Member.Name;
            }
            if (exp is NewArrayExpression)
            {
                NewArrayExpression ae = ((NewArrayExpression)exp);
                StringBuilder tmpstr = new StringBuilder();
                foreach (Expression ex in ae.Expressions)
                {
                    tmpstr.Append(ExpressionRouter(ex));
                    tmpstr.Append(",");
                }
                return tmpstr.ToString(0, tmpstr.Length - 1);
            }
            if (exp is MethodCallExpression)
            {
                MethodCallExpression mce = (MethodCallExpression)exp;
                if (mce.Method.Name.SameAs("Like"))
                    return string.Format("({0} like {1})", ExpressionRouter(mce.Arguments[0]), ExpressionRouter(mce.Arguments[1]));
                else if (mce.Method.Name.SameAs("NotLike"))
                    return string.Format("({0} not like {1})", ExpressionRouter(mce.Arguments[0]), ExpressionRouter(mce.Arguments[1]));
                else if (mce.Method.Name.SameAs("In"))
                    return string.Format("{0} in ({1})", ExpressionRouter(mce.Arguments[0]), ExpressionRouter(mce.Arguments[1]));
                else if (mce.Method.Name.SameAs("NotIn"))
                    return string.Format("{0} not in ({1})", ExpressionRouter(mce.Arguments[0]), ExpressionRouter(mce.Arguments[1]));
                else if (mce.Method.Name.SameAs("StartWith"))
                    return string.Format("{0} like '{1}%'", ExpressionRouter(mce.Arguments[0]), ExpressionRouter(mce.Arguments[1]));
                return null;
            }
            if (exp is ConstantExpression)
            {
                ConstantExpression ce = ((ConstantExpression)exp);
                if (ce.Value == null)
                    return "null";
                else if (ce.Value is ValueType)
                    return ce.Value.ToString();
                else if (ce.Value is string || ce.Value is DateTime || ce.Value is char)
                    return string.Format("'{0}'", ce.Value.ToString());
                return null;
            }
            if (exp is UnaryExpression)
            {
                UnaryExpression ue = ((UnaryExpression)exp);
                return ExpressionRouter(ue.Operand);
            }
            return null;
        }

        private string ExpressionTypeCast(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return " and ";
                case ExpressionType.Equal:
                    return " =";
                case ExpressionType.GreaterThan:
                    return " >";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return " or ";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                default:
                    return null;
            }
        }

        /// <summary>
        /// ConditionBuilder 并不支持生成Like操作，如 字符串的 StartsWith，Contains，EndsWith 并不能生成这样的SQL： Like ‘xxx%’, Like ‘%xxx%’ , Like ‘%xxx’ . 只要override VisitMethodCall 这个方法即可实现上述功能。
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected sealed override Expression VisitMethodCall(MethodCallExpression m)
        {
            string connectorWords = GetLikeConnectorWords(DataBaseType);
            string format;
            bool isCollections = m.Method.ToString().Contains("System.Collections.Generic");
            switch (m.Method.Name.ToUpper())
            {
                case "STARTSWITH":
                    format = "({0} like '{1}%')";
                    break;
                case "CONTAINS":
                    if (isCollections)
                    {
                        format = "({0} in ({1}))";
                    }
                    else
                    {
                        format = "({0} like '%{1}%')";
                    }
                    break;
                case "ENDSWITH":
                    format = "({0} like '%{1}')";
                    break;
                case "EQUALS":
                    format = "({0} {1} )";
                    break;
                default:
                    throw new NotSupportedException(m.NodeType + " is not supported!");
            }
            this.Visit(m.Object ?? m.Arguments[1]);
            this.Visit(m.Arguments[0]);
            string right = this.conditionParts.Pop();
            string left = this.conditionParts.Pop();
            this.conditionParts.Push(String.Format(format, left, right));
            return m;
        }

        /// <summary>
        /// 获得like语句链接符
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        private string GetLikeConnectorWords(DataBaseType databaseType)
        {
            string result = "+";
            switch (databaseType)
            {
                case DataBaseType.PostgreSql:
                case DataBaseType.Oracle:
                case DataBaseType.MySql:
                case DataBaseType.Sqlite:
                    result = "||";
                    break;
            }
            return result;
        }

        /// <summary>
        /// 获取或者设置数据库类型
        /// </summary>
        public DataBaseType DataBaseType { get; set; } = DataBaseType.SqlServer;

        /// <summary>
        /// 
        /// </summary>
        public string Condition
        {
            get
            {
                if (this.conditionParts.Count > 0)
                    return this.conditionParts.Pop();
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<object> Arguments => this.arguments ?? new List<object>();

        /// <summary>
        /// 字段是否加引号
        /// </summary>
        public bool WithQuotationMarks
        {
            get { return this.withQuotationMarks; }
            set { this.withQuotationMarks = value; }
        }
    }
}
