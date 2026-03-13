namespace WebStudyServer.Helper
{
    public class EncryptionHelper
    {
        public static byte[] GetSecretByteArr(string secret)
        {
            if (string.IsNullOrEmpty(secret))
            {
                return new byte[0];
            }
            var byteArr = Convert.FromBase64String(secret);
            return byteArr;
        }
    }
}
