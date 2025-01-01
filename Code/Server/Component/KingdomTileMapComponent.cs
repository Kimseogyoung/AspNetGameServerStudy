using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;
using WebStudyServer.Extension;
using Proto;
using WebStudyServer.Helper;
using IdGen;

namespace WebStudyServer.Component
{
    public class KingdomTileMapComponent : UserComponentBase<KingdomTileMapModel>
    {
        public KingdomTileMapComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
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
        

     /*   public KingdomItemManager Create(KingdomItemProto prt)
        {
            var mdlKingdomItem = base.Create(new KingdomItemModel
            {
                Num = prt.Num,
                State = EKingdomItemState.STORED,
                PlayerId = _userRepo.RpcContext.PlayerId,
                Type = prt.Type,
            });

            var mgrKingdomItem = new KingdomItemManager(_userRepo, mdlKingdomItem, prt);
            return mgrKingdomItem;
        }

        public KingdomItemManager Get(ulong id)
        {
            ReqHelper.ValidContext(TryGetInternal(id, out var mdlKingdomItem), "NOT_FOUND_KINGDOM_ITEM", () => new { Id = id });
            var mgrKingdomItem = new KingdomItemManager(_userRepo, mdlKingdomItem);
            return mgrKingdomItem;
        }

        private bool TryGetInternal(ulong id, out KingdomItemModel outKingdomItem)
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
        }*/
    }
}
