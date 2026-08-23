using WebStudyServer.Data;

namespace WebStudyServer
{
    public class ServiceBase
    {
        protected RpcContext RpcContext { get; private set; }
        protected ILogger Logger { get; private set; }
        protected GameDb Db { get; private set; }

        // 요청 주체의 스코프. 계산 프로퍼티인 이유는 PlayerId가 요청 도중 정해지기 때문이다
        // (신규 플레이어는 Player.TouchAsync 안에서 정해진다). 미리 담아두면 0인 스코프가 된다.
        protected UserScope OwnScope => Db.User(RpcContext.ShardId, RpcContext.PlayerId);

        // 세션에 찍히는 요청 값. 데이터 계층이 컨텍스트를 직접 읽지 않게 값으로 넘긴다.
        protected SessionStamp Stamp => new(RpcContext.ServerTime, RpcContext.Ip, RpcContext.DeviceKey);

        public ServiceBase(GameDb db, RpcContext rpcContext, ILogger logger)
        {
            Db = db;
            RpcContext = rpcContext;
            Logger = logger;
        }
    }
}
