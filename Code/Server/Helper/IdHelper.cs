using WebStudyServer.GAME;

namespace WebStudyServer.Helper
{
    public static class IdHelper
    {
        public static string GenerateGuidKey()
        {
            var guid = Guid.NewGuid();
            return guid.ToString();
        }

        public static ulong GenerateSfId()
        {
            var id = (ulong)APP.IdGenerator.CreateId();
            return id;
        }

        public static string GenerateRandomName()
        {
            var random = new Random();
            var number = random.Next(1, 1000);
            return $"{number}_PLAYER";
        }
    }
}
