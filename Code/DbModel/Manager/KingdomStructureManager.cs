using Proto;
using Protocol;
using WebStudyServer.Helper;
using WebStudyServer.Model;
using WebStudyServer.Repo;

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
            Prt = ProtoDb.Get<KingdomItemProto>(model.Num);
        }

        public void ValidChgAction(int cnt)
        {
            if (cnt > 0)
            {
                // 창고로 이동시켜야하므로 배치 상태인 것만 가능
                ReqHelper.ValidContext(_model.State is not EKingdomItemState.STORED and not EKingdomItemState.NONE, "NOT_PLACED_KINGDOM_STRUCTURE", () => new { _model.State });
            }
            else if (cnt < 0)
            {
                // 배치해야하므로 보유 상태인 것만 가능
                ReqHelper.ValidContext(_model.State == EKingdomItemState.STORED, "PLACED_KINGDOM_STRUCTURE", () => new { _model.State });
            }
        }

        public async Task ConstructAsync()
        {
            _model.State = EKingdomItemState.CONSTRUCTING;
            _model.EndTime = RpcCtx.ServerTime + TimeSpan.FromSeconds(Prt.ConstructSec);

            if (Prt.ConstructSec == 0)
            {
                _model.State = EKingdomItemState.READY;
                _model.EndTime = DateTime.MinValue;
            }

            await _userRepo.KingdomStructure.UpdateMdlAsync(_model);
        }

        public async Task SetReadyAsync(EKingdomItemState correctBefState)
        {
            ReqHelper.ValidContext(_model.State == correctBefState, "NOT_EQUAL_CORRECT_BEF_KINGDOM_STRUCTURE_STATE", () => new { _model.State, CorrectBefState = correctBefState });
            ReqHelper.ValidContext(_model.EndTime >= RpcCtx.ServerTime, "NOT_FINISHED_KINGDOM_STRUCTURE", () => new { _model.EndTime, RpcCtx.ServerTime });

            _model.EndTime = DateTime.MinValue;
            _model.State = EKingdomItemState.READY;
            await _userRepo.KingdomStructure.UpdateMdlAsync(_model);
        }

        public async Task StoreAsync()
        {
            _model.State = EKingdomItemState.STORED;
            _model.EndTime = DateTime.MinValue;
            await _userRepo.KingdomStructure.UpdateMdlAsync(_model);
        }

        public async Task PlaceAsync()
        {
            _model.State = EKingdomItemState.READY;
            _model.EndTime = DateTime.MinValue;
            await _userRepo.KingdomStructure.UpdateMdlAsync(_model);
        }

        public async Task DecTimeAsync()
        {
            _model.EndTime = DateTime.MinValue;
            _model.State = EKingdomItemState.READY;
            await _userRepo.KingdomStructure.UpdateMdlAsync(_model);
        }
    }
}
