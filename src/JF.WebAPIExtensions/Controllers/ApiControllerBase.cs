using JF.Json;
using JF.WebAPIExtensions.Responses;
using Microsoft.AspNetCore.Mvc;

namespace JF.WebAPIExtensions.Controllers
{
    /// <summary>
    /// WebApi接口基础类。
    /// </summary>
    public class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// JSON输出操作成功的API结果，HttpStatusCode = 200，无消息。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <param name="jsonSettingsHandler"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult Ok<T>(int code, string message, T data, JsonSettingsHandler jsonSettingsHandler = null)
        {
            var result = new ApiResult<T>
            {
                Code = code,
                Message = message,
                Data = data
            };

            return Ok(result, jsonSettingsHandler);
        }

        /// <summary>
        /// JSON输出，HttpStatusCode = 200。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <param name="jsonSettingsHandler"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult Ok<T>(ApiResult<T> response, JsonSettingsHandler jsonSettingsHandler = null)
        {
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
        public IActionResult JsonOk<T>(ApiResult<T> response, JsonSettingsHandler jsonSettingsHandler = null)
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
        public IActionResult XmlOk<T>(ApiResult<T> response)
        {
            HttpContext.Response.ContentType = "application/xml;charset=utf-8;";
            return Content(response.ToXml());
        }
    }
}
