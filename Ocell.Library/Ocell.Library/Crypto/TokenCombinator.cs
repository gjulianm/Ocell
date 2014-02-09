using AncoraMVVM.Base.IoC;
using System;
using System.Text;

namespace Ocell.Library.Crypto
{
    public static class TokenCombinator
    {
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

        public static Tuple<string, string> Decombine(string str)
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

            return new Tuple<string, string>(a, b);
        }

        public static Tuple<string, string> DecodeTokens(string received)
        {
            var crypto = Dependency.Resolve<ICryptoManager>();
            return Decombine(crypto.AESDecrypt(received, SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret));
        }

        public static string EncodeTokens(string token, string secret)
        {
            var crypto = Dependency.Resolve<ICryptoManager>();
            return crypto.AESEncrypt(Combine(token, secret), SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret);
        }
    }
}
