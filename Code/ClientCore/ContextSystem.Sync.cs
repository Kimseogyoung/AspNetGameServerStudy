using Proto;
using Protocol;

namespace ClientCore
{
    public partial class ContextSystem
    {
        public void SyncChgObj(ChgObjPacket pakChgObj)
        {
            var objTypeCategory = pakChgObj.Type.ToObjTyeCategory();
            switch (objTypeCategory)
            {
                case EObjType.EXP:
                    _player.Exp = pakChgObj.TotalAmount;
                    break;
                case EObjType.GOLD:
                    _player.Gold = pakChgObj.TotalAmount;
                    break;
                case EObjType.FREE_CASH:
                    _player.FreeCash = pakChgObj.TotalAmount;
                    break;
                case EObjType.REAL_CASH:
                    _player.RealCash = pakChgObj.TotalAmount;
                    break;
                case EObjType.TOTAL_CASH:
                    var realCashCost = Math.Min(_player.RealCash, pakChgObj.TotalAmount);
                    _player.RealCash -= realCashCost;
                    var freeCashCost = Math.Min(_player.FreeCash, pakChgObj.TotalAmount - realCashCost);
                    _player.FreeCash = freeCashCost;
                    break;
                case EObjType.POINT_START:
                    var pointType = pakChgObj.Type;
                    var pakPoint = GetPointForce(pointType);
                    pakPoint.Amount = pakChgObj.TotalAmount;
                    break;
                case EObjType.TICKET_START:
                    var ticketType = pakChgObj.Type;
                    var pakTicket = GetTicketForce(ticketType);
                    pakTicket.Amount = pakChgObj.TotalAmount;
                    break;
                case EObjType.COOKIE:
                    var pakCookie = GetCookieForce(pakChgObj.Num);
                    var prtCookie = APP.Prt.GetCookiePrt(pakChgObj.Num);

                    var soulStoneCnt = (int)pakChgObj.Amount * prtCookie.InitSoulStone;
                    if (pakCookie.State != ECookieState.AVAILABLE)
                    {
                        pakCookie.State = ECookieState.AVAILABLE;
                        soulStoneCnt -= prtCookie.InitSoulStone;
                    }
                    
                    if(soulStoneCnt > 0)
                    {
                        pakCookie.SoulStone += soulStoneCnt;
                    }
                    break;
                case EObjType.SOUL_STONE:
                    var prtCookieSoulStone = APP.Prt.GetCookieSoulStonePrt(pakChgObj.Num);
                    var pakCookie2 = GetCookieForce(prtCookieSoulStone.CookieNum);
                    pakCookie2.SoulStone += (int)pakChgObj.Amount;
                    break;
                case EObjType.ITEM:
                    var pakItem = GetItemForce(pakChgObj.Num);
                    pakItem.Amount = pakChgObj.TotalAmount;
                    break;
                default:
                    Console.WriteLine($"NO_HANDLING_CHG_OBJ_TYPE {objTypeCategory}");
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
            foreach (var pakPlacedKingdomItem in pakPlacedKingdomItemList)
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
            var kingdomStructure = _player.KingdomStructureList.Where(x => x.SfId == pakKingdomStructure.SfId).FirstOrDefault();
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

        public void SyncKingdomDeco(KingdomDecoPacket pakKingdomDeco)
        {
            var kingdomDeco = _player.KingdomDecoList.Where(x => x.Num == pakKingdomDeco.Num).FirstOrDefault();
            if (kingdomDeco == null)
            {
                _player.KingdomDecoList.Add(pakKingdomDeco);
            }
            else
            {
                kingdomDeco.Num = pakKingdomDeco.Num;
                kingdomDeco.TotalCnt = pakKingdomDeco.TotalCnt;
                kingdomDeco.UnplacedCnt = pakKingdomDeco.UnplacedCnt;
                kingdomDeco.State = pakKingdomDeco.State;
            }

            RefreshKingdom();
        }

        public void SyncCookie(CookiePacket pakCookie)
        {
            var cookie = _player.CookieList.Where(x => x.Num == pakCookie.Num).FirstOrDefault();
            if (cookie == null)
            {
                _player.CookieList.Add(pakCookie);
            }
            else
            {
                cookie.Num = pakCookie.Num;
                cookie.Lv = pakCookie.Lv;
                cookie.Star = pakCookie.Star;
                cookie.SoulStone = pakCookie.SoulStone;
                cookie.AccSoulStone = pakCookie.AccSoulStone;
                cookie.Flag = pakCookie.Flag;
                cookie.State = pakCookie.State;
            }

            PrintCookieList();
        }

        public void SyncWorld(WorldPacket pakWorld)
        {
            var world = GetWorldForce(pakWorld.Num);
         
            world.Num = pakWorld.Num;
            world.TopFinishStageOrder = pakWorld.TopFinishStageOrder;
            world.LastPlayStageNum = pakWorld.LastPlayStageNum;
            world.TopFinishStageNum = pakWorld.TopFinishStageNum;
            world.Flag = pakWorld.Flag;
            world.State = pakWorld.State;
        }

        public void SyncWorldStage(WorldStagePacket pakWorldStage)
        {
            var world = GetWorldStageForce(pakWorldStage.Num);
            world.Num = pakWorldStage.Num;
            world.Star= pakWorldStage.Star;   
        }

        public void SyncScheduleList(List<SchedulePacket> pakScheduleList)
        {
            _scheduleList.Clear();
            foreach(var pakSchedule in pakScheduleList)
            {
                _scheduleList.Add(pakSchedule);
                Console.WriteLine($"Schedule : {pakSchedule.Num}, {pakSchedule.ActiveStartTime} ~ {pakSchedule.ActiveEndTime}");
            }
        }

        private List<SchedulePacket> _scheduleList = new List<SchedulePacket>();

        private PointPacket GetPointForce(EObjType objType)
        {
            var num = (int)objType;
            var pakPoint = _player.PointList.FirstOrDefault(x => x.Num == num);
            if (pakPoint == null)
            {
                pakPoint = new PointPacket { Num = num };
                _player.PointList.Add(pakPoint);
            }
            return pakPoint;
        }

        private TicketPacket GetTicketForce(EObjType objType)
        {
            var num = (int)objType;
            var pakTicket = _player.TicketList.FirstOrDefault(x => x.Num == num);
            if (pakTicket == null)
            {
                pakTicket = new TicketPacket { Num = num };
                _player.TicketList.Add(pakTicket);
            }
            return pakTicket;
        }

        private ItemPacket GetItemForce(int num)
        {
            var pakItem = _player.ItemList.FirstOrDefault(x => x.Num == num);
            if (pakItem == null)
            {
                pakItem = new ItemPacket { Num = num };
                _player.ItemList.Add(pakItem);
            }
            return pakItem;
        }

        private CookiePacket GetCookieForce(int num)
        {
            var pakCookie = _player.CookieList.FirstOrDefault(x => x.Num == num);
            if (pakCookie == null)
            {
                pakCookie = new CookiePacket { Num = num };
                _player.CookieList.Add(pakCookie);
            }
            return pakCookie;
        }

        private WorldPacket GetWorldForce(int num)
        {
            var pakWorld = _player.WorldList.Where(x => x.Num == num).FirstOrDefault();
            if (pakWorld == null)
            {
                var prtWorld = APP.Prt.GetWorldPrt(num);
                pakWorld = new WorldPacket { Num = num };
                _player.WorldList.Add(pakWorld);
            }

            return pakWorld;
        }

        private WorldStagePacket GetWorldStageForce(int num)
        {
            var pakWorldStage = _player.WorldStageList.Where(x => x.Num == num).FirstOrDefault();
            if (pakWorldStage == null)
            {
                var prtStage = APP.Prt.GetWorldStagePrt(num);
                pakWorldStage = new WorldStagePacket { Num = num, WorldNum = prtStage.WorldNum };
                _player.WorldStageList.Add(pakWorldStage);
            }

            return pakWorldStage;
        }
    }
}
