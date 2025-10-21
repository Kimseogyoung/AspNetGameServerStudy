using Proto.Helper;
using Protocol;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace ClientCore
{
    public partial class ContextSystem
    {
        public async Task<KingdomBuyStructureResPacket> RequestKingdomBuyStructure(int kingdomItemNum)
        {
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(kingdomItemNum);
            var req = new KingdomBuyStructureReqPacket(kingdomItemNum, new CostObjPacket { Type = prtKingdomItem.CostObjType, Num = prtKingdomItem.CostObjNum, Amount = prtKingdomItem.CostObjAmount });
            var res = await RpcSystem.RequestAsync<KingdomBuyStructureReqPacket, KingdomBuyStructureResPacket>(req);

            SyncKingdomStructure(res.KingdomStructure);
            SyncChgObj(res.ChgObj);
            return res;
        }

        public async Task<KingdomFinishConstructStructureResPacket> RequestKingdomFinishConstructStructure(ulong kingdomStructureId, int kingdomItemNum)
        {
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(kingdomItemNum);
            var req = new KingdomFinishConstructStructureReqPacket(kingdomStructureId, kingdomItemNum);
            var res = await RpcSystem.RequestAsync<KingdomFinishConstructStructureReqPacket, KingdomFinishConstructStructureResPacket>(req);

            SyncKingdomStructure(res.KingdomStructure);
            return res;
        }

        public async Task<KingdomConstructStructureResPacket> RequestKingdomConstructureStructure(ulong kingdomStructureId, int x, int y)
        {
            var kingdomStructure = Player.KingdomStructureList.FirstOrDefault(x => x.SfId == kingdomStructureId);
            if (kingdomStructure == null)
            {
                Console.WriteLine($"NOT_FOUND_STRUCTURE_ITEM({kingdomStructureId})");
                return new KingdomConstructStructureResPacket { Info = _errorRes };
            }
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(kingdomStructure.Num);

            var reqCostList = new List<CostObjPacket>() { new CostObjPacket { Type = prtKingdomItem.ConstructObjType, Num = prtKingdomItem.ConstructObjNum, Amount = prtKingdomItem.ConstructObjAmount } };
            var req = new KingdomConstructStructureReqPacket(kingdomStructureId, kingdomStructure.Num, reqCostList, new TilePosPacket { X = x, Y = y });
            var res = await RpcSystem.RequestAsync<KingdomConstructStructureReqPacket, KingdomConstructStructureResPacket>(req);

            SyncKingdomStructure(res.KingdomStructure);
            SyncPlacedKingdomItemList(res.PlacedKingdomItemList);
            SyncChgObjList(res.ChgObjList);
            return res;
        }

        public async Task<KingdomBuyDecoResPacket> RequestKingdomBuyDeco(int kingdomItemNum)
        {
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(kingdomItemNum);
            var req = new KingdomBuyDecoReqPacket(kingdomItemNum,  new CostObjPacket { Type = prtKingdomItem.CostObjType, Num = prtKingdomItem.CostObjNum, Amount = prtKingdomItem.CostObjAmount });
            var res = await RpcSystem.RequestAsync<KingdomBuyDecoReqPacket, KingdomBuyDecoResPacket>(req);

            SyncKingdomDeco(res.KingdomDeco);
            SyncChgObj(res.ChgObj);
            return res;
        }

        public async Task<KingdomConstructDecoResPacket> RequestKingdomConstructDeco(int kingdomItemNum, int x, int y)
        {
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(kingdomItemNum);
            var req = new KingdomConstructDecoReqPacket(kingdomItemNum, new TilePosPacket { X = x, Y = y });
            var res = await RpcSystem.RequestAsync<KingdomConstructDecoReqPacket, KingdomConstructDecoResPacket>(req);

            SyncPlacedKingdomItemList(res.PlacedKingdomItemList);
            SyncKingdomDeco(res.KingdomDeco);
            return res;
        }

        public async Task<KingdomFinishCraftStructureResPacket> RequestKingdomFinishCraftStructure(ulong kingdomStructureId)
        {
            var kingdomStructure = Player.KingdomStructureList.FirstOrDefault(x => x.SfId == kingdomStructureId);
            if (kingdomStructure == null)
            {
                Console.WriteLine($"NOT_FOUND_STRUCTURE_ITEM({kingdomStructureId})");
                return new KingdomFinishCraftStructureResPacket { Info = _errorRes };
            }

            var req = new KingdomFinishCraftStructureReqPacket
            {
                KingdomStructureId = kingdomStructureId,
                KingdomItemNum = kingdomStructure.Num,
                Info = new ReqInfoPacket(),
            };

            var res = await RpcSystem.RequestAsync<KingdomFinishCraftStructureReqPacket, KingdomFinishCraftStructureResPacket>(req);

            SyncKingdomStructure(res.KingdomStructure);
            SyncChgObjList(res.ChgObjList);
            return res;
        }

        public void PrintKingdom()
        {
            // Print TileMap & Structure 상태 표시
            var tileMapStr = new StringBuilder();
            tileMapStr.AppendLine("[KingdomTileMap]");
            for (var y = 0; y < Player.KingdomMap.SizeY; y++)
            {
                for (var x = 0; x < Player.KingdomMap.SizeX; x++)
                {
                    tileMapStr.Append(_tileMap[y][x].ToString().PadLeft(4));
                }
                tileMapStr.AppendLine();
            }

            tileMapStr.AppendLine("[KingdomStructure]");
            foreach (var placedItem in Player.KingdomMap.PlacedKingdomItemList.Where(x => x.Type == Proto.EKingdomItemType.STRUCTURE))
            {
                var pakStructure = Player.KingdomStructureList.FirstOrDefault(x => x.SfId == placedItem.StructureItemId);
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

        private void RefreshKingdom()
        {
            // 초기화
            for (var y = 0; y < _tileMap.Count; y++)
            {
                _tileMap[y].Clear();
            }
            _tileMap.Clear();

            // 다시 TileMap 생성
            for (var y = 0; y < Player.KingdomMap.SizeY; y++)
            {
                _tileMap.Add(new List<ulong>());
                for (var x = 0; x < Player.KingdomMap.SizeX; x++)
                {
                    _tileMap[y].Add(0);
                }
            }

            // 오브젝트 배치
            foreach (var placedItem in Player.KingdomMap.PlacedKingdomItemList)
            {
                var tilePoses = KingdomHelper.GetTilePosRanges(placedItem.StartTileX, placedItem.StartTileY, placedItem.SizeX, placedItem.SizeY);
                foreach (var tilePos in tilePoses)
                {
                    _tileMap[tilePos.Y][tilePos.X] = placedItem.Id;
                }
            }

            // Print TileMap & Structure 상태 표시
            PrintKingdom();
        }


        private List<List<ulong>> _tileMap = new List<List<ulong>>();
    }
}
