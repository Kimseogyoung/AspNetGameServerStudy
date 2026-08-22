using Proto;
using Protocol;
using ServerCore;
using ServerCore.Extension;
using WebStudyServer.Data;
using WebStudyServer.Data.Queries;
using WebStudyServer.Helper;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Manager
{
    public partial class PlayerDetailManager : UserManagerBase<PlayerDetailModel>
    {
        public ulong Id => Model.PlayerId;

        public PlayerDetailManager(UserRepo userRepo, UserScope userScope, PlayerDetailModel model) : base(userRepo, model)
        {
            _userScope = userScope;
        }

        public CashPacket GetCashPacket()
        {
            return new CashPacket
            {
                FreeCash = _model.FreeCash,
                RealCash = _model.RealCash,
            };
        }

        // TODO: Reward관련 내용 별도 멤버변수로 빼는것 고려
        public async Task<ChgObjPacket> DecCostAsync(ObjValue valCostObj, string reason)
        {
            var amount = await DecCostAsync(valCostObj.Key.Type, valCostObj.Key.Num, valCostObj.Value, reason);
            var obj = new ChgObjPacket
            {
                TotalAmount = amount,
                Amount = valCostObj.Value,
                Type = valCostObj.Key.Type,
                Num = valCostObj.Key.Num,
            };
            return obj;
        }

        public async Task<double> DecCostAsync(EObjType objType, int objNum, double objAmount, string reason)
        {
            // 마이너스, 소수점 체크
            ReqHelper.ValidUnderFlowParam(objAmount, reason);
            var valObjAmount = ReqHelper.ValidWithoutDecimal(objAmount, reason);

            var objTypeCategory = objType.ToObjTyeCategory();
            switch (objTypeCategory)
            {
                case EObjType.EXP:
                    var exp = await DecExpInternalAsync(valObjAmount, reason);
                    return exp;
                case EObjType.GOLD:
                    var gold = await DecGoldInternalAsync(valObjAmount, reason);
                    return gold;
                case EObjType.TOTAL_CASH:
                    var totalCash = await DecCashInternalAsync(valObjAmount, reason);
                    return totalCash;
                case EObjType.POINT_START:
                    var pointNum = (int)objType;
                    var pointAmount = await DecPointInternalAsync(pointNum, valObjAmount, reason);
                    return pointAmount;
                case EObjType.TICKET_START:
                    var ticketNum = (int)objType;
                    var ticketAmount = await DecTicketInternalAsync(ticketNum, valObjAmount, reason);
                    return ticketAmount;
                case EObjType.ITEM:
                    var itemAmount = await DecItemInternalAsync(objNum, valObjAmount, reason);
                    return itemAmount;
                default:
                    throw new GameException(EErrorCode.PARAM, "NO_HANDLING_COST_OBJ_TYPE", new { ObjType = objType });
            }
        }

        public async Task<List<ChgObjPacket>> IncRewardListAsync(List<ObjValue> valRewardObjValList, string reason)
        {
            var objList = new List<ChgObjPacket>();
            foreach (var valReward in valRewardObjValList)
            {
                var obj = await IncRewardAsync(valReward, reason);
                objList.Add(obj);
            }
            return objList;
        }

        public async Task<ChgObjPacket> IncRewardAsync(ObjValue valRewardObjVal, string reason)
        {
            var amount = await IncRewardAsync(valRewardObjVal.Key.Type, valRewardObjVal.Key.Num, valRewardObjVal.Value, reason);
            var obj = new ChgObjPacket
            {
                TotalAmount = amount,
                Amount = valRewardObjVal.Value,
                Type = valRewardObjVal.Key.Type,
                Num = valRewardObjVal.Key.Num,
            };
            return obj;
        }

        public async Task<double> IncRewardAsync(EObjType objType, int objNum, double objAmount, string reason)
        {
            // 마이너스, 소수점 체크
            ReqHelper.ValidUnderFlowParam(objAmount, reason);
            var valObjAmount = ReqHelper.ValidWithoutDecimal(objAmount, reason);

            var objTypeCategory = objType.ToObjTyeCategory();
            switch (objTypeCategory)
            {
                case EObjType.GOLD:
                    var gold = await IncGoldInternalAsync(valObjAmount, reason);
                    return gold;
                case EObjType.EXP:
                    var exp = await IncExpInternalAsync(valObjAmount, reason);
                    return exp;
                case EObjType.REAL_CASH:
                    var realCash = await IncRealCashInternalAsync(valObjAmount, reason);
                    return realCash;
                case EObjType.FREE_CASH:
                    var freeCash = await IncFreeCashInternalAsync(valObjAmount, reason);
                    return freeCash;
                case EObjType.POINT_START:
                    var pointNum = (int)objType;
                    var pointAmount = await IncPointInternalAsync(pointNum, valObjAmount, reason);
                    return pointAmount;
                case EObjType.TICKET_START:
                    var ticketNum = (int)objType;
                    var ticketAmount = await IncTicketInternalAsync(ticketNum, valObjAmount, reason);
                    return ticketAmount;
                case EObjType.ITEM:
                    var itemAmount = await IncItemInternalAsync(objNum, valObjAmount, reason);
                    return itemAmount;
                case EObjType.COOKIE:
                    var cookieSoulStone1 = await IncCookieInternalAsync(objNum, (int)valObjAmount, reason);
                    return cookieSoulStone1;
                case EObjType.SOUL_STONE:
                    var cookieSoulStone2 = await IncSoulStoneInternalAsync(objNum, (int)valObjAmount, reason);
                    return cookieSoulStone2;
                default:
                    throw new GameException(EErrorCode.PARAM, "NO_HANDLING_REWARD_OBJ_TYPE", new { ObjType = objType });
            }
        }

        #region GOLD
        public Task<double> DecGoldAsync(double amount, string reason) => DecGoldInternalAsync(amount, reason);
        public Task<double> IncGoldAsync(double amount, string reason) => IncGoldInternalAsync(amount, reason);
        private async Task<double> DecGoldInternalAsync(double amount, string reason)
        {
            _ = _model.Gold;

            _ = _model.AccGold;

            _model.Gold -= amount;
            _model.AccGold -= amount;
            await _userRepo.PlayerDetail.UpdateMdlAsync(_model);
            return _model.Gold;
        }

        private async Task<double> IncGoldInternalAsync(double amount, string reason)
        {
            _ = _model.Gold;

            _ = _model.AccGold;

            _model.Gold += amount;
            _model.AccGold += amount;
            await _userRepo.PlayerDetail.UpdateMdlAsync(_model);
            return _model.Gold;
        }
        #endregion

        #region EXP
        public Task<double> DecExpAsync(double amount, string reason) => DecExpInternalAsync(amount, reason);
        public Task<double> IncExpAsync(double amount, string reason) => IncExpInternalAsync(amount, reason);
        private async Task<double> DecExpInternalAsync(double amount, string reason)
        {
            var befExp = _model.Exp;

            _ = _model.AccExp;

            ReqHelper.ValidEnough(amount, befExp, "PLAYER_EXP", reason);

            _model.Exp -= amount;
            _model.AccExp -= amount;
            await _userRepo.PlayerDetail.UpdateMdlAsync(_model);
            return _model.Exp;
        }

        private async Task<double> IncExpInternalAsync(double amount, string reason)
        {
            _ = _model.Exp;

            _ = _model.AccExp;

            _model.Exp += amount;
            _model.AccExp += amount;
            await _userRepo.PlayerDetail.UpdateMdlAsync(_model);
            return _model.Exp;
        }
        #endregion

        #region CASH
        public Task<double> DecCashAsync(double amount, string reason) => DecCashInternalAsync(amount, reason);
        public Task<double> IncFreeCashAsync(double amount, string reason) => IncFreeCashInternalAsync(amount, reason);
        public Task<double> IncRealCashAsync(double amount, string reason) => IncRealCashInternalAsync(amount, reason);
        private async Task<double> DecCashInternalAsync(double amount, string reason)
        {
            var befFreeCash = _model.FreeCash;
            var befAccFreeCash = _model.AccFreeCash;
            var befRealCash = _model.RealCash;
            var befAccRealCash = _model.AccRealCash;
            var befTotalCash = befFreeCash + befRealCash;

            _ = befAccFreeCash + befAccRealCash;

            ReqHelper.ValidEnough(amount, befTotalCash, "PLAYER_TOTAL_CASH", reason);

            // RealCash 먼저 소모
            var realCashCost = Math.Min(befRealCash, amount);
            var freeCashCost = amount - realCashCost;

            if (realCashCost > 0)
            {
                _model.RealCash -= realCashCost;
                _model.AccRealCash -= realCashCost;
            }

            if (freeCashCost > 0)
            {
                _model.FreeCash -= freeCashCost;
                _model.AccFreeCash -= freeCashCost;
            }

            await _userRepo.PlayerDetail.UpdateMdlAsync(_model);

            var totalCash = _model.RealCash + _model.FreeCash;
            return totalCash;
        }

        private async Task<double> IncFreeCashInternalAsync(double amount, string reason)
        {
            _ = _model.FreeCash;

            _ = _model.AccFreeCash;

            _model.FreeCash += amount;
            _model.AccFreeCash += amount;
            await _userRepo.PlayerDetail.UpdateMdlAsync(_model);
            return _model.FreeCash;
        }

        private async Task<double> IncRealCashInternalAsync(double amount, string reason)
        {
            _ = _model.RealCash;

            _ = _model.AccRealCash;

            _model.RealCash += amount;
            _model.AccRealCash += amount;
            await _userRepo.PlayerDetail.UpdateMdlAsync(_model);
            return _model.RealCash;
        }
        #endregion

        #region POINT
        private async Task<double> DecPointInternalAsync(int pointNum, double amount, string reason)
        {
            var pointSet = _userScope.Owned<PointModel>();
            var point = await pointSet.GetOrCreateAsync(pointNum);
            var pointAmount = point.DecAmount(amount, reason);
            await pointSet.UpdateAsync(point);
            return pointAmount;
        }

        private async Task<double> IncPointInternalAsync(int pointNum, double amount, string reason)
        {
            var pointSet = _userScope.Owned<PointModel>();
            var point = await pointSet.GetOrCreateAsync(pointNum);
            var pointAmount = point.IncAmount(amount);
            await pointSet.UpdateAsync(point);
            return pointAmount;
        }
        #endregion

        #region TICKET
        private async Task<double> DecTicketInternalAsync(int ticketNum, double amount, string reason)
        {
            var ticketSet = _userScope.Owned<TicketModel>();
            var ticket = await ticketSet.GetOrCreateAsync(ticketNum);
            var ticketAmount = ticket.DecAmount(amount, reason);
            await ticketSet.UpdateAsync(ticket);
            return ticketAmount;
        }

        private async Task<double> IncTicketInternalAsync(int ticketNum, double amount, string reason)
        {
            var ticketSet = _userScope.Owned<TicketModel>();
            var ticket = await ticketSet.GetOrCreateAsync(ticketNum);
            var ticketAmount = ticket.IncAmount(amount);
            await ticketSet.UpdateAsync(ticket);
            return ticketAmount;
        }
        #endregion

        #region COOKIE
        private async Task<double> IncCookieInternalAsync(int cookieNum, int amount, string reason)
        {
            var cookieSet = _userScope.Owned<CookieModel>();
            var cookie = await cookieSet.GetOrCreateAsync(cookieNum);
            var soulStone = cookie.IncCookie(amount, ProtoDb.Get<CookieProto>(cookieNum));
            await cookieSet.UpdateAsync(cookie);
            return soulStone;
        }

        private async Task<double> IncSoulStoneInternalAsync(int soulStoneNum, int amount, string reason)
        {
            var cookieSet = _userScope.Owned<CookieModel>();
            var cookie = await cookieSet.GetOrCreateBySoulStoneAsync(soulStoneNum);
            var soulStone = cookie.IncSoulStone(amount);
            await cookieSet.UpdateAsync(cookie);
            return soulStone;
        }
        #endregion

        #region ITEM
        private async Task<double> DecItemInternalAsync(int itemNum, double amount, string reason)
        {
            var itemSet = _userScope.Owned<ItemModel>();
            var item = await itemSet.GetOrCreateAsync(itemNum);
            var itemAmount = item.DecAmount(amount, reason);
            await itemSet.UpdateAsync(item);
            return itemAmount;
        }

        private async Task<double> IncItemInternalAsync(int itemNum, double amount, string reason)
        {
            var itemSet = _userScope.Owned<ItemModel>();
            var item = await itemSet.GetOrCreateAsync(itemNum);
            var itemAmount = item.IncAmount(amount);
            await itemSet.UpdateAsync(item);
            return itemAmount;
        }
        #endregion

        private readonly UserScope _userScope;
    }
}
