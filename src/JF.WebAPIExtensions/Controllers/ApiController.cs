using JF.Json;
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
        public new IActionResult Ok(JsonSettingsHandler jsonSettingsHandler = null)
        {
            return Success(true, jsonSettingsHandler);
        }

        /// <summary>
        /// JSON输出，HttpStatusCode = 200。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <param name="jsonSettingsHandler"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult Ok<T>(JFApiResponse<T> response, JsonSettingsHandler jsonSettingsHandler = null)
        {
            return JsonOk(response, jsonSettingsHandler);
        }

        /// <summary>
        /// 布尔结果返回
        /// </summary>
        /// <param name="success"></param>
        /// <param name="jsonSettingsHandler"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult BoolResult(bool success, JsonSettingsHandler jsonSettingsHandler = null)
        {
            if (success)
            {
                return Success("Success", jsonSettingsHandler);
            }
            else
            {
                return Failure("Failed", jsonSettingsHandler: jsonSettingsHandler);
            }
        }

        /// <summary>
        /// JSON输出操作成功的API结果，HttpStatusCode = 200，有消息。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">成功消息。</param>
        /// <param name="jsonSettingsHandler"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult Success<T>(T message, JsonSettingsHandler jsonSettingsHandler = null)
        {
            var response = JFApiResponse<T>.FromSuccess(message);

            return JsonOk(response, jsonSettingsHandler);
        }

        /// <summary>
        /// JSON输出操作失败的API结果，HttpStatusCode = 200。
        /// </summary>
        /// <param name="message">错误消息。</param>
        /// <param name="errorDetails">错误详细。</param>
        /// <param name="jsonSettingsHandler"></param>
        /// <returns></returns>
        public IActionResult Failure(string message, string errorDetails = null, JsonSettingsHandler jsonSettingsHandler = null)
        {
            var response = JFApiResponse<string>.FromFailure(message, errorDetails ?? message);

            return JsonOk(response, jsonSettingsHandler);
        }

        /// <summary>
        /// JSON输出，HttpStatusCode = 200。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <param name="jsonSettingsHandler"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult JsonOk<T>(JFApiResponse<T> response, JsonSettingsHandler jsonSettingsHandler = null)
        {
            HttpContext.Response.ContentType = "application/json;charset=utf-8;";
            return Content(response.ToJson(jsonSettingsHandler));
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
