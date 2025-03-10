using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;
using WebStudyServer.Extension;
using WebStudyServer.GAME;

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
                mdlCookie = CreateMdl(new CookieModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = cookieNum,
                    Lv = DEF.DEFAULT_LV,
                    SkillLv = DEF.DEFAULT_LV,
                });
            }

            var mgrPoint = new CookieManager(_userRepo, mdlCookie);
            return mgrPoint;
        }

        public CookieManager TouchBySoulStone(int soulStoneNum)
        {
            var prt = APP.Prt.GetCookieSoulStonePrt(soulStoneNum);
            var mgrCookie = Touch(prt.CookieNum);
            return mgrCookie;
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
