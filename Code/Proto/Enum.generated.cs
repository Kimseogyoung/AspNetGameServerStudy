using ProtoBuf;
namespace Proto
{

	public enum EKingdomItemType 
	{ 
		NONE = 0, //
		STRUCTURE = 1, //
		DECORATION = 2, //
		TRASH = 3, //
	}

	public enum EKingdomItemState 
	{ 
		NONE = 0, //
		CONSTRUCTING = 1, //
		READY = 2, //
		IN_PROGRESS = 3, //
		STORED = 4, //
	}

	public enum EKingdomItemSpecialType 
	{ 
		NONE = 0, //
		FORGE = 0, //
	}

	public enum ECookieState 
	{ 
		NONE = 0, //
		AVAILABLE = 1, //
	}

	public enum EObjType 
	{ 
		NONE = 0, //
		GOLD = 1, //
		STAR_CANDY = 2, //
		FREE_CASH = 3, //
		REAL_CASH = 4, //
		TOTAL_CASH = 5, //
		POINT_START = 100, //
		POINT_MILEAGE = 101, //
		POINT_C_GACHA_NORMAL = 111, //
		POINT_C_GACHA_SPECIAL = 112, //
		POINT_C_GACHA_DESTINY = 113, //
		POINT_END = 200, //
		TICKET_START = 200, //
		TICKET_STAMINA = 201, //
		TICKET_END = 300, //
		ITEM = 1000, //
		COOKIE = 10000, //
		KINGDOM_ITEM = 100000, //
	}

	public enum ESessionState 
	{ 
		NONE = 0, //
		ACTIVE = 1, //
		EXPIRED = 2, //
	}

	public enum EAccountState 
	{ 
		NONE = 0, //
		ACTIVE = 1, //
	}

	public enum EChannelState 
	{ 
		NONE = 0, //
		ACTIVE = 1, //
	}

	public enum EChannelType 
	{ 
		NONE = 0, //
		GUEST = 1, //
	}

	public enum EDeviceState 
	{ 
		NONE = 0, //
		ACTIVE = 1, //
	}

	public enum EPlayerState 
	{ 
		NONE = 0, //
		PREPARED = 1, //
		CHANGED_NAME_FIRST = 2, //
	}

	public enum EErrorCode 
	{ 
		NO_HANDLING_ERROR = -1, //
		OK = 1, //
		TIMEOUT = 101, //
		PROCESSED = 102, //
		CANCELED_OPERATION = 103, //
		USER_LOCK = 104, //
		GAME_CHANGE_NAME_EXIST_NAME = 1001, //
		PARAM = 10000, //
		PROTO = 20000, //
		CONTEXT = 30000, //
	}


	public class PrtEnum
	{
		public static IEnumerable<Type> GetEnums()
		{
			var list = new List<Type>();
		
			list.Add(typeof(EKingdomItemType)); 
			list.Add(typeof(EKingdomItemState)); 
			list.Add(typeof(EKingdomItemSpecialType)); 
			list.Add(typeof(ECookieState)); 
			list.Add(typeof(EObjType)); 
			list.Add(typeof(ESessionState)); 
			list.Add(typeof(EAccountState)); 
			list.Add(typeof(EChannelState)); 
			list.Add(typeof(EChannelType)); 
			list.Add(typeof(EDeviceState)); 
			list.Add(typeof(EPlayerState)); 
			list.Add(typeof(EErrorCode)); 
			return list;
		}

	}
}
