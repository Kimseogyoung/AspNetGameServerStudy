namespace WebStudyServer.Data
{
    // 요청에서 세션에 찍히는 값들. Transport 인접 계층이 만들어 값으로 넘긴다.
    public readonly record struct SessionStamp(DateTime ServerTime, string Ip, string DeviceKey);
}
