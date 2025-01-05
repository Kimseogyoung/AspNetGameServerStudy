using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;
using WebStudyServer.GAME;
using AutoMapper;
using Protocol.Packet.Custom;
using Protocol;

namespace WebStudyServer.Manager
{
    public partial class PlayerManager : UserManagerBase<PlayerModel>
    {
        public ulong Id => Model.Id;
       
        public PlayerManager(UserRepo userRepo, PlayerModel model) : base(userRepo, model)
        {
        }

        public PlayerPacket PreparePlayer(IMapper mapper)
        {
            // Player 초기 세팅
            var pakDefaultPlayer = APP.Cfg.PakDefaultPlayer;
            
            // ------------------------------------------------------------ 디폴트 모델 생성           
            // PlayerDetail
            var newMdlPlayerDetail = mapper.Map<PlayerDetailModel>(pakDefaultPlayer);
            newMdlPlayerDetail.PlayerId = _rpcContext.PlayerId;
            var mdlPlayerDetail = _userRepo.PlayerDetail.CreateMdl(newMdlPlayerDetail);

            // Cookie
            var mdlCookieList = new List<CookieModel>();
            foreach (var pakCookie in pakDefaultPlayer.CookieList)
            {
                var newMdlCookie = mapper.Map<CookieModel>(pakCookie);
                newMdlCookie.PlayerId = _rpcContext.PlayerId;
                var mdlCookie = _userRepo.Cookie.CreateMdl(newMdlCookie);
                mdlCookieList.Add(mdlCookie);
            }

            // KingdomStructure
            var mdlKindgomStructureList = new List<KingdomStructureModel>();
            foreach (var pakKingdomStructure in pakDefaultPlayer.KingdomStructureList)
            {
                var newMdlKingdomStructure = mapper.Map<KingdomStructureModel>(pakKingdomStructure);
                newMdlKingdomStructure.PlayerId = _rpcContext.PlayerId;
                var mdlKingdomStructure = _userRepo.KingdomStructure.CreateMdl(newMdlKingdomStructure);
                mdlKindgomStructureList.Add(mdlKingdomStructure);
            }

            // KingdomDeco
            var mdlKindgomDecoList = new List<KingdomDecoModel>();
            foreach (var pakKingdomDeco in pakDefaultPlayer.KingdomDecoList)
            {
                var newMdlKingdomDeco = mapper.Map<KingdomDecoModel>(pakKingdomDeco);
                newMdlKingdomDeco.PlayerId = _rpcContext.PlayerId;
                var mdlKingdomDeco = _userRepo.KingdomDeco.CreateMdl(newMdlKingdomDeco);
                mdlKindgomDecoList.Add(mdlKingdomDeco);
            }

            // KingdomMap
            var (newMdlKingdomMap, kingdomSnapshot) = KingdomMapManager.CreateKingdomMapModelDummy(pakDefaultPlayer.KingdomMap, mdlKindgomStructureList);
            newMdlKingdomMap.PlayerId = _rpcContext.PlayerId;
            var mdlKingdomMap = _userRepo.KingdomMap.CreateMdl(newMdlKingdomMap);

            _model.Lv = pakDefaultPlayer.Lv;
            _model.CastleLv = pakDefaultPlayer.CastleLv;
            _model.ProfileTitleNum = pakDefaultPlayer.ProfileTitleNum;
            _model.ProfileIconNum = pakDefaultPlayer.ProfileIconNum;
            _model.ProfileFrameNum = pakDefaultPlayer.ProfileFrameNum;
            _model.ProfileCookieNum = pakDefaultPlayer.ProfileCookieNum;
            _model.KingdomExp = pakDefaultPlayer.KingdomExp;
            _model.State = EPlayerState.PREPARED;
            _userRepo.Player.UpdateMdl(_model);
            // ------------------------------------------------------------ 디폴트 모델 생성 완료

            // ------------------------------------------------------------ 패킷 생성
            var pakPlayer = mapper.Map<PlayerPacket>(_model);
            pakPlayer.Gold = mdlPlayerDetail.Gold;
            pakPlayer.AccGold = mdlPlayerDetail.AccGold;
            pakPlayer.RealCash = mdlPlayerDetail.RealCash;
            pakPlayer.FreeCash = mdlPlayerDetail.FreeCash;
            pakPlayer.AccRealCash= mdlPlayerDetail.AccRealCash;
            pakPlayer.AccFreeCash = mdlPlayerDetail.AccFreeCash;

            pakPlayer.CookieList = mapper.Map<List<CookiePacket>>(mdlCookieList);
            pakPlayer.KingdomStructureList = mapper.Map<List<KingdomStructurePacket>>(mdlKindgomStructureList);
            pakPlayer.KingdomDecoList = mapper.Map<List<KingdomDecoPacket>>(mdlKindgomDecoList);
            pakPlayer.KingdomMap = new KingdomMapPacket
            {
                State = mdlKingdomMap.State,
                SizeX = mdlKingdomMap.SizeX,
                SizeY = mdlKingdomMap.SizeY,
                PlacedKingdomItemList = kingdomSnapshot.PlacedObjDict.Values.ToList()
            };

            return pakPlayer;
        }

