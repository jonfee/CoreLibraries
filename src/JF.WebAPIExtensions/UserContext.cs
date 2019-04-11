using JF.Authorizer;
using JF.WebAPIExtensions.Auth;
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

                if (TryGetTicketUser(out var ticketUser))
                {
                    userContext.UserID = ticketUser.ID;
                    userContext.UserName = ticketUser.Name;
                    userContext.UserData = ticketUser.UserData;
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

        /// <summary>
        /// 用户附件信息
        /// </summary>
        public object UserData { get; set; }

        #endregion

        #region behavious

        /// <summary>
        /// 获取当前登录授权的用户信息。
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static bool TryGetTicketUser(out TicketUser user)
        {
            user = null;

            try
            {
                if (JFHttpContext.Current.Items.ContainsKey(Settings.HTTPCONTEXT_ITEMNAME_WITH_TICKETUSER))
                {
                    user = JFHttpContext.Current.Items[Settings.HTTPCONTEXT_ITEMNAME_WITH_TICKETUSER] as TicketUser;
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
