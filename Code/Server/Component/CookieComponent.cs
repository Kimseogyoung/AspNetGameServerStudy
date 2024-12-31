using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;
using WebStudyServer.Extension;

namespace WebStudyServer.Component
{
    public class CookieComponent : UserComponentBase<CookieModel>
    {
        public CookieComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {
        }

        public CookieManager Touch(int cookieNum)
        {
            if (!TryGetInternal(cookieNum, out var mdlCookie))
            {
                mdlCookie = Create(new CookieModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = cookieNum,
                });
            }

            var mgrPoint = new CookieManager(_userRepo, mdlCookie);
            return mgrPoint;
        }

        public bool TryGetInternal(int num, out CookieModel outCookie)
        {
            CookieModel mdlCookie = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlCookie = sqlConnection.SelectByPk<CookieModel>(new { PlayerId = _userRepo.RpcContext.PlayerId, Num = num }, transaction);
            });

            outCookie = mdlCookie;
            return outCookie != null;
        }
    }
}
