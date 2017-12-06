using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Specialized;
using ReapalDDD;
using System.Net;
using System.IO;
using System.Net.Http;

namespace ReapalService
{
    public abstract class BaseProcessor<TResult>
    {
        private const string RequestEncodingName = "UTF-8";
        private const string ParameterEncodingName = "UTF-8";
        protected abstract string RequestAddress { get; }

        protected abstract string ServiceAddress { get; }


        protected abstract Dictionary<string, FileItem> FileList { get; }

        public static string ReapalMerchantId
        {
            get
            {
                return "123123";
            }
        }

        protected abstract Dictionary<string, object> PrepareRequestCore();

        /// <summary>
        /// 执行调用api
        /// </summary>
        /// <returns></returns>
        /// 
        public ExecResult<TResult> Execute()
        {
            var result = new ExecResult<TResult>();
            var target = string.Empty;
            //var request = string.Empty;
            var response = string.Empty;
            DateTime? reqTime = null;
            DateTime? resTime = null;
            try
            {
                target = GetRequestUrl();
                var request = PrepareRequest();
                reqTime = DateTime.Now;
                if (FileList == null || FileList.Count() < 1)
                {
                    var encoding = System.Text.Encoding.GetEncoding(ParameterEncodingName);
                    string requestStr = request.Keys.OrderBy(r => r).Join("&", item => item + "=" + System.Web.HttpUtility.UrlDecode(request[item] ?? string.Empty, encoding));
                    response = Common.SendPostRequestCore(target, requestStr, RequestEncodingName, null, null, null, null);
                }
                else
                {
                    response = Common.DoPost(target, request, FileList, ParameterEncodingName);
                }
                resTime = DateTime.Now;
                result = ParseCenterResponse(response);
            }
            catch (Exception ex)
            {
                result = new ExecResult<TResult>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            if (reqTime.HasValue)
            {
                // LogHelper.WriteErrorLog("DisplayAddProcess", ex.Message + "\n" + ex.StackTrace);
                //LoggerFactory.Instance.Logger_Info(
                //    string.Format("UserCenterService----target:{1}{0}reqTime:{2:yyyy-MM-dd HH:mm:ss.fff}{0}request:{3}{0}resTime:{4:yyyy-MM-dd HH:mm:ss.fff}{0}response:{5}{0}",
                //        Environment.NewLine, target, reqTime, request, resTime, response));
            }
            return result;
        }
        /// <summary>
        /// 获取请求url
        /// </summary>
        /// <returns></returns>
        private string GetRequestUrl()
        {
            return ServiceAddress + "/" + RequestAddress;
        }
        private IDictionary<string, string> PrepareRequest()
        {
            var parameters = PrepareRequestCore();
            string json = JsonConvert.SerializeObject(parameters);
            //加密业务数据--用AES对称加密算法
            string AESKey = AESEncryptor.GenerateAESKey();
            string strData = AESEncryptor.Encrypt(json, AESKey);
            //加密AESKey--用RSA非对称加密算法
            string strKey = RSAEncryptor.encryptData(AESKey, XmlConfig.ReapalPublicKeyCerUrl);
            string timestamps = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            //var dic = new Dictionary<string, object>();
            //dic.Add("merchant_id", ReapalMerchantId);
            //dic.Add("data", json);
            //dic.Add("encryptkey", strKey);
            //dic.Add("timestamp", timestamps);

            var dic = new Dictionary<string, object>();
            dic.Add("merchant_id", "100000000239166");
            dic.Add("data", "{\"cert_no\":\"412722199310223521\", \"face_id\":\"201709306319733661130403840\",\"cert_name\":\"孙晨杨\"}");
            dic.Add("encryptkey", "Cii0jhBrt2plxhT9r1Tl5YiG/Q++uNmBMGXROtfE39Tj3coIYKyybIHQm42NQv5wUN8ajexoAaeykzp+lNsX5dUavKr9Ch8gxpSJpX0+C6xrSHWF3EVxJ8Y/i9DP00/D4Bw+PoWs63JTmEcDiaqIZ5IU8LvGjygpaTnZ7+Ntn2d3OM3wYAZtnX+cHwDRYC7/kJi+Ezwf4UHJj8EKqa0+y8pACqh4k2x9StjF5AhhkWB5pPWlOXFljQcXhMeQgnPgwHVX1cGVFYSADeM7IAAmVVT6BWkGf+GWPIUdeiXrFk0l1qvenyH6UWgo/SXuLm85x+toV5XD9uaiADg4XDpN+A==");
            dic.Add("timestamp", "20170930112521148");


            IDictionary<string, string> dicCollection = new SortedDictionary<string, string>();
            dicCollection.Add("merchant_id", ReapalMerchantId);
            dicCollection.Add("data", strData);
            dicCollection.Add("encryptkey", strKey);
            dicCollection.Add("timestamp", timestamps);
            //dicCollection.Add("sign", Common.Sign(dic, XmlConfig.ReapalPrivateKeyPfxUrl));
            dicCollection.Add("sign", "123456");
            dicCollection.Add("sign_type", "RSA");
            return dicCollection;
        }

        private ExecResult<TResult> ParseCenterResponse(string response)
        {
            var result = new ExecResult<TResult>();
            var view = JsonConvert.DeserializeObject<ResponseView>(response);
            //解密
            string encryptkey = RSAEncryptor.decryptData(view.encryptkey, XmlConfig.ReapalPrivateKeyPfxUrl);
            var data = AESEncryptor.Decrypt(view.data, encryptkey);
            var sData = JsonConvert.DeserializeObject<BaseView>(data);
            if (sData.result_code == "0000")
            {
                result.Success = true;
            }
            else
            {
                result.MsgCode = sData.result_code;
                result.Message = sData.result_msg;
                result.Success = false;
            }
            return result;
        }

    }

}
