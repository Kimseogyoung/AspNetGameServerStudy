using Protocol;
using ServerCore.Serializer;

namespace Server
{
    // 부팅 시 한 번 만들어지는 불변 라우트 테이블 + 직렬화기 레지스트리. 싱글턴.
    // RpcService(요청마다 달라지는 상태를 다루는 Scoped)와 생명주기를 분리하기 위해 별도 클래스로 둔다.
    // 직렬화기(JsonDataSerializer/ProtoBufDataSerializer)는 상태가 없어서, RpcService가
    // Scoped라 매 요청 새로 만들어지는 것과 달리 여기서는 부팅 시 1번만 만들면 된다.
    public class RpcMethodRegistry
    {
        public RpcMethodRegistry(List<IRpcMethod> methodList)
        {
            foreach (var method in methodList)
            {
                _nameToMethodDict.Add(method.Name, method);
            }
        }

        public IReadOnlyDictionary<string, IRpcMethod> NameToMethodDict => _nameToMethodDict;
        public IReadOnlyDictionary<string, IDataSerializer> ContentTypeToSerializerDict => _contentTypeToSerializerDict;

        private readonly Dictionary<string, IRpcMethod> _nameToMethodDict = [];

        private readonly Dictionary<string, IDataSerializer> _contentTypeToSerializerDict = new()
        {
            {MsgProtocol.JsonContentType, new JsonDataSerializer()},
            {MsgProtocol.ProtoBufContentType, new ProtoBufDataSerializer()},
        };
    }
}
