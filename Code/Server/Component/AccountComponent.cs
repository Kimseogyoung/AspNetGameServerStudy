using Proto;
using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Component
{
    public class AccountComponent : AuthComponentBase
    {
        public static class Key
        {
            public static CacheKey Single(ulong id) => CacheKey.For<AccountModel>(id);
        }

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
            var mdlAccount = GetMdl(Key.Single(id), db => db.SelectByPk<AccountModel>(new { Id = id }));
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

            var repoAccount = CreateMdl(newAccount, e => Key.Single(e.Id));
            var mgrAccount = new AccountManager(_authRepo, repoAccount);

            _authRepo.RpcContext.SetAccountId(mgrAccount.Id);
            _authRepo.RpcContext.SetShardId(mgrAccount.Model.ShardId);
            return mgrAccount;
        }

        public void UpdateAccount(AccountModel mdlAccount)
        {
            UpdateMdl(mdlAccount, Key.Single(mdlAccount.Id));
        }
    }
}
