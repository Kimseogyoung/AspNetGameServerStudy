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

        public UserComponentBase(UserRepo userRepo, DBSqlExecutor executor)
        {
            _userRepo = userRepo;
            _executor = executor;
        }

        public T Create(T newValue)
        {
            // 데이터베이스에 삽입
            T mdl = null;
            _executor.Excute((sqlConnection, transaction) =>
            {
                mdl = sqlConnection.Insert<T>(newValue, transaction);
            });

            return mdl; // 새로 생성된 모델 반환
        }

        public void Update(T mdl)
        {
            _executor.Excute((sqlConnection, transaction) =>
            {
                sqlConnection.Update(mdl, transaction);
            });
        }
    }
}
