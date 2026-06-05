using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common.Models
{
    public class AesClass
    {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }


        public AesClass(byte[] key, byte[] iv)
        {
            this.Key = key;
            this.IV = iv;
        }

        public AesClass()
        {
        }

        public byte[] EncryptMessage(string message, byte[] key, byte[] IV)
        {
            byte[] encrypted;
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = IV;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (var streamWriter = new StreamWriter(cryStream))
                        {
                            streamWriter.Write(message);
                        }
                    }
                    encrypted = memoryStream.ToArray();
                }
            }
            return encrypted;
        }
        public string DecryptMessage(byte[] encryptText, byte[] key, byte[] IV)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = IV;
                    aes.Padding = PaddingMode.PKCS7;
                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using (var memoryStream = new MemoryStream(encryptText))
                    using (var cryStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    using (var output = new MemoryStream())
                    {
                        cryStream.CopyTo(output);
                        return Encoding.UTF8.GetString(output.ToArray());
                    }
                }
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"Decrypt failed! Bytes: {BitConverter.ToString(encryptText)}\nError:", ex.ToString());
                Console.WriteLine($"Key: {BitConverter.ToString(key)}");
                Console.WriteLine($"IV: {BitConverter.ToString(IV)}");
                throw;
            }
        }
    }
}
