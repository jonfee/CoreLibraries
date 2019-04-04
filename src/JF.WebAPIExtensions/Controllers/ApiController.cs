using JF.WebAPIExtensions.Responses;
using Microsoft.AspNetCore.Mvc;

namespace JF.WebAPIExtensions.Controllers
{
    /// <summary>
    /// WebApi接口基础类。
    /// </summary>
    public class ApiController : ControllerBase
    {
        /// <summary>
        /// JSON输出操作成功的API结果，HttpStatusCode = 200，无消息。
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public new IActionResult Ok()
        {
            return Success(true);
        }

        /// <summary>
        /// JSON输出操作成功的API结果，HttpStatusCode = 200，有消息。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">成功消息。</param>
        /// <returns></returns>
        [NonAction]
        public IActionResult Success<T>(T message)
        {
            var response = JFApiResponse<T>.FromSuccess(message);

            return JsonOk(response);
        }
        
        /// <summary>
        /// JSON输出操作失败的API结果，HttpStatusCode = 200。
        /// </summary>
        /// <param name="message">错误消息。</param>
        /// <param name="errorDetails">错误详细。</param>
        /// <returns></returns>
        public IActionResult Failure(string message, string errorDetails)
        {
            var response = JFApiResponse<string>.FromFailure(message, errorDetails);

            return JsonOk(response);
        }

        /// <summary>
        /// JSON输出，HttpStatusCode = 200。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult JsonOk<T>(JFApiResponse<T> response)
        {
            HttpContext.Response.ContentType = "application/json;charset=utf-8;";
            return Content(response.ToJson());
        }

        /// <summary>
        /// XML输出，HttpStatusCode = 200。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult XmlOk<T>(JFApiResponse<T> response)
        {
            HttpContext.Response.ContentType = "application/xml;charset=utf-8;";
            return Content(response.ToXml());
        }
    }
}
