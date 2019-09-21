using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace JF.Http
{
    public class HttpRequest
    {
        /// <summary>
        /// 使用post返回异步请求直接返回对象
        /// </summary>
        /// <typeparam name="TResult">返回对象类型</typeparam>
        /// <typeparam name="TData">请求对象类型</typeparam>
        /// <param name="url">请求链接</param>
        /// <param name="obj">请求对象数据</param>
        /// <param name="header"></param>
        /// <returns>请求返回的目标对象</returns>
        public static async Task<TResult> PostAsync<TResult, TData>(string url, TData obj, Dictionary<string, string> header = null)
            where TResult : class
        {
            String json = JsonConvert.SerializeObject(obj);
            return await PostJsonAsync<TResult>(url, json, header);
        }

        /// <summary>
        /// 使用post方法异步请求
        /// </summary>
        /// <param name="url">目标链接</param>
        /// <param name="data">发送的参数字符串</param>
        /// <returns>返回的字符串</returns>
        public static async Task<string> PostStringAsync(string url, string data = null, Dictionary<string, string> header = null)
        {
            using (var client = new HttpClient(new HttpClientHandler() { UseProxy = false }))
            {
                if (header != null)
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    foreach (var item in header)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                HttpContent content = data != null ? new StringContent(data) : null;

                HttpResponseMessage response = await client.PostAsync(url, content);

                return await resolveResponseResultAsync<string>(response);
            }
        }

        /// <summary>
        /// 使用post方法异步请求
        /// </summary>
        /// <param name="url">目标链接</param>
        /// <param name="json">发送的参数字符串，只能用json</param>
        /// <param name="header"></param>
        /// <returns>返回指定的数据类型</returns>
        public static async Task<TResult> PostJsonAsync<TResult>(string url, string json, Dictionary<string, string> header = null)
        {
            using (var client = new HttpClient(new HttpClientHandler() { UseProxy = false }))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (header != null)
                {
                    foreach (var item in header)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                HttpContent content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);

                return await resolveResponseResultAsync<TResult>(response);
            }
        }

        /// <summary>
        /// 使用get方法异步请求
        /// </summary>
        /// <param name="url">目标链接</param>
        /// <returns>返回的字符串</returns>
        public static async Task<string> GetAsync(string url, Dictionary<string, string> header = null)
        {
            using (var client = new HttpClient(new HttpClientHandler() { UseProxy = false }))
            {
                if (header != null)
                {
                    client.DefaultRequestHeaders.Clear();
                    foreach (var item in header)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                HttpResponseMessage response = await client.GetAsync(url);

                return await resolveResponseResultAsync<string>(response);
            }
        }

        /// <summary>
        /// 使用Get返回异步请求直接返回对象
        /// </summary>
        /// <typeparam name="T">请求对象类型</typeparam>
        /// <param name="url">请求链接</param>
        /// <returns>返回请求的对象</returns>
        public static async Task<T> GetObjectAsync<T>(string url)
        {
            string responseBody = await GetAsync(url); //请求当前账户的信息
            return JsonConvert.DeserializeObject<T>(responseBody);//把收到的字符串序列化
        }

        /// <summary>
        /// 模拟表单请求
        /// </summary>
        /// <typeparam name="TForm"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="url"></param>
        /// <param name="formData"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<TResult> PostByFormAsync<TForm, TResult>(string url, TForm formData, Dictionary<string, string> headers = null)
            where TForm : class, new()
        {
            var response = await PostByFormAsync(url, formData, headers);

            return await resolveResponseResultAsync<TResult>(response);
        }

        /// <summary>
        /// 模拟表单请求
        /// </summary>
        /// <typeparam name="TForm">表单对象</typeparam>
        /// <param name="url"></param>
        /// <param name="formData"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PostByFormAsync<TForm>(string url, TForm formData, Dictionary<string, string> headers = null)
            where TForm : class, new()
        {
            var modelType = formData.GetType();
            using (var client = new HttpClient(new HttpClientHandler() { UseProxy = false }))
            {
                #region HttpClient处理

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                #endregion

                #region  构建 MultipartFormDataContent

                var formDataContent = new MultipartFormDataContent();
                tryBuildMultipartFormDataContent(formData, ref formDataContent);

                #endregion

                return await client.PostAsync(url, formDataContent);
            }
        }

        /// <summary>
        /// 构建MultipartFormDataContent
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="formData"></param>
        /// <param name="formDataContent"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        private static bool tryBuildMultipartFormDataContent<TData>(TData formData, ref MultipartFormDataContent formDataContent, string paramName = null)
            where TData : class
        {
            if (formData == null) return false;

            try
            {
                formDataContent = formDataContent ?? new MultipartFormDataContent();
                var dic = new Dictionary<string, string>();
                var properties = formData.GetType().GetProperties();

                foreach (var pro in properties)
                {
                    var proName = string.IsNullOrEmpty(paramName) ? pro.Name : $"{paramName}.{pro.Name}";
                    var proValue = pro.GetValue(formData);
                    if (proValue == null) continue;

                    HttpContent content = null;
                    #region //文件的处理（这里需要特别注意，流只能读取一次，因为读取之后会把Stream.Positon(当前流中的位置)置为最后读取的位置，除非置为0，第二次才可读到）
                    if (pro.PropertyType == typeof(IFormFile))
                    {
                        #region 字节方式请求
                        var model = (IFormFile)proValue;
                        MemoryStream fileTarget = new MemoryStream();
                        model.OpenReadStream().CopyTo(fileTarget);
                        content = new ByteArrayContent(fileTarget.ToArray());
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse(model.ContentType);
                        content.Headers.ContentLength = model.Length;
                        formDataContent.Add(content, proName, model.FileName);

                        #endregion
                    }
                    #endregion
                    #region //文件集合的处理
                    else if (pro.PropertyType.GetInterfaces().Any(t => t == typeof(IEnumerable<IFormFile>)))
                    {
                        foreach (var child in ((IEnumerable<IFormFile>)pro.GetValue(formData)))
                        {
                            #region 字节方式请求
                            MemoryStream fileTarget = new MemoryStream();
                            child.OpenReadStream().CopyTo(fileTarget);
                            content = new ByteArrayContent(fileTarget.ToArray());
                            content.Headers.ContentType = MediaTypeHeaderValue.Parse(child.ContentType);
                            content.Headers.ContentLength = child.Length;
                            formDataContent.Add(content, proName, child.FileName);
                            #endregion
                        }
                    }
                    #endregion
                    #region //如果执意响应方是接收字节类型,那传输时不能用ByteArrayContent去填充,否则接收方认为这是一个非法数据,故要传base64格式,接收方会自动把base64转成字节接收
                    else if (pro.PropertyType == typeof(byte[]))
                    {
                        content = new StringContent(Convert.ToBase64String((byte[])pro.GetValue(formData)));
                        formDataContent.Add(content, proName);
                    }
                    #endregion
                    #region // 如果为可枚举集合
                    else if (pro.PropertyType != typeof(String) && pro.PropertyType.GetInterfaces().Any(t => t == typeof(System.Collections.IEnumerable)))
                    {
                        var list = pro.GetValue(formData) as IEnumerable<object>;
                        var index = 0;

                        foreach (var item in list)
                        {
                            tryBuildMultipartFormDataContent(item, ref formDataContent, $"{proName}[{index++}]");
                        }
                    }
                    #endregion
                    #region // 非原生类型
                    else if (pro.PropertyType != typeof(String) && !pro.PropertyType.IsPrimitive)
                    {
                        tryBuildMultipartFormDataContent(pro.GetValue(formData), ref formDataContent, proName);
                    }
                    #endregion
                    #region //其他类型统一按字符串处理
                    else
                    {
                        content = new StringContent(((string)pro.GetValue(formData).ToString()));
                        formDataContent.Add(content, proName);
                    }
                    #endregion
                }

            }
            catch
            {
                // nothing
            }

            return formDataContent != default(MultipartFormDataContent);
        }

        /// <summary>
        /// 将<see cref="HttpResponseMessage"/>信息解析为指定的类型。
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        private static async Task<TResult> resolveResponseResultAsync<TResult>(HttpResponseMessage responseMessage)
        {
            if (responseMessage == null) return default(TResult);

            TResult result = default(TResult);

            responseMessage.EnsureSuccessStatusCode();
            if (responseMessage.IsSuccessStatusCode)
            {
                string responseBody = await responseMessage.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<TResult>(responseBody);
            }

            return result;
        }

        /// <summary> 模拟表单请求/liuyl/2017/09/1 /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="value"></param>
        /// 
        /// <returns></returns>
        //public static string SendToWebByHttpClient<T>(string url, T value)
        //{
        //    var modelType = typeof(T);
        //    using (var client = new HttpClient())
        //    using (var formData = new MultipartFormDataContent())
        //    {
        //        //遍历SendData的所有成员
        //        foreach (var item in modelType.GetProperties())
        //        {
        //            HttpContent content;
        //            //文件的处理（这里需要特别注意，流只能读取一次，因为读取之后会把Stream.Positon(当前流中的位置)置为最后读取的位置，除非置为0，第二次才可读到）
        //            if (item.PropertyType == typeof(HttpPostedFileBase) && item.GetValue(value) != null)
        //            {
        //                #region Stream请求
        //                //Stream塞进Content会会导致读取这个流，所以之后不能再第二次利用
        //                var model = (HttpPostedFileBase)item.GetValue(value);
        //                content = new StreamContent(model.InputStream, model.ContentLength);
        //                content.Headers.ContentType = MediaTypeHeaderValue.Parse(model.ContentType);//ContentType必须赋值,否则文件接收不到此属性
        //                content.Headers.ContentLength = model.ContentLength;//ContentLength可不显式的赋值,会自动读取给StreamContent的内容长度
        //                formData.Add(content, item.Name, model.FileName);//文件类型,第三个参数必须要赋值,否则不认为这是一个文件

        //                #endregion

        //                #region 字节方式请求
        //                //var model = (HttpPostedFileBase)item.GetValue(value);
        //                //MemoryStream fileTarget = new MemoryStream();
        //                //model.InputStream.CopyTo(fileTarget);
        //                //content = new ByteArrayContent(fileTarget.ToArray());
        //                //content.Headers.ContentType = MediaTypeHeaderValue.Parse(model.ContentType);
        //                //content.Headers.ContentLength = model.ContentLength;
        //                //formData.Add(content, item.Name, model.FileName);

        //                #endregion
        //            }
        //            //文件的处理
        //            else if (item.PropertyType == typeof(HttpPostedFileWrapper) && item.GetValue(value) != null)
        //            {
        //                #region Stream请求
        //                var model = (HttpPostedFileWrapper)item.GetValue(value);
        //                content = new StreamContent(model.InputStream, model.ContentLength);
        //                content.Headers.ContentType = MediaTypeHeaderValue.Parse(model.ContentType);
        //                content.Headers.ContentLength = model.ContentLength;
        //                formData.Add(content, item.Name, model.FileName);

        //                #endregion
        //                #region 字节方式请求
        //                //var model = (HttpPostedFileWrapper)item.GetValue(value);
        //                //MemoryStream fileTarget = new MemoryStream();
        //                //model.InputStream.CopyTo(fileTarget);
        //                //content = new ByteArrayContent(fileTarget.ToArray());
        //                //content.Headers.ContentType = MediaTypeHeaderValue.Parse(model.ContentType);
        //                //content.Headers.ContentLength = model.ContentLength;
        //                //formData.Add(content, item.Name, model.FileName);

        //                #endregion

        //            }
        //            //文件集合的处理
        //            else if (item.PropertyType == typeof(List<HttpPostedFileBase>) && item.GetValue(value) != null)
        //            {
        //                foreach (var child in ((List<HttpPostedFileBase>)item.GetValue(value)))
        //                {
        //                    #region Stream请求
        //                    content = new StreamContent(child.InputStream, child.ContentLength);
        //                    content.Headers.ContentType = MediaTypeHeaderValue.Parse(child.ContentType);
        //                    content.Headers.ContentLength = child.ContentLength;
        //                    formData.Add(content, item.Name, child.FileName);
        //                    #endregion

        //                    #region 字节方式请求
        //                    //MemoryStream fileTarget = new MemoryStream();
        //                    //child.InputStream.CopyTo(fileTarget);
        //                    //content = new ByteArrayContent(fileTarget.ToArray());
        //                    //content.Headers.ContentType = MediaTypeHeaderValue.Parse(child.ContentType);
        //                    //content.Headers.ContentLength = child.ContentLength;
        //                    //formData.Add(content, item.Name, child.FileName);

        //                    #endregion
        //                }
        //            }
        //            //如果执意响应方是接收字节类型,那传输时不能用ByteArrayContent去填充,否则接收方认为这是一个非法数据,故要传base64格式,接收方会自动把base64转成字节接收
        //            else if (item.PropertyType == typeof(byte[]) && item.GetValue(value) != null)
        //            {
        //                content = new StringContent(Convert.ToBase64String((byte[])item.GetValue(value)));
        //                formData.Add(content, item.Name);
        //            }
        //            //其他类型统一按字符串处理(DateTime,Enum;long ;bool;int...)
        //            else if (item.GetValue(value) != null && (item.PropertyType != typeof(byte[]) || item.PropertyType != typeof(HttpPostedFileBase)))
        //            {
        //                content = new StringContent(((string)item.GetValue(value).ToString()));
        //                formData.Add(content, item.Name);
        //            }
        //        }

        //        var response = client.PostAsync(url, formData).Result;

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            //以下根据自己业务处理返回值
        //            var obj = JsonHandler.DeserializeObject<BaseViewModel>(response.ToString());
        //            if (obj != null)
        //            {
        //                var result = obj.ErrResult;
        //                if (result.ErrorCode != ErrorCode.OK)
        //                {
        //                    foreach (var message in result.Messages)
        //                    {
        //                        _Error += message;
        //                    }
        //                    return result.ErrorCode;
        //                }
        //            }
        //        }
        //        return string.Empty;
        //    }
        //}
    }
}
