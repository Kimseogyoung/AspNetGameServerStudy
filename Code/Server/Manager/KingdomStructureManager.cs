using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;
using WebStudyServer.GAME;

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

        public void Construct(int startTileX, int startTileY, int endTileX, int endTileY)
        {
/*            _model.StartTileX = startTileX;
            _model.StartTileY = startTileY;
            _model.EndTileX = endTileX;
            _model.EndTileY = endTileY;*/
            _model.State = EKingdomItemState.CONSTRUCTING;
            _model.EndTime = _rpcContext.ServerTime + TimeSpan.FromSeconds(Prt.ConstructSec);
            _userRepo.KingdomStructure.Update(_model);
        }

        public void ConstructDone()
        {
            _model.EndTime = DateTime.MinValue;
            _model.State = EKingdomItemState.READY;
            _userRepo.KingdomStructure.Update(_model);
        }

        public void Cancel()
        {
/*            _model.StartTileX = 0;
            _model.StartTileY = 0;
            _model.EndTileX = 0;
            _model.EndTileY = 0;*/
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
