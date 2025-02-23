/*using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;
using WebStudyServer.Extension;
using Proto;
using WebStudyServer.Helper;
using Protocol;

namespace WebStudyServer.Component
{
    public class PlacedKingdomItemComponent : UserComponentBase<PlacedKingdomItemModel>
    {
        public PlacedKingdomItemComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {

        }

        // NOTE: Pos에 해당하는 위치(범위) 타일이 비어있는지
        public void ValidEmptyTile(KingdomMapManager mgrKingdomMap, TilePosPacket reqStartPos, KingdomItemProto prtKingdomItem)
        {

            // NOTE: PlacedKingdomItem 데이터 버전
            // TODO: StartTileX, Y에 인덱스 걸기
            *//*            타일 범위는(InputStartTileX, InputStartTileY)부터(InputStartTileX + InputSizeX - 1, InputStartTileY + InputSizeY - 1)

                        InputStartTileX + InputSizeX - 1 < StartTileX(검색 범위가 오브젝트의 왼쪽에 위치)
                        InputStartTileX > StartTileX + SizeX - 1(검색 범위가 오브젝트의 오른쪽에 위치)
                        InputStartTileY + InputSizeY - 1 < StartTileY(검색 범위가 오브젝트의 위쪽에 위치)
                        InputStartTileY > StartTileY + SizeY - 1(검색 범위가 오브젝트의 아래쪽에 위치)

                        NOT(
                        InputStartTileX + InputSizeX - 1 < StartTileX OR
                        InputStartTileX > StartTileX + SizeX - 1 OR
                        InputStartTileY + InputSizeY - 1 < StartTileY OR
                        InputStartTileY > StartTileY + SizeY - 1
                        )*//*
        }

        public PlacedKingdomItemManager Create(KingdomItemProto prt, int posX, int posY, KingdomStructureManager mgrKingdomStructure = null)
        {
            if(prt.Type == EKingdomItemType.STRUCTURE)
            {
                ReqHelper.ValidParam(mgrKingdomStructure != null, "NULL_KINGDOM_STRUCTURE_FOR_PLACED_KINGDOM_ITEM");
            }

            var mdlPlacedKingdomItem = base.CreateMdl(new PlacedKingdomItemModel
            {
                Num = prt.Num,
                Type = prt.Type,
                StructureItemId = mgrKingdomStructure.Model.SfId,
                PlayerId = _userRepo.RpcContext.PlayerId,
                State = EPlacedKingdomItemState.NONE,
                StartTileX = posX,
                StartTileY = posY,
                SizeX = prt.SizeX,
                SizeY = prt.SizeY,
                Rotation = 0,
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
*/