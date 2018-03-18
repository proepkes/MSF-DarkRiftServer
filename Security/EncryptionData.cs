using System.Security.Cryptography;

namespace TundraServerPlugins
{
    internal class EncryptionData
    {
        public string AesKey;
        public byte[] AesKeyEncrypted;
    }
}