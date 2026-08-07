namespace WebStudyServer.Middleware
{
    // 요청 컨텍스트(RpcContext) 초기화 + 점검모드 판정만 담당한다.
    // 예외는 여기서 잡지 않고 전역 UseExceptionHandler(ErrorHandler)로 위임한다.
    public class ReqMiddleware
    {
        public ReqMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpCtx)
        {
            CancelReqException.ThrowCancelRequestException(httpCtx);

            var rpcContext = httpCtx.RequestServices.GetRequiredService<RpcContext>();
            rpcContext.Init(httpCtx);

            await _next(httpCtx);
        }

        private readonly RequestDelegate _next;

    }
}
