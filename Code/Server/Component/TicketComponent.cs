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
        public static class Key
        {
            public static CacheKey Single(ulong playerId, int num) => CacheKey.For<TicketModel>(playerId, playerId, num);
            public static CacheKey List(ulong playerId) => CacheKey.ListFor<TicketModel>(playerId);
        }

        public TicketComponent(UserRepo userRepo, IDbLayer db) : base(userRepo, db) { }

        protected override CacheKey KeyFor(TicketModel model) => Key.Single(model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => Key.List(playerId);

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
            outTicket = GetMdl(
                Key.Single(RpcCtx.PlayerId, num),
                db => db.SelectByPk<TicketModel>(new { RpcCtx.PlayerId, Num = num }));
            return outTicket != null;
        }
    }
}
