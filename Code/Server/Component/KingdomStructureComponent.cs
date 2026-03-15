using Proto;
using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.GAME;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;

namespace WebStudyServer.Component
{
    public class KingdomStructureComponent : UserComponentBase<KingdomStructureModel>
    {
        public static class Key
        {
            public static CacheKey Single(ulong playerId, ulong sfId) => CacheKey.For<KingdomStructureModel>(playerId, sfId);
            public static CacheKey List(ulong playerId) => CacheKey.For<KingdomStructureModel>(playerId);
        }

        public KingdomStructureComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(KingdomStructureModel model) => Key.Single(model.PlayerId, model.SfId);
        protected override CacheKey ListKeyFor(ulong playerId) => Key.List(playerId);

        public int GetKingdomStructureCnt(int num)
        {
            var mdlList = GetMdlList(x => num == x.Num);
            return mdlList.Count;
        }

        public KingdomStructureManager Create(KingdomItemProto prt)
        {
            var mdlKingdomStructure = CreateMdl(new KingdomStructureModel
            {
                SfId = IdHelper.GenerateSfId(),
                Num = prt.Num,
                State = EKingdomItemState.STORED,
                PlayerId = _userRepo.RpcContext.PlayerId,
            });

            return new KingdomStructureManager(_userRepo, mdlKingdomStructure, prt);
        }

        public KingdomStructureManager Get(ulong sfId)
        {
            ReqHelper.ValidContext(TryGetInternal(sfId, out var mdlKingdomStructure),
                "NOT_FOUND_KINGDOM_ITEM", () => new { SfId = sfId });
            return new KingdomStructureManager(_userRepo, mdlKingdomStructure);
        }

        public List<KingdomStructureManager> GetAllList(List<ulong> sfIdList)
        {
            if (sfIdList.Count == 0)
            {
                return [];
            }

            var mdlList = GetMdlList(x => sfIdList.Contains(x.SfId));
            ReqHelper.ValidContext(mdlList.Count != sfIdList.Count, "NOT_EQUAL_KINGDOM_ITEM_LIST",
                () => new { SfIdList = sfIdList, MdlIdList = mdlList.Select(x => x.SfId) });
            return [.. mdlList.Select(x => new KingdomStructureManager(_userRepo, x))];
        }

        private bool TryGetInternal(ulong sfId, out KingdomStructureModel outKingdomStructure)
        {
            var kingdomStructure = GetMdl(Key.Single(RpcCtx.PlayerId, sfId), db => db.SelectByPk<KingdomStructureModel>(new { SfId = sfId }));

            outKingdomStructure = kingdomStructure;
            return outKingdomStructure != null;
        }
    }
}
