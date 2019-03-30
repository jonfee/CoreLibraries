using JF.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace JF.WebAPIExtensions.Responses
{
    /// <summary>
    /// API结果模型。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JFApiResponse<T>
    {
        /// <summary>
        /// Code，
        /// 0-100为组件保护值，具体值对应的描述，请参考<see cref="ApiResultCode"/>枚举。
        /// 自定义Code值，请不要在此范围内。
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Data
        /// </summary>
        public T Data { get; set; }

        public static JFApiResponse<T> FromSuccess(T data)
        {
            return From(ApiResultCode.Success, string.Empty, data);
        }

        public static JFApiResponse<T> FromFailure(string message, T errorDetails)
        {
            return From(ApiResultCode.Failture, message, errorDetails);
        }

        public static JFApiResponse<T> From(ApiResultCode code, string message, T data)
        {
            return From((int)code, message, data);
        }

        public static JFApiResponse<T> From(int code, string message, T data)
        {
            return new JFApiResponse<T>
            {
                Code = code,
                Message = message,
                Data = data
            };
        }

        public static implicit operator OkObjectResult(JFApiResponse<T> response)
        {
            if (response == null) return null;

            var data = response.ToJson();

            return new OkObjectResult(data);
        }

        public static implicit operator String(JFApiResponse<T> response)
        {
            if (response == null) return null;

            return response.ToJson();
        }

        /// <summary>
        /// 输出为Json
        /// </summary>
        /// <param name="handler"></param>
        public void ResponseToJson(JsonSettingsHandler handler = null)
        {
            var data = ToJson(handler);

            Response(ResponseContentType.JSON, data);
        }

        public void ResponseToXML()
        {
            string xmlString = ToXml();

            Response(ResponseContentType.XML, xmlString);
        }

        public string ToJson(JsonSettingsHandler handler = null)
        {
            var settings = handler?.SerializerSettings;

            return JsonConvert.SerializeObject(new { code = Code, message = Message, data = Data }, Formatting.Indented, settings);
        }

        public string ToXml()
        {
            string xmlString = string.Empty;

            XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());
            using (var ms = new MemoryStream())
            {
                xmlSerializer.Serialize(ms, this);
                xmlString = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }

            return xmlString;
        }

        private void Response(ResponseContentType contentType, string data)
        {
            var response = JFHttpContext.Current.Response;

            response.Clear();
            response.ContentType = getContentType(contentType);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.WriteAsync(data).Wait();
        }

        private string getContentType(ResponseContentType type)
        {
            string contentType = string.Empty;

            switch (type)
            {
                case ResponseContentType.XML:
                    contentType = "application/xml;charset=utf-8;";
                    break;
                case ResponseContentType.JSON:
                default:
                    contentType = "application/json;charset=utf-8;";
                    break;
            }

            return contentType;
        }
    }
}
