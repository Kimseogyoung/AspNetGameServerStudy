using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Helper
{
    public class FlagHelper
    {
        public static (int sectionNum, int idx) GetUlongSection(ulong num)
        {
            var sectionNum = (int)(num / 64);
            var bitIdx = (int)(num % 64);
            return (sectionNum, bitIdx);
        }

        public static bool IsIdxOverflow(int idx, uint flag)
        {
            return idx >= 32;
        }

        public static bool IsFlagUp(int idx, uint flag)
        {
            if (IsIdxOverflow(idx, flag))
            {
                //_logger.Error("FLAG_IDX_IS_OVERFLOW Idx({Idx})", idx);
                return false;
            }

            var checker = 0x01 << idx;
            return (flag & checker) != 0;
        }

        public static void FlagUp(int idx, ref uint flag)
        {
            if (IsIdxOverflow(idx, flag))
            {
                //_logger.Error("FLAG_IDX_IS_OVERFLOW Idx({Idx})", idx);
                return;
            }

            var checker = 0x01 << idx;
            flag |= (uint)checker;
        }

        public static void FlagDown(int idx, ref uint flag)
        {
            if (IsIdxOverflow(idx, flag))
            {
                //_logger.Error("FLAG_IDX_IS_OVERFLOW Idx({Idx})", idx);
                return;
            }

            var checker = 0x01 << idx;
            flag &= ~(uint)checker;
        }

        public static bool IsIdxOverflow(int idx, ulong flag)
        {
            return idx >= 64;
        }

        public static bool IsFlagUp(int idx, ulong flag)
        {
            if (IsIdxOverflow(idx, flag))
            {
                //_logger.Error("FLAG_IDX_IS_OVERFLOW Idx({Idx})", idx);
                return false;
            }

            var checker = 1UL << idx;
            return (flag & checker) != 0;
        }

        public static void FlagUp(int idx, ref ulong flag)
        {
            if (IsIdxOverflow(idx, flag))
            {
                //_logger.Error("FLAG_IDX_IS_OVERFLOW Idx({Idx})", idx);
                return;
            }

            var checker = 1UL << idx; // Note: ulong 변수를 나타내기 위해 1UL을 사용
            flag |= checker;
        }

        public static void FlagDown(int idx, ref ulong flag)
        {
            if (IsIdxOverflow(idx, flag))
            {
                //_logger.Error("FLAG_IDX_IS_OVERFLOW Idx({Idx})", idx);
                return;
            }

            var checker = 1UL << idx; // Note: ulong 변수를 나타내기 위해 1UL을 사용
            flag &= ~checker;
        }

        public static List<int> GetFlagUpIdxList(ulong flag)
        {
            var idxList = new List<int>();
            for (var i = 0; i < 64; i++)
            {
                if (!IsFlagUp(i, flag))
                {
                    continue;
                }
                idxList.Add(i);
            }
            return idxList;
        }
    }
}
