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

        public async Task<AccountManager> GetActiveAsync(ulong accountId)
        {
            var (found, mgrAccount) = await TryGetAsync(accountId);
            ReqHelper.ValidContext(found, "NOT_FOUND_ACCOUNT", () => new { AccountId = accountId });
            ReqHelper.ValidContext(mgrAccount.IsActive(), "NOT_ACTIVE_ACCOUNT", () => new { AccountId = accountId, mgrAccount.Model.State });
            return mgrAccount;
        }

        public async Task<(bool Found, AccountManager? Value)> TryGetAsync(ulong id)
        {
            var mdlAccount = await GetMdlAsync(db => db.SelectByPk<AccountModel>(new { Id = id }));
            return mdlAccount == null ? (false, null) : (true, new AccountManager(_authRepo, mdlAccount));
        }

        public async Task<AccountManager> CreateAsync()
        {
            var newAccount = new AccountModel
            {
                ShardId = 0, // TODO: ShardId
                State = EAccountState.ACTIVE,
                AdditionalPlayerCnt = 0,
                ClientSecret = ""
            };

            var repoAccount = await CreateMdlAsync(newAccount);
            var mgrAccount = new AccountManager(_authRepo, repoAccount);

            _authRepo.RpcContext.SetAccountId(mgrAccount.Id);
            _authRepo.RpcContext.SetShardId(mgrAccount.Model.ShardId);
            return mgrAccount;
        }

        public Task UpdateAccountAsync(AccountModel mdlAccount)
        {
            return UpdateMdlAsync(mdlAccount);
        }
    }
}
