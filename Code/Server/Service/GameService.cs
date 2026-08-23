using AutoMapper;
using Proto;
using Protocol;
using Server.Repo;
using ServerCore;
using ServerCore.Helper;
using WebStudyServer;
using WebStudyServer.Data;
using WebStudyServer.Data.Queries;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace Server.Service
{
    public class GameService : ServiceBase
    {
        public GameService(GlobalDbRepo dbRepo, GameDb db, IMapper mapper, RpcContext rpcContext, ILogger<GameService> logger) : base(db, rpcContext, logger)
        {
            _dbRepo = dbRepo;
            _mapper = mapper;
        }

        public async Task<GameEnterResponsePacket> EnterAsync(GameEnterRequestPacket req)
        {
            // Player 는 스코프에 속한 것이 아니라 스코프의 소유자 자신이라, 스코프를 열기 전에
            // Id 가 정해져 있어야 한다. 0 인 스코프로 만들면 CreateAsync 가 Id 를 0 으로 덮는다.
            if (RpcContext.PlayerId == 0)
            {
                RpcContext.SetPlayerId(IdHelper.MakePlayerId(RpcContext.AccountId));
            }

            var playerSet = OwnScope.Owned<PlayerModel>();
            var mdlPlayer = await playerSet.GetOrCreateAsync(RpcContext.AccountId);

            if (mdlPlayer.State >= EPlayerState.PREPARED)
            {
                // Prepare 이후 접속시마다 처리해줘야할 것이 있으면 여기서 처리
                var pakPlayer = await LoadPlayerAsync(mdlPlayer);
                return new GameEnterResponsePacket
                {
                    Player = pakPlayer,
                };
            }
            else
            {
                var pakPlayer = await PreparePlayerAsync(mdlPlayer);

                var accountId = mdlPlayer.AccountId;
                await Db.Auth(accountId).CreatePlayerMapAsync(mdlPlayer.Id, OwnUser.ShardId);

                var (foundSession, mdlSession) = await Db.Sessions.TryGetByAccountIdAsync(accountId);
                if (foundSession && mdlSession.SetPlayerId(mdlPlayer.Id))
                {
                    await Db.Sessions.SaveAsync(mdlSession);
                }


                return new GameEnterResponsePacket
                {
                    Player = pakPlayer,
                };
            }
        }

        public async Task<GameChangeNameResponsePacket> ChangeNameFirstAsync(GameChangeNameRequestPacket req)
        {
            var reqName = req.PlayerName;
            var playerSet = OwnScope.Owned<PlayerModel>();
            var mdlPlayer = await playerSet.GetAsync();

            mdlPlayer.ValidState(EPlayerState.CHANGED_NAME_FIRST);

            // 중복 체크 (클라에 팝업)
            var (found, _) = await _dbRepo.AllUser.TryGetPlayerByNameAsync(reqName);
            ReqHelper.Valid(!found, EErrorCode.GAME_CHANGE_NAME_EXIST_NAME);

            // 변경
            mdlPlayer.ChangeName(reqName);
            await playerSet.UpdateAsync(mdlPlayer);

            return new GameChangeNameResponsePacket
            {
                PlayerName = mdlPlayer.ProfileName,
            };
        }

        private async Task<PlayerPacket> PreparePlayerAsync(PlayerModel mdlPlayer)
        {
            var userScope = OwnScope;

            // Player 초기 세팅
            var pakDefaultPlayer = Config<GameConfig>.Get().PakDefaultPlayer;

            // ------------------------------------------------------------ 디폴트 모델 생성
            // PlayerDetail
            var newMdlPlayerDetail = _mapper.Map<PlayerDetailModel>(pakDefaultPlayer);
            var mdlPlayerDetail = await userScope.Owned<PlayerDetailModel>().CreateAsync(newMdlPlayerDetail);

            // Cookie
            var cookieSet = userScope.Owned<CookieModel>();
            var mdlCookieList = new List<CookieModel>();
            foreach (var pakCookie in pakDefaultPlayer.CookieList)
            {
                var newMdlCookie = _mapper.Map<CookieModel>(pakCookie);
                var mdlCookie = await cookieSet.CreateAsync(newMdlCookie);
                mdlCookieList.Add(mdlCookie);
            }

            // KingdomStructure
            var mdlKindgomStructureList = new List<KingdomStructureModel>();
            foreach (var pakKingdomStructure in pakDefaultPlayer.KingdomStructureList)
            {
                var newMdlKingdomStructure = _mapper.Map<KingdomStructureModel>(pakKingdomStructure);
                newMdlKingdomStructure.PlayerId = RpcContext.PlayerId;
                var mdlKingdomStructure = await OwnUser.KingdomStructure.CreateMdlAsync(newMdlKingdomStructure);
                mdlKindgomStructureList.Add(mdlKingdomStructure);
            }

            // KingdomDeco
            var mdlKindgomDecoList = new List<KingdomDecoModel>();
            foreach (var pakKingdomDeco in pakDefaultPlayer.KingdomDecoList)
            {
                var newMdlKingdomDeco = _mapper.Map<KingdomDecoModel>(pakKingdomDeco);
                newMdlKingdomDeco.PlayerId = RpcContext.PlayerId;
                var mdlKingdomDeco = await OwnUser.KingdomDeco.CreateMdlAsync(newMdlKingdomDeco);
                mdlKindgomDecoList.Add(mdlKingdomDeco);
            }

            // KingdomMap
            var (newMdlKingdomMap, kingdomSnapshot) = KingdomMapManager.CreateKingdomMapModelDummy(pakDefaultPlayer.KingdomMap, mdlKindgomStructureList);
            newMdlKingdomMap.PlayerId = RpcContext.PlayerId;
            var mdlKingdomMap = await OwnUser.KingdomMap.CreateMdlAsync(newMdlKingdomMap);

            mdlPlayer.Lv = pakDefaultPlayer.Lv;
            mdlPlayer.CastleLv = pakDefaultPlayer.CastleLv;
            mdlPlayer.ProfileName = IdHelper.GenerateRandomName();
            mdlPlayer.ProfileTitleNum = pakDefaultPlayer.ProfileTitleNum;
            mdlPlayer.ProfileIconNum = pakDefaultPlayer.ProfileIconNum;
            mdlPlayer.ProfileFrameNum = pakDefaultPlayer.ProfileFrameNum;
            mdlPlayer.ProfileCookieNum = pakDefaultPlayer.ProfileCookieNum;
            mdlPlayer.KingdomExp = pakDefaultPlayer.KingdomExp;
            mdlPlayer.State = EPlayerState.PREPARED;
            await OwnScope.Owned<PlayerModel>().UpdateAsync(mdlPlayer);
            // ------------------------------------------------------------ 디폴트 모델 생성 완료

            // ------------------------------------------------------------ 패킷 생성
            var pakPlayer = _mapper.Map<PlayerPacket>(mdlPlayer);
            pakPlayer.Gold = mdlPlayerDetail.Gold;
            pakPlayer.AccGold = mdlPlayerDetail.AccGold;
            pakPlayer.RealCash = mdlPlayerDetail.RealCash;
            pakPlayer.FreeCash = mdlPlayerDetail.FreeCash;
            pakPlayer.AccRealCash = mdlPlayerDetail.AccRealCash;
            pakPlayer.AccFreeCash = mdlPlayerDetail.AccFreeCash;

            pakPlayer.CookieList = _mapper.Map<List<CookiePacket>>(mdlCookieList);
            pakPlayer.KingdomStructureList = _mapper.Map<List<KingdomStructurePacket>>(mdlKindgomStructureList);
            pakPlayer.KingdomDecoList = _mapper.Map<List<KingdomDecoPacket>>(mdlKindgomDecoList);
            pakPlayer.KingdomMap = new KingdomMapPacket
            {
                State = mdlKingdomMap.State,
                SizeX = mdlKingdomMap.SizeX,
                SizeY = mdlKingdomMap.SizeY,
                PlacedKingdomItemList = [.. kingdomSnapshot.PlacedObjDict.Values]
            };

            return pakPlayer;
        }

        private async Task<PlayerPacket> LoadPlayerAsync(PlayerModel mdlPlayer)
        {
            var userScope = OwnScope;

            var pakPlayer = _mapper.Map<PlayerPacket>(mdlPlayer);

            var mdlPlayerDetail = await userScope.Owned<PlayerDetailModel>().GetOrCreateAsync();
            pakPlayer.Gold = mdlPlayerDetail.Gold;
            pakPlayer.AccGold = mdlPlayerDetail.AccGold;
            pakPlayer.RealCash = mdlPlayerDetail.RealCash;
            pakPlayer.FreeCash = mdlPlayerDetail.FreeCash;
            pakPlayer.AccRealCash = mdlPlayerDetail.AccRealCash;
            pakPlayer.AccFreeCash = mdlPlayerDetail.AccFreeCash;

            pakPlayer.CookieList = _mapper.Map<List<CookiePacket>>(await userScope.Owned<CookieModel>().GetListAsync());
            pakPlayer.PointList = _mapper.Map<List<PointPacket>>(await userScope.Owned<PointModel>().GetListAsync());
            pakPlayer.TicketList = _mapper.Map<List<TicketPacket>>(await userScope.Owned<TicketModel>().GetListAsync());
            pakPlayer.ItemList = _mapper.Map<List<ItemPacket>>(await userScope.Owned<ItemModel>().GetListAsync());
            pakPlayer.KingdomStructureList = _mapper.Map<List<KingdomStructurePacket>>(await OwnUser.KingdomStructure.GetMdlListAsync());
            pakPlayer.KingdomDecoList = _mapper.Map<List<KingdomDecoPacket>>(await OwnUser.KingdomDeco.GetMdlListAsync());

            // 읽기만 하므로 Manager 가 필요 없다. 배치/저장이 걸린 KingdomService 쪽은 S10 까지 Manager 를 쓴다.
            var mdlKingdomMap = await userScope.Owned<KingdomMapModel>().GetOrCreateAsync();
            pakPlayer.KingdomMap = new KingdomMapPacket
            {
                State = mdlKingdomMap.State,
                SizeX = mdlKingdomMap.SizeX,
                SizeY = mdlKingdomMap.SizeY,
                PlacedKingdomItemList = [.. mdlKingdomMap.ParseSnapshot().PlacedObjDict.Values]
            };

            return pakPlayer;
        }

        private UserRepo OwnUser => _dbRepo.OwnUser;

        private readonly GlobalDbRepo _dbRepo;
        private readonly IMapper _mapper;
    }
}
