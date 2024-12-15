﻿using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Helper;

namespace WebStudyServer.Component
{
    public class PlayerComponent : UserComponentBase
    {
        public PlayerComponent(UserRepo userRepo) : base(userRepo)
        {
        }

        public bool TryCreatePlayer(out PlayerManager mgrPlayer)
        {
            var playerId = _userRepo.RpcContext.PlayerId;
            var isCreatePlayer = playerId == 0;

            PlayerModel mdlPlayer = null;
            if (isCreatePlayer)
            {
                // 신규 플레이어 생성
                mdlPlayer = _userRepo.CreatePlayer(new PlayerModel
                {
                    AccountId = _userRepo.RpcContext.AccountId,
                    Lv = 1,
                    ProfileName = IdHelper.GenerateRandomName(), // TODO: 중복 닉네임 제한 하고싶음.
                });
                _userRepo.RpcContext.SetPlayerId(mdlPlayer.Id);
            }
            else
            {
                // 기존 플레이어 로드
                mdlPlayer = _userRepo.GetPlayer(playerId);
            }

            if (mdlPlayer == null)
                throw new Exception("NOT_FOUND_PLAYER"); // TODO:  오류 발생

            mgrPlayer = new PlayerManager(_userRepo, mdlPlayer);
            return isCreatePlayer;
        }
    }
}
