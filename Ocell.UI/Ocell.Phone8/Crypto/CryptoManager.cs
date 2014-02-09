using Ocell.Library;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Ocell
{
    public class CryptoManager : ICryptoManager
    {
        public byte[] ProtectData(byte[] data)
        {
            return ProtectedData.Protect(data, null);
        }

        public byte[] UnprotectData(byte[] data)
        {
            return ProtectedData.Unprotect(data, null);
        }

        public string AESEncrypt(string str, string password, string salt)
        {
            try
            {
                using (Aes aes = new AesManaged())
                {
                    Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt));
                    aes.Key = deriveBytes.GetBytes(128 / 8);
                    aes.IV = aes.Key;
                    using (MemoryStream encryptionStream = new MemoryStream())
                    {
                        using (CryptoStream encrypt = new CryptoStream(encryptionStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            byte[] utfD1 = UTF8Encoding.UTF8.GetBytes(str);
                            encrypt.Write(utfD1, 0, utfD1.Length);
                            encrypt.FlushFinalBlock();
                        }

                        return Convert.ToBase64String(encryptionStream.ToArray());
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        public string AESDecrypt(string str, string password, string salt)
        {
            try
            {
                using (Aes aes = new AesManaged())
                {
                    Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt));
                    aes.Key = deriveBytes.GetBytes(128 / 8);
                    aes.IV = aes.Key;

                    using (MemoryStream decryptionStream = new MemoryStream())
                    {
                        using (CryptoStream decrypt = new CryptoStream(decryptionStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            byte[] encryptedData = Convert.FromBase64String(str);
                            decrypt.Write(encryptedData, 0, encryptedData.Length);
                            decrypt.Flush();
                        }
                        byte[] decryptedData = decryptionStream.ToArray();
                        return UTF8Encoding.UTF8.GetString(decryptedData, 0, decryptedData.Length);
                    }
                }
            }
            catch
            {
                return "";
            }
        }
    }
}
