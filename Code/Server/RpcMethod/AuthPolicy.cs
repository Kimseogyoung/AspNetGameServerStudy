using Proto;
using ServerCore;
using WebStudyServer;
using WebStudyServer.Helper;

namespace Server
{
    // 계정 인증 필요 (세션 로드 + AccountId 확인)
    public class AccountAuthPolicy : IRpcAuthPolicy
    {
        public static readonly AccountAuthPolicy Instance = new();

        public bool RequiresUserRepo => true;

        public void Validate(RpcContext rpcCtx)
        {
            ValidateSession(rpcCtx);
            ReqHelper.Valid(rpcCtx.AccountId != 0, EErrorCode.CONTEXT_ACCOUNT, () => new { rpcCtx.SessionKey });
        }

        private static void ValidateSession(RpcContext rpcCtx)
        {
            switch (rpcCtx.SessionLoadState)
            {
                case RpcContext.ESessionLoadState.LOADED:
                    return;
                case RpcContext.ESessionLoadState.NOT_FOUND:
                    throw new GameException(EErrorCode.SESSION_NOT_FOUND, "SESSION_NOT_FOUND",
                        new { rpcCtx.SessionKey });
                case RpcContext.ESessionLoadState.EXPIRED:
                    throw new GameException(EErrorCode.SESSION_EXPIRED, "SESSION_EXPIRED",
                        new { rpcCtx.SessionKey });
                default:
                    throw new GameException(EErrorCode.CONTEXT, "FAILED_SESSION_LOAD",
                        new { rpcCtx.SessionKey, rpcCtx.SessionLoadState });
            }
        }
    }

    // 계정 인증 + 플레이어 존재 필요 (AccountAuthPolicy를 합성해서 재사용)
    public class PlayerAuthPolicy : IRpcAuthPolicy
    {
        public static readonly PlayerAuthPolicy Instance = new();

        public bool RequiresUserRepo => true;

        public void Validate(RpcContext rpcCtx)
        {
            AccountAuthPolicy.Instance.Validate(rpcCtx);
            ReqHelper.Valid(rpcCtx.PlayerId != 0, EErrorCode.CONTEXT_PLAYER, () => new { rpcCtx.SessionKey, rpcCtx.AccountId });
        }
    }

    // 운영자 전용 (지금은 빈 정책 — 추후 IP 화이트리스트 등을 여기 추가할 자리)
    public class OpsAuthPolicy : IRpcAuthPolicy
    {
        public static readonly OpsAuthPolicy Instance = new();

        public bool RequiresUserRepo => false;

        public void Validate(RpcContext rpcCtx) { }
    }
}
