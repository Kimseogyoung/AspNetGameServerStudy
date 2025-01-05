using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;
using WebStudyServer.Extension;
using Proto;

namespace WebStudyServer.Component
{
    public class TicketComponent : UserComponentBase<TicketModel>
    {
        public TicketComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {

        }

        public TicketManager Touch(EObjType objType)
        {
            var ticketNum = (int)objType;

            if (!TryGetInternal(ticketNum, out var mdlTicket))
            {
                mdlTicket = CreateMdl(new TicketModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = ticketNum,
                });
            }

            var mgrTIcket = new TicketManager(_userRepo, mdlTicket);
            return mgrTIcket;
        }

        public bool TryGetInternal(int num, out TicketModel outTicket)
        {
            TicketModel mdlTIcket = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlTIcket = sqlConnection.SelectByPk<TicketModel>(new { PlayerId = _userRepo.RpcContext.PlayerId, Num = num }, transaction);
            });

            outTicket = mdlTIcket;
            return outTicket != null;
        }
    }
}
