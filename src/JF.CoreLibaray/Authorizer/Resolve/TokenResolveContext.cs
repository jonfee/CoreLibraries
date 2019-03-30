using System;
using System.Collections.Generic;

namespace JF.Authorizer.Resolve
{
    /// <summary>
    /// 令牌解析验证上下文
    /// </summary>
    public sealed class TokenResolveContext : IDisposable
    {
        #region private variables

        /// <summary>
        /// 解析过程中的错误信息。
        /// </summary>
        private List<string> errors;

        /// <summary>
        /// 从持久化方案中读取<see cref="JFToken"/>信息的委托方法。
        /// </summary>
        private Func<string, JFToken> readTokenFunc;

        #endregion

        #region contructors

        /// <summary>
        /// 实例化一个<see cref="TokenResolveContext"/>对象。
        /// </summary>
        /// <param name="tokenStr">令牌加密串</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="requestCheckIP">当前需要验证的IP</param>
        /// <param name="readTokenFunc">从持久化方案中读取<see cref="JFToken"/>信息的委托方法。</param>
        public TokenResolveContext(string token, string publicKey = null, string requestCheckIP = null, Func<string, JFToken> readTokenFunc = null)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
            this.Token = token;
            this.PublicKey = publicKey ?? Settings.DEFAULT_PUBLIC_KEY;
            this.CurrentCheckIP = requestCheckIP;
            this.readTokenFunc = readTokenFunc;
            this.errors = new List<string>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// 公钥
        /// </summary>
        internal string PublicKey { get; }

        /// <summary>
        /// 令牌加密串
        /// </summary>
        internal string Token { get; }

        /// <summary>
        /// 当前需要验证的IP，为NULL时表示不需要验证。
        /// </summary>
        internal string CurrentCheckIP { get; }

        /// <summary>
        /// 解析出的令牌代理。
        /// </summary>
        internal TokenAgent Agent { get; set; }

        /// <summary>
        /// 授权用户信息
        /// </summary>
        public AuthUser User { get; internal set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public List<string> Errors => this.errors;

        /// <summary>
        /// 是否解析并验证成功。
        /// </summary>
        public bool IsValid => User != null && (!HasError);

        /// <summary>
        /// 是否存在错误
        /// </summary>
        public bool HasError => this.errors != null && this.errors.Count > 0;

        #endregion

        #region Behavious

        /// <summary>
        /// 添加错误信息。
        /// </summary>
        /// <param name="error"></param>
        internal void AddErrors(string error)
        {
            this.errors.Add(error);
        }

        /// <summary>
        /// 解析出授权用户
        /// </summary>
        /// <returns></returns>
        public bool TryResolve(out AuthUser user)
        {
            this.errors.Clear();
            user = null;

            try
            {
                TokenValidator agentValidator = new AgentValidator();
                TokenValidator expireValidator = new ExpireValidator(this.readTokenFunc);
                TokenValidator ipValidator = new IPValidator();
                TokenValidator userValidator = new UserValidator();

                agentValidator.SetSuccessor(expireValidator);
                expireValidator.SetSuccessor(ipValidator);
                ipValidator.SetSuccessor(userValidator);

                if (agentValidator.TryResolve(this))
                {
                    user = this.User;
                }
            }
            catch
            {
                user = null;
            }

            return this.IsValid;
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        public void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    this.Agent = null;
                    this.User = null;
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~TokenCheckContext() {
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
