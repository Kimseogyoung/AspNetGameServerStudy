using Proto;

namespace Server.Helper
{
    public class GachaRandom
    {
        public GachaRandom(GachaScheduleProto prt, DateTime nowTime)
        {
            _prt = prt;
            _currentSeq = GachaConstant.GetCurrentNormalGachaSeq(nowTime);
        }

        public (int Num, int Cnt) Roll(bool isNormal)
        {
            var gradeRoll = _random.Next(GachaConstant.MaxWeight);

            var gradeWeightList = isNormal? GachaConstant.GetNormalGachaWeightList(_prt, _currentSeq) : GachaConstant.GetPickupGachaWeightList(_prt);
            var rolledGradeWeightIdx = RollGachaInternal(gradeWeightList.Select(x=>x.Weight));

            var cookieNumList = gradeWeightList[rolledGradeWeightIdx].NumList;
            var cookieNumRoll = _random.Next(cookieNumList.Count);
            var cookieNum = cookieNumList[cookieNumRoll];

            var detailWeightList = GachaConstant.GetDetailWeightList(_prt);
            var rolledDetailWeightIdx = RollGachaInternal(gradeWeightList.Select(x => x.Weight));
            if (rolledDetailWeightIdx == 0)
            {
                return (cookieNum, 1);
            }
            else
            {
                var soulStoneNum = GachaConstant.GetSoulStoneNum(cookieNum);
                var soulStoneCnt = rolledDetailWeightIdx;
                return (soulStoneNum, soulStoneCnt);
            }
        }

        public int RollGachaInternal(IEnumerable<int> weights)
        {
            var OriginRoll = _random.Next(GachaConstant.MaxWeight);
            var roll = OriginRoll;

            var idx = -1;
            foreach (var rate in weights)
            {
                idx++;
                if (roll < rate) // 뽑힘.
                {
                    return idx;
                }
                else
                {
                    roll -= rate;
                }
            }
          
            return -1;
        }

        private readonly Random _random = Random.Shared;

        private static int _currentSeq;
        private GachaScheduleProto _prt;
    }
}