using Proto;
using ServerCore.Serializer;
using WebStudyServer.Helper;
using WebStudyServer.Model;

namespace WebStudyServer.Data.Queries
{
    public static class KingdomQueries
    {
        // ── KingdomStructure. PK 가 SfId 라 스코프가 안 정해준다. 호출부가 만들어 넣는다 ──
        public static Task<KingdomStructureModel> CreateStructureAsync(this OwnedSet<KingdomStructureModel> set, ulong sfId, int num)
        {
            return set.CreateAsync(new KingdomStructureModel
            {
                SfId = sfId,
                Num = num,
                State = EKingdomItemState.STORED,
            });
        }

        public static async Task<KingdomStructureModel> GetStructureAsync(this OwnedSet<KingdomStructureModel> set, ulong sfId)
        {
            var (found, mdlStructure) = await set.TryGetAsync(x => x.SfId == sfId);
            ReqHelper.ValidContext(found, "NOT_FOUND_KINGDOM_ITEM", () => new { SfId = sfId });
            return mdlStructure;
        }

        public static async Task<int> GetStructureCntAsync(this OwnedSet<KingdomStructureModel> set, int num)
        {
            var list = await set.GetListAsync();
            return list.Count(x => x.Num == num);
        }

        // 요청한 것이 전부 있어야 한다. 하나라도 없으면 요청 자체가 잘못된 것이다.
        public static async Task<List<KingdomStructureModel>> GetStructureListAsync(this OwnedSet<KingdomStructureModel> set, List<ulong> sfIdList)
        {
            if (sfIdList.Count == 0)
            {
                return [];
            }

            var list = await set.GetListAsync();
            var mdlList = list.Where(x => sfIdList.Contains(x.SfId)).ToList();
            ReqHelper.ValidContext(mdlList.Count == sfIdList.Count, "NOT_EQUAL_KINGDOM_ITEM_LIST",
                () => new { SfIdList = sfIdList, MdlIdList = mdlList.Select(x => x.SfId) });
            return mdlList;
        }

        // ── KingdomDeco. PK 가 (PlayerId, Num) ──
        public static async Task<KingdomDecoModel> GetOrCreateDecoAsync(this OwnedSet<KingdomDecoModel> set, int num)
        {
            var (found, mdlDeco) = await set.TryGetAsync(x => x.Num == num);
            return found ? mdlDeco : await set.CreateAsync(new KingdomDecoModel { Num = num });
        }

        public static async Task<List<KingdomDecoModel>> GetDecoListAsync(this OwnedSet<KingdomDecoModel> set, List<int> numList)
        {
            if (numList.Count == 0)
            {
                return [];
            }

            var list = await set.GetListAsync();
            var mdlList = list.Where(x => numList.Contains(x.Num)).ToList();
            ReqHelper.ValidContext(mdlList.Count == numList.Count, "NOT_EQUAL_KINGDOM_ITEM_LIST",
                () => new { NumList = numList, MdlNumList = mdlList.Select(x => x.Num) });
            return mdlList;
        }

        // ── KingdomMap. 플레이어당 한 행 ──
        public static async Task<KingdomMapModel> GetOrCreateMapAsync(this OwnedSet<KingdomMapModel> set)
        {
            var list = await set.GetListAsync();
            if (list.Count > 0)
            {
                return list[0];
            }

            return await set.CreateAsync(new KingdomMapModel
            {
                Snapshot = "",
                State = EKingdomTileMapState.NONE,
            });
        }
    }
}
