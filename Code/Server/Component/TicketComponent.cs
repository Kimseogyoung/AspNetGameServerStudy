using Proto;
using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;

namespace WebStudyServer.Component
{
    public class TicketComponent : UserComponentBase<TicketModel>
    {
        public TicketComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(TicketModel model) => CacheKey.For<TicketModel>(model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For<TicketModel>(playerId);

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

            return new TicketManager(_userRepo, mdlTicket);
        }

        public bool TryGetInternal(int num, out TicketModel outTicket)
        {
            outTicket = GetMdl(x => x.PlayerId == RpcCtx.PlayerId && x.Num == num);
            return outTicket != null;
        }
    }
}
