using ProtoBuf;

namespace Protocol.Raid
{
    public enum EAuthResult
    {
        Success,
        SessionNotFound,
        SessionExpired,
        InvalidRequest,
    }

    [ProtoContract]
    public class AuthRequestPacket
    {
        [ProtoMember(1)]
        public string SessionKey { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string DeviceKey { get; set; } = string.Empty;
    }

    [ProtoContract]
    public class AuthResponsePacket
    {
        [ProtoMember(1)]
        public EAuthResult Result { get; set; }

        [ProtoMember(2)]
        public ulong AccountId { get; set; }

        [ProtoMember(3)]
        public ulong PlayerId { get; set; }

        [ProtoMember(4)]
        public int ShardId { get; set; }

        [ProtoMember(5)]
        public int PingIntervalSec { get; set; }
    }
}
