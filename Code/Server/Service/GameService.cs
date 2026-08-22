using AutoMapper;
using Proto;
using Protocol;
using Server.Repo;
using WebStudyServer;
using WebStudyServer.Helper;
using WebStudyServer.Model;
using WebStudyServer.Data;
using WebStudyServer.Repo;
using WebStudyServer.Service;

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
            var mgrPlayer = await OwnUser.Player.TouchAsync();

            if (mgrPlayer.Model.State >= Proto.EPlayerState.PREPARED)
            {
                // Prepare 이후 접속시마다 처리해줘야할 것이 있으면 여기서 처리
                var pakPlayer = await mgrPlayer.LoadPlayerAsync(_mapper, OwnScope);
                return new GameEnterResponsePacket
                {
                    Player = pakPlayer,
                };
            }
            else
            {
                var pakPlayer = await mgrPlayer.PreparePlayerAsync(_mapper, OwnScope);

                var accountId = mgrPlayer.Model.AccountId;
                var authRepo = _dbRepo.Auth;
                await authRepo.PlayerMap.CreateAsync(new PlayerMapModel
                {
                    AccountId = accountId,
                    PlayerId = mgrPlayer.Id,
                    ShardId = OwnUser.ShardId,
                });

                var mdlSession = await authRepo.Session.TryGetByAccountIdAsync(accountId);
                if (mdlSession != null)
                {
                    await mdlSession.SetPlayerIdAsync(mgrPlayer.Id);
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
            var mgrPlayer = await OwnUser.Player.TouchAsync();

            mgrPlayer.ValidState(EPlayerState.CHANGED_NAME_FIRST);

            // 중복 체크 (클라에 팝업)
            var (found, _) = await _dbRepo.AllUser.TryGetPlayerByNameAsync(reqName);
            ReqHelper.Valid(!found, EErrorCode.GAME_CHANGE_NAME_EXIST_NAME);

            // 변경
            await mgrPlayer.ChangeNameAsync(reqName);

            return new GameChangeNameResponsePacket
            {
                PlayerName = mgrPlayer.Model.ProfileName,
            };
        }

        private UserRepo OwnUser => _dbRepo.OwnUser;

        private readonly GlobalDbRepo _dbRepo;
        private readonly IMapper _mapper;
    }
}
