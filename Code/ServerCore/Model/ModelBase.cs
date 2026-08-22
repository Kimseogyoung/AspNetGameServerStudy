namespace ServerCore.Model
{
    public abstract class ModelBase
    {
        public DateTime UpdateTime { get; set; }
        public DateTime CreateTime { get; set; }

        // 같은 행인가. 캐시 리스트에서 항목을 교체할 때 쓴다.
        // virtual + 기본 구현으로 두면 생성기가 빠뜨린 모델이 조용히 참조 비교로 떨어진다.
        public abstract bool PkEquals(ModelBase other);
    }
}
