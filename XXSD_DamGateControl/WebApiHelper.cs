using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace XXSD_DamGateControl
{
    /// <summary>
    /// WebApi数据接口类
    /// </summary>
    public class WebApiHelper
    {
        /// <summary>
        /// 全部WebAPI接口数据,key:子系统标识,value:url地址
        /// </summary>
        static IDictionary<string, string> WebApiDomains;
        public static string GatewayUrl = string.Empty;

        /// <summary>
        /// 获取系统的全部WebAPI接口数据
        /// </summary>
        /// <param name="SysConfigApiDomain">系统域名地址</param>
        /// <param name="SysConfigApiDomainKey">系统域标识</param>
        public WebApiHelper(string gatewayUrl)
        {
            GatewayUrl = gatewayUrl;
        }
        public static string GetGatewayUrl()
        {
            bool isSuccess;
             var result = HttpUtil.sendHttpRequest(string.Format("{0}/sysconfig/getgatewayurl", GatewayUrl),
                string.Empty, "POST", Encoding.UTF8, "application/json", out isSuccess);
            var apiResult = JsonConvert.DeserializeObject<dynamic>(result);
            WebApiDomains = new Dictionary<string, string>();
            var url = apiResult.oData.ToString();
            return url;
        }

        public static T GetObjectFromWebData<T>(string apiDomain, string address, object parametersObject, string httpMethod) where T : class
        {
            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
            //这里使用自定义日期格式，如果不使用的话，默认是ISO8601格式  
            timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            var parameters = JsonConvert.SerializeObject(parametersObject, Formatting.Indented, timeConverter);  
            string json = GetStringFromWebData(apiDomain, address, parameters, httpMethod);
            Savelog(apiDomain, address, parameters, json);
            T t = ParseObjectFromString<T>(json);
            return t;
        }
        public static T GetObjectFromWebData<T>(string apiDomain, string address, string parameters, string httpMethod) where T : class
        {
            string json = GetStringFromWebData(apiDomain, address, parameters, httpMethod);
            Savelog(apiDomain, address, parameters, json);
            T t = ParseObjectFromString<T>(json);
            return t;
        }
        private static void Savelog(string apiDomain, string address, string parameters, string json)
        {
            try
            {
                if (!Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "\\LOG\\"))
                    Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + "\\LOG\\");
                using (StreamWriter sw = File.AppendText(System.AppDomain.CurrentDomain.BaseDirectory + "\\LOG\\" + apiDomain + DateTime.Now.ToString("yyyyMMdd") + ".log"))
                {
                    if (parameters == null)
                        sw.WriteLine("申请：[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] null");
                    else
                        sw.WriteLine("申请：[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + ConvertJsonString(parameters));
                    sw.WriteLine("--------------------------------------------------------------");
                    if (json == null)
                        sw.WriteLine("返回：[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] null");
                    else
                        sw.WriteLine("返回：[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + ConvertJsonString(json));
                    sw.WriteLine("==============================================================");
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(System.AppDomain.CurrentDomain.BaseDirectory + "\\LOG\\SaveLogError" + DateTime.Now.ToString("yyyyMMdd") + ".log"))
                {
                    sw.WriteLine(ex.Message);
                }
            }
        }
        public static string GetStringFromWebData(string apiDomain, string address, string parameters, string httpMethod)
        {
            //Uri addr = new Uri(new Uri(WebApiDomains[apiDomain]), address);
            var str = string.Format("{0}{1}/{2}", GetGatewayUrl(), apiDomain, address);
            var addr = new Uri(str);
            try
            {
                if (!Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "\\LOG\\"))
                    Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + "\\LOG\\");
                using (StreamWriter sw = File.AppendText(System.AppDomain.CurrentDomain.BaseDirectory + "\\LOG\\" + apiDomain + DateTime.Now.ToString("yyyyMMdd") + ".log"))
                {
                    sw.WriteLine("地址：[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + addr.ToString() + "/" + address + "");
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(System.AppDomain.CurrentDomain.BaseDirectory + "\\LOG\\SaveLogError" + DateTime.Now.ToString("yyyyMMdd") + ".log"))
                {
                    sw.WriteLine(ex.Message);
                }
            }
            string json = string.Empty;
            bool isSuccess;

            if (httpMethod.ToUpper() == "GET")
                json = HttpUtil.sendHttpRequest(addr.AbsoluteUri, parameters, httpMethod, out isSuccess);
            else if (httpMethod.ToUpper() == "POST")
                json = HttpUtil.sendHttpRequest(addr.AbsoluteUri, parameters, httpMethod, Encoding.UTF8, "application/json", out isSuccess);
            return json;
        }
        public static T ParseObjectFromString<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            var apiResult = JsonConvert.DeserializeObject<dynamic>(json);
            if (!apiResult.flag.Value)
                throw new Exception("错误：" + apiResult.msg.Value);
            if (apiResult == null || apiResult.oData == null) return null;
            var data = JsonConvert.DeserializeObject<T>(apiResult.oData.ToString());

            if (data == null) return null;
            T t = data as T;

            return t;
        }
        public static T ParseObjectFromStringInternal<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            var data = JsonConvert.DeserializeObject<T>(json);

            if (data == null) return null;
            T t = data as T;

            return t;
        }
        private static string ConvertJsonString(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }
    }

    /// <summary>
    /// Http请求核心
    /// </summary>
    public static class HttpUtil
    {
        #region POST请求  
        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <param name="requestUrl">请求地址</param>
        /// <param name="parameterStr">请求参数字符串</param>      
        /// <param name="isSuccess">是否连接成功</param>
        /// <returns></returns>
        public static string postHttpRequest(string requestUrl, string parameterStr, out bool isSuccess)
        {
            return postHttpRequest(requestUrl, parameterStr, Encoding.UTF8, out isSuccess);
        }

        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <param name="requestUrl">请求地址</param>
        /// <param name="parameterStr">请求参数字符串</param>         
        /// <param name="dataEncoding">字符编码</param>        
        /// <param name="isSuccess">是否连接成功</param>
        /// <returns></returns>
        public static string postHttpRequest(string requestUrl, string parameterStr, Encoding dataEncoding, out bool isSuccess)
        {
            return postHttpRequest(requestUrl, parameterStr, dataEncoding, string.Empty, out isSuccess);
        }

        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <param name="requestUrl">请求地址</param>
        /// <param name="parameterStr">请求参数字符串</param>     
        /// <param name="dataEncoding">字符编码</param>
        /// <param name="contentType">请求内容格式</param>
        /// <param name="isSuccess">是否连接成功</param>
        /// <returns></returns>
        public static string postHttpRequest(string requestUrl, string parameterStr, Encoding dataEncoding,
            string contentType, out bool isSuccess)
        {
            return sendHttpRequest(requestUrl, parameterStr, "POST", dataEncoding, contentType, out isSuccess);
        }
        #endregion

        #region 发送Http请求
        /// <summary>
        /// 发送Http请求
        /// </summary>
        /// <param name="requestUrl">请求地址</param>
        /// <param name="parameterStr">请求参数字符串</param>
        /// <param name="httpMethod">请求方式</param>
        /// <param name="isSuccess">是否连接成功</param>
        /// <returns></returns>
        public static string sendHttpRequest(string requestUrl, string parameterStr, string httpMethod, out bool isSuccess)
        {
            return sendHttpRequest(requestUrl, parameterStr, httpMethod, Encoding.UTF8, out isSuccess);
        }

        /// <summary>
        /// 发送Http请求
        /// </summary>
        /// <param name="requestUrl">请求地址</param>
        /// <param name="parameterStr">请求参数字符串</param>
        /// <param name="httpMethod">请求方式</param>        
        /// <param name="dataEncoding">字符编码</param>        
        /// <param name="isSuccess">是否连接成功</param>
        /// <returns></returns>
        public static string sendHttpRequest(string requestUrl, string parameterStr, string httpMethod,
            Encoding dataEncoding, out bool isSuccess)
        {
            return sendHttpRequest(requestUrl, parameterStr, httpMethod, dataEncoding, string.Empty, out isSuccess);
        }

        /// <summary>
        /// 发送Http请求
        /// </summary>
        /// <param name="requestUrl">请求地址</param>
        /// <param name="parameterStr">请求参数字符串</param>
        /// <param name="httpMethod">请求方式</param>        
        /// <param name="dataEncoding">字符编码</param>
        /// <param name="contentType">请求内容格式</param>
        /// <param name="isSuccess">是否连接成功</param>
        /// <returns></returns>
        public static string sendHttpRequest(string requestUrl, string parameterStr, string httpMethod,
            Encoding dataEncoding, string contentType, out bool isSuccess)
        {
            try
            {
                WebClient webClient = new WebClient();
                if (!string.IsNullOrEmpty(contentType))
                {
                    webClient.Headers.Add("Content-Type", contentType);
                }
                byte[] responseData = null;
                if (CompareIgnoreCase(httpMethod, "GET"))
                {
                    string fullRequestUrl = requestUrl;
                    if (!parameterStr.isNullOrEmpty())  //存在QueryString参数
                    {
                        fullRequestUrl = string.Format("{0}?{1}", requestUrl, Uri.EscapeDataString(parameterStr));
                    }
                    responseData = webClient.DownloadData(fullRequestUrl);
                }
                else
                {
                    if (string.IsNullOrEmpty(parameterStr)) parameterStr = string.Empty;
                    byte[] parameterData = dataEncoding.GetBytes(parameterStr);
                    responseData = webClient.UploadData(requestUrl, httpMethod, parameterData);//得到返回字符流
                }
                isSuccess = true;
                return dataEncoding.GetString(responseData);
            }
            catch (WebException webEx)
            {
                Trace.TraceError(webEx.FormatLogMessage());
                if (webEx.Status == WebExceptionStatus.ProtocolError)
                {
                    isSuccess = true;
                    var httpResponse = (HttpWebResponse)webEx.Response;
                    using (var stream = httpResponse.GetResponseStream())
                    {
                        return getStringFromStream(stream, dataEncoding.WebName);
                    }
                }
                else
                {
                    isSuccess = false;
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.FormatLogMessage());
                isSuccess = false;
                return string.Empty;
            }
        }

        /// <summary>
        /// 忽略大小写判断字符串是否相等
        /// </summary>
        /// <param name="target">原始字符串</param>
        /// <param name="looktarget">对比字符串</param>
        /// <returns></returns>
        public static bool CompareIgnoreCase(string target, string looktarget)
        {
            return (0 == string.Compare(target, looktarget, true));
        }

        /// <summary>
        /// 判断空字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool isNullOrEmpty(this string str)
        {
            return (str ?? "").Length == 0;
        }

        /// <summary>
        /// 格式化异常信息
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <returns></returns>
        public static string FormatLogMessage(this Exception ex)
        {
            if (ex != null)
            {
                return string.Format("{0}{1}{2}{3}", ex.Message, Environment.NewLine, ex.StackTrace, Environment.NewLine);
            }
            return string.Empty;
        }

        /// <summary>
        /// 转换Stream为字符串
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encodeName">编码格式</param>
        /// <returns></returns>
        public static string getStringFromStream(Stream stream, string encodeName)
        {
            byte[] streamBytes = getBytesFromStream(stream);
            if (streamBytes.isNotEmpty())
            {
                return Encoding.GetEncoding(encodeName).GetString(streamBytes);
            }
            return string.Empty;
        }

        /// <summary>
        /// 转换Stream为二进制字节
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] getBytesFromStream(Stream stream)
        {
            if (stream != null)
            {
                byte[] fileBytes = new byte[stream.Length];
                stream.Read(fileBytes, 0, fileBytes.Length);
                return fileBytes;
            }
            return null;
        }

        /// <summary>
        /// 序列（或集合）包含元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool isNotEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return false;
            }
            return source.Any();
        }

        #endregion
    }
}
