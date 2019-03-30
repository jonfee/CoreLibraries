namespace JF.Authorizer.Produce
{
    /// <summary>
    /// 令牌加工处理程序抽象基类。
    /// </summary>
    internal abstract class ProduceHandler
    {
        protected ProduceHandler Successor;

        public ProduceHandler() { }

        public void SetSuccessor(ProduceHandler successor)
        {
            this.Successor = successor;
        }

        /// <summary>
        /// 加工令牌
        /// </summary>
        /// <param name="context"></param>
        public virtual void Processing(TokenProduceContext context)
        {
            this.Successor?.Processing(context);
        }
    }
}
