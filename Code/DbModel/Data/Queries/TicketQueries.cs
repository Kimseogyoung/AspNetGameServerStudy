using WebStudyServer.Model;

namespace WebStudyServer.Data.Queries
{
    public static class TicketQueries
    {
        public static async Task<TicketModel> GetOrCreateAsync(this OwnedSet<TicketModel> set, int num)
        {
            var (found, ticket) = await set.TryGetAsync(x => x.Num == num);
            return found ? ticket : await set.CreateAsync(new TicketModel { Num = num });
        }
    }
}
