using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Manager
{
    public class DeviceManager : AuthManagerBase<DeviceModel>
    {
        public DeviceManager(AuthRepo authRepo, DeviceModel model) : base(authRepo, model)
        {
        }


    }
}
