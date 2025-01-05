using System.Drawing;
using WebStudyServer.Extension;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public class UserComponentBase<T> where T :ModelBase
    {
        protected DBSqlExecutor _executor;
        protected UserRepo _userRepo;
        protected RpcContext _rpcContext => _userRepo.RpcContext;

        public UserComponentBase(UserRepo userRepo, DBSqlExecutor executor)
        {
            _userRepo = userRepo;
            _executor = executor;
        }

        public T CreateMdl(T newValue)
        {
            // 데이터베이스에 삽입
            T mdl = null;
            newValue.UpdateTime = DateTime.UtcNow;
            newValue.CreateTime = DateTime.UtcNow;
            _executor.Excute((sqlConnection, transaction) =>
            {
                mdl = sqlConnection.Insert<T>(newValue, transaction);
            });

            return mdl; // 새로 생성된 모델 반환
        }

        public void UpdateMdl(T mdl)
        {
            mdl.UpdateTime = DateTime.UtcNow;
            _executor.Excute((sqlConnection, transaction) =>
            {
                sqlConnection.Update(mdl, transaction);
            });
        }

        public List<T> GetMdlList()
        {
            List<T> mdlList= null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlList = sqlConnection.SelectListByPlayerId<T>(_userRepo.RpcContext.PlayerId, transaction).ToList();
            });

            return mdlList;
        }
    }
}
