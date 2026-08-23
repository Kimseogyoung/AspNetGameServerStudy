using Protocol.Packet.Custom;
using ServerCore.Serializer;

namespace WebStudyServer.Model
{
    public partial class KingdomMapModel
    {
        // 푼 스냅샷을 Snapshot 컬럼에 다시 넣는다.
        public void SetSnapshot(KingdomMapSnapshotPacket snapshot)
        {
            Snapshot = JsonDataSerializer.SerializeStr(snapshot);
        }

        // Snapshot 컬럼(직렬화된 JSON)을 푼다. 프로퍼티로 두면 DapperExtension 이 이걸
        // DB 컬럼으로 보기 때문에 메서드여야 한다.
        public KingdomMapSnapshotPacket ParseSnapshot()
        {
            return string.IsNullOrEmpty(Snapshot)
                ? new KingdomMapSnapshotPacket()
                : JsonDataSerializer.DeserializeStr<KingdomMapSnapshotPacket>(Snapshot);
        }
    }
}
