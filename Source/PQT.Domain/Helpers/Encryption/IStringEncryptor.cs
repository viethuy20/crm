namespace PQT.Domain.Helpers.Encryption
{
    public interface IStringEncryptor
    {
        string Encrypt(string data);
        string Decrypt(string data);
    }
}
