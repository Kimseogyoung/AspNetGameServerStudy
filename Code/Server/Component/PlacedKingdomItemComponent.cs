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
    public class PlacedKingdomItemComponent : UserComponentBase<PlacedKingdomItemModel>
    {
        public PlacedKingdomItemComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {

        }

        public PlacedKingdomItemManager Create(KingdomItemProto prt)
        {
            var mdlPlacedKingdomItem = base.Create(new PlacedKingdomItemModel
            {
                Num = prt.Num,
                PlayerId = _userRepo.RpcContext.PlayerId,
            });

            var mgrPlacedKingdomItem = new PlacedKingdomItemManager(_userRepo, mdlPlacedKingdomItem, prt);
            return mgrPlacedKingdomItem;
        }

        public PlacedKingdomItemManager Get(ulong id)
        {
            ReqHelper.ValidContext(TryGetInternal(id, out var mdlPlacedKingdomItem), "NOT_FOUND_KINGDOM_ITEM", () => new { Id = id });
            var mgrPlacedKingdomItem = new PlacedKingdomItemManager(_userRepo, mdlPlacedKingdomItem);
            return mgrPlacedKingdomItem;
        }

        private bool TryGetInternal(ulong id, out PlacedKingdomItemModel outPlacedKingdomItem)
        {
            PlacedKingdomItemModel mdlPlacedKingdomItem = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlPlacedKingdomItem = sqlConnection.SelectByPk<PlacedKingdomItemModel>(new { Id = id }, transaction);
            });

            outPlacedKingdomItem = mdlPlacedKingdomItem;
            if(outPlacedKingdomItem != null)
            {
                ReqHelper.ValidContext(mdlPlacedKingdomItem.PlayerId == _userRepo.RpcContext.PlayerId, "NOT_EQUAL_PLACED_KINGDOM_ITEM_PLAYER_ID", 
                    () => new { Id = id, PlayerId = _userRepo.RpcContext.PlayerId, PlacedKingdomItemPlayerId = mdlPlacedKingdomItem.PlayerId });
            }
            return outPlacedKingdomItem != null;
        }
    }
}
