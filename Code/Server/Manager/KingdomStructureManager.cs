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

        public void FinishConstruct()
        {
            // TODO: 종료 시간 검증

            _model.EndTime = DateTime.MinValue;
            _model.State = EKingdomItemState.READY;
            _userRepo.KingdomStructure.Update(_model);
        }

        public void Store(PlacedKingdomItemPacket placedKingdomItem)
        {
            _model.State = EKingdomItemState.STORED;
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
