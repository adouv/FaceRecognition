using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using ReapalDDD;
using System.Net.Http;

namespace ReapalService
{
    public static class Common
    {
        private static int _timeout = 100000;
        public static string Sign(Dictionary<string, object> contents, string privateKey)
        {
            var sortedContents = string.Join("&", from key in contents.Keys
                                                  where key != "sign" && !key.Equals("sign_type")
                                                  orderby key
                                                  select key.ToLower() + "=" + (contents[key] ?? string.Empty));
            return RSASignUtil.RsaSign(sortedContents, @"D:\itrus001.pfx", "123456");

            #region 格式不一样
            // return RSASignUtil.RSASign(sortedContents, @"D:\target_rsa_privatekey1.txt", "utf-8","RSA2",true);
            // return RSASignUtil.RSASign(sortedContents, RSAFaceKey.RSAprivatekey, "utf-8","RSA2",false); 
            #endregion

        }

        public static string  HttpClientPost(string url, IDictionary<string, string> textParams, Dictionary<string, FileItem> fileParams, string charset)
        {
            HttpClient hc = new HttpClient();
            var mc = new MultipartFormDataContent();
            mc.Add(new FormUrlEncodedContent(textParams));
            // fileParams.ForEach(i => mc.Add(new ByteArrayContent(i.Content), i.Name, i.FileName));
            foreach (var item in fileParams)
            {
                mc.Add(new ByteArrayContent(item.Value.Content),item.Key,item.Value.FileName);
            }
            var rsp = hc.PostAsync(url, mc).GetAwaiter().GetResult();
            if (rsp.IsSuccessStatusCode)
            {
               return rsp.Content.ReadAsStringAsync().Result;
            }
            return string.Empty;
        }


        //public static string HttpClientPost(string url, IDictionary<string, string> textParams, List<FileItem> fileParams, string charset)
        //{
        //    HttpClient hc = new HttpClient();
        //    var mc = new MultipartFormDataContent();
        //    mc.Add(new FormUrlEncodedContent(textParams));
        //    fileParams.ForEach(i => mc.Add(new ByteArrayContent(i.Content), i.Name, i.FileName));

        //    var rsp = hc.PostAsync(url, mc).GetAwaiter().GetResult();

        //    // HttpResponseMessage
        //    // Encoding encoding = Encoding.GetEncoding(rsp.CharacterSet);
        //    // return GetResponseAsString(rsp, encoding);
        //}

        /// <summary>
        /// 执行带文件上传的HTTP POST请求。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="textParams">请求文本参数</param>
        /// <param name="fileParams">请求文件参数</param>
        /// <param name="charset">编码字符集</param>
        /// <returns>HTTP响应</returns>
        public static string DoPost(string url, IDictionary<string, string> textParams, IDictionary<string, FileItem> fileParams, string charset)
        {
            string boundary = DateTime.Now.Ticks.ToString("X"); // 随机分隔线

            HttpWebRequest req = GetWebRequest(url, "POST");
            req.ContentType = "multipart/form-data;charset=" + charset + ";boundary=" + boundary;

            Stream reqStream = req.GetRequestStream();
            byte[] itemBoundaryBytes = Encoding.GetEncoding(charset).GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endBoundaryBytes = Encoding.GetEncoding(charset).GetBytes("\r\n--" + boundary + "--\r\n");

            // 组装文本请求参数
            string textTemplate = "Content-Disposition:form-data;name=\"{0}\"\r\nContent-Type:text/plain\r\n\r\n{1}";
            IEnumerator<KeyValuePair<string, string>> textEnum = textParams.GetEnumerator();
            while (textEnum.MoveNext())
            {
                string textEntry = string.Format(textTemplate, textEnum.Current.Key, textEnum.Current.Value);
                byte[] itemBytes = Encoding.GetEncoding(charset).GetBytes(textEntry);
                reqStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
                reqStream.Write(itemBytes, 0, itemBytes.Length);
            }

            // 组装文件请求参数
            string fileTemplate = "Content-Disposition:form-data;name=\"{0}\";filename=\"{1}\"\r\nContent-Type:{2}\r\n\r\n";
            IEnumerator<KeyValuePair<string, FileItem>> fileEnum = fileParams.GetEnumerator();
            while (fileEnum.MoveNext())
            {
                string key = fileEnum.Current.Key;
                FileItem fileItem = fileEnum.Current.Value;
                string fileEntry = string.Format(fileTemplate, key, fileItem.FileName, fileItem.GetMimeType(fileItem.FileName));
                byte[] itemBytes = Encoding.GetEncoding(charset).GetBytes(fileEntry);
                reqStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
                reqStream.Write(itemBytes, 0, itemBytes.Length);

                byte[] fileBytes = fileItem.Content;
                reqStream.Write(fileBytes, 0, fileBytes.Length);
            }
            //foreach (var item in fileParams)
            //{
            //    string fileEntry = string.Format(fileTemplate, "image_cert", "image_cert", new FileItem().GetMimeType(item.FileName));
            //    byte[] itemBytes = Encoding.GetEncoding(charset).GetBytes(fileEntry);
            //    reqStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
            //    reqStream.Write(itemBytes, 0, itemBytes.Length);
            //    byte[] fileBytes = item.Content;
            //    reqStream.Write(fileBytes, 0, fileBytes.Length);
            //}

            reqStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
            reqStream.Close();

            HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();
            Encoding encoding = Encoding.GetEncoding(rsp.CharacterSet);
            return GetResponseAsString(rsp, encoding);
        }
        public static HttpWebRequest GetWebRequest(string url, string method)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.ServicePoint.Expect100Continue = false;
            req.Method = method;
            req.KeepAlive = true;
            req.UserAgent = "ReapalNet";
            req.Timeout = _timeout;
            return req;
        }

