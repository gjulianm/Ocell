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
using System.Collections.Generic;
using System.Security.Cryptography;

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
                    byte[] unencryptedBytes = ProtectedData.Unprotect(_encryptedPass, null);
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
                    _encryptedPass = ProtectedData.Protect(unencryptedBytes, null);
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
