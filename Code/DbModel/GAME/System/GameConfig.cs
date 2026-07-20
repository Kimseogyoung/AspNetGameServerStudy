using Microsoft.EntityFrameworkCore;
using Protocol;
using ServerCore;
using ServerCore.Serializer;

namespace WebStudyServer
{
    // Server(Web/RPC) 전용 설정. 인프라(DB/Cache/Redis 등) 공용 설정은 ServerCore.CoreConfig 참고.
    // appsettings의 "Game" 섹션에서 읽음.
    public class GameConfig : IConfig
    {
        public bool UseSwagger { get; private set; }
        public MySqlServerVersion? DbVersion { get; private set; }
        public TimeSpan SessionExpireSpan { get; private set; } = new();
        public TimeSpan SessionGracePeriodSpan { get; private set; } = TimeSpan.FromDays(30);
        public string DefaultPlayerPath { get; private set; } = string.Empty;
        public PlayerPacket PakDefaultPlayer { get; private set; } = new();

        public bool IsShowErrorDetail { get; private set; }
        public bool UseStrictValidation { get; private set; }
        public string ForceContentType { get; private set; }

        public void Init(IConfiguration config, IHostEnvironment environ)
        {
            UseSwagger = config.GetValue("Game:UseSwagger", false);
            DbVersion = new MySqlServerVersion(config.GetValue("Db:Version", "0.0.0"));

            SessionExpireSpan = config.GetValue("Game:SessionExpireSpan", TimeSpan.FromMinutes(20));
            SessionGracePeriodSpan = config.GetValue("Game:SessionGracePeriodSpan", TimeSpan.FromDays(30));

            IsShowErrorDetail = config.GetValue("Game:IsShowErrorDetail", false);
            UseStrictValidation = config.GetValue("Game:UseStrictValidation", true);
            ForceContentType = config.GetValue("Game:ForceContentType", MsgProtocol.JsonContentType);

            DefaultPlayerPath = config.GetValue("Game:DefaultPlayerPath", "Data/DefaultPlayer.json");
            var defaultPlayerJson = File.ReadAllText(DefaultPlayerPath);
            PakDefaultPlayer = JsonDataSerializer.DeserializeStr<PlayerPacket>(defaultPlayerJson);
        }
    }
}
