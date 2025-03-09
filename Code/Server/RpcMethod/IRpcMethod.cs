using Microsoft.OpenApi.Models;
using WebStudyServer;

namespace Server
{
    public interface IRpcMethod
    {
        public Type Req { get; }
        public Type Res { get; }
        string Name { get; }
        Task<object> RunAsync(RpcContext rpcCtx, HttpContext httpCtx, object rpcReq);
        List<OpenApiParameter> CreateOpenApiParameters();
        OpenApiRequestBody CreateOpenApiRequestBody();
        OpenApiResponses CreateOpenApiResponse();
    }
}
