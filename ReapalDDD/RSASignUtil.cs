using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ReapalDDD
{
  public  class RSASignUtil
    {
        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="data">要签名的数据</param>
        /// <param name="privateKeyPem">RSA私钥文件或psa私钥串</param>
        /// <param name="charset"></param>
        /// <param name="signType">RSA或RSA2</param>
        /// <param name="keyFromFile">true读文件false 读字符串</param>
        /// <returns></returns>
        public static string RSASign(string data, string privateKeyPem, string charset, string signType, bool keyFromFile)
        {
            return AlipaySignature.RSASign(data, privateKeyPem, charset, signType, keyFromFile);
        }
        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="resData">要签名的数据</param>
        /// <param name="privateKey">私钥证书地址</param>
        /// <param name="Pwd">密码</param>
        /// <returns></returns>
        public static string RsaSign(string resData, string privateKey, string Pwd)
        {
            byte[] DataToEncrypt = Encoding.UTF8.GetBytes(resData);
            X509Certificate2 cert = new X509Certificate2(privateKey, Pwd);
            var rsa = cert.GetRSAPrivateKey();
            byte[] data1 = rsa.SignData(DataToEncrypt, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            string result = Convert.ToBase64String(data1);
            return result;
        }
    }
}
