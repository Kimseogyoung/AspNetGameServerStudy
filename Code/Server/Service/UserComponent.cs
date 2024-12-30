using WebStudyServer.Component;
using WebStudyServer.Repo;

namespace WebStudyServer.Service
{
    public class UserComponent// 이름 고민중.
    {
        public PlayerComponent Player => _playerComponent;
        public PlayerDetailComponent PlayerDetail => _playerDetailComponent;

        public UserComponent(UserRepo userRepo, PlayerComponent playerComponent, PlayerDetailComponent playerDetailComponent)
        {
            _userRepo = userRepo;

            // TODO: Lazy형태로 ㄱㄱ
            _playerComponent = playerComponent;
            _playerDetailComponent = playerDetailComponent;
        }


        private readonly UserRepo _userRepo;
        private readonly PlayerComponent _playerComponent;
        private readonly PlayerDetailComponent _playerDetailComponent;
    }
}
