using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            ASCIIEncoding ByteConverter = new ASCIIEncoding();

            string dataString = "Data to Sign";

            // Create byte arrays to hold original, encrypted, and decrypted data.
            byte[] originalData = ByteConverter.GetBytes(dataString);

            //  byte[] DataToEncrypt = Encoding.UTF8.GetBytes(resData);
            X509Certificate2 cert = new X509Certificate2(@"D:\itrus001.pfx", "123456");
            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)cert.PrivateKey;
            RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();
            RSAParameters Key = rsa.ExportParameters(true);
            byte[] signData = RsaSign(rsa, originalData);
          

            if (VerifySignedHash(originalData, signData, Key))
            {
                Console.WriteLine(1);
            }
            else
            {

            };
        }
        public static byte[] RsaSign(RSACryptoServiceProvider rsa, byte[] originalData)
        {

            // rsa.ExportParameters(true);
            // SHA1 sh = new SHA1CryptoServiceProvider();
            //  SHA256 sh1 = new SHA256CryptoServiceProvider();
            //byte[] data1 = rsa.SignData(DataToEncrypt, "SHA256");
            byte[] data1 = rsa.SignData(originalData, "SHA1");
            //string result = Convert.ToBase64String(data1);

            return data1;
        }
        public static bool VerifySignedHash(byte[] DataToVerify, byte[] SignedData, RSAParameters Key)
        {
            try
            {
                // Create a new instance of RSACryptoServiceProvider using the 
                // key from RSAParameters.
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                RSAalg.ImportParameters(Key);

                // Verify the data using the signature.  Pass a new instance of SHA1CryptoServiceProvider
                // to specify the use of SHA1 for hashing.
                return RSAalg.VerifyData(DataToVerify, new SHA1CryptoServiceProvider(), SignedData);

            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return false;
            }
        }
    }
}
