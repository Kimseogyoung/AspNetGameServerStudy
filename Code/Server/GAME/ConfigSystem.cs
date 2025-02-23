using System.Net.Sockets;
using System.Net;
using Microsoft.EntityFrameworkCore;
using WebStudyServer.StartUp;
using Microsoft.VisualBasic;
using System.Diagnostics;
using Protocol;
using WebStudyServer.Helper;

namespace WebStudyServer
{ 
    public class ConfigSystem
    {
        public string LogFolder { get; private set; } = string.Empty;
        public LogLevel LogLevel { get; private set; } = LogLevel.Debug;
        public int ServerNum { get; private set; } = -1;
        public string ServerIp { get; private set; } = string.Empty;
        public string EnvName { get; private set; } = string.Empty;
        public bool UseSwagger { get; private set; }
        public bool UseUserLock { get; private set; }
        public TimeSpan UserLockTimeoutSpan { get; private set; } = new();
        public MySqlServerVersion? DbVersion { get; private set; }
        public List<string> CenterDbConnectionStrList { get; private set; } = new();
        public List<string> UserDbConnectionStrList { get; private set; } = new();
        public List<string> AuthDbConnectionStrList { get; private set; } = new();
        public TimeSpan SessionExpireSpan { get; private set; } = new();
        public string DefaultPlayerPath { get; private set; } = string.Empty;
        public PlayerPacket PakDefaultPlayer { get; private set; } = new();

        public bool IsShowErrorDetail { get; private set; }
        public bool UseStrictValidation { get; private set; }
        public string ForceContentType { get; private set; }

        public void Init(IConfiguration config, IHostEnvironment environ)
        {
            ServerIp = GetServerIp();
            EnvName = environ.EnvironmentName;

            UseSwagger = config.GetValue("UseSwagger", false);
            UseUserLock = config.GetValue("UseUserLock", false);
            UserLockTimeoutSpan = config.GetValue("UserLockTimeoutSpan", TimeSpan.FromMinutes(20));

            SessionExpireSpan = config.GetValue("SessionExpireSpan", TimeSpan.FromMinutes(20));

            DbVersion = new MySqlServerVersion(config.GetValue("Db:Version", "0.0.0"));
            UserDbConnectionStrList = config.GetValueStringList("Db:UserDb:ConnectionStrList");
            AuthDbConnectionStrList = config.GetValueStringList("Db:AuthDb:ConnectionStrList");
            CenterDbConnectionStrList = config.GetValueStringList("Db:CenterDb:ConnectionStrList");

            LogFolder = config.GetValue("Logging:Folder", "logs");
            LogLevel = config.GetValue("Logging:Level", LogLevel.Debug);

            IsShowErrorDetail = config.GetValue("IsShowErrorDetail", false);
            UseStrictValidation = config.GetValue("UseStrictValidation", true);
            ForceContentType = config.GetValue("Protocol:ForceContentType", MsgProtocol.JsonContentType);

            DefaultPlayerPath = config.GetValue("DefaultPlayerPath", "Data/DefaultPlayer.json");
            var defaultPlayerJson = File.ReadAllText(DefaultPlayerPath);
            PakDefaultPlayer = SerializeHelper.JsonDeserialize<PlayerPacket>(defaultPlayerJson);
        }

        private string GetServerIp()
        {
            // 호스트 이름 가져오기
            var hostName = Dns.GetHostName();

            // 호스트 정보 가져오기
            var addresses = Dns.GetHostAddresses(hostName);

            // 첫 번째 IPv4 주소 출력
            foreach (var ip in addresses)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "";
        }
    }
}
