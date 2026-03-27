using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ServerTest
{
    /// <summary>
    /// WebApplicationFactory: 실제 서버를 인메모리로 띄워서 E2E 테스트
    /// - DB/Cache는 InMemory 사용 (appsettings.yaml에 설정)
    /// - Proto CSV 경로는 appsettings.yaml에 설정된 상대경로 사용
    /// - CollectionDefinition으로 모든 테스트 클래스에 단일 인스턴스 공유 (Proto 정적 딕셔너리 중복 초기화 방지)
    /// </summary>
    [CollectionDefinition("GameServer")]
    public class GameServerCollection : ICollectionFixture<GameServerFactory> { }

    public class GameServerFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // appsettings.yaml이 테스트 출력 디렉토리에 있으므로
            // 별도 설정 없이 바로 사용 가능.
        }
    }
}
