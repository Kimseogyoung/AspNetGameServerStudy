using Proto;
using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;

namespace WebStudyServer.Component
{
    public class PointComponent : UserComponentBase<PointModel>
    {
        public static class Key
        {
            public static CacheKey Single(ulong playerId, int num) => CacheKey.For<PointModel>(playerId, num);
            public static CacheKey List(ulong playerId) => CacheKey.For<PointModel>(playerId);
        }

        public PointComponent(UserRepo userRepo, IDbLayer db) : base(userRepo, db) { }

        protected override CacheKey KeyFor(PointModel model) => Key.Single(model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => Key.List(playerId);

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
            outPoint = GetMdl(
                Key.Single(RpcCtx.PlayerId, num),
                db => db.SelectByPk<PointModel>(new { RpcCtx.PlayerId, Num = num }));
            return outPoint != null;
        }
    }
}
