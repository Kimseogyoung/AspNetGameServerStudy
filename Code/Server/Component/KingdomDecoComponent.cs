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
    public class KingdomDecoComponent : UserComponentBase<KingdomDecoModel>
    {
        public KingdomDecoComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {

        }

        public KingdomDecoManager Touch(int itemNum)
        {

            if (!TryGetInternal(itemNum, out var mdlDeco))
            {
                mdlDeco = CreateMdl(new KingdomDecoModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = itemNum,
                });
            }

            var mgrDeco = new KingdomDecoManager(_userRepo, mdlDeco);
            return mgrDeco;
        }

        public KingdomDecoManager Create(KingdomItemProto prt)
        {
            var mdlKingdomDeco = base.CreateMdl(new KingdomDecoModel
            {
                Num = prt.Num,
                State = EKingdomItemState.STORED,
                PlayerId = _userRepo.RpcContext.PlayerId,
            });

            var mgrKingdomDeco = new KingdomDecoManager(_userRepo, mdlKingdomDeco, prt);
            return mgrKingdomDeco;
        }


        public List<KingdomDecoManager> GetAllList(List<int> numList)
        {
            if (numList.Count == 0)
            {
                return new List<KingdomDecoManager>();
            }

            var mdlKingdomDecoList = GetListInternal(numList);
            ReqHelper.ValidContext(mdlKingdomDecoList.Count != numList.Count, "NOT_EQUAL_KINGDOM_ITEM_LIST", () => new { NumList = numList, MdlNumList = mdlKingdomDecoList.Select(x => x.Num) });
            var mgrKingdomStructureList = mdlKingdomDecoList.Select(x => new KingdomDecoManager(_userRepo, x)).ToList();
            return mgrKingdomStructureList;
        }

        private List<KingdomDecoModel> GetListInternal(List<int> numList)
        {
            List<KingdomDecoModel> mdlKingdomDecoList = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlKingdomDecoList = sqlConnection.SelectListByConditions<KingdomDecoModel>(new { PlayerId = _rpcContext.PlayerId, Num = numList }, transaction).ToList();
            });

            return mdlKingdomDecoList;
        }

        private bool TryGetInternal(int num, out KingdomDecoModel outKingdomDeco)
        {
            KingdomDecoModel mdlKingdomDeco = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlKingdomDeco = sqlConnection.SelectByPk<KingdomDecoModel>(new { PlayerId = _rpcContext.PlayerId, Num = num }, transaction);
            });

            outKingdomDeco = mdlKingdomDeco;
            return outKingdomDeco != null;
        }
    }
}
