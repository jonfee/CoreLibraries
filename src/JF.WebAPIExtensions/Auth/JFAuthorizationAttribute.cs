using JF.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace JF.WebAPIExtensions.Auth
{
    /// <summary>
    /// 自定义授权（登录校验）属性特性。
    /// 使用此特性前，请确认是否已在HTTP请求管理中注入<see cref="AuthorizationMiddlewareExtensions.UseAuthorizationHandler"/>服务。
    /// </summary>
    public class JFAuthorizationAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 是否允许匿名访问。
        /// 如果为False(非匿名)时，会再次检测是否存在<see cref="AllowAnonymousAttribute"/>特性，
        /// 若存在<see cref="AllowAnonymousAttribute"/>特性时，同样视为允许匿名访问。
        /// </summary>
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// 是否已登录
        /// </summary>
        protected bool IsLogged { get; private set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!IsAnonymous)
            {
                IsAnonymous = IsAnonymousCheck(context);
            }

            IsLogged = UserContext.Current.IsLogged;

            if (!IsAnonymous && !IsLogged) throw new JFAuthorizationException("请求无效，未经授权的请求或授权信息已失效。");
        }

        /// <summary>
        /// 是否无需登录授权检测
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool IsAnonymousCheck(ActionExecutingContext context)
        {
            bool isAnonymous = false;

            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                isAnonymous = controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true)
                    .Any(attr => attr.GetType().Equals(typeof(AllowAnonymousAttribute)));
            }

            return isAnonymous;
        }
    }
}
