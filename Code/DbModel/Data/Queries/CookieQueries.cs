using Proto;
using WebStudyServer.Model;

namespace WebStudyServer.Data.Queries
{
    public static class CookieQueries
    {
        public static async Task<CookieModel> GetOrCreateAsync(this OwnedSet<CookieModel> set, int num)
        {
            var (found, cookie) = await set.TryGetAsync(x => x.Num == num);
            return found ? cookie : await set.CreateAsync(GetDefaultCookieModel(num));
        }

        // 신규 쿠키의 기본값. 만드는 자리가 둘(단건 GetOrCreate, 벌크 지급)이라 여기 모은다.
        public static CookieModel GetDefaultCookieModel(int num)
        {
            return new CookieModel
            {
                Num = num,
                Lv = DEF.DEFAULT_LV,
                SkillLv = DEF.DEFAULT_LV,
            };
        }

        // 소울스톤 번호로 대상 쿠키를 찾는다
        public static Task<CookieModel> GetOrCreateBySoulStoneAsync(this OwnedSet<CookieModel> set, int soulStoneNum)
        {
            var prt = ProtoDb.Get<CookieSoulStoneProto>(soulStoneNum);
            return set.GetOrCreateAsync(prt.CookieNum);
        }
    }
}
