using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace PQT.Web.Infrastructure.Helpers
{
    public static class StringCipher
    {

        public static string EnCrypt(string text, string aesKey)
        {

            RijndaelManaged aes = new RijndaelManaged();
            aes.BlockSize = 128;
            aes.KeySize = 128;
            aes.Padding = PaddingMode.Zeros;
            aes.Mode = CipherMode.ECB;
            //		aes.Mode = CipherMode.CBC;      // CBCモードを利用する場合は設定をこちらに

            aes.Key = System.Convert.FromBase64String(aesKey);

            // CBCモードを利用する場合はIVの設定を行う
            //		aes.IV = System.Text.Encoding.UTF8.GetBytes( AesIV );
            using (var encrypt = aes.CreateEncryptor())
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptStream = new CryptoStream(memoryStream, encrypt, CryptoStreamMode.Write))
                    {
                        byte[] text_bytes = Encoding.UTF8.GetBytes(text);

                        cryptStream.Write(text_bytes, 0, text_bytes.Length);
                        cryptStream.FlushFinalBlock();

                        byte[] encrypted = memoryStream.ToArray();
                        memoryStream.Close();
                        cryptStream.Close();
                        return Convert.ToBase64String(encrypted);
                    }
                }
            }
        }

        public static string Decrypt(string cryptText, string aesKey)
        {
            var keyArray = System.Text.Encoding.UTF8.GetBytes(aesKey);
            PadToMultipleOf(ref keyArray, 8);
            var aes = new RijndaelManaged();
            aes.BlockSize = 128;
            aes.KeySize = 128;
            aes.Key = keyArray;   
            aes.Padding = PaddingMode.None;
            aes.Mode = CipherMode.ECB;
            //		aes.Mode = CipherMode.CBC;  // CBCモードを利用する場合は設定をこちらに

            // CBCモードを利用する場合はIVの設定を行う
            //		aes.IV = System.Text.Encoding.UTF8.GetBytes( AesIV );

            using (var decryptor = aes.CreateDecryptor())
            {
                byte[] encrypted = System.Convert.FromBase64String(cryptText);
                byte[] planeText = new byte[encrypted.Length];
                using (var memoryStream = new MemoryStream(encrypted))
                {
                    using (var cryptStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        cryptStream.Read(planeText, 0, planeText.Length);
                        memoryStream.Close();
                        cryptStream.Close();
                        return Encoding.UTF8.GetString(planeText);
                    }
                }
            }
        }

        public static void PadToMultipleOf(ref byte[] src, int pad)
        {
            int len = (src.Length + pad - 1) / pad * pad;
            Array.Resize(ref src, len);
        }

    }
}