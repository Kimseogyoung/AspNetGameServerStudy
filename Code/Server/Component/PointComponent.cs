using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;
using WebStudyServer.Extension;
using Proto;

namespace WebStudyServer.Component
{
    public class PointComponent : UserComponentBase<PointModel>
    {
        public PointComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {

        }

        public PointManager Touch(EObjType objType)
        {
            var pointNum = (int)objType;

            if (!TryGetInternal(pointNum, out var mdlPoint))
            {
                mdlPoint = Create(new PointModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = pointNum,
                });
            }

            var mgrPoint = new PointManager(_userRepo, mdlPoint);
            return mgrPoint;
        }

        public bool TryGetInternal(int num, out PointModel outPoint)
        {
            PointModel mdlPoint = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlPoint = sqlConnection.SelectByPk<PointModel>(new { PlayerId = _userRepo.RpcContext.PlayerId, Num = num }, transaction);
            });

            outPoint = mdlPoint;
            return outPoint != null;
        }
    }
}
