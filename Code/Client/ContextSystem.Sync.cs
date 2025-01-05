using Proto;
using Protocol;

namespace Client
{
    public partial class ContextSystem
    {
        public void SyncChgObj(ChgObjPacket pakChgObj)
        {
            var objTypeCategory = pakChgObj.Type.ToObjTyeCategory();
            switch (objTypeCategory)
            {
                case EObjType.EXP:
                    break;
                case EObjType.GOLD:
                    break;
                case EObjType.FREE_CASH:
                    break;
                case EObjType.REAL_CASH:
                    break;
                case EObjType.TOTAL_CASH:
                    break;
                case EObjType.POINT_START:
                    break;
                case EObjType.TICKET_START:
                    break;
                case EObjType.COOKIE:
                    break;
                case EObjType.ITEM:
                    break;
                case EObjType.KINGDOM_ITEM:
                    break;
            }
        }

        public void SyncChgObjList(List<ChgObjPacket> pakChgObjList)
        {
            foreach (var pakChgObj in pakChgObjList)
            {
                SyncChgObj(pakChgObj);
            }
        }

        public void SyncPlacedKingdomItemList(List<PlacedKingdomItemPacket> pakPlacedKingdomItemList)
        {
            foreach(var pakPlacedKingdomItem in pakPlacedKingdomItemList)
            {
                var placedKingdomItem = _player.KingdomMap.PlacedKingdomItemList.Where(x => x.Id == pakPlacedKingdomItem.Id).FirstOrDefault();
                if (placedKingdomItem == null)
                {
                    _player.KingdomMap.PlacedKingdomItemList.Add(pakPlacedKingdomItem);
                }
                else
                {
                    placedKingdomItem.Num = pakPlacedKingdomItem.Num;
                    placedKingdomItem.Id = pakPlacedKingdomItem.Id;
                    placedKingdomItem.StructureItemId = pakPlacedKingdomItem.StructureItemId;
                    placedKingdomItem.Type = pakPlacedKingdomItem.Type;
                    placedKingdomItem.StartTileX = pakPlacedKingdomItem.StartTileX;
                    placedKingdomItem.StartTileY = pakPlacedKingdomItem.StartTileY;
                    placedKingdomItem.SizeX = pakPlacedKingdomItem.SizeX;
                    placedKingdomItem.SizeY = pakPlacedKingdomItem.SizeY;
                    placedKingdomItem.Rotation = pakPlacedKingdomItem.Rotation;
                    placedKingdomItem.Type = pakPlacedKingdomItem.Type;
                    placedKingdomItem.State = pakPlacedKingdomItem.State;
                }
            }

            RefreshKingdom();
        }

        public void SyncKingdomStructure(KingdomStructurePacket pakKingdomStructure)
        {
            var kingdomStructure = _player.KingdomStructureList.Where(x => x.Id == pakKingdomStructure.Id).FirstOrDefault();
            if (kingdomStructure == null)
            {
                _player.KingdomStructureList.Add(pakKingdomStructure);
            }
            else
            {
                kingdomStructure.Num = pakKingdomStructure.Num;
                kingdomStructure.Flag = pakKingdomStructure.Flag;
                kingdomStructure.EndTime = pakKingdomStructure.EndTime;
                kingdomStructure.State = pakKingdomStructure.State;
            }

            RefreshKingdom();
        }
    }
}
