using NLog.LayoutRenderers.Wrappers;
using Proto.Helper;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{
    public partial class ContextSystem
    {
        public async Task RequestKingdomBuyStructure(int kingdomItemNum)
        {
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(kingdomItemNum);
            var req = new KingdomBuyStructureReqPacket(kingdomItemNum, new CostObjPacket { Type = prtKingdomItem.CostObjType, Num = prtKingdomItem.CostObjNum, Amount = prtKingdomItem.CostObjAmount });
            var res = await _rpcSystem.RequestAsync<KingdomBuyStructureReqPacket, KingdomBuyStructureResPacket>(req);

            SyncKingdomStructure(res.KingdomStructure);
            SyncChgObj(res.ChgObj);
        }

        public async Task RequestKingdomFinishConstructStructure(ulong kingdomStructureId, int kingdomItemNum)
        {
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(kingdomItemNum);
            var req = new KingdomFinishConstructStructureReqPacket(kingdomStructureId, kingdomItemNum);
            var res = await _rpcSystem.RequestAsync<KingdomFinishConstructStructureReqPacket, KingdomFinishConstructStructureResPacket>(req);

            SyncKingdomStructure(res.KingdomStructure);
        }

        public async Task RequestKingdomConstructureStructure(ulong kingdomStructureId, int x, int y)
        {
            var kingdomStructure = _player.KingdomStructureList.FirstOrDefault(x => x.SfId == kingdomStructureId);
            if (kingdomStructure == null)
            {
                Console.WriteLine($"NOT_FOUND_STRUCTURE_ITEM({kingdomStructureId})");
                return;
            }
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(kingdomStructure.Num);

            var reqCostList = new List<CostObjPacket>() { new CostObjPacket { Type = prtKingdomItem.ConstructObjType, Num = prtKingdomItem.ConstructObjNum, Amount = prtKingdomItem.ConstructObjAmount } };
            var req = new KingdomConstructStructureReqPacket(kingdomStructureId, kingdomStructure.Num, reqCostList, new TilePosPacket { X = x, Y = y });
            var res = await _rpcSystem.RequestAsync<KingdomConstructStructureReqPacket, KingdomConstructStructureResPacket>(req);

            SyncKingdomStructure(res.KingdomStructure);
            SyncPlacedKingdomItemList(res.PlacedKingdomItemList);
            SyncChgObjList(res.ChgObjList);
        }

        public async Task RequestKingdomBuyDeco(int kingdomItemNum)
        {
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(kingdomItemNum);
            var req = new KingdomBuyDecoReqPacket(kingdomItemNum,  new CostObjPacket { Type = prtKingdomItem.CostObjType, Num = prtKingdomItem.CostObjNum, Amount = prtKingdomItem.CostObjAmount });
            var res = await _rpcSystem.RequestAsync<KingdomBuyDecoReqPacket, KingdomBuyDecoResPacket>(req);

            SyncKingdomDeco(res.KingdomDeco);
            SyncChgObj(res.ChgObj);
        }

        public async Task RequestKingdomConstructDeco(int kingdomItemNum, int x, int y)
        {
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(kingdomItemNum);
            var req = new KingdomConstructDecoReqPacket(kingdomItemNum, new TilePosPacket { X = x, Y = y });
            var res = await _rpcSystem.RequestAsync<KingdomConstructDecoReqPacket, KingdomConstructDecoResPacket>(req);

            SyncPlacedKingdomItemList(res.PlacedKingdomItemList);
            SyncKingdomDeco(res.KingdomDeco);
        }

        public async Task RequestKingdomFinishCraftStructure(ulong kingdomStructureId)
        {
            var kingdomStructure = _player.KingdomStructureList.FirstOrDefault(x => x.SfId == kingdomStructureId);
            if (kingdomStructure == null)
            {
                Console.WriteLine($"NOT_FOUND_STRUCTURE_ITEM({kingdomStructureId})");
                return;
            }

            var req = new KingdomFinishCraftStructureReqPacket
            {
                KingdomStructureId = kingdomStructureId,
                KingdomItemNum = kingdomStructure.Num,
                Info = new ReqInfoPacket(),
            };

            var res = await _rpcSystem.RequestAsync<KingdomFinishCraftStructureReqPacket, KingdomFinishCraftStructureResPacket>(req);

            SyncKingdomStructure(res.KingdomStructure);
            SyncChgObjList(res.ChgObjList);
        }

        private void RefreshKingdom()
        {
            // 초기화
            for (var y = 0; y < _tileMap.Count; y++)
            {
                _tileMap[y].Clear();
            }
            _tileMap.Clear();

            // 다시 TileMap 생성
            for (var y = 0; y < _player.KingdomMap.SizeY; y++)
            {
                _tileMap.Add(new List<ulong>());
                for (var x = 0; x < _player.KingdomMap.SizeX; x++)
                {
                    _tileMap[y].Add(0);
                }
            }

            // 오브젝트 배치
            foreach (var placedItem in _player.KingdomMap.PlacedKingdomItemList)
            {
                var tilePoses = KingdomHelper.GetTilePosRanges(placedItem.StartTileX, placedItem.StartTileY, placedItem.SizeX, placedItem.SizeY);
                foreach (var tilePos in tilePoses)
                {
                    _tileMap[tilePos.Y][tilePos.X] = placedItem.Id;
                }
            }

            // Print TileMap & Structure 상태 표시
            var tileMapStr = new StringBuilder();
            tileMapStr.AppendLine("[KingdomTileMap]");
            for (var y = 0; y < _player.KingdomMap.SizeY; y++)
            {
                for (var x = 0; x < _player.KingdomMap.SizeX; x++)
                {
                    tileMapStr.Append(_tileMap[y][x].ToString().PadLeft(4));
                }
                tileMapStr.AppendLine();
            }

            tileMapStr.AppendLine("[KingdomStructure]");
            foreach (var placedItem in _player.KingdomMap.PlacedKingdomItemList.Where(x=>x.Type == Proto.EKingdomItemType.STRUCTURE))
            {
                var pakStructure = _player.KingdomStructureList.FirstOrDefault(x => x.SfId == placedItem.StructureItemId);
                if (pakStructure == null)
                {
                    tileMapStr.AppendLine($"NOT_FOUND_STRUCTURE_ITEM({placedItem.StructureItemId})");
                    continue;
                }

                var prtKingdomStructure = APP.Prt.GetKingdomItemPrt(placedItem.Num);
                tileMapStr.AppendLine($"Id({placedItem.Id}:{placedItem.StructureItemId}) Num({prtKingdomStructure.Num}) Name({prtKingdomStructure.Name}) State({placedItem.State}) Pos({placedItem.StartTileX},{placedItem.StartTileY})");
            }
            Console.WriteLine(tileMapStr.ToString());
        }

        private List<List<ulong>> _tileMap = new List<List<ulong>>();
    }
}
