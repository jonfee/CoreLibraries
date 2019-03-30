using JF.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace JF.ExceptionHandler
{
    /// <summary>
    /// 异常处一时中间件扩展类
    /// </summary>
    public static class ExceptionMiddlewareExtensions
    {
        /// <summary>
        /// 注册一个异常处理器。
        /// </summary>
        /// <param name="app"></param>
        /// <param name="handle"><see cref="ExceptionHandle"/>异常处理委托。</param>
        /// <returns></returns>
        public static IApplicationBuilder UseExceptionHandler(this IApplicationBuilder app, ExceptionHandler handle = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<ExceptionHandleMiddleware>(handle);
        }
    }

    /// <summary>
    /// 异常处理中间件
    /// </summary>
    public class ExceptionHandleMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ExceptionHandler handle;

        public ExceptionHandleMiddleware(RequestDelegate next, ExceptionHandler handle = null)
        {
            this.next = next;
            this.handle = handle;
        }

        public async Task Invoke(HttpContext context)
        {
            Exception exception = null;

            try
            {
                await next(context);
            }
            catch (JFAuthorizationException authEx)
            {
                exception = authEx;
            }
            catch (JFException hqEx)
            {
                exception = hqEx;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                if (exception != null)
                {
                    handle?.Invoke(exception);
                }
            }

            await next.Invoke(context);
        }
    }
}
