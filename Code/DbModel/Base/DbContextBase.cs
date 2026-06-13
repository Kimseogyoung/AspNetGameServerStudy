using Microsoft.EntityFrameworkCore;

namespace WebStudyServer.Base
{
    public class DbContextBase : DbContext, ILeasable
    {
        public FactoryBase Factory { get; set; } = null;

        public int PoolIdx { get; set; } = -1;

        public int ShardId { get; set; } = -1;

        public int LeaseCnt { get; set; } = 0;

        public DateTime CreateTime { get; set; }


        public void OnLease()
        {

        }

        public override void Dispose()
        {
            if (LeaseCnt > 0)
            {
                throw new Exception($"LEASE_CNT:{LeaseCnt}");
            }

            GC.SuppressFinalize(this);
            base.Dispose();
        }
    }
}
