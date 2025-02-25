using Proto;
using System.Text;
using WebStudyServer;
using WebStudyServer.GAME;
using WebStudyServer.Helper;

namespace Server.Helper
{
    // 확률 합산.
    public struct GachaWeightStruct
    {
        public EGradeType Grade { get; private set; }
        public int Weight { get; private set; }
        public List<int> NumList { get; private set; }
        public GachaWeightStruct(EGradeType grade, int weight, List<int> numList)
        {
            Grade = grade;
            Weight = weight;
            NumList = numList;
        }
    }

    // 개별 확률
    public struct GachaProbIndividualStruct
    {
        public EGradeType Grade { get; private set; }
        public float Prob { get; private set; }
        public int Num { get; private set; }
        public GachaProbIndividualStruct(EGradeType grade, float prob, int num)
        {
            Grade = grade;
            Prob = prob;
            Num = num;
        }
    }
    public class GachaConstant
    {
        public const int MaxWeight = 1000000;

        // 미리 계산
        private static Dictionary<string, List<GachaWeightStruct>> _gradeGachaWeightDict = new(); // key별로 확률 , NumList를 저장
        private static Dictionary<int, List<GachaWeightStruct>> _detailGachaWeightDict = new();

        private static List<ScheduleProto> _prtScheduleList;
        private static List<GachaScheduleProto> _prtGachaScheduleList;
        private static List<GachaProbProto> _prtGachaProbList;
        private static List<GachaItemProto> _prtGachaItemList;
        private static List<CookieProto> _prtCookieList;

        private static Dictionary<int, CookieSoulStoneProto> _cookieNumSoulStoneDict = new();

        public static void Init(List<ScheduleProto> prtScheduleList, List<GachaScheduleProto> prtGachaScheduleList, List<GachaProbProto> prtGachaProbList, List<GachaItemProto> prtGachaItemList, List<CookieProto> prtCookieList, List<CookieSoulStoneProto> prtSoulStoneList)
        {
            _prtScheduleList = prtScheduleList;
            _prtGachaScheduleList = prtGachaScheduleList;
            _prtGachaProbList = prtGachaProbList;
            _prtGachaItemList = prtGachaItemList;
            _prtCookieList = prtCookieList;
            _cookieNumSoulStoneDict = prtSoulStoneList.ToDictionary(x => x.CookieNum);

            ValidNormalGacha();
            ValidPickupGacha();
        }

        public static int GetSoulStoneNum(int cookieNum)
        {
            if (!_cookieNumSoulStoneDict.TryGetValue(cookieNum, out var prtSoulStone))
            {
                throw new Exception($"NOT_FOUND_SOUL_STONE_NUM:{cookieNum}");
            }
            return prtSoulStone.Num;
        }

        public static List<int> GetDetailWeightList(GachaScheduleProto prtGachaSchedule)
        {
            var prtGachaProb = _prtGachaProbList.Find(x => x.Num == prtGachaSchedule.GachaProbNum);
            return prtGachaProb.DetailWeightList;
        }

        public static List<GachaWeightStruct> GetNormalGachaWeightList(GachaScheduleProto prtGachaSchedule, int gachaSeq)
        {
            var key = MakeNormalGachaKey(prtGachaSchedule.Num, gachaSeq);
            if (!_gradeGachaWeightDict.TryGetValue(key, out var rateStructList))
            {
                throw new Exception($"NOT_FOUND_GACHA_KEY:{key}");
            }
            return rateStructList;
        }

        public static List<GachaWeightStruct> GetPickupGachaWeightList(GachaScheduleProto prtGachaSchedule)
        {
            var key = MakePickupGachaKey(prtGachaSchedule.Num);
            if (!_gradeGachaWeightDict.TryGetValue(key, out var rateStructList))
            {
                throw new Exception($"NOT_FOUND_GACHA_KEY:{key}");
            }
            return rateStructList;
        }

        private static void ValidNormalGacha()
        {
            var prtGachaSchedule = _prtGachaScheduleList.Where(x => x.Seq == 0).FirstOrDefault();
            var prtGacha = _prtGachaProbList.Find(x => x.Num == prtGachaSchedule.GachaProbNum);

            if (prtGacha == null)
            {
                throw new Exception($"NOT_FOUND_GACHA_PROB_NUM:{prtGacha.Num}");
            }

            foreach (var scheduleSeq in _prtGachaScheduleList.Select(x => x.Seq))
            {
                var key = MakeNormalGachaKey(prtGachaSchedule.Num, scheduleSeq);
                if (_gradeGachaWeightDict.ContainsKey(key)) // 이미 캐싱해둔거 있으면 스킵
                {
                    continue;
                }

                if (!TryGetNormalGachaWeightList(prtGacha, scheduleSeq, out var weightList))
                {
                    new Exception("FAILED_TRY_GET_NORMAL_GACHA_WEIGHT_LIST");
                }

                _gradeGachaWeightDict.Add(key, weightList);
            }
        }

