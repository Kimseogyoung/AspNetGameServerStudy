using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Helper;
using Server.Repo.Database;
using Proto;

namespace WebStudyServer.Component
{
    public class KingdomDecoComponent : UserComponentBase<KingdomDecoModel>
    {
        public static class Key
        {
            public static CacheKey Single(ulong playerId, int num) => CacheKey.For<KingdomDecoModel>(playerId, playerId, num);
            public static CacheKey List(ulong playerId) => CacheKey.ListFor<KingdomDecoModel>(playerId);
        }

        public KingdomDecoComponent(UserRepo userRepo, IDbLayer db) : base(userRepo, db) { }

        protected override CacheKey KeyFor(KingdomDecoModel model) => Key.Single(model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => Key.List(playerId);

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
                return new List<KingdomDecoManager>();
            }

            var mdlList = GetMdlList(x => numList.Contains(x.Num));
            ReqHelper.ValidContext(mdlList.Count != numList.Count, "NOT_EQUAL_KINGDOM_ITEM_LIST",
                () => new { NumList = numList, MdlNumList = mdlList.Select(x => x.Num) });
            return mdlList.Select(x => new KingdomDecoManager(_userRepo, x)).ToList();
        }

        private bool TryGetInternal(int num, out KingdomDecoModel outKingdomDeco)
        {
            outKingdomDeco = GetMdl(
                Key.Single(_rpcContext.PlayerId, num),
                db => db.SelectByPk<KingdomDecoModel>(new { PlayerId = _rpcContext.PlayerId, Num = num }));
            return outKingdomDeco != null;
        }
    }
}
