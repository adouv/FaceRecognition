using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    class Program
    {
        static void Main(string[] args)
        {
            var dic = new Dictionary<string, object>();
           // dic.Add("a", "1");
           // dic.Add("b", "3");
            //dic.Add("c", "1");
            //dic.Add("d", "3");
            dic.Add("merchant_id", "100000000239166");
            dic.Add("data", "{\"cert_no\":\"412722199310223521\", \"face_id\":\"201709306319733661130403840\",\"cert_name\":\"孙晨杨\"}");
            dic.Add("encryptkey", "Cii0jhBrt2plxhT9r1Tl5YiG/Q++uNmBMGXROtfE39Tj3coIYKyybIHQm42NQv5wUN8ajexoAaeykzp+lNsX5dUavKr9Ch8gxpSJpX0+C6xrSHWF3EVxJ8Y/i9DP00/D4Bw+PoWs63JTmEcDiaqIZ5IU8LvGjygpaTnZ7+Ntn2d3OM3wYAZtnX+cHwDRYC7/kJi+Ezwf4UHJj8EKqa0+y8pACqh4k2x9StjF5AhhkWB5pPWlOXFljQcXhMeQgnPgwHVX1cGVFYSADeM7IAAmVVT6BWkGf+GWPIUdeiXrFk0l1qvenyH6UWgo/SXuLm85x+toV5XD9uaiADg4XDpN+A==");
            dic.Add("timestamp", "20170930112521148");
            Sign(dic, "");
        }
        public static string Sign(Dictionary<string, object> contents, string privateKey)
        {
            var sortedContents = string.Join("&", from key in contents.Keys
            where key != "sign" && !key.Equals("sign_type")
                                                  orderby key
                                                  select key.ToLower() + "=" + (contents[key] ?? string.Empty));
            //string sortedContents = "";

            //foreach (var item in contents)
            //{
            //    if (!item.Key.Equals("sign") && !item.Key.Equals("sign_type"))
            //        sortedContents += item.Key + "=" + item.Value + "&";
            //}
            return RsaSign(sortedContents.Trim('&'), @"D:\rongbao.pfx", "123456");
        }
        public static string RsaSign(string resData, string privateKey, string Pwd)
        {
            byte[] DataToEncrypt = Encoding.UTF8.GetBytes(resData);
            X509Certificate2 cert = new X509Certificate2(privateKey, Pwd);
            var rsa = cert.GetRSAPrivateKey();
            byte[] data1 = rsa.SignData(DataToEncrypt, HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
            string result = Convert.ToBase64String(data1);
            return result;
        }
    }
}
