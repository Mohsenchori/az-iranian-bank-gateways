using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Threading.Tasks;

namespace PGTesterApp.Business
{
    public class AES2
    {
        string AES_Key = string.Empty;
        string AES_IV = string.Empty;
        public AES2(string AES_Key, string AES_IV)
        {
            this.AES_Key = AES_Key;
            this.AES_IV = AES_IV;
        }

        public bool Encrypt(String Input, out string encryptedString)
        {
            try
            {
                var aes = new RijndaelManaged();
                aes.KeySize = 256;
                aes.BlockSize = 256;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Convert.FromBase64String(this.AES_Key);
                aes.IV = Convert.FromBase64String(this.AES_IV);

                var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] xBuff = null;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                    {
                        byte[] xXml = Encoding.UTF8.GetBytes(Input);
                        cs.Write(xXml, 0, xXml.Length);
                    }

                    xBuff = ms.ToArray();
                }

                encryptedString = Convert.ToBase64String(xBuff);
                return true;
            }
            catch (Exception ex)
            {
                encryptedString = string.Empty;
                return false;
            }
        }

        public static bool GenerateKey(out string key, out string vector)
        {
            try
            {
                RijndaelManaged aesEncryption = new RijndaelManaged();
                aesEncryption.KeySize = 256;
                aesEncryption.BlockSize = 256;
                aesEncryption.Mode = CipherMode.CBC;
                aesEncryption.Padding = PaddingMode.PKCS7;
                aesEncryption.GenerateIV();
                string ivStr = Convert.ToBase64String(aesEncryption.IV);
                aesEncryption.GenerateKey();
                string keyStr = Convert.ToBase64String(aesEncryption.Key);
                key = keyStr;
                vector = ivStr;
                return true;
            }
            catch (Exception ex)
            {
                key = string.Empty;
                vector = string.Empty;
                return false;
            }
        }

        public bool Decrypt(String Input, out string decodedString)
        {
            try
            {
                RijndaelManaged aes = new RijndaelManaged();
                aes.KeySize = 256;
                aes.BlockSize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Convert.FromBase64String(this.AES_Key);
                aes.IV = Convert.FromBase64String(this.AES_IV);

                var decrypt = aes.CreateDecryptor();
                byte[] xBuff = null;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                    {
                        byte[] xXml = Convert.FromBase64String(Input);
                        cs.Write(xXml, 0, xXml.Length);
                    }

                    xBuff = ms.ToArray();
                }

                decodedString = Encoding.UTF8.GetString(xBuff);
                return true;
            }
            catch (Exception ex)
            {
                decodedString = string.Empty;
                return false;
            }
        }

    }
}
