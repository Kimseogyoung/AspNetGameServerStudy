using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Extension;
using WebStudyServer.Helper;

namespace WebStudyServer.Manager
{
    public partial class PlayerDetailManager : UserManagerBase<PlayerDetailModel>
    {
        public ulong Id => Model.PlayerId;

        public PlayerDetailManager(UserRepo userRepo, PlayerDetailModel model/*, PointComponent*/) : base(userRepo, model)
        {
        }
    
        private double DecCost(EObjType objType, int objNum, double objAmount)
        {
            // TODO: 마이너스, 소수점 체크
            var objTypeCategory = objType.ToObjTyeCategory();
            switch (objTypeCategory)
            {
                // TODO: 보유량 체크
                case EObjType.GOLD:
                    var gold = DecGoldInternal(objAmount);
                    return gold;
                case EObjType.STAR_CANDY:
                    break;
                case EObjType.TOTAL_CASH:
                    var totalCash = DecCashnternal(objAmount);
                    return totalCash;
                case EObjType.POINT_START:
                    var pointNum = (int)objType;
                    var pointAmount = DecPointInternal(pointNum, objAmount);
                    return pointAmount;
                case EObjType.TICKET_START:
                    var ticketNum = (int)objType;
                    var ticketAmount = DecTicketInternal(ticketNum, objAmount);
                    return ticketAmount;
                default:
                    throw new GameException(EErrorCode.PARAM, "NO_HANDLING_COST_OBJ_TYPE", new { ObjType = objType });
            }

            return 0;
        }

        private double IncReward(EObjType objType, int objNum, double objAmount)
        {
            // TODO: 마이너스, 소수점 체크
            var objTypeCategory = objType.ToObjTyeCategory();
            switch (objTypeCategory)
            {
                case EObjType.GOLD:
                    var gold = IncGoldInternal(objAmount);
                    return gold;
                case EObjType.STAR_CANDY:
                    break;
                case EObjType.REAL_CASH:
                    var realCash = IncRealCashInternal(objAmount);
                    return realCash;
                case EObjType.FREE_CASH:
                    var freeCash = IncFreeCashInternal(objAmount);
                    return freeCash;
                case EObjType.POINT_START:
                    var pointNum = (int)objType;
                    var pointAmount = IncPointInternal(pointNum, objAmount);
                    return pointAmount;
                case EObjType.TICKET_START:
                    var ticketNum = (int)objType;
                    var ticketAmount = IncTicketInternal(ticketNum, objAmount);
                    return ticketAmount;
                case EObjType.ITEM:
                    break;
                case EObjType.COOKIE:
                    var cookieStarExp = IncCookieInternal(objNum, (int)objAmount);
                    return cookieStarExp;
                case EObjType.KINGDOM_ITEM:
                    break;
                default:
                    throw new GameException(EErrorCode.PARAM, "NO_HANDLING_REWARD_OBJ_TYPE", new { ObjType = objType });
            }

            return 0;
        }

        #region GOLD
        private double DecGoldInternal(double amount)
        {
            var befGold = _model.Gold;
            var befAccGold = _model.AccGold;

            _model.Gold -= amount;
            _model.AccGold -= amount;
            _userRepo.PlayerDetail.Update(_model);
            return _model.Gold;
        }

        private double IncGoldInternal(double amount)
        {
            var befGold = _model.Gold;
            var befAccGold = _model.AccGold;

            _model.Gold += amount;
            _model.AccGold += amount;
            _userRepo.PlayerDetail.Update(_model);
            return _model.Gold;
        }
        #endregion

        #region CASH
        private double DecCashnternal(double amount)
        {
            var befFreeCash = _model.FreeCash;
            var befAccFreeCash = _model.AccFreeCash;
            var befRealCash = _model.RealCash;
            var befAccRealCash = _model.AccRealCash;
            var befTotalCash = befFreeCash + befRealCash;
            var befAccTotalCash = befAccFreeCash + befAccRealCash;

            // RealCash 먼저 소모
            double realCashCost = Math.Min(befRealCash, amount);
            double freeCashCost = amount - realCashCost;

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

            _userRepo.PlayerDetail.Update(_model);

            var totalCash = _model.RealCash + _model.FreeCash;
            return totalCash;
        }

        private double IncFreeCashInternal(double amount)
        {
            var befGold = _model.FreeCash;
            var befAccGold = _model.AccFreeCash;

            _model.FreeCash += amount;
            _model.AccFreeCash += amount;
            _userRepo.PlayerDetail.Update(_model);
            return _model.FreeCash;
        }

        private double IncRealCashInternal(double amount)
        {
            var befGold = _model.RealCash;
            var befAccGold = _model.AccRealCash;

            _model.RealCash += amount;
            _model.AccRealCash += amount;
            _userRepo.PlayerDetail.Update(_model);
            return _model.RealCash;
        }
        #endregion

        #region POINT
        private double DecPointInternal(int pointNum, double amount)
        {
            var mgrPoint = _userRepo.Point.Touch((EObjType)pointNum);
            var pointAmount = mgrPoint.DecAmount(amount);
            return pointAmount;
        }

        private double IncPointInternal(int pointNum, double amount)
        {
            var mgrPoint = _userRepo.Point.Touch((EObjType)pointNum);
            var pointAmount = mgrPoint.IncAmount(amount);
            return pointAmount;
        }
        #endregion

        #region TICKET
        private double DecTicketInternal(int ticketNum, double amount)
        {
            var mgrPoint = _userRepo.Ticket.Touch((EObjType)ticketNum);
            var pointAmount = mgrPoint.DecAmount(amount);
            return pointAmount;
        }

        private double IncTicketInternal(int ticketNum, double amount)
        {
            var mgrPoint = _userRepo.Ticket.Touch((EObjType)ticketNum);
            var pointAmount = mgrPoint.IncAmount(amount);
            return pointAmount;
        }
        #endregion

        #region COOKIE
        private double IncCookieInternal(int cookieNum, int starExp)
        {
            var mgrPoint = _userRepo.Cookie.Touch(cookieNum);
            var pointAmount = mgrPoint.IncStarExp(starExp);
            return pointAmount;
        }
        #endregion


    }
}
