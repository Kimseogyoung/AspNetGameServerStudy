using Proto;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Component
{
    public class DeviceComponent : AuthComponentBase
    {
        public DeviceComponent(AuthRepo authRepo, IDbExecutorFactory dbFactory) : base(authRepo, dbFactory)
        {
        }

        public bool TryGet(string idfv, out DeviceManager mgrDevice)
        {
            mgrDevice = null;

            if (!TryGetInternal(idfv, out var mdlDevice))
            {
                return false;
            }

            mgrDevice = new DeviceManager(_authRepo, mdlDevice);
            return true;
        }

        public DeviceManager Create(string idfv)
        {
            var newDevice = new DeviceModel
            {
                Key = idfv,
                Idfa = "",
                AccountId = _authRepo.RpcContext.AccountId,
                State = EDeviceState.ACTIVE,
                Country = "",
                GeoIpCountry = "",
                Language = ""
            };

            var repoDevice = CreateInternal(newDevice);
            var mgrDevice = new DeviceManager(_authRepo, repoDevice);
            return mgrDevice;
        }

        public void Update(DeviceModel mdlDevice)
        {
            _dbFactory.Execute(db => db.Update(mdlDevice));
        }

        private DeviceModel CreateInternal(DeviceModel inChannel)
        {
            DeviceModel newDevice = null;
            // 데이터베이스에 삽입
            _dbFactory.Execute(db =>
            {
                newDevice = db.Insert(inChannel);
            });

            return newDevice; // 새로 생성된 계정 모델 반환
        }

        private bool TryGetInternal(string deviceKey, out DeviceModel outDevice)
        {
            DeviceModel mdlDevice = null;

            _dbFactory.Execute(db =>
            {
                mdlDevice = db.SelectByPk<DeviceModel>(new { Key = deviceKey });
            });
            outDevice = mdlDevice;
            return outDevice != null;
        }
    }
}
