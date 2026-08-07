using WebStudyServer;

namespace Server
{
    // RpcMethod가 실행 전에 적용하는 인증 정책. 새 정책이 필요하면 이 인터페이스의
    // 구현체를 추가하면 되고, RpcMethod.cs 자체는 다시 건드리지 않아도 된다.
    public interface IRpcAuthPolicy
    {
        // 검증만 한다. 실패 시 GameException을 던진다. 부수효과(DB 리포 오픈 등)는 없다.
        void Validate(RpcContext rpcCtx);

        // 검증 통과 후 유저 개인 DB 리포(GlobalDbRepo.OwnUser)를 열어야 하는 정책인지.
        bool RequiresUserRepo { get; }
    }
}