        private static void ValidPickupGacha()
        {
            foreach (var prtGachaSchedule in _prtGachaScheduleList)
            {
                var prtGachaProb = _prtGachaProbList.Find(x => x.Num == prtGachaSchedule.GachaProbNum);

                if (!TryGetPickupHeroGachaWeightList(prtGachaSchedule, prtGachaProb, out var outWeightList))
                {
                    new Exception("FAILED_TRY_GET_PICK_UP_GACHA_WEIGHT_LIST");
                }

                // 검증 코드 주석
            /*    var includedNumsWithoutPickup = GetPickupIncluededNumList(prtGachaSchedule, prtGachaSchedule.PickupItemNumArr);
                for (var i = 0; i < prtGachaProb.RateArr.Count; i++)
                {
                    var grade = (EGradeType)i + 1;
                    var numList = new List<int>();

                    switch (prtGachaSchedule.GachaObjType)
                    {
                        case EObjectType.HERO:
                            numList = HeroProto.ProtoCollection.Where(x => x.Grade == grade && includedNumsWithoutPickup.Contains(x.Num)).Select(x => x.Num).ToList();
                            break;
                        case EObjectType.ARTIFACT:
                            numList = ArtifactProto.ProtoCollection.Where(x => x.Grade == grade && includedNumsWithoutPickup.Contains(x.Num)).Select(x => x.Num).ToList();
                            break;
                        default:
                            throw new GameException(GameErrorCode.WrongServerProto, $"WRONG_GACHA_OBJ_TYPE:{prtGachaSchedule.GachaObjType}");
                    }*/
                

               /* var includedNums = GachaProbHelper.GetPickupIncluededNums(prtGachaSchedule);
                for (var i = 0; i < prtGachaProb.PickupRateArr.Count; i++)
                {
                    var rate = prtGachaProb.PickupRateArr[i];
                    var pickupItemNum = prtGachaSchedule.PickupItemNumArr[i];
                    if (prtGachaProb.PickupCnt >= i + 1)
                    {
                        // 영웅 확률 제외가 필요하여 주석처리
                        //ReqHelper.ValidServer(rate != 0, $"ZERO_PICKUP_RATE:{rate}", new { Tag= prtGachaSchedule.Tag, Order = prtGachaSchedule.Order, GachaType = prtGacha.Type, PickupCnt = prtGacha.PickupCnt });
                        ReqHelper.ValidServer(pickupItemNum != 0, $"ZERO_PICKUP_ITEM_NUM:{pickupItemNum}", new { Tag = prtGachaSchedule.Tag, Order = prtGachaSchedule.Order, GachaType = prtGachaProb.Type, PickupCnt = prtGachaProb.PickupCnt });
                        ReqHelper.ValidServer(includedNums.Contains(pickupItemNum), $"NOT_INCLUEDED_PICKUP_ITEM_NUM_IN_GACHA_ITEM:{pickupItemNum}", new { Tag = prtGachaSchedule.Tag, Order = prtGachaSchedule.Order, GachaType = prtGachaProb.Type, PickupCnt = prtGachaProb.PickupCnt });
                    }
                    else
                    {
                        ReqHelper.ValidServer(rate == 0, $"NOT_ZERO_PICKUP_RATE:{rate}", new { Tag = prtGachaSchedule.Tag, Order = prtGachaSchedule.Order, GachaType = prtGachaProb.Type, PickupCnt = prtGachaProb.PickupCnt });
                        ReqHelper.ValidServer(pickupItemNum == 0, $"NOT_ZERO_PICKUP_ITEM_NUM:{pickupItemNum}", new { Tag = prtGachaSchedule.Tag, Order = prtGachaSchedule.Order, GachaType = prtGachaProb.Type, PickupCnt = prtGachaProb.PickupCnt });
                        continue;
                    }

                    switch (prtGachaSchedule.GachaObjType)
                    {
                        case EObjectType.HERO:
                            ProtoHelper.ValidProtoPk<HeroProto>(pickupItemNum);
                            break;
                        case EObjectType.ARTIFACT:
                            ProtoHelper.ValidProtoPk<ArtifactProto>(pickupItemNum);
                            break;
                        default:
                            throw new GameException(GameErrorCode.WrongServerProto, "WRONG_GACHA_OBJ_TYPE");
                    }
                }*/

                var key = MakePickupGachaKey(prtGachaSchedule.Num);
                _gradeGachaWeightDict.Add(key, outWeightList);
            }
        }

        public static float GetProb(int weight, int decimalPlaces)
        {
            var prob = MathF.Round(100 * weight / MaxWeight, decimalPlaces);
            return prob;
        }