        public PlayerPacket LoadPlayer(IMapper mapper)
        {
            var pakPlayer = mapper.Map<PlayerPacket>(_model);

            var mdlPlayerDetail = _userRepo.PlayerDetail.Touch();
            pakPlayer.Gold = mdlPlayerDetail.Model.Gold;
            pakPlayer.AccGold = mdlPlayerDetail.Model.AccGold;
            pakPlayer.RealCash = mdlPlayerDetail.Model.RealCash;
            pakPlayer.FreeCash = mdlPlayerDetail.Model.FreeCash;
            pakPlayer.AccRealCash = mdlPlayerDetail.Model.AccRealCash;
            pakPlayer.AccFreeCash = mdlPlayerDetail.Model.AccFreeCash;

            var mdlCookieList = _userRepo.Cookie.GetMdlList();
            pakPlayer.CookieList = mapper.Map<List<CookiePacket>>(_userRepo.Cookie.GetMdlList());
            pakPlayer.PointList = mapper.Map<List<PointPacket>>(_userRepo.Point.GetMdlList());
            pakPlayer.TicketList = mapper.Map<List<TicketPacket>>(_userRepo.Ticket.GetMdlList());
            pakPlayer.ItemList = mapper.Map<List<ItemPacket>>(_userRepo.Item.GetMdlList());
            pakPlayer.KingdomStructureList = mapper.Map<List<KingdomStructurePacket>>(_userRepo.KingdomStructure.GetMdlList());
            pakPlayer.KingdomDecoList = mapper.Map<List<KingdomDecoPacket>>(_userRepo.KingdomDeco.GetMdlList());

            var mgrKingdomMap = _userRepo.KingdomMap.Touch();
            pakPlayer.KingdomMap = new KingdomMapPacket
            {
                State = mgrKingdomMap.Model.State,
                SizeX = mgrKingdomMap.Model.SizeX,
                SizeY = mgrKingdomMap.Model.SizeY,
                PlacedKingdomItemList = mgrKingdomMap.Snapshot.PlacedObjDict.Values.ToList()
            };

            return pakPlayer;
        }

        public bool IsValidState(EPlayerState state)
        {
            return Model.State <= state;
        }

        public void ValidState(EPlayerState state)
        {
            ReqHelper.ValidContext(IsValidState(state), "ALREADY_PASSED_PLAYER_STATE", () => new { MdlState = Model.State, ValState = state });
        }

        public void ChangeName(string name)
        {
            Model.ProfileName = name;
            _userRepo.Player.UpdateMdl(Model);
        }
    }
}
