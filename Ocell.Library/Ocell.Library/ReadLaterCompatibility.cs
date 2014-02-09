
using AncoraMVVM.Base.IoC;
namespace Ocell.Library
{
    public class AuthPair
    {
        private byte[] _encryptedPass;
        public string User { get; set; }
        public string Password
        {
            get
            {
                if (_encryptedPass == null)
                    return null;
                else
                {
                    byte[] unencryptedBytes = Dependency.Resolve<ICryptoManager>().UnprotectData(_encryptedPass);
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
                    _encryptedPass = Dependency.Resolve<ICryptoManager>().ProtectData(unencryptedBytes);
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
