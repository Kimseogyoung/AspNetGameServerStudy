using Protocol.Context;

namespace Server.Extension
{
    public static class ListExtension
    {
        public static void AddOrInc(this List<ObjValue> objValList, ObjValue objValue)
        {
            var findObjValue = objValList.Find(x => x.Key == objValue.Key);
            if (findObjValue == null)
            {
                objValList.Add(objValue);
            }
            else
            {
                findObjValue.Value += objValue.Value;
            }
        }

        public static void AddOrInc(this List<ObjValue> objValList, List<ObjValue> objValList2)
        {
            foreach(var objValue in objValList2)
            {
                objValList.AddOrInc(objValue);
            }
        }
    }
}
