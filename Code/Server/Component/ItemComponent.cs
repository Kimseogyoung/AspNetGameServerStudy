using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;
using WebStudyServer.Extension;
using WebStudyServer.GAME;

namespace WebStudyServer.Component
{
    public class ItemComponent : UserComponentBase<ItemModel>
    {
        public ItemComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {
        }

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

            var mgrItem = new ItemManager(_userRepo, mdlItem);
            return mgrItem;
        }

        public bool TryGetInternal(int num, out ItemModel outItem)
        {
            ItemModel mdlItem = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlItem = sqlConnection.SelectByPk<ItemModel>(new { PlayerId = _userRepo.RpcContext.PlayerId, Num = num }, transaction);
            });

            outItem = mdlItem;
            return outItem != null;
        }
    }
}
