using Proto;
using Proto.Helper;
using Protocol;
using Protocol.Packet.Custom;
using WebStudyServer.Helper;

namespace WebStudyServer.Model
{
    // 타일맵 계산. DB·스코프·컨텍스트를 모르고 스냅샷만 다룬다.
    //
    // KingdomMapService 만 부른다. 저장은 그쪽 일이고 여기는 판정과 배치 시뮬레이션만 한다.
    // 이 저장소에서 DB 없이 검증할 수 있는 유일한 덩어리라 따로 두었다.
    //
    // TileMap 은 [y][x] 다. FillTileMap 이 세로(y)를 바깥, 가로(x)를 안쪽으로 채운다.
    // 옛 ValidEmptyTile 만 [x][y] 로 읽어서 전치된 칸을 검사했고, 그 탓에 실제 위치가
    // 차 있어도 거울 위치가 비어 있으면 배치가 통과했다.
    internal static class KingdomTileMap
    {
        public static TilePosPacket ValidEmptyTile(
            KingdomMapSnapshotPacket snapshot, TilePosPacket reqStartPos, KingdomItemProto prtKingdomItem, int sizeX, int sizeY)
        {
            var tilePosRanges = KingdomHelper.GetTilePosRanges(reqStartPos.X, reqStartPos.Y, prtKingdomItem.SizeX, prtKingdomItem.SizeY);
            foreach (var tilePos in tilePosRanges)
            {
                if (tilePos.X < 0 || tilePos.Y < 0 || tilePos.X >= sizeX || tilePos.Y >= sizeY)
                {
                    ReqHelper.ValidParam(false, "OUT_OF_KINGDOM_MAP", () => new { ReqX = tilePos.X, ReqY = tilePos.Y, SizeX = sizeX, SizeY = sizeY });
                }

                var tileObjId = snapshot.TileMap[tilePos.Y][tilePos.X];
                ReqHelper.ValidContext(tileObjId == 0, "NOT_EMPTY_TILE", () => new { ReqX = tilePos.X, ReqY = tilePos.Y, ObjId = tileObjId });
            }

            return reqStartPos;
        }

        // 아이템 하나를 배치한다. 빈 칸인지는 호출부가 ValidEmptyTile 로 먼저 본다.
        public static void PlaceItem(
            KingdomMapSnapshotPacket snapshot, KingdomItemProto prtKingdomItem, TilePosPacket valStartTilePos, ulong structId)
        {
            var newPlacedObj = MakePlacedObj(++snapshot.ObjIdCounter, prtKingdomItem, valStartTilePos, structId);

            var tilePosRanges = KingdomHelper.GetTilePosRanges(valStartTilePos.X, valStartTilePos.Y, prtKingdomItem.SizeX, prtKingdomItem.SizeY);
            foreach (var tilePos in tilePosRanges)
            {
                snapshot.TileMap[tilePos.Y][tilePos.X] = newPlacedObj.Id;
            }

            snapshot.PlacedObjDict.Add(newPlacedObj.Id, newPlacedObj);
        }

