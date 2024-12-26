using WebStudyServer.Base;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Component
{
    public class AccountComponent : AuthComponentBase
    {
        public AccountComponent(AuthRepo authRepo) : base(authRepo)
        {
        }

        public AccountManager GetActiveAccount(ulong accountId)
        {
            ReqHelper.ValidContext(TryGetAccount(accountId, out var mgrAccount), "NOT_FOUND_ACCOUNT", ()=> new {AccountId = accountId});
            ReqHelper.ValidContext(mgrAccount.IsActive(), "NOT_ACTIVE_ACCOUNT", () => new { AccountId = accountId, State = mgrAccount.Model.State });
            return mgrAccount;
        }

        public bool TryGetAccount(ulong accountId, out AccountManager mgrAccount)
        {
            mgrAccount = null;

            if (!_authRepo.TryGetAccount(accountId, out var mdlAccount))
            {
                return false;
            }

            mgrAccount = new AccountManager(_authRepo, mdlAccount);
            return true;
        }

        public AccountManager CreateAccount()
        {
            var newAccount = new AccountModel
            {
                ShardId = 0, // TODO: ShardId
                State = Proto.EAccountState.ACTIVE,
                AdditionalPlayerCnt = 0,
                ClientSecret = ""
            };

            var repoAccount = _authRepo.CreateAccount(newAccount);
            var mgrAccount = new AccountManager(_authRepo, repoAccount);

            _authRepo.RpcContext.SetAccountId(mgrAccount.Id);
            _authRepo.RpcContext.SetShardId(mgrAccount.Model.ShardId);
            return mgrAccount;
        }
    }
}
