using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.GAME;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;

namespace WebStudyServer.Component
{
    public class ItemComponent : UserComponentBase<ItemModel>
    {
        public static class Key
        {
            public static CacheKey Single(ulong playerId, int num) => CacheKey.For<ItemModel>(playerId, playerId, num);
            public static CacheKey List(ulong playerId) => CacheKey.ListFor<ItemModel>(playerId);
        }

        public ItemComponent(UserRepo userRepo, IDbLayer db) : base(userRepo, db) { }

        protected override CacheKey KeyFor(ItemModel model) => Key.Single(model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => Key.List(playerId);

        public ItemManager Touch(int itemNum)
        {
            if (!TryGetInternal(itemNum, out var mdlItem))
            {
                var prt = APP.Prt.GetItemPrt(itemNum);
                mdlItem = CreateMdl(new ItemModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = itemNum,
                    Type = prt.Type,
                });
            }

            return new ItemManager(_userRepo, mdlItem);
        }

        public bool TryGetInternal(int num, out ItemModel outItem)
        {
            outItem = GetMdl(
                Key.Single(_rpcContext.PlayerId, num),
                db => db.SelectByPk<ItemModel>(new { PlayerId = _rpcContext.PlayerId, Num = num }));
            return outItem != null;
        }
    }
}
