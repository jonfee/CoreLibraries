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
    public class ApiResult<T>
    {
        /// <summary>
        /// Code
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
        
        public static ApiResult<T> From(int code, string message, T data)
        {
            return new ApiResult<T>
            {
                Code = code,
                Message = message,
                Data = data
            };
        }

        public static implicit operator OkObjectResult(ApiResult<T> response)
        {
            if (response == null) return null;

            var data = response.ToJson();

            return new OkObjectResult(data);
        }

        public static implicit operator String(ApiResult<T> response)
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
