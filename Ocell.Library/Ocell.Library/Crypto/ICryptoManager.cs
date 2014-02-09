
namespace Ocell.Library
{
    public interface ICryptoManager
    {
        byte[] ProtectData(byte[] data);
        byte[] UnprotectData(byte[] data);
        string AESEncrypt(string str, string password, string salt);
        string AESDecrypt(string str, string password, string salt);
    }
}
