using ServerCore;

namespace ServerCore.Helper
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
            var id = (ulong)IdGeneratorProvider.Get().CreateId();
            return id;
        }

        // 계정 하나가 가질 수 있는 플레이어 수만큼 자리를 띄운다.
        public static ulong MakePlayerId(ulong accountId)
        {
            return accountId * 10;
        }

        public static string GenerateRandomName()
        {
            var random = new Random();
            var number = random.Next(1, 1000);
            return $"{number}_PLAYER";
        }
    }
}
