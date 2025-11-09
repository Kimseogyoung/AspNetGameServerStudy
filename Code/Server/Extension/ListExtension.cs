using Protocol;

namespace Server.Extension
{
    public static class ListExtension
    {
        public static void AddOrInc(this List<ObjValue> objValList, ObjValue objValue)
        {
            if(objValue.Key.IsNone())
            {
                return;
            }    

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

        public static List<ObjValue> Merge(this List<ObjValue> objValList)
        {
            var newList = new List<ObjValue>();
            foreach (var objValue in objValList)
            {
                var newObjValue = new ObjValue(objValue.Key, objValue.Value);
                newList.AddOrInc(newObjValue);
            }
            return newList;
        }
    }
}
