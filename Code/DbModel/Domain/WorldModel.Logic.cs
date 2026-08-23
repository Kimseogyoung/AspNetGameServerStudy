using Proto;

namespace WebStudyServer.Model
{
    public partial class WorldModel
    {
        // 진행 상태. 마지막 스테이지를 깨면 이 값이 된다.
        private const int FinishState = 10;

        public bool IsFinish()
        {
            return State == FinishState;
        }

        // 아직 안 깬 첫 스테이지. 없으면 이 월드는 끝난 것이다.
        //
        // GetByMk 는 CSV 행 순서를 그대로 돌려주므로 WorldStage.csv 가 Order 오름차순이라는
        // 전제 위에 있다. 행 순서가 바뀌면 "다음 스테이지"와 아래 Last() 의 "마지막"이 어긋난다.
        public bool TryGetTopOpenStagePrt(out WorldStageProto prtNextStage)
        {
            prtNextStage = ProtoDb.GetByMk<WorldStageProto>(Num).FirstOrDefault(x => x.Order > TopFinishStageOrder);
            return prtNextStage != null;
        }

        public void FinishStage(WorldStageProto prtStage)
        {
            LastPlayStageNum = prtStage.Num;

            if (TopFinishStageOrder >= prtStage.Order)
            {
                return;
            }

            TopFinishStageOrder = prtStage.Order;
            TopFinishStageNum = prtStage.Num;

            // 마지막 스테이지를 깼으면 월드 종료
            if (ProtoDb.GetByMk<WorldStageProto>(Num).Last().Num == prtStage.Num)
            {
                State = FinishState;
            }
        }

        public void RewardStar(int aftRewardStar)
        {
            RecvStarReward = aftRewardStar;
        }
    }
}
