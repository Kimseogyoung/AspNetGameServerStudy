using Proto;
using ServerCore.Helper;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Component
{
    public class KingdomStructureComponent : UserComponentBase<KingdomStructureModel>
    {
        public KingdomStructureComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(KingdomStructureModel model) => CacheKey.For(CacheKeyTags.KingdomStructureModel, model.PlayerId, model.SfId);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For(CacheKeyTags.KingdomStructureModel, playerId);

        public async Task<int> GetKingdomStructureCntAsync(int num)
        {
            var mdlList = await GetMdlListAsync(x => num == x.Num);
            return mdlList.Count;
        }

        public async Task<KingdomStructureManager> CreateAsync(KingdomItemProto prt)
        {
            var mdlKingdomStructure = await CreateMdlAsync(new KingdomStructureModel
            {
                SfId = IdHelper.GenerateSfId(),
                Num = prt.Num,
                State = EKingdomItemState.STORED,
                PlayerId = _userRepo.RpcContext.PlayerId,
            });

            return new KingdomStructureManager(_userRepo, mdlKingdomStructure, prt);
        }

        public async Task<KingdomStructureManager> GetAsync(ulong sfId)
        {
            var mdlKingdomStructure = await TryGetInternalAsync(sfId);
            ReqHelper.ValidContext(mdlKingdomStructure != null, "NOT_FOUND_KINGDOM_ITEM", () => new { SfId = sfId });
            return new KingdomStructureManager(_userRepo, mdlKingdomStructure);
        }

        public async Task<List<KingdomStructureManager>> GetAllListAsync(List<ulong> sfIdList)
        {
            if (sfIdList.Count == 0)
            {
                return [];
            }

            var mdlList = await GetMdlListAsync(x => sfIdList.Contains(x.SfId));
            ReqHelper.ValidContext(mdlList.Count == sfIdList.Count, "NOT_EQUAL_KINGDOM_ITEM_LIST",
                () => new { SfIdList = sfIdList, MdlIdList = mdlList.Select(x => x.SfId) });
            return [.. mdlList.Select(x => new KingdomStructureManager(_userRepo, x))];
        }

        private Task<KingdomStructureModel?> TryGetInternalAsync(ulong sfId)
        {
            return GetMdlAsync(x => x.SfId == sfId);
        }
    }
}