        // 보관/이동/배치를 복제 맵에서 한꺼번에 돌려보고, 전부 유효하면 그 스냅샷을 돌려준다.
        // 원본을 안 건드리므로 중간에 던져도 상태가 반쯤 바뀌지 않는다.
        public static KingdomMapSnapshotPacket SimulatePlaceItems(
            KingdomMapSnapshotPacket snapshot,
            List<ulong> reqStoreIdList, List<ChgKingdomItemPacket> reqChgItemList, List<ChgKingdomItemPacket> reqPlaceItemList,
            int sizeX, int sizeY,
            out Dictionary<ulong, int> structureDeltaCntDict, out Dictionary<int, int> decoDeltaCntDict)
        {
            structureDeltaCntDict = [];
            decoDeltaCntDict = [];

            var copySnapshot = snapshot.DeepCopy();

            // 보관/이동시킬 것을 먼저 치운다
            var deleteItemIdList = new List<ulong>(reqStoreIdList);
            deleteItemIdList.AddRange(reqChgItemList.Select(x => x.PlacedItemId));

            foreach (var deletedPlaceItemId in deleteItemIdList)
            {
                ReqHelper.ValidContext(copySnapshot.PlacedObjDict.TryGetValue(deletedPlaceItemId, out var placedItem),
                    "NOT_FOUND_PLACED_KINGDOM_ITEM", () => new { PlaceItemId = deletedPlaceItemId });

                if (reqStoreIdList.Contains(deletedPlaceItemId))
                {
                    switch (placedItem.Type)
                    {
                        case EKingdomItemType.STRUCTURE:
                            structureDeltaCntDict.TryAdd(placedItem.StructureItemId, 0);
                            structureDeltaCntDict[placedItem.StructureItemId]++;
                            break;
                        case EKingdomItemType.DECO:
                            decoDeltaCntDict.TryAdd(placedItem.Num, 0);
                            decoDeltaCntDict[placedItem.Num]++;
                            break;
                    }
                }

                copySnapshot.PlacedObjDict.Remove(deletedPlaceItemId);
                foreach (var tilePos in KingdomHelper.GetTilePosRanges(placedItem.StartTileX, placedItem.StartTileY, placedItem.SizeX, placedItem.SizeY))
                {
                    copySnapshot.TileMap[tilePos.Y][tilePos.X] = 0;
                }
            }

            // 새로 놓을 타일을 모아 서로 겹치는지, 기존과 겹치는지 본다
            var placeTilePosList = new List<TilePos>();
            foreach (var reqChgItem in reqChgItemList)
            {
                var prtKingdomItem = ProtoDb.Get<KingdomItemProto>(reqChgItem.Num);
                placeTilePosList.AddRange(KingdomHelper.GetTilePosRanges(reqChgItem.TilePos.X, reqChgItem.TilePos.Y, prtKingdomItem.SizeX, prtKingdomItem.SizeY));
            }

            foreach (var reqPlaceItem in reqPlaceItemList)
            {
                var prtKingdomItem = ProtoDb.Get<KingdomItemProto>(reqPlaceItem.Num);
                placeTilePosList.AddRange(KingdomHelper.GetTilePosRanges(reqPlaceItem.TilePos.X, reqPlaceItem.TilePos.Y, prtKingdomItem.SizeX, prtKingdomItem.SizeY));
            }

            ReqHelper.ValidContext(!HasOverlappingTiles(placeTilePosList), "OVERLAPPING_TILES", () => new { PlaceTilePosList = placeTilePosList });

            foreach (var placeTilePos in placeTilePosList)
            {
                ReqHelper.ValidContext(placeTilePos.X >= 0 && placeTilePos.Y >= 0 && placeTilePos.X < sizeX && placeTilePos.Y < sizeY,
                    "OUT_OF_KINGDOM_MAP", () => new { ReqX = placeTilePos.X, ReqY = placeTilePos.Y, SizeX = sizeX, SizeY = sizeY });

                var tileItemId = copySnapshot.TileMap[placeTilePos.Y][placeTilePos.X];
                ReqHelper.ValidContext(tileItemId == 0, "NOT_EMPTY_TILE", () => new { ReqX = placeTilePos.X, ReqY = placeTilePos.Y, TileItemId = tileItemId });
            }

            // ── 검증 완료. 여기부터 복제본에 실제로 놓는다 ──
            foreach (var reqPlaceItem in reqPlaceItemList)
            {
                ReqHelper.ValidContext(reqPlaceItem.PlacedItemId == 0, "MUST_BE_ZERO_PLACE_KINGDOM_ITEM", () => new { ReqPlaceItemId = reqPlaceItem.PlacedItemId });

                var prtKingdomItem = ProtoDb.Get<KingdomItemProto>(reqPlaceItem.Num);
                var newPlacedObj = MakePlacedObj(++copySnapshot.ObjIdCounter, prtKingdomItem, reqPlaceItem.TilePos, reqPlaceItem.StructureId);
                AddPlacedObj(copySnapshot, newPlacedObj);

                switch (newPlacedObj.Type)
                {
                    case EKingdomItemType.STRUCTURE:
                        structureDeltaCntDict.TryAdd(newPlacedObj.StructureItemId, 0);
                        structureDeltaCntDict[newPlacedObj.StructureItemId]--;
                        break;
                    case EKingdomItemType.DECO:
                        decoDeltaCntDict.TryAdd(newPlacedObj.Num, 0);
                        decoDeltaCntDict[newPlacedObj.Num]--;
                        break;
                }
            }

            foreach (var reqChgItem in reqChgItemList)
            {
                ReqHelper.ValidContext(reqChgItem.PlacedItemId != 0, "ZERO_CHG_KINGDOM_ITEM", () => new { ReqChgItemId = reqChgItem.PlacedItemId });

                var prtKingdomItem = ProtoDb.Get<KingdomItemProto>(reqChgItem.Num);
                AddPlacedObj(copySnapshot, MakePlacedObj(reqChgItem.PlacedItemId, prtKingdomItem, reqChgItem.TilePos, reqChgItem.StructureId));
            }

            return copySnapshot;
        }

