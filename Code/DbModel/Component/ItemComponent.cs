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

        protected override CacheKey KeyFor(ItemModel model) => CacheKey.For(CacheKeyTags.ItemModel, model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For(CacheKeyTags.ItemModel, playerId);

        public async Task<ItemManager> TouchAsync(int itemNum)
        {
            var mdlItem = await TryGetInternalAsync(itemNum);
            if (mdlItem == null)
            {
                var prt = ProtoDb.Get<ItemProto>(itemNum);
                mdlItem = await CreateMdlAsync(new ItemModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = itemNum,
                    Type = prt.Type,
                });
            }

            return new ItemManager(_userRepo, mdlItem);
        }

        public Task<ItemModel?> TryGetInternalAsync(int num)
        {
            return GetMdlAsync(x => x.PlayerId == RpcCtx.PlayerId && x.Num == num);
        }
    }
}
