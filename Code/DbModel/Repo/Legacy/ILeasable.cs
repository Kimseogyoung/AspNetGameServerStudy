namespace WebStudyServer
{
    public interface ILeasable : IDisposable
    {
        public FactoryBase Factory { get; set; }

        public int PoolIdx { get; set; }

        public int ShardId { get; set; }

        public int LeaseCnt { get; set; }

        public DateTime CreateTime { get; set; }

        //public bool IsDisposed { get; set; }

        public void OnLease();
    }
}
