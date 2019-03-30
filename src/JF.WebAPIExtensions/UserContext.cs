using JF.Authorizer;
using System.Web;

namespace JF.WebAPIExtensions
{
    /// <summary>
    /// 用户上下文
    /// </summary>
    public class UserContext
    {
        #region contructors

        private UserContext() { }

        /// <summary>
        /// 当前请求的用户上下文对象实例。
        /// </summary>
        public static UserContext Current
        {
            get
            {
                var userContext = new UserContext();

                if (TryGetAuthUser(out var authUser))
                {
                    userContext.UserID = authUser.UserID;
                    userContext.UserName = authUser.Name;
                }

                return userContext;
            }
        }

        #endregion

        #region properties

        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool IsLogged => !string.IsNullOrEmpty(UserID);

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }

        #endregion

        #region behavious

        /// <summary>
        /// 获取当前登录授权的用户信息。
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static bool TryGetAuthUser(out AuthUser user)
        {
            user = null;

            try
            {
                if (JFHttpContext.Current.Items.ContainsKey(Settings.HTTPCONTEXT_ITEMNAME_WITH_AUTHUSER))
                {
                    user = JFHttpContext.Current.Items[Settings.HTTPCONTEXT_ITEMNAME_WITH_AUTHUSER] as AuthUser;
                }
            }
            catch
            {
                user = null;
            }

            return user != null;
        }

        #endregion
    }
}
