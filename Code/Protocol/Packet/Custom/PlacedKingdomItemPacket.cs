using ProtoBuf;
using Proto;

// NOTE: 생성기 대상이 아니다. Data/Csv/Model/User/PlacedKingdomItem.csv 는 제거되었다.
//
// 원래 Model CSV로 관리했으나 DB 테이블도 모델 클래스도 없는 Packet 전용이었다
// (전 컬럼 ProtocolType = Packet -> 생성기가 모델도 테이블도 만들지 않는다).
// CSV에 남겨두면 "테이블이 있는 엔티티"로 오인되고, 실제로 쓰이지 않는
// Model/Manager/Component 잔해가 붙어 있었다.
//
// ProtoMember 번호를 바꾸지 말 것. 이 타입은 와이어 전송뿐 아니라
// KingdomMapSnapshotPacket 에 담겨 KingdomMap.Snapshot 컬럼에 직렬화되어
// 저장된다. 번호가 밀리면 이미 저장된 스냅샷이 깨진다.
namespace Protocol
{
    [ProtoContract]
    public partial class PlacedKingdomItemPacket
    {
        [ProtoMember(1)]
        public ulong Id { get; set; } = default;

        [ProtoMember(2)]
        public ulong PlayerId { get; set; } = default;

        [ProtoMember(3)]
        public ulong StructureItemId { get; set; } = default;

        [ProtoMember(4)]
        public EKingdomItemType Type { get; set; } = default;

        [ProtoMember(5)]
        public int Num { get; set; } = default;

        [ProtoMember(6)]
        public EPlacedKingdomItemState State { get; set; } = default;

        [ProtoMember(7)]
        public int StartTileX { get; set; } = default;

        [ProtoMember(8)]
        public int StartTileY { get; set; } = default;

        [ProtoMember(9)]
        public int SizeX { get; set; } = default;

        [ProtoMember(10)]
        public int SizeY { get; set; } = default;

        [ProtoMember(11)]
        public int Rotation { get; set; } = default;
    }
}
