using System.IO;
using System.Security.Cryptography;

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
            string decrypted = string.Empty;
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = IV;
                var encryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var memoryStream = new MemoryStream(encryptText))
                {
                    using (var cryStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Read))
                    {
                        using (var streamReader = new StreamReader(cryStream))
                        {
                            decrypted = streamReader.ReadToEnd();
                        }
                    }
                }
            }
            return decrypted;
        }
    }
}
