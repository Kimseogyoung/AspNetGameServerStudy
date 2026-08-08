using Proto;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Component
{
    public class KingdomDecoComponent : UserComponentBase<KingdomDecoModel>
    {
        public KingdomDecoComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(KingdomDecoModel model) => CacheKey.For(CacheKeyTags.KingdomDecoModel, model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For(CacheKeyTags.KingdomDecoModel, playerId);

        public async Task<KingdomDecoManager> TouchAsync(int itemNum)
        {
            var mdlDeco = await TryGetInternalAsync(itemNum);
            if (mdlDeco == null)
            {
                mdlDeco = await CreateMdlAsync(new KingdomDecoModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = itemNum,
                });
            }

            return new KingdomDecoManager(_userRepo, mdlDeco);
        }

        public async Task<KingdomDecoManager> CreateAsync(KingdomItemProto prt)
        {
            var mdlKingdomDeco = await CreateMdlAsync(new KingdomDecoModel
            {
                Num = prt.Num,
                State = EKingdomItemState.STORED,
                PlayerId = _userRepo.RpcContext.PlayerId,
            });

            return new KingdomDecoManager(_userRepo, mdlKingdomDeco, prt);
        }

        public async Task<List<KingdomDecoManager>> GetAllListAsync(List<int> numList)
        {
            if (numList.Count == 0)
            {
                return [];
            }

            var mdlList = await GetMdlListAsync(x => numList.Contains(x.Num));
            ReqHelper.ValidContext(mdlList.Count == numList.Count, "NOT_EQUAL_KINGDOM_ITEM_LIST",
                () => new { NumList = numList, MdlNumList = mdlList.Select(x => x.Num) });
            return [.. mdlList.Select(x => new KingdomDecoManager(_userRepo, x))];
        }

        private Task<KingdomDecoModel?> TryGetInternalAsync(int num)
        {
            return GetMdlAsync(x => x.PlayerId == RpcCtx.PlayerId && x.Num == num);
        }
    }
}
