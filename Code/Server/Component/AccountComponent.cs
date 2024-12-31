using Proto;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Component
{
    public class AccountComponent : AuthComponentBase
    {
        public AccountComponent(AuthRepo authRepo, DBSqlExecutor executor) : base(authRepo, executor)
        {
        }

        public AccountManager GetActive(ulong accountId)
        {
            ReqHelper.ValidContext(TryGet(accountId, out var mgrAccount), "NOT_FOUND_ACCOUNT", ()=> new {AccountId = accountId});
            ReqHelper.ValidContext(mgrAccount.IsActive(), "NOT_ACTIVE_ACCOUNT", () => new { AccountId = accountId, State = mgrAccount.Model.State });
            return mgrAccount;
        }

        public bool TryGet(ulong id, out AccountManager outAccount)
        {
            AccountModel mdlAccount = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlAccount = sqlConnection.SelectByPk<AccountModel>(new { Id = id }, transaction);
            });

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

            var repoAccount = CreateAccountInternal(newAccount);
            var mgrAccount = new AccountManager(_authRepo, repoAccount);

            _authRepo.RpcContext.SetAccountId(mgrAccount.Id);
            _authRepo.RpcContext.SetShardId(mgrAccount.Model.ShardId);
            return mgrAccount;
        }

        public void UpdateAccount(AccountModel mdlAccount)
        {
            _executor.Excute((sqlConnection, transaction) =>
            {
                sqlConnection.Update(mdlAccount, transaction);
            });
        }

        private AccountModel CreateAccountInternal(AccountModel newAccount)
        {
            // 데이터베이스에 삽입
            _executor.Excute((sqlConnection, transaction) =>
            {
                newAccount = sqlConnection.Insert<AccountModel>(newAccount, transaction);
            });

            return newAccount; // 새로 생성된 계정 모델 반환
        }
    }
}
