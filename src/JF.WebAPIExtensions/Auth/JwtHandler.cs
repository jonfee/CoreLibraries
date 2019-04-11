using JF.Authorizer;
using JF.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace JF.WebAPIExtensions.Auth
{
    /// <summary>
    /// 持票验证处理者
    /// </summary>
    internal sealed class JwtHandler
    {
        private HttpContext context;

        private AuthorizationOptions options;

        public JwtHandler(HttpContext context, AuthorizationOptions options)
        {
            this.context = context;
            this.options = options;
        }

        /// <summary>
        /// 处理授权信息
        /// </summary>
        /// <returns></returns>
        public void Handle()
        {
            List<string> errors = null;
            try
            {
                if (TryGetToken(context, out var token))
                {
                    var tokenProvider = new TokenProvider(options);

                    if (tokenProvider.TryReadToken(token, options.ReadTokenFunc, out var agentCode, out var user, out errors))
                    {
                        if (options.Validates.ValidateHost && user.Host != context.Request.Host.Host)
                            throw new JFAuthorizationException("未授权的客户端。");

                        // 用户登录的用户信息写入上下文
                        AttachToHttpContext(user);

                        // 令牌续约
                        TryRenewalContract(agentCode, tokenProvider.GetNoTagToken(token));
                    }
                }
            }
            catch (JFException ex)
            {
                errors.Add(ex.Message);
            }
            catch
            {
                errors.Add("授权验证异常。");
            }

            if (errors != null && errors.Count > 0)
            {
                // 将错误信息记录在HttpContext
                context.Items.Add(Settings.HTTPCONTEXT_ITEMNAME_WITH_TOKENERRORS, errors);
            }
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

        /// <summary>
        /// 用户登录的用户信息写入上下文
        /// </summary>
        /// <param name="user"></param>
        private void AttachToHttpContext(TicketUser user)
        {
            if (user == null) return;

            // 用户登录的用户信息写入上下文
            context.Items.Add(Settings.HTTPCONTEXT_ITEMNAME_WITH_TICKETUSER, user);

            Claim[] claims = user;

            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        }

        /// <summary>
        /// 续约令牌。
        /// 如果为滑动过期策略，则续约；
        /// 否则直接返回True，不做续约处理。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool TryRenewalContract(string key, string token)
        {
            bool success = true;

            try
            {
                // 如果为滑动过期，则更新令牌过期时间
                if (options.ExpireMode == TokenExpireMode.SlidingTime)
                {
                    // 更新令牌信息
                    if (options.UpdateTokenFunc != null)
                    {
                        var expireMinutes = options.ExpireMinutes > 0 ? options.ExpireMinutes : Settings.DEFAULT_EXPIRE_MINUTES;

                        var jfToken = new JFToken
                        {
                            Token = token,
                            ExpireTicks = DateTime.Now.AddMinutes(expireMinutes).Ticks
                        };

                        success = options.UpdateTokenFunc.Invoke(key, jfToken);
                    }
                }
            }
            catch
            {
                success = false;
            }

            return success;
        }
    }
}
