namespace ServerCore
{
    // 버그로 인한 예상 못한 예외가 아니라, 서버가 의도적으로 던지는(에러코드로 응답해야 하는) 예외.
    public interface IServerExpectedException
    {
        int ErrorCode { get; }
        object ErrorArgs { get; }
    }
}
