using Proto;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using ServerCore.Extension;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Component
{
    public class AccountComponent : AuthComponentBase
    {
        public AccountComponent(AuthRepo authRepo, IRepository repository) : base(authRepo, repository)
        {
        }

        public AccountManager GetActive(ulong accountId)
        {
            ReqHelper.ValidContext(TryGet(accountId, out var mgrAccount), "NOT_FOUND_ACCOUNT", () => new { AccountId = accountId });
            ReqHelper.ValidContext(mgrAccount.IsActive(), "NOT_ACTIVE_ACCOUNT", () => new { AccountId = accountId, mgrAccount.Model.State });
            return mgrAccount;
        }

        public bool TryGet(ulong id, out AccountManager outAccount)
        {
            var mdlAccount = GetMdl(db => db.SelectByPk<AccountModel>(new { Id = id }));
            outAccount = new AccountManager(_authRepo, mdlAccount);
            return mdlAccount != null;
        }

        public AccountManager Create()
        {
            var newAccount = new AccountModel
            {
                ShardId = 0, // TODO: ShardId
                State = EAccountState.ACTIVE,
                AdditionalPlayerCnt = 0,
                ClientSecret = ""
            };

            var repoAccount = CreateMdl(newAccount);
            var mgrAccount = new AccountManager(_authRepo, repoAccount);

            _authRepo.RpcContext.SetAccountId(mgrAccount.Id);
            _authRepo.RpcContext.SetShardId(mgrAccount.Model.ShardId);
            return mgrAccount;
        }

        public void UpdateAccount(AccountModel mdlAccount)
        {
            UpdateMdl(mdlAccount);
        }
    }
}