        /// <summary>
        /// 把响应流转换为文本。
        /// </summary>
        /// <param name="rsp">响应流对象</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>响应文本</returns>
        private static string GetResponseAsString(HttpWebResponse rsp, Encoding encoding)
        {
            StringBuilder result = new StringBuilder();
            Stream stream = null;
            StreamReader reader = null;

            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, encoding);

                // 按字符读取并写入字符串缓冲
                int ch = -1;
                while ((ch = reader.Read()) > -1)
                {
                    // 过滤结束符
                    char c = (char)ch;
                    if (c != '\0')
                    {
                        result.Append(c);
                    }
                }
            }
            finally
            {
                // 释放资源
                if (reader != null) reader.Close();
                if (stream != null) stream.Close();
                if (rsp != null) rsp.Close();
            }

            return result.ToString();
        }

        //body是要传递的参数,格式"roleId=1&uid=2"
        //post的cotentType填写:
        //"application/x-www-form-urlencoded"
        //soap填写:"text/xml; charset=utf-8"
        public static string PostHttp(string url, string body)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 20000;

            byte[] btBodys = Encoding.UTF8.GetBytes(body);
            httpWebRequest.ContentLength = btBodys.Length;
            httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            string responseContent = streamReader.ReadToEnd();

            httpWebResponse.Close();
            streamReader.Close();
            httpWebRequest.Abort();
            httpWebResponse.Close();

            return responseContent;
        }

        public static string SendPostRequestCore(string url, string content, string encodingName, int? timeout, CookieCollection cookies, NameValueCollection headers, string contentType)
        {
            HttpWebRequest request = null;
            try
            {
                // 创建HTTP请求
                request = CreateHttpRequest(url, "POST", timeout);
                // 写入Cookie
                WriteRequestCookie(request, cookies);
                // 写入header值
                WriteRequestHeader(request, headers);
                if (!string.IsNullOrWhiteSpace(contentType))
                {
                    request.ContentType = contentType;
                }
                //这个在Post的时候，一定要加上，如果服务器返回错误，他还会继续再去请求，不会使用之前的错误数据，做返回数据
                //request.ServicePoint.Expect100Continue = false;   
                // 写入POST数据
                WriteRequestContent(request, content, encodingName);
                // 接收HTTP响应
                return ReceiveResponse(request, encodingName);
            }
            catch (Exception ex)
            {
                var result = ex.Message;
                if (request != null)
                {
                    request.Abort();
                }
                return result;
            }
            finally
            {
                if (request != null)
                {
                    request.Abort();
                }
            }
        }
        private static string ReceiveResponse(WebRequest request, string encodingName)
        {
            HttpWebResponse response = null;
            try
            {
                // 获取HTTP响应
                response = request.GetResponse() as HttpWebResponse;
                // 接收响应数据
                return ReceiveResponse(response, encodingName);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
        }
        private static string ReceiveResponse(HttpWebResponse response, string encoding)
        {
            var result = new StringBuilder();
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream, Encoding.GetEncoding(string.IsNullOrEmpty(response.ContentEncoding) ? encoding : response.ContentEncoding)))
                {
                    while (reader.Peek() > 0)
                    {
                        result.Append(reader.ReadLine());
                    }
                }
            }
            return result.ToString();
        }

        private static void WriteRequestHeader(HttpWebRequest request, NameValueCollection headers)
        {
            if (headers != null && headers.Count > 0)
            {
                if (request.Headers == null)
                    request.Headers = new WebHeaderCollection();
                request.Headers.Add(headers);
            }
        }
        private static void WriteRequestContent(WebRequest request, string content, string encodingName)
        {
            if (content != null)
            {
                var requestData = Encoding.GetEncoding(encodingName).GetBytes(content);
                request.ContentLength = requestData.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                using (var requestSream = request.GetRequestStream())
                {
                    requestSream.Write(requestData, 0, requestData.Length);
                }
            }
        }
        private static HttpWebRequest CreateHttpRequest(string url, string method, int? timeout)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = method;
            request.KeepAlive = false;
            if (timeout.HasValue)
                request.Timeout = timeout.Value;
            return request;
        }

        private static void WriteRequestCookie(HttpWebRequest request, CookieCollection cookies)
        {
            if (cookies != null && cookies.Count > 0)
            {
                if (request.CookieContainer == null)
                    request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
        }
    }
}
