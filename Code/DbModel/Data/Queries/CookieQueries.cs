using Proto;
using WebStudyServer.Model;

namespace WebStudyServer.Data.Queries
{
    public static class CookieQueries
    {
        public static async Task<CookieModel> GetOrCreateAsync(this OwnedSet<CookieModel> set, int num)
        {
            var (found, cookie) = await set.TryGetAsync(x => x.Num == num);
            return found ? cookie : await set.CreateAsync(new CookieModel
            {
                Num = num,
                Lv = DEF.DEFAULT_LV,
                SkillLv = DEF.DEFAULT_LV,
            });
        }

        // 소울스톤 번호로 대상 쿠키를 찾는다
        public static Task<CookieModel> GetOrCreateBySoulStoneAsync(this OwnedSet<CookieModel> set, int soulStoneNum)
        {
            var prt = ProtoDb.Get<CookieSoulStoneProto>(soulStoneNum);
            return set.GetOrCreateAsync(prt.CookieNum);
        }
    }
}
