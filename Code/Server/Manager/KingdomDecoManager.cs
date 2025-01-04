using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;
using WebStudyServer.GAME;
using System.Security.Cryptography.Xml;
using System.Globalization;
using Protocol;

namespace WebStudyServer.Manager
{
    public partial class KingdomDecoManager : UserManagerBase<KingdomDecoModel>
    {
        public KingdomItemProto Prt { get; private set; }
        public KingdomDecoManager(UserRepo userRepo, KingdomDecoModel model, KingdomItemProto prt) : base(userRepo, model)
        {
            Prt = prt;
        }

        public KingdomDecoManager(UserRepo userRepo, KingdomDecoModel model) : base(userRepo, model)
        {
            Prt = APP.Prt.GetKingdomItemPrt(model.Num);
        }

        public void Inc(int cnt, string reason)
        {
            ReqHelper.ValidUnderFlowParam(cnt, $"DECO_CNT:{reason}");

            var befTotalCnt = _model.TotalCnt;
            var befUnplacedCnt = _model.UnplacedCnt;

            // 최대 보유량 검증
            ReqHelper.ValidContext(_model.TotalCnt + cnt <= Prt.MaxCnt, "FULL_MAX_DECO_CNT",
                () => new { Num = _model.Num, TotalCnt = _model.TotalCnt, PrtMaxCnt = Prt.MaxCnt });

            _model.TotalCnt += cnt;
            _model.UnplacedCnt += cnt;
            _userRepo.KingdomDeco.Update(_model);
        }

        public void Construct()
        {
            var cnt = 1;
            ReqHelper.ValidContext(_model.UnplacedCnt >= cnt, "NOT_ENOUGH_DECO_CNT", () => new { Num = _model.Num, UnplacedCnt = _model.UnplacedCnt, DecCnt = cnt });
            var befTotalCnt = _model.TotalCnt;
            var befUnplacedCnt = _model.UnplacedCnt;

            _model.UnplacedCnt -= cnt;
            _userRepo.KingdomDeco.Update(_model);
        }

        public void Store(PlacedKingdomItemPacket placedKingdomItem, int cnt = 1)
        {
            var placedCnt = _model.TotalCnt - _model.UnplacedCnt;
            ReqHelper.ValidContext(placedCnt >= cnt, "NOT_ENOUGH_DECO_CNT", () => new { Num = _model.Num, UnplacedCnt = _model.UnplacedCnt, DecCnt = cnt });
            var befTotalCnt = _model.TotalCnt;
            var befUnplacedCnt = _model.UnplacedCnt;

            _model.UnplacedCnt += cnt;
            _userRepo.KingdomDeco.Update(_model);

            //placedKingdomItem 로그
        }

        /* public void Construct(int startTileX, int startTileY, int endTileX, int endTileY)
         {
             _model.StartTileX = startTileX;
             _model.StartTileY = startTileY;
             _model.EndTileX = endTileX;
             _model.EndTileY = endTileY;
             _model.State = EKingdomItemState.CONSTRUCTING;
             _model.EndTime = _rpcContext.ServerTime + TimeSpan.FromSeconds(Prt.ConstructSec);
             _userRepo.KingdomItem.Update(_model);
         }

         public void ConstructDone()
         {
             _model.EndTime = DateTime.MinValue;
             _model.State = EKingdomItemState.READY;
             _userRepo.KingdomItem.Update(_model);
         }

         public void Cancel()
         {
             _model.StartTileX = 0;
             _model.StartTileY = 0;
             _model.EndTileX = 0;
             _model.EndTileY = 0;
             _model.State = EKingdomItemState.STORED;
             _model.EndTime = DateTime.MinValue;
             _userRepo.KingdomItem.Update(_model);
         }

         public void DecTime()
         {
             _model.EndTime = DateTime.MinValue;
             _model.State = EKingdomItemState.READY;
             _userRepo.KingdomItem.Update(_model);
         }*/
    }
}
