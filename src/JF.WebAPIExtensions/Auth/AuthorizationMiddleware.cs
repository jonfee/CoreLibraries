using JF.Authorizer;
using JF.WebAPIExtensions.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JF.WebAPIExtensions.Auth
{
    /// <summary>
    /// 登录授权、验证中间件扩展类
    /// </summary>
    public static class AuthorizationMiddlewareExtensions
    {
        /// <summary>
        /// 注册一个登录授权、验证处理器。
        /// 会根据HTTP请求，自动获取登录授权信息。
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options">用户登录授权/验证中间件配置。</param>
        /// <returns></returns>
        public static IApplicationBuilder UseAuthorizationHandler(this IApplicationBuilder app, AuthorizationOptions options)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            // 注入静态的HttpContext对象
            app = app.UseStaticHttpContext();

            return app.UseMiddleware<AuthorizationMiddleware>(options);
        }
    }

    /// <summary>
    /// 登录授权、验证中间件
    /// </summary>
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate next;
        private readonly AuthorizationOptions options;

        public AuthorizationMiddleware(RequestDelegate next, AuthorizationOptions options)
        {
            this.next = next;
            this.options = options ?? throw new ArgumentNullException(nameof(options));

        }

        public async Task Invoke(HttpContext context)
        {
            JwtHandler handler = new JwtHandler(context, this.options);

            handler.Handle();

            await next.Invoke(context);
        }
    }
}
