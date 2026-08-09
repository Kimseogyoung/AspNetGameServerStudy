using Proto;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using ServerCore.Extension;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Component
{
    public class DeviceComponent : AuthComponentBase
    {
        public DeviceComponent(AuthRepo authRepo, IRepository repository) : base(authRepo, repository)
        {
        }

        public async Task<(bool Found, DeviceManager? Value)> TryGetAsync(string idfv)
        {
            var mdlDevice = await GetMdlAsync(db => db.SelectByPkAsync<DeviceModel>(new { Key = idfv }));
            return mdlDevice == null ? (false, null) : (true, new DeviceManager(_authRepo, mdlDevice));
        }

        public async Task<DeviceManager> CreateAsync(string idfv)
        {
            var repoDevice = await CreateMdlAsync(new DeviceModel
            {
                Key = idfv,
                Idfa = "",
                AccountId = _authRepo.RpcContext.AccountId,
                State = EDeviceState.ACTIVE,
                Country = "",
                GeoIpCountry = "",
                Language = ""
            });

            return new DeviceManager(_authRepo, repoDevice);
        }

        public Task UpdateAsync(DeviceModel mdlDevice)
        {
            return UpdateMdlAsync(mdlDevice);
        }
    }
}
