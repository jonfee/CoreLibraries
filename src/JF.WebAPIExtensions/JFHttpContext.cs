using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace JF.WebAPIExtensions
{
    /// <summary>
    /// 自定义的HttpContext。
    /// 通过注入Http请求管道的方式，可以在Http管道程序中通过调用<see cref="JFHttpContext.Current"/>来找到当前的<see cref="HttpContext"/>对象。
    /// </summary>
    public static class JFHttpContext
    {
        private static IHttpContextAccessor _contextAccessor;

        public static HttpContext Current => _contextAccessor?.HttpContext;

        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
    }

    public static class HttpContextExtensions
    {
        private readonly static object locker = new object();

        /// <summary>
        /// 注册一个<see cref="IHttpContextAccessor"/>对象实例到当前请求上下文。
        /// 如果<see cref="JFHttpContext.Current"/>对象存在，则不会重复构建。
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
        {
            if (JFHttpContext.Current == null)
            {
                lock (locker)
                {
                    if (JFHttpContext.Current == null)
                    {
                        var accessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();

                        JFHttpContext.Configure(accessor);
                    }
                }
            }

            return app;
        }
    }
}
