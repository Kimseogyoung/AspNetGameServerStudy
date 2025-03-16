using Proto;
using Protocol;
using Server.Helper;

namespace WebStudyServer.Helper
{
    // 요청 데이터가 유효한지 검증하는 헬퍼 클래스
    public static class ReqHelper
    {
        public static void Valid(bool isValid, EErrorCode errorCode, Func<dynamic> getArgsFunc = null)
            => Valid(isValid, (int)errorCode, errorCode.ToString(), getArgsFunc);

        public static void Valid(bool isValid, int errorCode, string message, Func<dynamic> getArgsFunc = null)
        {
            if (!isValid)
            {
                dynamic args = null;
                if (getArgsFunc != null)
                {
                    args = getArgsFunc();
                }

                throw new GameException(errorCode, message, args);
            }
        }

        public static void ValidParam(bool isValid, string message, Func<dynamic> getArgsFunc = null)
        {
            Valid(isValid, (int)EErrorCode.PARAM, message, getArgsFunc);
        }

        public static void ValidContext(bool isValid, string message, Func<dynamic> getArgsFunc = null)
        {
            Valid(isValid, (int)EErrorCode.CONTEXT, message, getArgsFunc);
        }

        public static void ValidProto(bool isValid, string message, Func<dynamic> getArgsFunc = null)
        {
            Valid(isValid, (int)EErrorCode.PROTO, message, getArgsFunc);
        }

        public static int ValidUnderFlowParam(int reqValue, string paramName, bool allowZero = false) => (int)ValidUnderFlowParam((double)reqValue, paramName, allowZero);
        
        public static double ValidUnderFlowParam(double reqValue, string paramName, bool allowZero = false)
        {
            if (allowZero)
            {
                ReqHelper.ValidProto(reqValue >= 0, "NOT_EQUAL_COST_TYPE", () => new {ParamName = paramName, Value = reqValue});
            }
            else
            {
                ReqHelper.ValidProto(reqValue > 0, "NOT_EQUAL_COST_TYPE", () => new { ParamName = paramName, Value = reqValue });
            }

            return reqValue;
        }

        public static double ValidWithoutDecimal(double reqValue, string paramName)
        {
            var roundValue = Math.Round(reqValue);
            ReqHelper.ValidProto(Tolerance.AreEquals(roundValue, reqValue), "NOT_ALLOW_DECIMAL_VALUE", () => new { ParamName = paramName, Value = reqValue });
            return roundValue;
        }

        public static ObjPacket ValidCost(CostObjPacket reqCost, EObjType valObjType, int valObjNum, double valObjAmount, string reason)
        {
            ReqHelper.ValidProto(reqCost.Type == valObjType, "NOT_EQUAL_COST_TYPE", () => new { Reason = reason, ReqCost = reqCost, ValType = valObjType });
            ReqHelper.ValidProto(reqCost.Num == valObjNum, "NOT_EQUAL_COST_NUM", () => new { Reason = reason, ReqCost = reqCost, ValNum = valObjNum });
            ReqHelper.ValidProto(reqCost.Amount == valObjAmount, "NOT_EQUAL_COST_AMOUNT", () => new { Reason = reason, ReqCost = reqCost, ValAmount = valObjAmount });
            var valObj = new ObjPacket { Type = reqCost.Type, Num = reqCost.Num, Amount = reqCost.Amount };
            return valObj;
        }

        public static void ValidEnough(double reqAmount, double repoAmount, string param, string reason)
        {
            ReqHelper.ValidProto(reqAmount <= repoAmount, "NOT_ENOUGH", () => new { Param = param, Reason = reason, ReqAmount = reqAmount, ValAmount = repoAmount });
        }

        public static ObjValue ValidReward(ObjValue reqReward, EObjType valObjType, int valObjNum, double valObjAmount, string reason)
        {
            return ValidReward(reqReward, new ObjValue(valObjType, valObjNum, valObjAmount), reason);
        }

        public static ObjValue ValidReward(ObjValue reqReward, ObjValue valReward, string reason)
        {
            ReqHelper.ValidProto(reqReward.Key == valReward.Key, "NOT_EQUAL_REWARD_OBJ_KEY", () => new { Reason = reason, ReqRewardKey = reqReward.Key, ValRewardKey = valReward.Key });
            ReqHelper.ValidProto(reqReward.Value == valReward.Value, "NOT_EQUAL_REWARD_VALUE", () => new { Reason = reason, ReqReward = reqReward, ValReward = valReward });
            return reqReward;
        }

        public static List<ObjValue> ValidRewardList(List<ObjValue> reqRewardList, List<ObjValue> valRewardList, string reason)
        {
            foreach(var reqReward in reqRewardList)
            {
                var valReward = valRewardList.Find(x => x.Key == reqReward.Key);
                ReqHelper.ValidProto(valReward != null, "NOT_EXIST_REWARD", () => new { Reason = reason, ReqReward = reqReward, ValRewardList = valRewardList });
                ReqHelper.ValidProto(reqReward.Value == valReward.Value, "NOT_EQUAL_REWARD_VALUE", () => new { Reason = reason, ReqReward = reqReward, ValReward = valReward });
            }
     
            return reqRewardList;
        }
    }
}
