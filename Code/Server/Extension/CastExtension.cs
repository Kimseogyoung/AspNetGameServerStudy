using Proto;

namespace WebStudyServer.Extension
{
    public static class CastExtension
    {
        public static EObjType ToObjTyeCategory(this EObjType objType)
        {
            if (objType >= EObjType.POINT_START && objType < EObjType.POINT_END)
            {
                return EObjType.POINT_START;
            }
            else if(objType >= EObjType.TICKET_START && objType <= EObjType.TICKET_END)
            {
                return EObjType.TICKET_START;
            }
            else
            {
                return objType;
            }
        }
    }
}
