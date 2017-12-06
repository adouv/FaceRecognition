using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ReapalDDD
{
    public class RSAEncryptor
    {
        public RSAEncryptor() { }


        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="resData">需要加密的字符串</param>
        /// <param name="publicKey">公钥</param> 
        /// <returns>明文</returns>
        public static string encryptData(string resData, string publicKey)
        {

            byte[] DataToEncrypt = Encoding.ASCII.GetBytes(resData);
#if DEBUG
            X509Certificate2 cert = new X509Certificate2(publicKey, "123456");
#else
                        X509Certificate2 cert = new X509Certificate2(publicKey, "001485");
#endif

            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)cert.PublicKey.Key;
            SHA1 sh = new SHA1CryptoServiceProvider();
            byte[] data1 = rsa.Encrypt(DataToEncrypt, false);

            string result = Convert.ToBase64String(data1);

            return result;
        }

        public static string encryptData(string resData, string publicKey, string Pwd)
        {

            byte[] DataToEncrypt = Encoding.ASCII.GetBytes(resData);
            X509Certificate2 cert = new X509Certificate2(publicKey, Pwd);

            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)cert.PublicKey.Key;
            SHA1 sh = new SHA1CryptoServiceProvider();
            byte[] data1 = rsa.Encrypt(DataToEncrypt, false);

            string result = Convert.ToBase64String(data1);

            return result;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="resData">加密字符串</param>
        /// <param name="privateKey">私钥</param> 
        /// <returns>明文</returns>
        public static string decryptData(string resData, string privateKey)
        {

            byte[] DataToDecrypt = Convert.FromBase64String(resData);
            string result = "";
#if DEBUG
            X509Certificate2 cert = new X509Certificate2(privateKey, "123456");
#else
            X509Certificate2 cert = new X509Certificate2(privateKey, "001485");
#endif

            //将证书testCertificate.pfx的私钥强制转换为一个RSACryptoServiceProvider对象，用于解密操作
            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)cert.PrivateKey;

            byte[] source = rsa.Decrypt(DataToDecrypt, false);

            result = UTF8Encoding.UTF8.GetString(source);
            return result;
        }

        public static string decryptData(string resData, string privateKey, string Pwd)
        {

            byte[] DataToDecrypt = Convert.FromBase64String(resData);
            string result = "";

            X509Certificate2 cert = new X509Certificate2(privateKey, Pwd);
            //将证书testCertificate.pfx的私钥强制转换为一个RSACryptoServiceProvider对象，用于解密操作
            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)cert.PrivateKey;

            byte[] source = rsa.Decrypt(DataToDecrypt, false);

            result = UTF8Encoding.UTF8.GetString(source);
            return result;
        }
    }
}