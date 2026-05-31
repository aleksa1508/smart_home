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

        // client recieve xml public key
        public RsaClass(string publicKeyXml)
        {
            _rsa = new RSACryptoServiceProvider(2048);
            _rsa.FromXmlString(publicKeyXml);
        }

        // server exoort public key in xml format
        public string ExportPublicKey()
        {
            return _rsa.ToXmlString(false); // false =only public key
        }

        // client encription with public key
        public byte[] Encrypt(string plainText)
        {
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            return _rsa.Encrypt(data, false);
        }

        public string Decrypt(byte[] cipherText)
        {
            byte[] data = _rsa.Decrypt(cipherText, false);
            return Encoding.UTF8.GetString(data);
        }
        public byte[] EncryptBytes(byte[] data)
        {
            return _rsa.Encrypt(data, false);
        }
        public byte[] DecryptBytes(byte[] cipherText)
        {
            return _rsa.Decrypt(cipherText, false);
        }
    }
}
