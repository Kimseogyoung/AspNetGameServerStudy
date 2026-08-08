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

        protected override CacheKey KeyFor(PointModel model) => CacheKey.For(CacheKeyTags.PointModel, model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For(CacheKeyTags.PointModel, playerId);

        public async Task<PointManager> TouchAsync(EObjType objType)
        {
            var pointNum = (int)objType;

            var mdlPoint = await TryGetInternalAsync(pointNum);
            if (mdlPoint == null)
            {
                mdlPoint = await CreateMdlAsync(new PointModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = pointNum,
                });
            }

            return new PointManager(_userRepo, mdlPoint);
        }

        public Task<PointModel?> TryGetInternalAsync(int num)
        {
            return GetMdlAsync(x => x.PlayerId == RpcCtx.PlayerId && x.Num == num);
        }
    }
}