        public static float GetProb(float weight, int decimalPlaces)
        {
            var prob = MathF.Round(100 * weight / MaxWeight, decimalPlaces);
            return prob;
        }

        public static int GetCurrentNormalGachaSeq(DateTime nowTime)
        {
            // TODO: 캐시
            var prtLastSchedule = _prtScheduleList.OrderByDescending(x => x.ActiveStartTime).FirstOrDefault(x => x.Type == EScheduleType.GACHA && x.ActiveStartTime <= nowTime);
            var prtGachaSchedule = _prtGachaScheduleList.Find(x => x.Num == prtLastSchedule.Num);

            return prtGachaSchedule.Seq;
        }

        // 개별 확률 얻기
        public static List<GachaProbIndividualStruct> GetIndividualProbList(List<GachaWeightStruct> weightList, int decimalPlaces)
        {
            var individualList = new List<GachaProbIndividualStruct>();
            foreach (var weight in weightList)
            {
                var grade = weight.Grade;
                var individualWeight = (float)weight.Weight / (float)weight.NumList.Count;
                var individualProb = GetProb(individualWeight, decimalPlaces);
                for (var i = 0; i < weight.NumList.Count; i++)
                {
                    var individual = new GachaProbIndividualStruct(grade, individualProb, weight.NumList[i]);
                    individualList.Add(individual);
                }
            }
            return individualList;
        }

        public static bool TryGetNormalGachaWeightList(GachaProbProto prtGachaProb, int gachaSeq, out List<GachaWeightStruct> outWeightList)
        {
            var includedNumList = GetNormalIncluededNumList(gachaSeq);
            outWeightList = new List<GachaWeightStruct>();

            for (var i = 0; i < prtGachaProb.GradeWeightList.Count; i++)
            {
                var grade = (EGradeType)i + 1;
                var weight = prtGachaProb.GradeWeightList[i];
                var gradeCookieNumList = _prtCookieList.Where(x => x.GradeType == grade && includedNumList.Contains(x.Num)).Select(x => x.Num).ToList();

                var weightStruct = new GachaWeightStruct(grade, weight, gradeCookieNumList);
                outWeightList.Add(weightStruct);
            }

            return true;
        }

        public static bool TryGetPickupHeroGachaWeightList(GachaScheduleProto prtGachaSchedule, GachaProbProto prtGachaProb, out List<GachaWeightStruct> outWeightList)
        {
            var includedNumWithoutPickupList = GetPickupIncluededNumList(prtGachaSchedule.Tag, prtGachaSchedule.Seq, prtGachaSchedule.PickupCookieNumList);

            outWeightList = new List<GachaWeightStruct>();

            for (var i = 0; i < prtGachaProb.GradeWeightList.Count; i++)
            {
                var grade = (EGradeType)i + 1;
                var weight = prtGachaProb.GradeWeightList[i];
                var gradeCookieNumList = _prtCookieList.Where(x => x.GradeType == grade && includedNumWithoutPickupList.Contains(x.Num)).Select(x => x.Num).ToList();

                var weightStruct = new GachaWeightStruct(grade, weight, gradeCookieNumList);
                outWeightList.Add(weightStruct);
            }

            for (var i = 0; i < prtGachaProb.PickupWeightList.Count; i++)
            {
                var weight = prtGachaProb.GradeWeightList[i];
                var pickupItemNum = prtGachaSchedule.PickupCookieNumList[i];
              
                // 픽업 확률
                var weightStruct = new GachaWeightStruct(EGradeType.NONE, weight, new List<int>() { pickupItemNum });
                outWeightList.Add(weightStruct);
            }

            return true;
        }


        public static List<int> GetNormalIncluededNumList(int seq)
        {
            var includedNumList = _prtGachaItemList.Where(x => x.Type == EGachaItemType.ORIGINAL && x.Seq <= seq).Select(x=>x.Num).ToList();
            return includedNumList;
        }

        public static List<int> GetPickupIncluededNumList(string tag, int seq, List<int> excluededNumList = null)
        {
            var includedNumList = _prtGachaItemList
            .Where(x => x.Type == EGachaItemType.ORIGINAL && x.Seq <= seq 
            || x.Type == EGachaItemType.SPECIAL && x.Seq <= seq && x.Tag == tag)
            .Select(x=>x.Num)
            .ToList();

            if (excluededNumList != null)
            {
                includedNumList.RemoveAll(x => excluededNumList.Contains(x));
            }

            return includedNumList;
        }
        
        public static string MakePickupGachaKey(int scheduleNum)
        {
            return "pickup:" + scheduleNum;
        }

        public static string MakeNormalGachaKey(int scheduleNum, int seq)
        {
            return "normal:" + scheduleNum + ":" + seq;
        }
    }

}