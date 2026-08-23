using Proto;
using WebStudyServer.Helper;

namespace WebStudyServer.Model
{
    // State 는 준비 단계를 나타내는 진행도라 되돌아가지 않는다. 그래서 비교가 <= 다.
    public partial class PlayerModel
    {
        public bool IsValidState(EPlayerState state)
        {
            return State <= state;
        }

        public void ValidState(EPlayerState state)
        {
            ReqHelper.ValidContext(IsValidState(state), "ALREADY_PASSED_PLAYER_STATE", () => new { MdlState = State, ValState = state });
        }

        public void ChangeName(string name)
        {
            ProfileName = name;
        }
    }
}
