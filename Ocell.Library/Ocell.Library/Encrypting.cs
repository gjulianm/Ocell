using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using DanielVaughan.Linq;

namespace Ocell.Library
{
    public static class Encrypting
    {
        public static string EncryptString(string Str, string Password, string Salt)
        {
            try
            {
                using (Aes aes = new AesManaged())
                {
                    Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(Password, Encoding.UTF8.GetBytes(Salt));
                    aes.Key = deriveBytes.GetBytes(128 / 8);
                    aes.IV = aes.Key;
                    using (MemoryStream encryptionStream = new MemoryStream())
                    {
                        using (CryptoStream encrypt = new CryptoStream(encryptionStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            byte[] utfD1 = UTF8Encoding.UTF8.GetBytes(Str);
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

        public static string DecryptString(string Str, string Password, string Salt)
        {
            try
            {
                using (Aes aes = new AesManaged())
                {
                    Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(Password, Encoding.UTF8.GetBytes(Salt));
                    aes.Key = deriveBytes.GetBytes(128 / 8);
                    aes.IV = aes.Key;

                    using (MemoryStream decryptionStream = new MemoryStream())
                    {
                        using (CryptoStream decrypt = new CryptoStream(decryptionStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            byte[] encryptedData = Convert.FromBase64String(Str);
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

        public static string Combine(string a, string b)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("{0}|{1}¬", a.Length, b.Length);
            int i;

            for (i = 0; i < a.Length && i < b.Length; i++)
            {
                builder.Append(a[i]);
                builder.Append(b[i]);
            }

            for (; i < a.Length; i++)
                builder.Append(a[i]);

            for (; i < b.Length; i++)
                builder.Append(b[i]);

            return builder.ToString();
        }

        public static Pair<string, string> Decombine(string str)
        {
            string a = "", b = "";
            string aStrLen = "", bStrLen = "";
            int aLen, bLen;
            int i;
            int currentAchars, currentBchars;

            for (i = 0; i < str.Length && str[i] != '|'; i++)
                aStrLen += str[i];

            if (i == str.Length)
                return null;

            i++;

            for (; i < str.Length && str[i] != '¬'; i++)
                bStrLen += str[i];

            if (i == str.Length)
                return null;

            i++;

            if (!int.TryParse(aStrLen, out aLen) || !int.TryParse(bStrLen, out bLen))
                return null;

            currentAchars = 0;
            currentBchars = 0;
            int startIndex; // Flag to avoid infinite loops

            while (i < str.Length)
            {
                startIndex = i;
                if (currentAchars < aLen)
                {
                    currentAchars++;
                    a += str[i];
                    i++;
                }

                if (i < str.Length && currentBchars < bLen)
                {
                    currentBchars++;
                    b += str[i];
                    i++;
                }

                if (i == startIndex)
                    break;
            }

            return new Pair<string, string>(a, b);
        }

        public static Pair<string, string> DecodeTokens(string received)
        {
            return Decombine(DecryptString(received, SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret));
        }

        public static string EncodeTokens(string token, string secret)
        {
            return EncryptString(Combine(token, secret), SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret);
        }
    }
}
