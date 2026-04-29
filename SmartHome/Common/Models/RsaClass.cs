namespace Common.Models
{
    using System.Security.Cryptography;
    using System.Text;

    public class RsaClass
    {
        private RSACryptoServiceProvider _rsa;

        // Server — generiše par ključeva
        public RsaClass()
        {
            _rsa = new RSACryptoServiceProvider(2048);
        }

        // Klijent — prima XML javni ključ
        public RsaClass(string publicKeyXml)
        {
            _rsa = new RSACryptoServiceProvider(2048);
            _rsa.FromXmlString(publicKeyXml);
        }

        // Server izvozi javni ključ kao XML string
        public string ExportPublicKey()
        {
            return _rsa.ToXmlString(false); // false = samo javni ključ
        }

        // Klijent enkriptuje javnim ključem
        public byte[] Encrypt(string plainText)
        {
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            return _rsa.Encrypt(data, false); // false = PKCS#1
        }

        // Server dekriptuje privatnim ključem
        public string Decrypt(byte[] cipherText)
        {
            byte[] data = _rsa.Decrypt(cipherText, false);
            return Encoding.UTF8.GetString(data);
        }
        // Enkriptuje byte[] umjesto stringa
        public byte[] EncryptBytes(byte[] data)
        {
            return _rsa.Encrypt(data, false);
        }

        // Dekriptuje nazad u byte[]
        public byte[] DecryptBytes(byte[] cipherText)
        {
            return _rsa.Decrypt(cipherText, false);
        }
    }
}
