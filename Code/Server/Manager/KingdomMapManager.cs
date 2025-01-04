using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;
using WebStudyServer.GAME;
using Protocol.Packet.Custom;
using Protocol;
using System.Collections.Generic;

namespace WebStudyServer.Manager
{
    public partial class KingdomMapManager : UserManagerBase<KingdomMapModel>
    {
        public int XSize => _model.XSize;
        public int YSize => _model.XSize;
        public KingdomMapSnapshotPacket Snapshot { get; private set; }

        public KingdomMapManager(UserRepo userRepo, KingdomMapModel model) : base(userRepo, model)
        {
            if (model.Snapshot == null)
            {
                this.Snapshot = new KingdomMapSnapshotPacket();
                RefreshTileMap();
            }
            else
            {
                this.Snapshot = SerializeHelper.JsonDeserialize<KingdomMapSnapshotPacket>(model.Snapshot);

                if (this.Snapshot.Ver != KingdomMapSnapshotPacket.c_curVer)
                {
                    // TODO: 버전 다를때 처리
                    //this.Snapshot = new KingdomMapSnapshotPacket();
                    switch (this.Snapshot.Ver)
                    {
                        case KingdomMapSnapshotPacket.c_ver_1:
                            break;
                    }
                }

                RefreshTileMap();
            }
        }

        public TilePosPacket ValidEmptyTile(TilePosPacket reqStartPos, KingdomItemProto prtKingdomItem)
        {
            var tilePosRanges = GetTilePosRanges(reqStartPos.X, reqStartPos.Y, prtKingdomItem.SizeX, prtKingdomItem.SizeY);

            foreach (var tilePos in tilePosRanges)
            {
                if (tilePos.X < 0 || tilePos.Y < 0 || tilePos.X >= this.XSize|| tilePos.Y >= this.XSize)
                {
                    ReqHelper.ValidParam(false, "OUT_OF_KINGDOM_MAP", () => new { ReqX = tilePos.X, ReqY = tilePos.Y, SizeX = this.XSize, SizeY = this.YSize});
                }

                var tileObjId = this.Snapshot.TileMap[tilePos.X][tilePos.Y];
                ReqHelper.ValidContext(tileObjId == 0, "NOT_EMPTY_TILE", () => new { ReqX = tilePos.X, ReqY = tilePos.Y, ObjId = tileObjId });
            }

            return reqStartPos;
        }