        // 기본 플레이어 데이터로 첫 맵 스냅샷을 만든다.
        // 배치 목록에 실제로 없는 구조물이 섞여 있으면(기본 데이터 오류) 버린다.
        public static KingdomMapSnapshotPacket BuildInitialSnapshot(KingdomMapPacket pak, List<KingdomStructureModel> mdlStructureList)
        {
            var deletePlacedItemList = new List<PlacedKingdomItemPacket>();
            foreach (var placedItem in pak.PlacedKingdomItemList)
            {
                if (placedItem.Type != EKingdomItemType.STRUCTURE)
                {
                    continue;
                }

                var mdlStructure = mdlStructureList.Find(x => x.Num == placedItem.Num);
                if (mdlStructure == null)
                {
                    // TODO: 로그. DefaultPlayer.json 에 없는 구조물이 배치돼 있다.
                    deletePlacedItemList.Add(placedItem);
                    continue;
                }

                placedItem.StructureItemId = mdlStructure.SfId;
            }

            var placedObjDict = pak.PlacedKingdomItemList
                .Where(x => !deletePlacedItemList.Contains(x))
                .ToDictionary(x => x.Id, x => x);

            var snapshot = new KingdomMapSnapshotPacket
            {
                ObjIdCounter = (ulong)pak.PlacedKingdomItemList.Count,
                PlacedObjDict = placedObjDict,
            };

            FillTileMap(snapshot, pak.SizeX, pak.SizeY);
            foreach (var placedItem in placedObjDict.Values)
            {
                foreach (var tilePos in KingdomHelper.GetTilePosRanges(placedItem.StartTileX, placedItem.StartTileY, placedItem.SizeX, placedItem.SizeY))
                {
                    snapshot.TileMap[tilePos.Y][tilePos.X] = placedItem.Id;
                }
            }

            return snapshot;
        }

        // TileMap 을 sizeX * sizeY 로 채운다. 이미 있으면 모자란 만큼만 늘린다.
        public static void FillTileMap(KingdomMapSnapshotPacket snapshot, int sizeX, int sizeY)
        {
            if (snapshot.TileMap.Count == 0)
            {
                for (var y = 0; y < sizeY; y++)
                {
                    snapshot.TileMap.Add([]);
                    for (var x = 0; x < sizeX; x++)
                    {
                        snapshot.TileMap[y].Add(0);
                    }
                }

                return;
            }

            if (snapshot.TileMap.Count >= sizeY && snapshot.TileMap[0].Count >= sizeX)
            {
                // TODO: 맵이 줄어드는 경우가 실제로 있는지 로그
                return;
            }

            for (var y = 0; y < sizeY; y++)
            {
                if (y >= snapshot.TileMap.Count)
                {
                    snapshot.TileMap.Add([]);
                }

                for (var x = 0; x < sizeX; x++)
                {
                    if (x >= snapshot.TileMap[y].Count)
                    {
                        snapshot.TileMap[y].Add(0);
                    }
                }
            }
        }

        // 타입은 프로토에서 읽는다. 호출부가 넘기게 뒀더니 단건 배치 경로가 STRUCTURE 를
        // 박아 넣어, 데코를 놓아도 구조물로 기록되고 나중에 보관이 막혔다.
        private static PlacedKingdomItemPacket MakePlacedObj(
            ulong id, KingdomItemProto prtKingdomItem, TilePosPacket tilePos, ulong structId)
        {
            return new PlacedKingdomItemPacket
            {
                Id = id,
                SizeX = prtKingdomItem.SizeX,
                SizeY = prtKingdomItem.SizeY,
                StartTileX = tilePos.X,
                StartTileY = tilePos.Y,
                State = EPlacedKingdomItemState.NONE,
                StructureItemId = structId,
                Num = prtKingdomItem.Num,
                Rotation = 0,
                Type = prtKingdomItem.Type,
            };
        }

        // 아이템이 차지하는 칸 전체를 마킹한다. 옛 코드는 시작 칸 하나만 찍었고,
        // 스냅샷이 저장된 뒤 다음 요청이 나머지 칸을 빈 칸으로 읽어 겹쳐 놓을 수 있었다.
        // TRASH 를 뺀 모든 아이템이 3x3 또는 2x2 라 전부 해당됐다.
        private static void AddPlacedObj(KingdomMapSnapshotPacket snapshot, PlacedKingdomItemPacket newPlacedObj)
        {
            snapshot.PlacedObjDict.Add(newPlacedObj.Id, newPlacedObj);

            foreach (var tilePos in KingdomHelper.GetTilePosRanges(newPlacedObj.StartTileX, newPlacedObj.StartTileY, newPlacedObj.SizeX, newPlacedObj.SizeY))
            {
                var tileItemId = snapshot.TileMap[tilePos.Y][tilePos.X];
                ReqHelper.ValidContext(tileItemId == 0, "NOT_EMPTY_TILE2",
                    () => new { ReqX = tilePos.X, ReqY = tilePos.Y, TileItemId = tileItemId });

                snapshot.TileMap[tilePos.Y][tilePos.X] = newPlacedObj.Id;
            }
        }

        private static bool HasOverlappingTiles(List<TilePos> placeTilePosList)
        {
            var uniqueTileSet = new HashSet<TilePos>();
            foreach (var tile in placeTilePosList)
            {
                if (!uniqueTileSet.Add(tile))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
