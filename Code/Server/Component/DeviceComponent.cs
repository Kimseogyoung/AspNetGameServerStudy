using Proto;
using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Component
{
    public class DeviceComponent : AuthComponentBase
    {
        public static class Key
        {
            public static CacheKey Single(string deviceKey) => CacheKey.For<DeviceModel>(deviceKey);
        }

        public DeviceComponent(AuthRepo authRepo, IRepository repository) : base(authRepo, repository)
        {
        }

        public bool TryGet(string idfv, out DeviceManager mgrDevice)
        {
            mgrDevice = null;
            var mdlDevice = GetMdl(Key.Single(idfv), db => db.SelectByPk<DeviceModel>(new { Key = idfv }));
            if (mdlDevice == null) return false;
            mgrDevice = new DeviceManager(_authRepo, mdlDevice);
            return true;
        }

        public DeviceManager Create(string idfv)
        {
            var repoDevice = CreateMdl(new DeviceModel
            {
                Key = idfv,
                Idfa = "",
                AccountId = _authRepo.RpcContext.AccountId,
                State = EDeviceState.ACTIVE,
                Country = "",
                GeoIpCountry = "",
                Language = ""
            }, e => Key.Single(e.Key));

            return new DeviceManager(_authRepo, repoDevice);
        }

        public void Update(DeviceModel mdlDevice)
        {
            UpdateMdl(mdlDevice, Key.Single(mdlDevice.Key));
        }
    }
}
