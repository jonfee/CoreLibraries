﻿using System;

namespace JF.Authorizer.Produce
{
    /// <summary>
    /// 令牌生成上下文
    /// </summary>
    public sealed class TokenProduceContext : IDisposable
    {
        #region private variables

        /// <summary>
        /// 令牌。
        /// </summary>
        private string token;

        #endregion

        #region contructors

        /// <summary>
        /// 初始化一个<see cref="TokenProduceContext"/>上下文实例。
        /// </summary>
        /// <param name="user">需要授权生成令牌的用户</param>
        /// <param name="publicKey">设置公钥</param>
        /// <param name="expireMinutes">令牌过期时间（单位 ：分钟）</param>
        /// <param name="mode">失效模式，枚举<see cref="TokenExpireMode"/></param>
        public TokenProduceContext(AuthUser user, string publicKey = null, int? expireMinutes = null, TokenExpireMode mode = TokenExpireMode.AbsoluteTime)
        {
            this.User = user ?? throw new ArgumentNullException(nameof(user));
            this.Agent = new TokenAgent { CreatedTicks = DateTime.Now.Ticks };
            this.PublicKey = publicKey ?? Settings.DEFAULT_PUBLIC_KEY;
            this.ExpireMode = mode;
            this.Agent.ExpireTicks = DateTime.Now.AddMinutes(expireMinutes ?? Settings.DEFAULT_EXPIRE_MINUTES).Ticks;
        }

        #endregion

        #region properties

        /// <summary>
        /// 公钥
        /// </summary>
        public string PublicKey { get; }

        /// <summary>
        /// 失效模式
        /// </summary>
        public TokenExpireMode ExpireMode { get; }

        /// <summary>
        /// 授权代理
        /// </summary>
        internal TokenAgent Agent { get; private set; }

        /// <summary>
        /// 授权用户
        /// </summary>
        public AuthUser User { get; }

        #endregion

        #region Behavious

        /// <summary>
        /// 设置私钥
        /// </summary>
        /// <param name="key"></param>
        public void SetPrivateKey(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            this.Agent.Sercert = key;
        }

        /// <summary>
        /// 设置用户令牌密文
        /// </summary>
        /// <param name="ciphertext"></param>
        public void SetUserCiphertext(string ciphertext)
        {
            if (string.IsNullOrEmpty(ciphertext)) throw new ArgumentNullException(nameof(ciphertext));

            this.Agent.Ciphertext = ciphertext;
        }

        /// <summary>
        /// 设置代理用户编号
        /// </summary>
        /// <param name="code"></param>
        public void SetAgentCode(string code)
        {
            if (string.IsNullOrEmpty(code)) throw new ArgumentNullException(nameof(code));

            this.Agent.Code = code;
        }

        /// <summary>
        /// 设置Token
        /// </summary>
        /// <param name="token"></param>
        public void SetToken(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

            this.token = token;
        }

        /// <summary>
        /// 产出令牌。
        /// </summary>
        /// <returns></returns>
        public JFToken Output()
        {
            ProduceHandler agentHandler = new ProduceAgentHandler();
            ProduceHandler tokenHandler = new ProduceTokenHandler();

            agentHandler.SetSuccessor(tokenHandler);

            agentHandler.Processing(this);

            return new JFToken
            {
                Token = this.token,
                ExpireTicks = this.Agent.ExpireTicks
            };
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    this.Agent = null;
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                this.token = null;

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~TokenMakeContext() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
