using Microsoft.EntityFrameworkCore;
using WebStudyServer.Repo.Database;
using WebStudyServer.GAME;
using WebStudyServer.Extension;
using WebStudyServer.Model;
using Server.Repo;

namespace WebStudyServer
{
    public partial class Startup
    {
        public void Resource(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddScoped<DbRepo>();

            // Auth
            DapperExtension.Init<AccountModel>("Id");
            DapperExtension.Init<ChannelModel>("Key");
            DapperExtension.Init<DeviceModel>("Key");
            DapperExtension.Init<SessionModel>("AccountId");
            DapperExtension.Init<PlayerMapModel>("AccountId");

            // User
            DapperExtension.Init<PlayerModel>("Id");
            DapperExtension.Init<PlayerDetailModel>("PlayerId");
            DapperExtension.Init<CookieModel>("PlayerId", "Num");
            DapperExtension.Init<KingdomMapModel>("PlayerId");
            DapperExtension.Init<KingdomStructureModel>("SfId");
            DapperExtension.Init<KingdomDecoModel>("PlayerId", "Num");
            //DapperExtension.Init<PlacedKingdomItemModel>("Id");
            DapperExtension.Init<ItemModel>("PlayerId", "Num");
            DapperExtension.Init<PointModel>("PlayerId", "Num");
            DapperExtension.Init<TicketModel>("PlayerId", "Num");
            DapperExtension.Init<CashChangeLogModel>("SfId");
            DapperExtension.Init<GachaLogModel>("SfId");
            DapperExtension.Init<WorldModel>("PlayerId", "Num");
            DapperExtension.Init<WorldStageModel>("PlayerId", "Num");

            // Center
            DapperExtension.Init<ScheduleModel>("Num");

            ConnectionTest();
        }

        private void ConnectionTest()
        {
            foreach (var connectionStr in APP.Cfg.UserDbConnectionStrList)
            {
                var excutor = DBSqlExecutor.StartTransaction(connectionStr, System.Data.IsolationLevel.ReadCommitted);
                excutor.Commit();
            }

            foreach (var connectionStr in APP.Cfg.AuthDbConnectionStrList)
            {
                var excutor = DBSqlExecutor.StartTransaction(connectionStr, System.Data.IsolationLevel.ReadCommitted);
                excutor.Commit();
            }

            foreach (var connectionStr in APP.Cfg.CenterDbConnectionStrList)
            {
                var excutor = DBSqlExecutor.StartTransaction(connectionStr, System.Data.IsolationLevel.ReadCommitted);
                excutor.Commit();
            }
        }
    }
}
