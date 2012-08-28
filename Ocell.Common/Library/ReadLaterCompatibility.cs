using System;
#if WINDOWS_PHONE
using System.Security.Cryptography;
#else
using Windows.Security.Cryptography.DataProtection;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace Ocell.Library
{
    public class AuthPair
    {
        private byte[] _encryptedPass;
        public string User {get; set;}
        public string Password
        {
            get
            {
                if (_encryptedPass == null)
                    return null;
                else
                {
#if WINDOWS_PHONE
                    byte[] unencryptedBytes = ProtectedData.Unprotect(_encryptedPass, null);
#else
                    var provider = new DataProtectionProvider("OCELL");
                    var unprotectTask = provider.UnprotectAsync(_encryptedPass.AsBuffer()).AsTask();
                    unprotectTask.Wait();
                    byte[] unencryptedBytes = unprotectTask.Result.ToArray();
#endif
                    return System.Text.Encoding.UTF8.GetString(unencryptedBytes, 0, unencryptedBytes.Length);
                }
            }
            set
            {
                if (value == null)
                    _encryptedPass = null;
                else
                {
                    byte[] unencryptedBytes = System.Text.Encoding.UTF8.GetBytes(value);
#if WINDOWS_PHONE
                    _encryptedPass = ProtectedData.Protect(unencryptedBytes, null);
#else
                    var provider = new DataProtectionProvider("OCELL");
                    var protectTask = provider.ProtectAsync(_encryptedPass.AsBuffer()).AsTask();
                    protectTask.Wait();
                    _encryptedPass = protectTask.Result.ToArray();
#endif
                }
            }
        }
    }

    public class ReadLaterCredentials
    {
        public AuthPair Instapaper { get; set; }
        public AuthPair Pocket { get; set; }
    }
}
