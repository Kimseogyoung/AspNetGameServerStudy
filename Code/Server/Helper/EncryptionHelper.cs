namespace WebStudyServer.Helper
{
    public class EncryptionHelper
    {
        public static byte[] GetSecretByteArr(string secret)
        {
            if (string.IsNullOrEmpty(secret))
            {
                return [];
            }
            var byteArr = Convert.FromBase64String(secret);
            return byteArr;
        }
    }
}
