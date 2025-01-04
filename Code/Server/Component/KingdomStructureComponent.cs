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
    public class KingdomStructureComponent : UserComponentBase<KingdomStructureModel>
    {
        public KingdomStructureComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {

        }
        
        public int GetKingdomStructureCnt(int num)
        {
            var cnt = 0;
            _executor.Excute((sqlConnection, transaction) =>
            {
                var mdlList = sqlConnection.SelectListByConditions<KingdomStructureModel>(new { Num = num }, transaction);
                cnt = mdlList.Count();
            });

            return cnt;
        }

        public KingdomStructureManager Create(KingdomItemProto prt)
        {
            var mdlKingdomStructure = base.Create(new KingdomStructureModel
            {
                Num = prt.Num,
                State = EKingdomItemState.STORED,
                PlayerId = _userRepo.RpcContext.PlayerId,
            });

            var mgrKingdomStructure = new KingdomStructureManager(_userRepo, mdlKingdomStructure, prt);
            return mgrKingdomStructure;
        }

        public KingdomStructureManager Get(ulong id)
        {
            ReqHelper.ValidContext(TryGetInternal(id, out var mdlKingdomStructure), "NOT_FOUND_KINGDOM_ITEM", () => new { Id = id });
            var mgrKingdomStructure = new KingdomStructureManager(_userRepo, mdlKingdomStructure);
            return mgrKingdomStructure;
        }

        public List<KingdomStructureManager> GetAllList(List<ulong> idList)
        {
            if(idList.Count == 0)
            {
                return new List<KingdomStructureManager>();
            }

            var mdlKingdomStructureList = GetListInternal(idList);
            ReqHelper.ValidContext(mdlKingdomStructureList.Count != idList.Count, "NOT_EQUAL_KINGDOM_ITEM_LIST", () => new { IdList = idList, MdlIdList = mdlKingdomStructureList.Select(x => x.Id) });
            var mgrKingdomStructureList = mdlKingdomStructureList.Select(x=>new KingdomStructureManager(_userRepo, x)).ToList();
            return mgrKingdomStructureList;
        }

        private List<KingdomStructureModel> GetListInternal(List<ulong> idList)
        {
            List<KingdomStructureModel> mdlKingdomStructureList = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlKingdomStructureList = sqlConnection.SelectListByConditions<KingdomStructureModel>(new { Id = idList }, transaction).ToList();
            });

            return mdlKingdomStructureList;
        }

        private bool TryGetInternal(ulong id, out KingdomStructureModel outKingdomStructure)
        {
            KingdomStructureModel mdlKingdomStructure = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlKingdomStructure = sqlConnection.SelectByPk<KingdomStructureModel>(new { Id = id }, transaction);
            });

            outKingdomStructure = mdlKingdomStructure;
            if(outKingdomStructure != null)
            {
                ReqHelper.ValidContext(mdlKingdomStructure.PlayerId == _userRepo.RpcContext.PlayerId, "NOT_EQUAL_KINGDOM_ITEM_PLAYER_ID", 
                    () => new { Id = id, PlayerId = _userRepo.RpcContext.PlayerId, KingdomStructurePlayerId = mdlKingdomStructure.PlayerId });
            }
            return outKingdomStructure != null;
        }
    }
}
