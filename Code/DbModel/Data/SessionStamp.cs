namespace WebStudyServer.Data
{
    // 요청에서 세션에 찍히는 값들. Transport 인접 계층이 만든다.
    // 데이터 계층이 IGameContext 를 직접 읽지 않게 하려고 값으로 끊었다.
    public readonly record struct SessionStamp(DateTime ServerTime, string Ip, string DeviceKey);
}
