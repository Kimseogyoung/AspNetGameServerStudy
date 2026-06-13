using ServerCore.Repo.Database;

namespace ServerCore.Extension
{
    // Init<T> 한 줄로 DapperExtension + InMemoryPkRegistry 동시 등록
    public static class ModelRegistration
    {
        public static void Init<T>(params string[] keyFields)
        {
            DapperExtension.Init<T>(keyFields);
            InMemoryPkRegistry.Init<T>(keyFields);
        }
    }
}
