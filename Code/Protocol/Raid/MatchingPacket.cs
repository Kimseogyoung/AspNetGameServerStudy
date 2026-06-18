using ProtoBuf;

namespace Protocol.Raid
{
    public enum EMatchingResult
    {
        Success,
        AlreadyMatching,
        AlreadyInRoom,
        NotMatching,
        InvalidBoss,
    }

    [ProtoContract]
    public class RoomMemberInfo
    {
        [ProtoMember(1)] public ulong SfId { get; set; }
        [ProtoMember(2)] public string ProfileName { get; set; } = string.Empty;
    }

    [ProtoContract]
    public class MatchingStartReqPacket
    {
        [ProtoMember(1)] public int BossNum { get; set; }
    }

    [ProtoContract]
    public class MatchingStartResPacket
    {
        [ProtoMember(1)] public EMatchingResult Result { get; set; }
    }

    [ProtoContract]
    public class MatchingCancelReqPacket { }

    [ProtoContract]
    public class MatchingCancelResPacket
    {
        [ProtoMember(1)] public EMatchingResult Result { get; set; }
    }

    [ProtoContract]
    public class MatchingCompleteNotifyPacket
    {
        [ProtoMember(1)] public string RoomId { get; set; } = string.Empty;
        [ProtoMember(2)] public int BossNum { get; set; }
        [ProtoMember(3)] public List<RoomMemberInfo> Members { get; set; } = new();
    }
}
