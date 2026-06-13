using Proto;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Extension;
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

        public bool TryGet(string idfv, out DeviceManager mgrDevice)
        {
            mgrDevice = null;
            var mdlDevice = GetMdl(db => db.SelectByPk<DeviceModel>(new { Key = idfv }));
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
            });

            return new DeviceManager(_authRepo, repoDevice);
        }

        public void Update(DeviceModel mdlDevice)
        {
            UpdateMdl(mdlDevice);
        }
    }
}
