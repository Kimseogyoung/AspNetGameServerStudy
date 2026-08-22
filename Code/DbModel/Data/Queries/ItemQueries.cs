using Proto;
using WebStudyServer.Model;

namespace WebStudyServer.Data.Queries
{
    public static class ItemQueries
    {
        public static async Task<ItemModel> GetOrCreateAsync(this OwnedSet<ItemModel> set, int num)
        {
            var (found, item) = await set.TryGetAsync(x => x.Num == num);
            if (found)
            {
                return item;
            }

            var prt = ProtoDb.Get<ItemProto>(num);
            return await set.CreateAsync(new ItemModel
            {
                Num = num,
                Type = prt.Type,
            });
        }
    }
}
