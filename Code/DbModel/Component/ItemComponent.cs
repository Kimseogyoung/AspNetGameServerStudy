using Proto;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Component
{
    public class ItemComponent : UserComponentBase<ItemModel>
    {
        public ItemComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(ItemModel model) => CacheKey.For<ItemModel>(model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For<ItemModel>(playerId);

        public ItemManager Touch(int itemNum)
        {
            if (!TryGetInternal(itemNum, out var mdlItem))
            {
                var prt = ProtoDb.Get<ItemProto>(itemNum);
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
            outItem = GetMdl(x => x.PlayerId == RpcCtx.PlayerId && x.Num == num);
            return outItem != null;
        }
    }
}
