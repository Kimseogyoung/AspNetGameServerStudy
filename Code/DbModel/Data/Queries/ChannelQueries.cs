using Proto;
using WebStudyServer.Model;

namespace WebStudyServer.Data.Queries
{
    // 로드한 컬렉션을 거르는 확장 메서드. 새 SQL 안 씀 - 여기서 쿼리를 날리면 캐시가 모르는
    // 두 번째 사본이 생기고 무효화 조건을 추론 못함.
    public static class ChannelQueries
    {
        public static ChannelModel Active(this List<ChannelModel> channels)
        {
            return channels.FirstOrDefault(x => x.State == EChannelState.ACTIVE);
        }
    }
}