        // TODO: 너무 길어서 개선 필요
        // Store, Chg, Place 액션을 전부 모아서  복제 맵에서 검증 및 실행 시뮬레이션 함.
        // 결과가 유효한지 한번에 검증. 유효하면 Snapshot 리턴
        public KingdomMapSnapshotPacket ValiePlaceItemsSnapshot(List<ulong> reqStoreIdList, List<ChgKingdomItemPacket> reqChgItemList, List<ChgKingdomItemPacket> reqPlaceItemList, 
            out Dictionary<ulong, int> structureDeltaCntDict, out Dictionary<int, int> decoDeltaCntDict)
        {
            structureDeltaCntDict = new();
            decoDeltaCntDict = new();

            var copySnapshot = this.Snapshot.DeepCopy();

            // 보관/이동시킬 타일 정보 생성 (Store, Chg 리스트)
            var deleteItemIdList = reqStoreIdList;
            deleteItemIdList.AddRange(reqChgItemList.Select(x => x.PlacedItemId));

            var deleteTilePosList = new List<TilePosPacket>();
            foreach (var deletedPlaceItemId in deleteItemIdList)
            {
                ReqHelper.ValidContext(copySnapshot.PlacedObjDict.TryGetValue(deletedPlaceItemId, out var placedItem), "NOT_FOUND_PLACED_KINGDOM_ITEM", () => new { PlaceItemId = deletedPlaceItemId });

                if (reqStoreIdList.Contains(deletedPlaceItemId))
                {
                    // Store하는 경우
                    switch(placedItem.Type)
                    {
                        case EKingdomItemType.STRUCTURE:
                            structureDeltaCntDict.TryAdd(deletedPlaceItemId, 0);
                            structureDeltaCntDict[placedItem.Id]++;
                            break;
                        case EKingdomItemType.DECO:
                            decoDeltaCntDict.TryAdd(placedItem.Num, 0);
                            decoDeltaCntDict[placedItem.Num]++;
                            break;
                    }
                }

                // 오브젝트 정보 삭제
                copySnapshot.PlacedObjDict.Remove(deletedPlaceItemId);
                var tilePoses = GetTilePosRanges(placedItem.StartTileX, placedItem.StartTileY, placedItem.SizeX, placedItem.SizeY);
                deleteTilePosList.AddRange(tilePoses);

                // 보관/이동 타일 기존 정보 삭제
                foreach (var tilePos in deleteTilePosList)
                {
                    copySnapshot.TileMap[tilePos.Y][tilePos.X] = 0;
                }
            }

            // 이제
            // 1. Chg, Place 들 끼리 서로 충돌하지 않는지 검증.
            // 2. Chg, Place(새 위치)들이 기존 타일과 충돌하는지 검증    
            // 이게 확인되면 그대로 저장해도 OK
            var placeTilePosList = new List<TilePosPacket>(); // . Chg, Place 합산 Tile리스트
            foreach (var reqChgItem in reqChgItemList)
            {
                var prtKingdomItem = APP.Prt.GetKingdomItemPrt(reqChgItem.Num);
                var tilePosRanges = GetTilePosRanges(reqChgItem.TilePos.X, reqChgItem.TilePos.Y, prtKingdomItem.SizeX, prtKingdomItem.SizeY);
                placeTilePosList.AddRange(tilePosRanges);
            }

            foreach (var reqPlaceItem in reqPlaceItemList)
            {
                var prtKingdomItem = APP.Prt.GetKingdomItemPrt(reqPlaceItem.Num);
                var tilePosRanges = GetTilePosRanges(reqPlaceItem.TilePos.X, reqPlaceItem.TilePos.Y, prtKingdomItem.SizeX, prtKingdomItem.SizeY);
                placeTilePosList.AddRange(tilePosRanges);
            }

            // 1. Chg, Place 들 끼리 서로 충돌하지 않는지 검증. (placeTilePosList) 사용
            ReqHelper.ValidContext(!HasOverlappingTiles(placeTilePosList), "OVERLAPPING_TILES", () => new { PlaceTilePosList = placeTilePosList });

            // 2. Chg, Place(새 위치)들이 기존 타일과 충돌하는지 검증 
            foreach (var placeTilePos in placeTilePosList)
            {
                ReqHelper.ValidContext(placeTilePos.X >= 0 && placeTilePos.Y >= 0 && placeTilePos.X < this.XSize && placeTilePos.Y < this.YSize, "OUT_OF_KINGDOM_MAP", () => new { ReqX = placeTilePos.X, ReqY = placeTilePos.Y, SizeX = this.XSize, SizeY = this.YSize });

                var tileItemId = copySnapshot.TileMap[placeTilePos.Y][placeTilePos.X];
                ReqHelper.ValidContext(tileItemId == 0, "NOT_EMPTY_TILE", () => new { ReqX = placeTilePos.X, ReqY = placeTilePos.Y, TileItemId = tileItemId });
            }

            // ----------------------- 검증 완료 -------------------------
            // CreateItem (Item을 배치하기위해 ItemId 미리 생성)
            foreach (var reqPlaceItem in reqPlaceItemList)
            {
                ReqHelper.ValidContext(reqPlaceItem.PlacedItemId == 0, "MUST_BE_ZERO_PLACE_KINGDOM_ITEM", () => new { ReqPlaceItemId = reqPlaceItem.PlacedItemId });

                var prtKingdomItem = APP.Prt.GetKingdomItemPrt(reqPlaceItem.Num);
                var newPlacedObjId = ++copySnapshot.ObjIdCounter;
                var newPlacedObj = new PlacedKingdomItemPacket
                {
                    Id = newPlacedObjId,
                    SizeX = prtKingdomItem.SizeX,
                    SizeY = prtKingdomItem.SizeY,
                    StartTileX = reqPlaceItem.TilePos.X,
                    StartTileY = reqPlaceItem.TilePos.Y,
                    State = EPlacedKingdomItemState.NONE,
                    StructureItemId = reqPlaceItem.StructureId,
                    Num = prtKingdomItem.Num,
                    Rotation = 0,
                    Type = prtKingdomItem.Type,
                };
                copySnapshot.PlacedObjDict.Add(newPlacedObj.Id, newPlacedObj);
                ReqHelper.ValidContext(copySnapshot.TileMap[newPlacedObj.StartTileY][newPlacedObj.StartTileX] == 0, "NOT_EMPTY_TILE2", () => new { ReqX = newPlacedObj.StartTileX, ReqY = newPlacedObj.StartTileY, TileItemId = newPlacedObj.Id });
                copySnapshot.TileMap[newPlacedObj.StartTileY][newPlacedObj.StartTileX] = newPlacedObj.Id;

                // Store하는 경우
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

            // ChgItem 배치
            foreach (var reqChgItem in reqChgItemList)
            {
                ReqHelper.ValidContext(reqChgItem.PlacedItemId != 0, "ZERO_CHG_KINGDOM_ITEM", () => new { ReqChgItemId = reqChgItem.PlacedItemId });

                var prtKingdomItem = APP.Prt.GetKingdomItemPrt(reqChgItem.Num);
                var newPlacedObj = new PlacedKingdomItemPacket
                {
                    Id = reqChgItem.PlacedItemId,
                    SizeX = prtKingdomItem.SizeX,
                    SizeY = prtKingdomItem.SizeY,
                    StartTileX = reqChgItem.TilePos.X,
                    StartTileY = reqChgItem.TilePos.Y,
                    State = EPlacedKingdomItemState.NONE,
                    StructureItemId = reqChgItem.StructureId,
                    Num = prtKingdomItem.Num,
                    Rotation = 0,
                    Type = prtKingdomItem.Type,
                };
                copySnapshot.PlacedObjDict.Add(newPlacedObj.Id, newPlacedObj);
                ReqHelper.ValidContext(copySnapshot.TileMap[newPlacedObj.StartTileY][newPlacedObj.StartTileX] == 0, "NOT_EMPTY_TILE2", () => new { ReqX = newPlacedObj.StartTileX, ReqY = newPlacedObj.StartTileY, TileItemId = newPlacedObj.Id });
                copySnapshot.TileMap[newPlacedObj.StartTileY][newPlacedObj.StartTileX] = newPlacedObj.Id;
            }

/*            this.Snapshot = copySnapshot;
            _model.Snapshot = SerializeHelper.JsonSerialize(this.Snapshot);
            _userRepo.KingdomMap.Update(_model);*/

            return copySnapshot;
        }

        public void SaveSnapshot(KingdomMapSnapshotPacket newSnapshot = null)
        {
            if (newSnapshot != null)
            {
                this.Snapshot = newSnapshot;
            }

            _model.Snapshot = SerializeHelper.JsonSerialize(this.Snapshot);
            _userRepo.KingdomMap.Update(_model);
        }

        public List<PlacedKingdomItemPacket> ValidExistPlacedItemList(List<ulong> placedItemList)
        {
            var validPlacedItemList = new List<PlacedKingdomItemPacket>();
            foreach (var placeItemId in placedItemList)
            {
                ReqHelper.ValidContext(Snapshot.PlacedObjDict.TryGetValue(placeItemId, out var placedItem), "NOT_FOUND_PLACE_KINGDOM_ITEM", () => new { PlaceItemId = placeItemId });
                validPlacedItemList.Add(placedItem);
            }
            return validPlacedItemList; 
        }

        public void ConstructStructure(KingdomStructureManager mgrStructManager, TilePosPacket valStartTilePos)
        {
            ConstructItemInternal(mgrStructManager.Prt, valStartTilePos, mgrStructManager.Model.Id);
        }

        public void ConstructDeco(KingdomDecoManager mgrDecoManager, TilePosPacket valStartTilePos)
        {
            ConstructItemInternal(mgrDecoManager.Prt, valStartTilePos, 0);
        }

/*        public void StoreItemList(List<PlacedKingdomItemPacket> placedKingdomItemlist)
        {
            foreach (var placedKingdomItem in placedKingdomItemlist)
            {
                if (!this.Snapshot.PlacedObjDict.ContainsKey(placedKingdomItem.Id))
                {
                    // 로그
                    continue;
                }

                // 오브젝트 정보 삭제
                this.Snapshot.PlacedObjDict.Remove(placedKingdomItem.Id);
                var tilePoses = GetTilePosRanges(placedKingdomItem.StartTileX, placedKingdomItem.StartTileY, placedKingdomItem.SizeX, placedKingdomItem.SizeY);

                // 타일 정보 삭제
                foreach(var tilePos in tilePoses)
                {
                    this.Snapshot.TileMap[tilePos.Y][tilePos.X] = 0;
                }
            }

            SaveSnapshot();
        }*/

        private void RefreshTileMap()
        {
            if (this.Snapshot.TileMap.Count == 0) // 완전 빈 상태
            {
                for (var y = 0; y < this.YSize; y++)
                {
                    this.Snapshot.TileMap.Add(new List<ulong>());
                    for (var x = 0; x < this.XSize; x++)
                    {
                        this.Snapshot.TileMap[y].Add(0);
                    }
                }
                return;
            }

            var tileMapYSize = this.Snapshot.TileMap.Count;
            var tileMapXSize = this.Snapshot.TileMap[0].Count;

            if (tileMapYSize < this.YSize || tileMapXSize < this.XSize)
            {
                for (var y = 0; y < this.YSize; y++)
                {
                    if (y >= this.Snapshot.TileMap.Count)
                    {
                        this.Snapshot.TileMap.Add(new List<ulong>());
                    }

                    for (var x = 0; x < this.XSize; x++)
                    {
                        if (x >= this.Snapshot.TileMap[y].Count)
                        {
                            this.Snapshot.TileMap[y].Add(0);
                        }
                    }
                }
            }
            else if (tileMapYSize > this.YSize || tileMapXSize > this.XSize)
            {
                // TODO: 이 경우가 실제로 일어나는지 로그 
            }
        }

        private void ConstructItemInternal(KingdomItemProto prtKingdomItem, TilePosPacket valStartTilePos, ulong structId)
        {
            var newPlacedObjId = ++this.Snapshot.ObjIdCounter;
            var newPlacedObj = new PlacedKingdomItemPacket
            {
                Id = newPlacedObjId,
                SizeX = prtKingdomItem.SizeX,
                SizeY = prtKingdomItem.SizeY,
                StartTileX = valStartTilePos.X,
                StartTileY = valStartTilePos.Y,
                State = EPlacedKingdomItemState.NONE,
                StructureItemId = structId,
                Num = prtKingdomItem.Num,
                Rotation = 0,
                Type = EKingdomItemType.STRUCTURE,
            };

            var tilePosRanges = GetTilePosRanges(valStartTilePos.X, valStartTilePos.Y, prtKingdomItem.SizeX, prtKingdomItem.SizeY);
            foreach (var tilePos in tilePosRanges)
            {
                this.Snapshot.TileMap[tilePos.Y][tilePos.X] = newPlacedObj.Id;
            }

            this.Snapshot.PlacedObjDict.Add(newPlacedObj.Id, newPlacedObj);
            SaveSnapshot();
        }

        private bool HasOverlappingTiles(List<TilePosPacket> placeTilePosList)
        {
            var uniqueTileSet = new HashSet<(int, int)>();  // (X, Y) 튜플을 사용

            foreach (var tile in placeTilePosList)
            {
                var tilePosition = (tile.X, tile.Y);  // X, Y를 튜플로 변환
                if (!uniqueTileSet.Add(tilePosition)) // 중복이 있으면 Add가 false를 반환
                {
                    return true; // 중복된 좌표가 있으면 true 반환
                }
            }

            return false; // 중복되는 좌표가 없으면 false 반환
        }


        private IEnumerable<TilePosPacket> GetTilePosRanges(int startX, int startY, int sizeX, int sizeY)
        {
            var tilePosRangeList = new List<TilePosPacket>();
            for (var y = 0; y < sizeX; y++)
            {
                for (var x = 0; x < sizeY; x++)
                {
/*                    tilePosRangeList.Add(new TilePosPacket
                    {
                        X = startPos.X + x,
                        Y = startPos.Y + y,
                    });
*/
                    yield return new TilePosPacket
                    {
                        X = startX + x,
                        Y = startY + y,
                    };
                }
            }
        }
    }
}
