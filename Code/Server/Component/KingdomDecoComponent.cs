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
                mdlDeco = Create(new KingdomDecoModel
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
            var mdlKingdomDeco = base.Create(new KingdomDecoModel
            {
                Num = prt.Num,
                State = EKingdomItemState.STORED,
                PlayerId = _userRepo.RpcContext.PlayerId,
            });

            var mgrKingdomDeco = new KingdomDecoManager(_userRepo, mdlKingdomDeco, prt);
            return mgrKingdomDeco;
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
