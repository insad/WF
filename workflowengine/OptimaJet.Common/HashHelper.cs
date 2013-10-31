using System;
using System.Security.Cryptography;
using System.Text;

namespace OptimaJet.Common
{
    public static class HashHelper
    {
        public static string GenerateSalt()
        {
            var data = new byte[0x10];
            new RNGCryptoServiceProvider().GetBytes(data);
            return Convert.ToBase64String(data);
        }

        public static string GenerateStringHash(string stringForHashing, string salt)
        {
            return GenerateStringHash(stringForHashing, salt, HashAlgorithm.Create("SHA1"));
        }

        public static string GenerateStringHash(string stringForHashing)
        {
            return GenerateStringHash(stringForHashing, string.Empty, HashAlgorithm.Create("MD5"));
        }

        public static string GenerateStringHash(string stringForHashing, HashAlgorithm hashAlgorithm)
        {
            if (hashAlgorithm is KeyedHashAlgorithm)
                throw new NotSupportedException("It is impossible to create Hash with KeyedHashAlgorithm and empty Salt");

            return GenerateStringHash(stringForHashing, string.Empty, hashAlgorithm);
        }

        public static string GenerateStringHash(string stringForHashing, string salt, HashAlgorithm hashAlgorithm)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(stringForHashing);
            byte[] src = Convert.FromBase64String(salt);
            byte[] inArray;
            if (hashAlgorithm is KeyedHashAlgorithm)
            {
                var keyedHashAlgorithm = (KeyedHashAlgorithm) hashAlgorithm;
                if (keyedHashAlgorithm.Key.Length == src.Length)
                {
                    keyedHashAlgorithm.Key = src;
                }
                else if (keyedHashAlgorithm.Key.Length < src.Length)
                {
                    var dst = new byte[keyedHashAlgorithm.Key.Length];
                    Buffer.BlockCopy(src, 0, dst, 0, dst.Length);
                    keyedHashAlgorithm.Key = dst;
                }
                else
                {
                    int count;
                    var buffer = new byte[keyedHashAlgorithm.Key.Length];
                    for (int i = 0; i < buffer.Length; i += count)
                    {
                        count = Math.Min(src.Length, buffer.Length - i);
                        Buffer.BlockCopy(src, 0, buffer, i, count);
                    }
                    keyedHashAlgorithm.Key = buffer;
                }

                inArray = keyedHashAlgorithm.ComputeHash(bytes);
            }
            else
            {
                var buffer = new byte[src.Length + bytes.Length];
                Buffer.BlockCopy(src, 0, buffer, 0, src.Length);
                Buffer.BlockCopy(bytes, 0, buffer, src.Length, bytes.Length);
                inArray = hashAlgorithm.ComputeHash(buffer);
            }

            return Convert.ToBase64String(inArray);
        }
    }
}