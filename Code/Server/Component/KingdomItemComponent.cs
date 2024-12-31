using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;
using WebStudyServer.Extension;
using Proto;
using WebStudyServer.Helper;

namespace WebStudyServer.Component
{
    public class KingdomItemComponent : UserComponentBase<KingdomItemModel>
    {
        public KingdomItemComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {

        }

    /*    public KingdomItemManager Touch(int itemNum)
        {

            if (!TryGetInternal(itemNum, out var mdlTicket))
            {
                mdlTicket = Create(new KingdomItemModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = itemNum,
                });
            }

            var mgrTIcket = new TicketManager(_userRepo, mdlTicket);
            return mgrTIcket;
        }*/

        public bool TryGetInternal(ulong id, out KingdomItemModel outKingdomItem)
        {
            KingdomItemModel mdlKingdomItem = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlKingdomItem = sqlConnection.SelectByPk<KingdomItemModel>(new { Id = id }, transaction);
            });

            outKingdomItem = mdlKingdomItem;
            if(outKingdomItem != null)
            {
                ReqHelper.ValidContext(mdlKingdomItem.PlayerId == _userRepo.RpcContext.PlayerId, "NOT_EQUAL_KINGDOM_ITEM_PLAYER_ID", 
                    () => new { Id = id, PlayerId = _userRepo.RpcContext.PlayerId, KingdomItemPlayerId = mdlKingdomItem.PlayerId });
            }
            return outKingdomItem != null;
        }
    }
}
