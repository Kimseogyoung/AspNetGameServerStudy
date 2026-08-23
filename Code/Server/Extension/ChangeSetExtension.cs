using Protocol;
using WebStudyServer.Model;

namespace Server.Extension
{
    // ChangeSet -> 와이어. Amount 는 이번에 변한 양, TotalAmount 는 변한 뒤의 현재 값이다.
    public static class ChangeSetExtension
    {
        public static ChgObjPacket ToPacket(this ChangeSet change)
        {
            return new ChgObjPacket
            {
                Type = change.Type,
                Num = change.Num,
                Amount = change.Delta,
                TotalAmount = change.After,
            };
        }

        public static List<ChgObjPacket> ToPacketList(this List<ChangeSet> changeList)
        {
            return changeList.ConvertAll(x => x.ToPacket());
        }
    }
}
