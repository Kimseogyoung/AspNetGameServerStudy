using ServerCore;
using ServerCore.Repo.Cache;
using WebStudyServer;

namespace Server.Service
{
    // Seq 기반 응답 재전송 전용 캐시. SessionKey+Seq로 성공한 응답 바이트를 캐싱해서,
    // 같은 요청이 재전송되면 재실행 없이 캐시된 바이트를 그대로 돌려준다.
    // SessionKey가 없는(로그인 전) 요청은 캐싱하지 않는다.
    //
    // ICacheSession은 요청 스코프(Scoped)이며 GlobalDbRepo와 같은 인스턴스를 공유한다.
    // 즉 Set()은 GlobalDbRepo.Commit() 시 flush, Rollback() 시 폐기되는 pending write라서
    // 성공한 응답만 캐시에 남고 실패한 요청은 재시도가 실제로 다시 실행된다.
    //
    // CacheType.InMemory(InMemoryCacheLayer)는 요청 스코프라 요청이 끝나면 그 안의 값이
    // 사라지므로, 재전송 캐싱 자체가 성립하지 않는다. 그래서 CacheType.Redis일 때만 동작시킨다.
    public class ResponseCacheService
    {
        public ResponseCacheService(ICacheSession cacheSession)
        {
            _cacheSession = cacheSession;
            _enabled = Config<CoreConfig>.Get().CacheType == CacheType.Redis;
        }

        public bool TryGet(RpcContext rpcCtx, out byte[] cachedBody)
        {
            if (!_enabled || string.IsNullOrEmpty(rpcCtx.SessionKey))
            {
                cachedBody = null;
                return false;
            }

            return _cacheSession.TryGet(MakeKey(rpcCtx), out cachedBody);
        }

        public void Set(RpcContext rpcCtx, byte[] body)
        {
            if (!_enabled || string.IsNullOrEmpty(rpcCtx.SessionKey))
            {
                return;
            }

            _cacheSession.Set(MakeKey(rpcCtx), body);
        }

        private static CacheKey MakeKey(RpcContext rpcCtx)
        {
            // For<T>()는 타입명을 리플렉션으로 붙이기 때문에, 타입 리네임 시 키가 조용히
            // 바뀌는 걸 피하려고 여기서는 명시적 리터럴로 키를 만든다.
            return new CacheKey($"RpcResponseCache:{rpcCtx.SessionKey}:{rpcCtx.Seq}");
        }

        private readonly ICacheSession _cacheSession;
        private readonly bool _enabled;
    }
}
