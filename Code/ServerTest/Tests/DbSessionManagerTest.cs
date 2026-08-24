using ServerCore.Repo.Database;
using Xunit;

namespace ServerTest.Tests
{
    /// <summary>
    /// DbSessionManager 세션 수명 테스트 (DB 없이 돈다)
    /// - 세션은 첫 쿼리에서 만들어진다. 쿼리가 없으면 아무것도 안 만든다.
    /// - 커밋/롤백이 도중에 실패해도 만들어진 세션은 전부 닫혀야 한다.
    /// </summary>
    public class DbSessionManagerTest
    {
        [Fact]
        public void Open_WithoutQuery_CreatesNothing()
        {
            var factory = new FakeDbSessionFactory();
            var manager = new DbSessionManager(factory);

            manager.Open("a");
            manager.Open("b");
            manager.Commit();

            Assert.Equal(0, factory.CreatedCount);
        }

        [Fact]
        public async Task Query_CreatesSessionOnce()
        {
            var factory = new FakeDbSessionFactory();
            var manager = new DbSessionManager(factory);

            var session = manager.Open("a");
            await session.ExecuteAsync(_ => Task.CompletedTask);
            await session.ExecuteAsync(_ => Task.CompletedTask);

            Assert.Equal(1, factory.CreatedCount);
        }

        [Fact]
        public async Task Commit_PartialFailure_ClosesEverySession()
        {
            var factory = new FakeDbSessionFactory();
            factory.Sessions["b"].ThrowOnCommit = true;

            var manager = new DbSessionManager(factory);
            await MaterializeAsync(manager, "a", "b", "c");

            Assert.ThrowsAny<Exception>(() => manager.Commit());

            // 성공한 것, 실패한 것, 차례가 오지 않은 것 모두 닫혀야 한다.
            Assert.True(factory.Sessions["a"].Closed, "커밋에 성공한 세션이 안 닫혔다");
            Assert.True(factory.Sessions["b"].Closed, "커밋에 실패한 세션이 안 닫혔다");
            Assert.True(factory.Sessions["c"].Closed, "차례가 오지 않은 세션이 안 닫혔다");
        }

        [Fact]
        public async Task Rollback_PartialFailure_ClosesEverySession()
        {
            var factory = new FakeDbSessionFactory();
            factory.Sessions["a"].ThrowOnRollback = true;

            var manager = new DbSessionManager(factory);
            await MaterializeAsync(manager, "a", "b");

            Assert.ThrowsAny<Exception>(() => manager.Rollback());

            Assert.True(factory.Sessions["a"].Closed, "롤백에 실패한 세션이 안 닫혔다");
            Assert.True(factory.Sessions["b"].Closed, "차례가 오지 않은 세션이 안 닫혔다");
        }

        [Fact]
        public async Task Dispose_ClosesOpenSessions()
        {
            var factory = new FakeDbSessionFactory();

            using (var manager = new DbSessionManager(factory))
            {
                await MaterializeAsync(manager, "a");
            }

            Assert.True(factory.Sessions["a"].Closed, "커밋도 롤백도 안 탄 세션이 안 닫혔다");
        }

        private static async Task MaterializeAsync(DbSessionManager manager, params string[] connectionStrList)
        {
            foreach (var connectionStr in connectionStrList)
            {
                await manager.Open(connectionStr).ExecuteAsync(_ => Task.CompletedTask);
            }
        }

        private class FakeDbSessionFactory : IDbSessionFactory
        {
            public Dictionary<string, FakeDbSession> Sessions { get; } = new()
            {
                ["a"] = new FakeDbSession(),
                ["b"] = new FakeDbSession(),
                ["c"] = new FakeDbSession(),
            };

            public int CreatedCount { get; private set; }

            public IDbSession Create(string connectionString)
            {
                CreatedCount++;
                return Sessions[connectionString];
            }
        }

        private class FakeDbSession : IDbSession
        {
            public bool ThrowOnCommit { get; set; }
            public bool ThrowOnRollback { get; set; }
            public bool Closed { get; private set; }

            public Task ExecuteAsync(Func<IDbExecutor, Task> action) => Task.CompletedTask;
            public Task<T> ExecuteAsync<T>(Func<IDbExecutor, Task<T>> func) => Task.FromResult(default(T));

            public void Commit()
            {
                if (ThrowOnCommit)
                {
                    throw new InvalidOperationException("COMMIT_FAILED");
                }

                Closed = true;
            }

            public void Rollback()
            {
                if (ThrowOnRollback)
                {
                    throw new InvalidOperationException("ROLLBACK_FAILED");
                }

                Closed = true;
            }

            public void Close()
            {
                Closed = true;
            }
        }
    }
}
