using Protocol.Packet.Custom;
using ServerCore.Serializer;

namespace WebStudyServer.Model
{
    public partial class KingdomMapModel
    {
        // Snapshot 컬럼(직렬화된 JSON)을 푼다. 프로퍼티로 두면 DapperExtension 이 public
        // 프로퍼티를 DB 컬럼으로 보기 때문에 메서드여야 한다.
        //
        // 결과를 모델이 들고 있지 않는 이유는 저장 계약 때문이다. 들고 있으면 푼 것을 고친 뒤
        // 다시 직렬화하는 걸 잊어도 컴파일이 되고, 조용히 저장되지 않는다. 읽는 쪽은 그때그때
        // 풀고, 고쳐 쓰는 쪽은 KingdomMapManager 가 맡는다(S10 에서 정리).
        public KingdomMapSnapshotPacket ParseSnapshot()
        {
            return string.IsNullOrEmpty(Snapshot)
                ? new KingdomMapSnapshotPacket()
                : JsonDataSerializer.DeserializeStr<KingdomMapSnapshotPacket>(Snapshot);
        }
    }
}
