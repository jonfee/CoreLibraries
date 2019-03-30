namespace JF.Authorizer.Resolve
{
    /// <summary>
    /// 解析器
    /// </summary>
    internal abstract class TokenValidator
    {
        protected TokenValidator Successor;

        public void SetSuccessor(TokenValidator successor)
        {
            this.Successor = successor;
        }

        /// <summary>
        /// 开始解析
        /// </summary>
        /// <param name="context"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public abstract bool TryResolve(TokenResolveContext context);
    }
}
