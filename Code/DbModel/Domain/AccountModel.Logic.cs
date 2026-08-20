using Proto;
using WebStudyServer.Helper;

namespace WebStudyServer.Model
{
    public partial class AccountModel
    {
        public bool IsActive()
        {
            return State >= EAccountState.NONE;
        }

        // 조회 실패(NOT_FOUND_ACCOUNT)는 AuthScope가 던짐
        public AccountModel EnsureActive()
        {
            ReqHelper.ValidContext(IsActive(), "NOT_ACTIVE_ACCOUNT", () => new { AccountId = Id, State });
            return this;
        }
    }
}
