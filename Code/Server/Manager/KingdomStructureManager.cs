using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;
using WebStudyServer.GAME;
using Protocol;

namespace WebStudyServer.Manager
{
    public partial class KingdomStructureManager : UserManagerBase<KingdomStructureModel>
    {
        public KingdomItemProto Prt { get; private set; }
        public KingdomStructureManager(UserRepo userRepo, KingdomStructureModel model, KingdomItemProto prt) : base(userRepo, model)
        {
            Prt = prt;
        }

        public KingdomStructureManager(UserRepo userRepo, KingdomStructureModel model) : base(userRepo, model)
        {
            Prt = APP.Prt.GetKingdomItemPrt(model.Num);
        }

        public void ValidChgAction(int cnt)
        {
            if (cnt > 0)
            {
                // 창고로 이동시켜야하므로 배치 상태인 것만 가능
                ReqHelper.ValidContext(_model.State != EKingdomItemState.STORED && _model.State != EKingdomItemState.NONE, "NOT_PLACED_KINGDOM_STRUCTURE", () => new { State = _model.State });
            }
            else if (cnt < 0)
            {
                // 배치해야하므로 보유 상태인 것만 가능
                ReqHelper.ValidContext(_model.State == EKingdomItemState.STORED, "PLACED_KINGDOM_STRUCTURE", () => new { State = _model.State });
            }
        }

        public void Construct()
        {
            _model.State = EKingdomItemState.CONSTRUCTING;
            _model.EndTime = _rpcContext.ServerTime + TimeSpan.FromSeconds(Prt.ConstructSec);

            if (Prt.ConstructSec == 0)
            {
                _model.State = EKingdomItemState.READY;
                _model.EndTime = DateTime.MinValue;
            }

            _userRepo.KingdomStructure.Update(_model);
        }

        public void SetReady(EKingdomItemState correctBefState)
        {
            ReqHelper.ValidContext(_model.State == correctBefState, "NOT_EQUAL_CORRECT_BEF_KINGDOM_STRUCTURE_STATE", () => new { State = _model.State, CorrectBefState = correctBefState });
            ReqHelper.ValidContext(_model.EndTime >= _rpcContext.ServerTime, "NOT_FINISHED_KINGDOM_STRUCTURE", () => new { EndTime = _model.EndTime, ServerTime = _rpcContext.ServerTime });

            _model.EndTime = DateTime.MinValue;
            _model.State = EKingdomItemState.READY;
            _userRepo.KingdomStructure.Update(_model);
        }

        public void Store()
        {
            _model.State = EKingdomItemState.STORED;
            _model.EndTime = DateTime.MinValue;
            _userRepo.KingdomStructure.Update(_model);
        }

        public void Place()
        {
            _model.State = EKingdomItemState.READY;
            _model.EndTime = DateTime.MinValue;
            _userRepo.KingdomStructure.Update(_model);
        }

        public void DecTime()
        {
            _model.EndTime = DateTime.MinValue;
            _model.State = EKingdomItemState.READY;
            _userRepo.KingdomStructure.Update(_model);
        }
    }
}
