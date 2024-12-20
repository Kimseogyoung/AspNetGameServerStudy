using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proto
{

    public enum EErrorCode
    {
        NO_HANDLING_ERROR = -1,
        OK = 1,


        TIMEOUT = 101,
        PROCESSED = 102, //이미 처리된 요청
        CANCELED_OPERATION = 103,

        USER_LOCK = 104,

        GAME_CHANGE_NAME_EXIST_NAME = 1001,

        PARAM = 10000,
        PROTO = 20000,
        CONTEXT = 30000,
    }
    public enum ESessionState
    {
        NONE = 0,
        ACTIVE = 1,
        EXPIRED = 2
    }

    public enum EAccountState
    {
        NONE = 0,
        ACTIVE = 1,
    }

    public enum EChannelState
    {
        NONE = 0,
        ACTIVE = 1
    }

    public enum EChannelType
    {
        NONE = 0,
        GUEST = 1
    }

    public enum EDeviceState
    {
        NONE = 0,
        ACTIVE = 1
    }

    public enum EPlayerState
    {
        NONE = 0,
        PREPARED = 1,
        CHANGED_NAME_FIRST = 2,
    }

    public enum EKingdomObjType
    {
        NONE = 0,
        STRUCTURE = 1, // 건물
        DECORATION = 2, // 기타 장식물 등
    }

    public enum EKingdomObjState
    {
        NONE = 0,
        CONSTRUCTING = 1,
        READY = 2,
        IN_PROGRESS = 3,
        STORED = 4          // 창고에 보관 중
    }

    public enum ECookieState
    {
        NONE = 0,
        AVAILABLE = 1 
    }
}
