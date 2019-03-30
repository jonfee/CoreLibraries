using JF.Exceptions;
using System;

namespace JF.ExceptionHandler
{
    /// <summary>
    /// 异常详情
    /// </summary>
    public class ExceptionDetails
    {
        public ExceptionDetails()
        {
            this.OSVersion = Environment.OSVersion.VersionString;
            this.OSBit = Environment.Is64BitProcess ? 64 : 32;
            this.ProcessorCount = Environment.ProcessorCount;
            this.CreatedTime = DateTime.Now;
        }

        /// <summary>
        /// 异常等级
        /// </summary>
        public ExceptionLevel Level
        {
            get
            {
                ExceptionLevel level = default(ExceptionLevel);

                if (Exception is JFValidateException)
                {
                    level = ExceptionLevel.Validation;
                }
                else if (Exception is JFAuthorizationException)
                {
                    level = ExceptionLevel.Authority;
                }
                else if(Exception is JFOperationAccessException)
                {
                    level = ExceptionLevel.OperationAccess;
                }
                else if (Exception is JFBusinessException)
                {
                    level = ExceptionLevel.Business;
                }
                else if (Exception is JFException)
                {
                    level = ExceptionLevel.DefaultCustom;
                }
                else
                {
                    level = ExceptionLevel.Normal;
                }

                return level;
            }
        }

        /// <summary>
        /// 异常详细信息。
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 操作系统版本。
        /// </summary>
        public string OSVersion { get; }

        /// <summary>
        /// 操作系统位数。
        /// </summary>
        public int OSBit { get; }

        /// <summary>
        /// CPU处理数。
        /// </summary>
        public int ProcessorCount { get; }

        /// <summary>
        /// 异常产生时间。
        /// </summary>
        public DateTime CreatedTime { get; }

        public static ExceptionDetails From(Exception ex)
        {
            if (ex == null) return null;

            return new ExceptionDetails
            {
                Exception = ex
            };
        }

        /// <summary>
        /// 将<see cref="Exception"/>对象隐式转换为<see cref="ExceptionDetails"/>类型。
        /// </summary>
        /// <param name="ex"></param>
        public static implicit operator ExceptionDetails(Exception ex)
        {
            return From(ex);
        }
    }
}
