using WebStudyServer.Model;

namespace WebStudyServer.Data.Queries
{
    public static class PointQueries
    {
        public static async Task<PointModel> GetOrCreateAsync(this OwnedSet<PointModel> set, int num)
        {
            var (found, point) = await set.TryGetAsync(x => x.Num == num);
            return found ? point : await set.CreateAsync(new PointModel { Num = num });
        }
    }
}
