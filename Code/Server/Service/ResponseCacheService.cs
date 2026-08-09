using ServerCore;
using ServerCore.Repo.Cache;
using WebStudyServer;

namespace Server.Service
{
    // Seq 기반 응답 재전송 전용 캐시. SessionKey+Seq로 성공한 응답 객체를 캐싱해서,
    // 같은 요청이 재전송되면 재실행 없이 캐시된 객체를 그대로 돌려준다.
    // SessionKey가 없는(로그인 전) 요청은 캐싱하지 않는다.
    public class ResponseCacheService
    {
        public ResponseCacheService(ICacheSession cacheSession)
        {
            _cacheSession = cacheSession;
            _enabled = Config<CoreConfig>.Get().CacheType == CacheType.Redis;
        }

        public async Task<(bool Hit, object Body)> TryGetAsync(RpcContext rpcCtx, Type responseType)
        {
            if (!_enabled || string.IsNullOrEmpty(rpcCtx.SessionKey))
            {
                return (false, null);
            }

            var result = await _cacheSession.TryGetAsync(MakeKey(rpcCtx), responseType);
            return (result.Hit, result.Value);
        }

        public async Task SetAsync(RpcContext rpcCtx, object resObj)
        {
            if (!_enabled || string.IsNullOrEmpty(rpcCtx.SessionKey))
            {
                return;
            }

            await _cacheSession.SetAsync(MakeKey(rpcCtx), resObj);
        }

        private static CacheKey MakeKey(RpcContext rpcCtx)
        {
            return CacheKey.For(CacheKeyTags.RpcResponseCache, rpcCtx.SessionKey, rpcCtx.Seq);
        }

        private readonly ICacheSession _cacheSession;
        private readonly bool _enabled;
    }
}
