using JF.Authorizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JF.WebAPIExtensions
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
            try
            {
                if (TryGetToken(context, out var token))
                {
                    if (TryGetAuthUser(context, token, out string agentCode, out AuthUser user, out var errors))
                    {
                        // 用户登录的用户信息写入上下文
                        context.Items.Add(Settings.HTTPCONTEXT_ITEMNAME_WITH_AUTHUSER, user);

                        // 如果为滑动过期，则更新令牌过期时间
                        if (options.ExpireMode == TokenExpireMode.SlidingTime)
                        {
                            // 更新令牌信息
                            TryUpdateToken(context, agentCode, token, options.ExpireMinutes);
                        }
                    }
                    else
                    {
                        // 将错误信息记录在HttpContext
                        context.Items.Add(Settings.HTTPCONTEXT_ITEMNAME_WITH_TOKENERRORS, errors);
                    }
                }
            }
            catch { }

            await next.Invoke(context);
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="context"></param>
        /// <param name="agentCode"></param>
        /// <param name="token"></param>
        /// <param name="expireMinutes"></param>
        /// <returns></returns>
        private bool TryUpdateToken(HttpContext context, string agentCode, string token, int expireMinutes)
        {
            bool success = true;

            try
            {
                if (options.UpdateTokenFunc != null)
                {
                    expireMinutes = expireMinutes > 0 ? expireMinutes : Settings.DEFAULT_EXPIRE_MINUTES;

                    var jfToken = new JFToken
                    {
                        Token = token,
                        ExpireTicks = DateTime.Now.AddMinutes(expireMinutes).Ticks
                    };

                    success = options.UpdateTokenFunc.Invoke(agentCode, jfToken);
                }
            }
            catch
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// 从HTTP请求中，获取授权登录的用户信息。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <param name="user"></param>
        /// <param name="agentCode"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private bool TryGetAuthUser(HttpContext context, string token, out string agentCode, out AuthUser user, out List<string> errors)
        {
            user = null;
            agentCode = null;
            errors = null;

            try
            {
                var tokenProvider = new TokenProvider(options.PublicKey);
                var requestIP = options.NeedCheckIP ? context.Connection.RemoteIpAddress.ToString() : null;

                tokenProvider.TryResolve(token, requestIP, options.ReadTokenFunc, out agentCode, out user, out errors);
            }
            catch
            {
                user = null;
            }

            return user != null;
        }

        /// <summary>
        /// 从HTTP请求中，获取授权令牌信息。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool TryGetToken(HttpContext context, out string token)
        {
            token = null;

            try
            {
                if (context.Request.Headers.ContainsKey(options.HttpHeaderWith))
                {
                    token = context.Request.Headers[options.HttpHeaderWith];
                }
            }
            catch
            {
                token = string.Empty;
            }

            return !string.IsNullOrEmpty(token);
        }
    }

    /// <summary>
    /// 用户登录授权/验证中间件配置。
    /// </summary>
    public class AuthorizationOptions
    {
        /// <summary>
        /// 登录授权令牌令牌在HTTP请求的Header中的名称。
        /// </summary>
        public string HttpHeaderWith { get; set; }

        /// <summary>
        /// 公钥
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// 是否需要校验IP
        /// </summary>
        public bool NeedCheckIP { get; set; }

        /// <summary>
        /// 过期策略
        /// </summary>
        public TokenExpireMode ExpireMode { get; set; }

        /// <summary>
        /// 失效的时间（单位：分钟）
        /// </summary>
        public int ExpireMinutes { get; set; }

        /// <summary>
        /// 根据授权代理编号读取服务器端存储的令牌信息委托方法。
        /// 如未提供此委托，将按令牌自解析后的过期时间做为令牌失效的验证标准。
        /// </summary>
        public Func<string, JFToken> ReadTokenFunc { get; set; }

        /// <summary>
        /// 更新指定授权代理的令牌信息。
        /// 一般为滑动过期时，更新令牌失效时间。
        /// </summary>
        public Func<string, JFToken, bool> UpdateTokenFunc { get; set; }
    }
}
