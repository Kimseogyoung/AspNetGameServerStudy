using ProtoBuf;
namespace Proto
{

	public enum EKingdomItemType 
	{ 
		NONE = 0, //
		STRUCTURE = 1, //
		DECO = 2, //
		TRASH = 3, //
	}

	public enum EKingdomItemState 
	{ 
		NONE = 0, //
		CONSTRUCTING = 1, //
		READY = 2, //
		CRAFTING = 3, //
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

	public enum ECookieSoulStoneType 
	{ 
		COOKIE = 0, //
		SOUL_STONE = 1, //
	}

	public enum EKingdomTileMapState 
	{ 
		NONE = 0, //
	}

	public enum EPlacedKingdomItemState 
	{ 
		NONE = 0, //
		DELETED = -1, //
	}

	public enum EKingdomStructureFlagType 
	{ 
		NONE = 0, //
		BUILD = 1, //
	}

	public enum EItemType 
	{ 
		NONE = 0, //
		CONSUMABLE = 1, //
		FUNCTIONAL = 2, //
		RELIC = 3, //
		CRAFTING = 4, //
	}

	public enum EGradeType 
	{ 
		NONE = 0, //
		COMMON = 1, //
		RARE = 2, //
		EPIC = 3, //
		SUPER_EPIC = 4, //
		ANCIENT = 5, //
		LEGENDARY = 6, //
	}

	public enum ECookieRollType 
	{ 
		NONE = 0, //
		DEFENDER = 1, //
		FIGHTER = 2, //
		ASSASSIN = 3, //
		BOMBER = 4, //
		MAGE = 5, //
		SUPPORTER = 6, //
		HEALER = 7, //
		SNIPER = 8, //
	}

	public enum EFormationPositionType 
	{ 
		NONE = 0, //
		FRONT = 1, //
		MID = 2, //
		REAR = 3, //
	}

	public enum EScheduleType 
	{ 
		NONE = 0, //
		GACHA = 10, //
		ATTENDANCE = 20, //
	}

	public enum EScheduleTimeType 
	{ 
		NONE = 0, //
		TOTAL = 1, //
		CONTENT = 2, //
		REWARD = 3, //
	}

	public enum EGachaItemType 
	{ 
		NONE = 0, //
		ORIGINAL = 1, //
		SPECIAL = 2, //
	}

	public enum EWorldType 
	{ 
		NORMAL = 0, //
		DARK = 1, //
		MASTER = 2, //
	}

	public enum EWorldStageType 
	{ 
		STAGE = 0, //
		REWARD = 1, //
		VILLAGE = 2, //
	}

	public enum EObjType 
	{ 
		NONE = 0, //
		EXP = 1, //
		GOLD = 2, //
		FREE_CASH = 3, //
		REAL_CASH = 4, //
		TOTAL_CASH = 5, //
		POINT_START = 100, //
		POINT_MILEAGE = 101, //
		POINT_COOKIE_LV = 102, //
		POINT_C_GACHA_NORMAL = 111, //
		POINT_C_GACHA_SPECIAL = 112, //
		POINT_C_GACHA_DESTINY = 113, //
		POINT_END = 200, //
		TICKET_START = 200, //
		TICKET_STAMINA = 201, //
		TICKET_END = 300, //
		COOKIE = 1000, //
		SOUL_STONE = 1001, //
		ITEM = 10000, //
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
			list.Add(typeof(ECookieSoulStoneType)); 
			list.Add(typeof(EKingdomTileMapState)); 
			list.Add(typeof(EPlacedKingdomItemState)); 
			list.Add(typeof(EKingdomStructureFlagType)); 
			list.Add(typeof(EItemType)); 
			list.Add(typeof(EGradeType)); 
			list.Add(typeof(ECookieRollType)); 
			list.Add(typeof(EFormationPositionType)); 
			list.Add(typeof(EScheduleType)); 
			list.Add(typeof(EScheduleTimeType)); 
			list.Add(typeof(EGachaItemType)); 
			list.Add(typeof(EWorldType)); 
			list.Add(typeof(EWorldStageType)); 
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
