using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ServerCore
{
    public enum DbType
    {
        MySql,
        InMemory,
    }

    public enum CacheType
    {
        InMemory,
        Redis,
    }

    public class CoreConfig : IConfig
    {
        public string LogFolder { get; private set; } = string.Empty;
        public LogLevel LogLevel { get; private set; } = LogLevel.Debug;
        public int ServerNum { get; private set; } = -1;
        public string ServerIp { get; private set; } = string.Empty;
        public string EnvName { get; private set; } = string.Empty;

        public bool UseUserLock { get; private set; }
        public TimeSpan UserLockTimeoutSpan { get; private set; } = new();

        public DbType DbType { get; private set; } = DbType.MySql;
        public CacheType CacheType { get; private set; } = CacheType.InMemory;
        public string RedisConnectionString { get; private set; } = string.Empty;
        public TimeSpan CacheDefaultTtl { get; private set; } = TimeSpan.FromMinutes(30);

        public List<string> UserDbConnectionStrList { get; private set; } = [];
        public List<string> AuthDbConnectionStrList { get; private set; } = [];
        public List<string> CenterDbConnectionStrList { get; private set; } = [];

        public void Init(IConfiguration config, IHostEnvironment environ)
        {
            ServerIp = GetServerIp();
            EnvName = environ.EnvironmentName;

            UseUserLock = config.GetValue("UseUserLock", false);
            UserLockTimeoutSpan = config.GetValue("UserLockTimeoutSpan", TimeSpan.FromMinutes(20));

            DbType = config.GetValue("Db:Type", DbType.MySql);
            CacheType = config.GetValue("Cache:Type", CacheType.InMemory);
            RedisConnectionString = config.GetValue("Cache:ConnectionString", string.Empty);
            CacheDefaultTtl = config.GetValue("Cache:DefaultTtl", TimeSpan.FromMinutes(30));

            UserDbConnectionStrList = GetValueStringList(config, "Db:UserDb:ConnectionStrList");
            AuthDbConnectionStrList = GetValueStringList(config, "Db:AuthDb:ConnectionStrList");
            CenterDbConnectionStrList = GetValueStringList(config, "Db:CenterDb:ConnectionStrList");

            LogFolder = config.GetValue("Logging:Folder", "logs");
            LogLevel = config.GetValue("Logging:Level", LogLevel.Debug);
        }

        private static string GetServerIp()
        {
            var hostName = Dns.GetHostName();
            var addresses = Dns.GetHostAddresses(hostName);

            foreach (var ip in addresses)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "";
        }

        private static List<string> GetValueStringList(IConfiguration config, string key)
        {
            var strValue = config.GetValue<string>(key);
            if (string.IsNullOrEmpty(strValue))
            {
                return [];
            }

            return JsonSerializer.Deserialize<List<string>>(strValue)!;
        }
    }
}
