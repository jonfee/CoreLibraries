using JF.ComponentModel;
using JF.WebAPIExtensions.Responses;
using System.Collections;

namespace JF.WebAPIExtensions.Requests
{
    /// <summary>
    /// API请求数据模型接口
    /// </summary>
    public abstract class JFApiRequest : ValidationModel
    {
        /// <summary>
        /// 直接输出错误信息，如果验证失败的时候。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual bool ResponseWriteIfValidateFailture(ResponseContentType type = ResponseContentType.JSON)
        {
            bool success = base.IsValid(out Hashtable errors);

            if (!success)
            {
                var errorResult = JFApiResponse<Hashtable>.From(ApiResultCode.ApiParamterError, "", errors);

                if (type == ResponseContentType.JSON)
                {
                    errorResult.ResponseToJson();
                }
                else if (type == ResponseContentType.XML)
                {
                    errorResult.ResponseToXML();
                }
            }

            return success;
        }
    }
}
