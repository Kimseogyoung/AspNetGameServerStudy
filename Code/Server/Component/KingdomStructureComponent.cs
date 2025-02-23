using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;
using WebStudyServer.Extension;
using Proto;
using WebStudyServer.Helper;
using WebStudyServer.GAME;

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
            var mdlKingdomStructure = base.CreateMdl(new KingdomStructureModel
            {
                SfId = IdHelper.GenerateSfId(),
                Num = prt.Num,
                State = EKingdomItemState.STORED,
                PlayerId = _userRepo.RpcContext.PlayerId,
            });

            var mgrKingdomStructure = new KingdomStructureManager(_userRepo, mdlKingdomStructure, prt);
            return mgrKingdomStructure;
        }

        public KingdomStructureManager Get(ulong sfId)
        {
            ReqHelper.ValidContext(TryGetInternal(sfId, out var mdlKingdomStructure), "NOT_FOUND_KINGDOM_ITEM", () => new { SfId = sfId });
            var mgrKingdomStructure = new KingdomStructureManager(_userRepo, mdlKingdomStructure);
            return mgrKingdomStructure;
        }

        public List<KingdomStructureManager> GetAllList(List<ulong> sfIdList)
        {
            if(sfIdList.Count == 0)
            {
                return new List<KingdomStructureManager>();
            }

            var mdlKingdomStructureList = GetListInternal(sfIdList);
            ReqHelper.ValidContext(mdlKingdomStructureList.Count != sfIdList.Count, "NOT_EQUAL_KINGDOM_ITEM_LIST", () => new { SfIdList = sfIdList, MdlIdList = mdlKingdomStructureList.Select(x => x.SfId) });
            var mgrKingdomStructureList = mdlKingdomStructureList.Select(x=>new KingdomStructureManager(_userRepo, x)).ToList();
            return mgrKingdomStructureList;
        }

        private List<KingdomStructureModel> GetListInternal(List<ulong> idList)
        {
            List<KingdomStructureModel> mdlKingdomStructureList = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlKingdomStructureList = sqlConnection.SelectListByConditions<KingdomStructureModel>(new { SfId = idList }, transaction).ToList();
            });

            return mdlKingdomStructureList;
        }

        private bool TryGetInternal(ulong sfId, out KingdomStructureModel outKingdomStructure)
        {
            KingdomStructureModel mdlKingdomStructure = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlKingdomStructure = sqlConnection.SelectByPk<KingdomStructureModel>(new { SfId = sfId }, transaction);
            });

            outKingdomStructure = mdlKingdomStructure;
            if(outKingdomStructure != null)
            {
                ReqHelper.ValidContext(mdlKingdomStructure.PlayerId == _userRepo.RpcContext.PlayerId, "NOT_EQUAL_KINGDOM_ITEM_PLAYER_ID", 
                    () => new { SfId = sfId, PlayerId = _userRepo.RpcContext.PlayerId, KingdomStructurePlayerId = mdlKingdomStructure.PlayerId });
            }
            return outKingdomStructure != null;
        }
    }
}
