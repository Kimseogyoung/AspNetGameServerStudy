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
    public class KingdomMapComponent : UserComponentBase<KingdomMapModel>
    {
        public KingdomMapComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {

        }

        public KingdomMapManager Touch()
        {
            if (!TryGetInternal(out var mdlTicket))
            {
                mdlTicket = CreateMdl(new KingdomMapModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Snapshot = "",
                    State = EKingdomTileMapState.NONE,
                });
            }

            var mgrTileMap = new KingdomMapManager(_userRepo, mdlTicket);
            return mgrTileMap;
        }


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

        private bool TryGetInternal(out KingdomMapModel outKingdomItem)
        {
            KingdomMapModel mdlKingdomItem = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlKingdomItem = sqlConnection.SelectByPk<KingdomMapModel>(new { PlayerId = _rpcContext.PlayerId }, transaction);
            });

            outKingdomItem = mdlKingdomItem;
            return outKingdomItem != null;
        }
    }
}
