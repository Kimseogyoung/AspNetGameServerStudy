using AutoMapper;
using Proto;
using Protocol;
using Protocol.Packet.Custom;
using ServerCore;
using ServerCore.Helper;
using WebStudyServer.Data;
using WebStudyServer.Helper;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Manager
{
    public partial class PlayerManager : UserManagerBase<PlayerModel>
    {
        public ulong Id => Model.Id;

        public PlayerManager(UserRepo userRepo, PlayerModel model) : base(userRepo, model)
        {
        }

        public async Task<PlayerPacket> PreparePlayerAsync(IMapper mapper, UserScope userScope)
        {
            // Player 초기 세팅
            var pakDefaultPlayer = Config<GameConfig>.Get().PakDefaultPlayer;

            // ------------------------------------------------------------ 디폴트 모델 생성
            // PlayerDetail
            var newMdlPlayerDetail = mapper.Map<PlayerDetailModel>(pakDefaultPlayer);
            newMdlPlayerDetail.PlayerId = RpcCtx.PlayerId;
            var mdlPlayerDetail = await _userRepo.PlayerDetail.CreateMdlAsync(newMdlPlayerDetail);

            // Cookie
            var cookieSet = userScope.Owned<CookieModel>();
            var mdlCookieList = new List<CookieModel>();
            foreach (var pakCookie in pakDefaultPlayer.CookieList)
            {
                var newMdlCookie = mapper.Map<CookieModel>(pakCookie);
                var mdlCookie = await cookieSet.CreateAsync(newMdlCookie);
                mdlCookieList.Add(mdlCookie);
            }

            // KingdomStructure
            var mdlKindgomStructureList = new List<KingdomStructureModel>();
            foreach (var pakKingdomStructure in pakDefaultPlayer.KingdomStructureList)
            {
                var newMdlKingdomStructure = mapper.Map<KingdomStructureModel>(pakKingdomStructure);
                newMdlKingdomStructure.PlayerId = RpcCtx.PlayerId;
                var mdlKingdomStructure = await _userRepo.KingdomStructure.CreateMdlAsync(newMdlKingdomStructure);
                mdlKindgomStructureList.Add(mdlKingdomStructure);
            }

            // KingdomDeco
            var mdlKindgomDecoList = new List<KingdomDecoModel>();
            foreach (var pakKingdomDeco in pakDefaultPlayer.KingdomDecoList)
            {
                var newMdlKingdomDeco = mapper.Map<KingdomDecoModel>(pakKingdomDeco);
                newMdlKingdomDeco.PlayerId = RpcCtx.PlayerId;
                var mdlKingdomDeco = await _userRepo.KingdomDeco.CreateMdlAsync(newMdlKingdomDeco);
                mdlKindgomDecoList.Add(mdlKingdomDeco);
            }

            // KingdomMap
            var (newMdlKingdomMap, kingdomSnapshot) = KingdomMapManager.CreateKingdomMapModelDummy(pakDefaultPlayer.KingdomMap, mdlKindgomStructureList);
            newMdlKingdomMap.PlayerId = RpcCtx.PlayerId;
            var mdlKingdomMap = await _userRepo.KingdomMap.CreateMdlAsync(newMdlKingdomMap);

            _model.Lv = pakDefaultPlayer.Lv;
            _model.CastleLv = pakDefaultPlayer.CastleLv;
            _model.ProfileName = IdHelper.GenerateRandomName();
            _model.ProfileTitleNum = pakDefaultPlayer.ProfileTitleNum;
            _model.ProfileIconNum = pakDefaultPlayer.ProfileIconNum;
            _model.ProfileFrameNum = pakDefaultPlayer.ProfileFrameNum;
            _model.ProfileCookieNum = pakDefaultPlayer.ProfileCookieNum;
            _model.KingdomExp = pakDefaultPlayer.KingdomExp;
            _model.State = EPlayerState.PREPARED;
            await _userRepo.Player.UpdateMdlAsync(_model);
            // ------------------------------------------------------------ 디폴트 모델 생성 완료

            // ------------------------------------------------------------ 패킷 생성
            var pakPlayer = mapper.Map<PlayerPacket>(_model);
            pakPlayer.Gold = mdlPlayerDetail.Gold;
            pakPlayer.AccGold = mdlPlayerDetail.AccGold;
            pakPlayer.RealCash = mdlPlayerDetail.RealCash;
            pakPlayer.FreeCash = mdlPlayerDetail.FreeCash;
            pakPlayer.AccRealCash = mdlPlayerDetail.AccRealCash;
            pakPlayer.AccFreeCash = mdlPlayerDetail.AccFreeCash;

            pakPlayer.CookieList = mapper.Map<List<CookiePacket>>(mdlCookieList);
            pakPlayer.KingdomStructureList = mapper.Map<List<KingdomStructurePacket>>(mdlKindgomStructureList);
            pakPlayer.KingdomDecoList = mapper.Map<List<KingdomDecoPacket>>(mdlKindgomDecoList);
            pakPlayer.KingdomMap = new KingdomMapPacket
            {
                State = mdlKingdomMap.State,
                SizeX = mdlKingdomMap.SizeX,
                SizeY = mdlKingdomMap.SizeY,
                PlacedKingdomItemList = [.. kingdomSnapshot.PlacedObjDict.Values]
            };

            return pakPlayer;
        }

        public async Task<PlayerPacket> LoadPlayerAsync(IMapper mapper, UserScope userScope)
        {
            var pakPlayer = mapper.Map<PlayerPacket>(_model);

            var mdlPlayerDetail = await _userRepo.PlayerDetail.TouchAsync(userScope);
            pakPlayer.Gold = mdlPlayerDetail.Model.Gold;
            pakPlayer.AccGold = mdlPlayerDetail.Model.AccGold;
            pakPlayer.RealCash = mdlPlayerDetail.Model.RealCash;
            pakPlayer.FreeCash = mdlPlayerDetail.Model.FreeCash;
            pakPlayer.AccRealCash = mdlPlayerDetail.Model.AccRealCash;
            pakPlayer.AccFreeCash = mdlPlayerDetail.Model.AccFreeCash;

            pakPlayer.CookieList = mapper.Map<List<CookiePacket>>(await userScope.Owned<CookieModel>().GetListAsync());
            pakPlayer.PointList = mapper.Map<List<PointPacket>>(await userScope.Owned<PointModel>().GetListAsync());
            pakPlayer.TicketList = mapper.Map<List<TicketPacket>>(await userScope.Owned<TicketModel>().GetListAsync());
            pakPlayer.ItemList = mapper.Map<List<ItemPacket>>(await userScope.Owned<ItemModel>().GetListAsync());
            pakPlayer.KingdomStructureList = mapper.Map<List<KingdomStructurePacket>>(await _userRepo.KingdomStructure.GetMdlListAsync());
            pakPlayer.KingdomDecoList = mapper.Map<List<KingdomDecoPacket>>(await _userRepo.KingdomDeco.GetMdlListAsync());

            var mgrKingdomMap = await _userRepo.KingdomMap.TouchAsync();
            pakPlayer.KingdomMap = new KingdomMapPacket
            {
                State = mgrKingdomMap.Model.State,
                SizeX = mgrKingdomMap.Model.SizeX,
                SizeY = mgrKingdomMap.Model.SizeY,
                PlacedKingdomItemList = [.. mgrKingdomMap.Snapshot.PlacedObjDict.Values]
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

        public async Task ChangeNameAsync(string name)
        {
            Model.ProfileName = name;
            await _userRepo.Player.UpdateMdlAsync(Model);
        }
    }
}
