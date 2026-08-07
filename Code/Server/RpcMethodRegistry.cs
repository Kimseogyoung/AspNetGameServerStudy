namespace Server
{
    // 부팅 시 한 번 만들어지는 불변 라우트 테이블. 싱글턴.
    // RpcService(요청마다 달라지는 상태를 다루는 Scoped)와 생명주기를 분리하기 위해 별도 클래스로 둔다.
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

        private readonly Dictionary<string, IRpcMethod> _nameToMethodDict = [];
    }
}
