namespace WebStudyServer.Repo.Database
{
    // Singleton — 상태 없이 IDbSession를 생성하는 역할만 담당.
    // DbType에 따라 MySql / InMemory 구현체를 DI로 교체한다.
    public interface IDbSessionFactory
    {
        IDbSession Create(string connectionString);
    }
}
