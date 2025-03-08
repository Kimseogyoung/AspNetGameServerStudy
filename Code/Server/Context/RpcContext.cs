using Microsoft.AspNetCore.Http;
using Proto;
using Protocol;
using System.Diagnostics;
using System.Net.Http;
using WebStudyServer.Component;
using WebStudyServer.Extension;
using WebStudyServer.Helper;
using WebStudyServer.Repo;

namespace WebStudyServer
{
    public class RpcContext
    {
        public string SessionKey { get; private set; } = string.Empty;
        public ESessionLoadState SessionLoadState { get; private set; } = ESessionLoadState.INITIALIZED;
        public ulong AccountId { get; private set; }
        public ulong PlayerId { get; private set; }
        public int ShardId { get; private set; }
        public DateTime ServerTime { get; private set; } = DateTime.UtcNow;
        public DateTime PlayerTime { get; private set; } = DateTime.UtcNow;

        // 요청 정보
        public long Seq { get; private set; }
        public string Ip { get; private set; } = string.Empty;
        public string DeviceKey { get; private set; } = string.Empty;
        public string HostUrl { get; private set; } = string.Empty;
        public string ApiHash { get; private set;} = string.Empty;
        public string ApiPath { get; private set; } = string.Empty;
        public long Timestamp { get; private set; }
        public string Country { get; private set; }

        public RpcContext(ILogger<RpcContext> logger)
        {
            _logger = logger;
        }

        public void Init(HttpContext httpContext)
        {
            _logger.Debug("Init RpcContext");

            // 요청 정보 로드
            SetSeq(httpContext);
            SetIp(httpContext);
            SetDeviceKey(httpContext);
            SetHostUrl(httpContext);
            SetApiHash(httpContext);
            SetTimestamp(httpContext);
            SetCountry(httpContext);

            // 세션 & 유저 정보 로드
            LoadSession(httpContext);
        }

        // 유저 정보
        public void SetAccountId(ulong accountId)
        {
            this.AccountId = accountId;
        }

        public void SetShardId(int shardId)
        {
            this.ShardId = shardId;
        }

        public void SetPlayerId(ulong playerId)
        {
            this.PlayerId = playerId;
        }

        public void SetSessionKey(string sessionKey)
        {
            this.SessionKey = sessionKey;
        }

        public void LoadSession(HttpContext httpContext)
        {
            if (SessionLoadState != ESessionLoadState.INITIALIZED)
            {
                _logger.Debug("SkipLoadSession");
                return;
            }

            _logger.Debug("LoadSession");
            SessionLoadState = ESessionLoadState.LOADED;

            var sessionKey = GetQueryValue(httpContext, MsgProtocol.Query_SessionKey);
            SetSessionKey(sessionKey);

            if (string.IsNullOrEmpty(sessionKey))
            {
                return;
            }

            // TODO: 점검 상태일때 세션 만료
            //

            var authRepo = AuthRepo.CreateInstance(this);
            authRepo.Init(0);
            var sessionComp = authRepo.Session;

            if (!sessionComp.TryGetByKey(sessionKey, out var mgrSession))
            {
                _logger.Error("NOT_FOUND_SESSION Key({Key})", sessionKey);
                SessionLoadState = ESessionLoadState.NOT_FOUND;
                return;
            }

            var isUpdate = mgrSession.Extend();

            if (mgrSession.IsExpire())
            {
                SessionLoadState = ESessionLoadState.EXPIRED;
                return;
            }

            // 세션 정보 저장
            SetPlayerId(mgrSession.Model.PlayerId);
            SetAccountId(mgrSession.Model.AccountId);
            SetShardId(mgrSession.Model.ShardId);

            authRepo.Commit();
        }

        // 요청 정보
        private void SetSeq(HttpContext httpContext)
        {
            var seq = GetQueryValue(httpContext, MsgProtocol.Query_Seq);
            this.Seq = string.IsNullOrEmpty(seq) ? 0 : long.Parse(seq);
        }

        private void SetIp(HttpContext httpContext)
        {
            var ip = GetIp(httpContext);
            this.Ip = ip;
        }

        private void SetDeviceKey(HttpContext httpContext)
        {
            var deviceKey = "";
            this.DeviceKey = deviceKey;
        }

        private void SetHostUrl(HttpContext httpContext)
        {
            var host = httpContext.Request.Host.ToString();
            var http = httpContext.Request.IsHttps ? "https" : "http";
            var hostUrl = $"{http}://{host}";
            this.HostUrl = hostUrl;
        }

        private void SetApiHash(HttpContext httpContext)
        {
            var urlPath = httpContext.Request.Path.ToString();
            this.ApiPath = urlPath;
            this.ApiHash = HashHelper.CalculateSha256Hash(urlPath).Substring(0, 10);
        }

        public void SetTimestamp(HttpContext httpContext)
        {
            var timestamp = GetQueryValue(httpContext, MsgProtocol.Query_Timestamp);
            this.Timestamp = string.IsNullOrEmpty(timestamp)? 0: long.Parse(timestamp);
        }

        public void SetCountry(HttpContext httpContext)
        {
            var country = GetHeaderValue(httpContext, "CloudFront-Viewer-Country");
            this.Country = country;
        }

        private string GetIp(HttpContext httpCtx, string forwardedHeaderKey = "X-Forwarded-For")
        {
            var reqHeaders = httpCtx.Request.Headers;

            if (reqHeaders.ContainsKey(forwardedHeaderKey))
            {
                var forwardIpStr = reqHeaders[forwardedHeaderKey].FirstOrDefault();
                var forwardIps = forwardIpStr.Split(","); // ip1, ip2, ..."
                if (forwardIps.Length > 0)
                {
                    var forwardIp = forwardIps[0];
                    if (!string.IsNullOrEmpty(forwardIp))
                    {
                        return forwardIp;
                    }
                }
            }

            var remoteIp = httpCtx.Connection.RemoteIpAddress?.ToString();
            return remoteIp;
        }

        private string GetQueryValue(HttpContext httpContext, string key)
        {
            if (!httpContext.Request.Query.TryGetValue(key, out var value))
            {
                return string.Empty;
            }

            return value.ToString();
        }

        private string GetHeaderValue(HttpContext httpContext, string key)
        {
            var value = httpContext.Request.Headers[key].ToString();
            return value;
        }

        public enum ESessionLoadState
        {
            INITIALIZED,
            LOADED,
            EXPIRED,
            NOT_FOUND
        }

        // 유저 정보
        private readonly ILogger _logger;
    }
}
