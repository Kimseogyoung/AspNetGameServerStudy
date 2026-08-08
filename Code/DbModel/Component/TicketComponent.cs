using Proto;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Component
{
    public class TicketComponent : UserComponentBase<TicketModel>
    {
        public TicketComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(TicketModel model) => CacheKey.For(CacheKeyTags.TicketModel, model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For(CacheKeyTags.TicketModel, playerId);

        public async Task<TicketManager> TouchAsync(EObjType objType)
        {
            var ticketNum = (int)objType;

            var mdlTicket = await TryGetInternalAsync(ticketNum);
            if (mdlTicket == null)
            {
                mdlTicket = await CreateMdlAsync(new TicketModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = ticketNum,
                });
            }

            return new TicketManager(_userRepo, mdlTicket);
        }

        public Task<TicketModel?> TryGetInternalAsync(int num)
        {
            return GetMdlAsync(x => x.PlayerId == RpcCtx.PlayerId && x.Num == num);
        }
    }
}
