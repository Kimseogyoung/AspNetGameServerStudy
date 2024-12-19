﻿using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;

namespace WebStudyServer.Manager
{
    public class PlayerManager : UserManagerBase<PlayerModel>
    {
        public ulong Id => Model.Id;

        public PlayerManager(UserRepo userRepo, PlayerModel model) : base(userRepo, model)
        {
        }

        public void PreparePlayer()
        {
            // Player 초기 세팅
            Model.State = Proto.EPlayerState.PREPARED;
            _userRepo.UpdatePlayer(Model);
        }

        public void ValidState(EPlayerState state)
        {
            ReqHelper.ValidContext(Model.State < state, "ALREADY_PASSED_PLAYER_STATE", () => new { MdlState = Model.State, ValState = state });
        }

        public void ChangeName(string name)
        {
            Model.ProfileName = name;
            _userRepo.UpdatePlayer(Model);
        }
    }
}
