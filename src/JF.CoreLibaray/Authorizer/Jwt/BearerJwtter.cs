using JF.Common;
using JF.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JF.Authorizer
{
    /// <summary>
    /// 基于Bearer的JWT处理器
    /// </summary>
    internal sealed class BearerJwtter : IJwtter
    {
        /// <summary>
        /// JWT令牌头部标识
        /// </summary>
        public string JWT_TAG { get; } = "Bearer ";

        /// <summary>
        /// 读取Token信息，仅支持从接口调用。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="option"></param>
        /// <param name="readTokenFunc"></param>
        /// <param name="agentCode"></param>
        /// <param name="user"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        bool IJwtter.TryReadToken(string token, JwtAuthorizerOption option, Func<string, JFToken> readTokenFunc, out string agentCode, out TicketUser user, out List<string> errors)
        {
            return TryReadToken(token, option, readTokenFunc, out agentCode, out user, out errors);
        }

        /// <summary>
        /// 输出Token信息，仅支持从接口调用。
        /// </summary>
        /// <param name="user"></param>
        /// <param name="option"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        string IJwtter.WriteToken(TicketUser user, JwtAuthorizerOption option, out JFAgentToken token)
        {
            return WriteToken(user, option, out token);
        }

        /// <summary>
        /// 读取出Token信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="option"></param>
        /// <param name="readTokenFunc"></param>
        /// <param name="agentCode"></param>
        /// <param name="user"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public bool TryReadToken(string token, BearerOption option, Func<string, JFToken> readTokenFunc, out string agentCode, out TicketUser user, out List<string> errors)
        {
            agentCode = null;
            user = null;
            errors = null;

            if (token.StartsWith(JWT_TAG))
            {
                token = token.TrimStart(JWT_TAG);
            }

            try
            {
                if (TryResolveClaimsPrincipal(token, option, out var claimsPrincipal))
                {
                    user = claimsPrincipal;

                    JFAgentToken requestToken = new JFToken
                    {
                        Token = token,
                        ExpireTicks = user.TicketTime.AddMinutes(option.ExpireMinutes).Ticks
                    };

                    #region 验证
                    if (readTokenFunc != null)
                    {
                        var jfToken = readTokenFunc.Invoke(requestToken.AgentCode);

                        if (jfToken == null) throw new JFValidateException($"令牌无效。");

                        if (!jfToken.Token.Equals(requestToken.Token)) throw new JFValidateException("令牌已失效。");

                        if (jfToken.ExpireTicks < DateTime.Now.Ticks) throw new JFValidateException("令牌已失效。");
                    }
                    else
                    {
                        if (requestToken.ExpireTicks < DateTime.Now.Ticks) throw new JFException();
                    }
                    #endregion

                    agentCode = requestToken.AgentCode;
                }
            }
            catch (Exception ex)
            {
                errors = new List<string> { ex.Message };
                user = null;
            }

            return user != null;
        }

        /// <summary>
        /// 输出Token信息
        /// </summary>
        /// <param name="user"></param>
        /// <param name="option"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public string WriteToken(TicketUser user, BearerOption option, out JFAgentToken token)
        {
            token = null;

            user.TicketTime = DateTime.Now;

            //对称秘钥
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(option.SecretKey));

            //签名证书(秘钥，加密算法)
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expire = DateTime.Now.AddMinutes(option.ExpireMinutes);

            Claim[] claims = user;

            //生成token  [注意]需要nuget添加Microsoft.AspNetCore.Authentication.JwtBearer包，并引用System.IdentityModel.Tokens.Jwt命名空间
            var securityToken = new JwtSecurityToken(option.Issuer, option.Audience, claims, DateTime.Now, expire, creds);

            token = new JFToken
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                ExpireTicks = expire.Ticks
            };
            
            return token.Token;
        }

        /// <summary>
        /// 从token中解析出<see cref="ClaimsPrincipal"/>信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="option"></param>
        /// <param name="claims"></param>
        /// <returns></returns>
        private bool TryResolveClaimsPrincipal(string token, JwtAuthorizerOption option, out ClaimsPrincipal claims)
        {
            var handler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(option.SecretKey);

            var tokenSecure = handler.ReadToken(token) as SecurityToken;

            var validations = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidAudience = option.Audience,
                ValidIssuer = option.Issuer,
                ValidateAudience = option.Validates.ValidateAudience,
                ValidateIssuer = option.Validates.ValidateIssuer
            };

            claims = handler.ValidateToken(token, validations, out tokenSecure);

            return claims != null;
        }
    }
}
