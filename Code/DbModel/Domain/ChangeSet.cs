using Proto;

namespace WebStudyServer.Model
{
    // 재화 하나가 얼마나 변했는지. 서버 런타임 전용이라 직렬화 대상이 아니다.
    // 와이어 매핑은 Server.Extension.ChangeSetExtension 에 있다.
    //
    // Type/Num 은 요청이 지목한 ObjKey 를 그대로 실어 나른다. 모델이 정하지 않는다 -
    // SOUL_STONE 은 쿠키 모델을 바꾸지만 응답에는 소울스톤 번호가 실려야 한다.
    public readonly record struct ChangeSet(EObjType Type, int Num, double Before, double After)
    {
        public double Delta => After - Before;

        public static ChangeSet Of(EObjType type, int num, double before, double after)
        {
            return new ChangeSet(type, num, before, after);
        }
    }
}
