using ServerCore.Serializer;

namespace RaidServer.Network
{
    // Protocol.Raid.EProtocolType(와이어 프로토콜) <-> IDataSerializer 레지스트리.
    // PacketProcessor(수신 역직렬화)와 SessionService(송신 직렬화) 양쪽에서 공유.
    // RpcService._contentTypeToSerializerDict와 동일한 패턴.
    public class PacketSerializerProvider
    {
        public IDataSerializer Get(Protocol.Raid.EProtocolType protocolType)
        {
            var key = (EProtocolType)(byte)protocolType;
            if (!_serializerDict.TryGetValue(key, out var serializer))
            {
                throw new Exception($"NOT_FOUND_SERIALIZER ProtocolType({protocolType})");
            }

            return serializer;
        }

        private readonly Dictionary<EProtocolType, IDataSerializer> _serializerDict = new()
        {
            { EProtocolType.Json, new JsonDataSerializer() },
            { EProtocolType.Protobuf, new ProtoBufDataSerializer() },
        };
    }
}
