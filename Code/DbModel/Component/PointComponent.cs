using Proto;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Component
{
    public class PointComponent : UserComponentBase<PointModel>
    {
        public PointComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(PointModel model) => CacheKey.For<PointModel>(model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For<PointModel>(playerId);

        public PointManager Touch(EObjType objType)
        {
            var pointNum = (int)objType;

            if (!TryGetInternal(pointNum, out var mdlPoint))
            {
                mdlPoint = CreateMdl(new PointModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = pointNum,
                });
            }

            return new PointManager(_userRepo, mdlPoint);
        }

        public bool TryGetInternal(int num, out PointModel outPoint)
        {
            outPoint = GetMdl(x => x.PlayerId == RpcCtx.PlayerId && x.Num == num);
            return outPoint != null;
        }
    }
}
