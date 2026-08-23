using Proto;
using Protocol;
using Protocol.Packet.Custom;
using WebStudyServer.Data.Queries;
using WebStudyServer.Model;

namespace WebStudyServer.Data
{
    // 킹덤 맵의 읽기·쓰기 진입점. 호출부가 보는 것은 여기뿐이다.
    //
    // 스냅샷은 KingdomMap.Snapshot 컬럼에 JSON 으로 들어 있다. 푼 것을 고친 뒤 다시 넣는
    // 세 단계를 한 메서드 안에 묶어두는 이유는, 호출부가 마지막 단계를 잊어도 컴파일이 되고
    // 조용히 저장이 안 되기 때문이다.
    //
    // 타일 계산 자체는 KingdomTileMap 이 한다. 이쪽은 로드와 저장만 안다.
    public static class KingdomMapService
    {
        public static async Task<KingdomMapSnapshotPacket> LoadSnapshotAsync(UserScope userScope)
        {
            var mdlMap = await userScope.Owned<KingdomMapModel>().GetOrCreateMapAsync();
            return LoadSnapshot(mdlMap);
        }

        // 배치할 수 있는 자리인지 본다. 쓰지 않는다.
        public static async Task<TilePosPacket> ValidEmptyTileAsync(
            UserScope userScope, TilePosPacket reqStartPos, KingdomItemProto prtKingdomItem)
        {
            var mdlMap = await userScope.Owned<KingdomMapModel>().GetOrCreateMapAsync();
            var snapshot = LoadSnapshot(mdlMap);
            return KingdomTileMap.ValidEmptyTile(snapshot, reqStartPos, prtKingdomItem, mdlMap.SizeX, mdlMap.SizeY);
        }

        // 아이템 하나를 놓고 저장한다. 바뀐 스냅샷을 돌려준다.
        public static async Task<KingdomMapSnapshotPacket> PlaceItemAsync(
            UserScope userScope, KingdomItemProto prtKingdomItem, TilePosPacket valStartTilePos, ulong structId)
        {
            var mapSet = userScope.Owned<KingdomMapModel>();
            var mdlMap = await mapSet.GetOrCreateMapAsync();
            var snapshot = LoadSnapshot(mdlMap);

            KingdomTileMap.PlaceItem(snapshot, prtKingdomItem, valStartTilePos, structId);

            await SaveSnapshotAsync(mapSet, mdlMap, snapshot);
            return snapshot;
        }

        // 보관/이동/배치를 한꺼번에 검증한다. 저장은 하지 않는다 -
        // 구조물·장식 수량 검증이 남아 있어서 호출부가 그것까지 통과한 뒤 SaveSnapshotAsync 를 부른다.
        // out 대신 튜플로 돌려준다. out 을 쓰면 async 를 못 써서 동기 블로킹이 된다.
        public static async Task<(KingdomMapSnapshotPacket Snapshot, Dictionary<ulong, int> StructureDeltaCntDict, Dictionary<int, int> DecoDeltaCntDict)>
            ValidPlaceItemsAsync(
                UserScope userScope,
                List<ulong> reqStoreIdList, List<ChgKingdomItemPacket> reqChgItemList, List<ChgKingdomItemPacket> reqPlaceItemList)
        {
            var mdlMap = await userScope.Owned<KingdomMapModel>().GetOrCreateMapAsync();
            var snapshot = LoadSnapshot(mdlMap);

            var valSnapshot = KingdomTileMap.SimulatePlaceItems(
                snapshot, reqStoreIdList, reqChgItemList, reqPlaceItemList, mdlMap.SizeX, mdlMap.SizeY,
                out var structureDeltaCntDict, out var decoDeltaCntDict);

            return (valSnapshot, structureDeltaCntDict, decoDeltaCntDict);
        }

        // 신규 플레이어의 첫 맵. 기본 데이터의 배치를 그대로 넣는다.
        public static async Task<(KingdomMapModel Mdl, KingdomMapSnapshotPacket Snapshot)> CreateInitialAsync(
            UserScope userScope, KingdomMapPacket pak, List<KingdomStructureModel> mdlStructureList)
        {
            var snapshot = KingdomTileMap.BuildInitialSnapshot(pak, mdlStructureList);
            var mdlMap = new KingdomMapModel
            {
                State = pak.State,
                SizeX = pak.SizeX,
                SizeY = pak.SizeY,
            };
            mdlMap.SetSnapshot(snapshot);

            mdlMap = await userScope.Owned<KingdomMapModel>().CreateAsync(mdlMap);
            return (mdlMap, snapshot);
        }

        public static async Task SaveSnapshotAsync(UserScope userScope, KingdomMapSnapshotPacket snapshot)
        {
            var mapSet = userScope.Owned<KingdomMapModel>();
            var mdlMap = await mapSet.GetOrCreateMapAsync();
            await SaveSnapshotAsync(mapSet, mdlMap, snapshot);
        }

        // 푼 스냅샷을 모델이 들고 있지 않으므로 여기서 항상 타일맵 크기를 맞춘다.
        private static KingdomMapSnapshotPacket LoadSnapshot(KingdomMapModel mdlMap)
        {
            var snapshot = mdlMap.ParseSnapshot();
            KingdomTileMap.FillTileMap(snapshot, mdlMap.SizeX, mdlMap.SizeY);
            return snapshot;
        }

        private static Task SaveSnapshotAsync(OwnedSet<KingdomMapModel> mapSet, KingdomMapModel mdlMap, KingdomMapSnapshotPacket snapshot)
        {
            mdlMap.SetSnapshot(snapshot);
            return mapSet.UpdateAsync(mdlMap);
        }
    }
}
