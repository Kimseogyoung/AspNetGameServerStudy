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

        public static ObjPacket ValidReward(ObjPacket reqReward, EObjType valObjType, int valObjNum, double valObjAmount, string reason)
        {
            ReqHelper.ValidProto(reqReward.Type == valObjType, "NOT_EQUAL_REWARD_TYPE", () => new { Reason = reason, ReqReward = reqReward, ValType = valObjType });
            ReqHelper.ValidProto(reqReward.Num == valObjNum, "NOT_EQUAL_REWARD_NUM", () => new { Reason = reason, ReqReward = reqReward, ValNum = valObjNum });
            ReqHelper.ValidProto(reqReward.Amount == valObjAmount, "NOT_EQUAL_REWARD_AMOUNT", () => new { Reason = reason, ReqReward = reqReward, ValAmount = valObjAmount });
            var valObj = new ObjPacket { Type = reqReward.Type, Num = reqReward.Num, Amount = reqReward.Amount };
            return valObj;
        }
    }
}
