using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Client__.Net_.MVVM.Helpers
{
    public static class EncryptionHelper
    {
        private static readonly string EncryptionKey = "R3s5fXTGwi8bko11N6FoEQl+yldQZeOtuMYLsCvwkNU="; // Change to a strong key

        public static string Encrypt(string plainText)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes.Take(16).ToArray();
                aes.GenerateIV();
                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                    return Convert.ToBase64String(aes.IV.Concat(encryptedBytes).ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);
            byte[] allBytes = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes.Take(16).ToArray();
                aes.IV = allBytes.Take(16).ToArray();
                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] encryptedBytes = allBytes.Skip(16).ToArray();
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
    }
}
