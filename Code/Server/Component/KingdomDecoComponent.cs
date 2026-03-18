using Proto;
using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;

namespace WebStudyServer.Component
{
    public class KingdomDecoComponent : UserComponentBase<KingdomDecoModel>
    {
        public KingdomDecoComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(KingdomDecoModel model) => CacheKey.For<KingdomDecoModel>(model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For<KingdomDecoModel>(playerId);

        public KingdomDecoManager Touch(int itemNum)
        {
            if (!TryGetInternal(itemNum, out var mdlDeco))
            {
                mdlDeco = CreateMdl(new KingdomDecoModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = itemNum,
                });
            }

            return new KingdomDecoManager(_userRepo, mdlDeco);
        }

        public KingdomDecoManager Create(KingdomItemProto prt)
        {
            var mdlKingdomDeco = CreateMdl(new KingdomDecoModel
            {
                Num = prt.Num,
                State = EKingdomItemState.STORED,
                PlayerId = _userRepo.RpcContext.PlayerId,
            });

            return new KingdomDecoManager(_userRepo, mdlKingdomDeco, prt);
        }

        public List<KingdomDecoManager> GetAllList(List<int> numList)
        {
            if (numList.Count == 0)
            {
                return [];
            }

            var mdlList = GetMdlList(x => numList.Contains(x.Num));
            ReqHelper.ValidContext(mdlList.Count != numList.Count, "NOT_EQUAL_KINGDOM_ITEM_LIST",
                () => new { NumList = numList, MdlNumList = mdlList.Select(x => x.Num) });
            return [.. mdlList.Select(x => new KingdomDecoManager(_userRepo, x))];
        }

        private bool TryGetInternal(int num, out KingdomDecoModel outKingdomDeco)
        {
            outKingdomDeco = GetMdl(x => x.PlayerId == RpcCtx.PlayerId && x.Num == num);
            return outKingdomDeco != null;
        }
    }
}
