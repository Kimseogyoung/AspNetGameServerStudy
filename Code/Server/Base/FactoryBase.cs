using System.Reflection.Metadata.Ecma335;

namespace WebStudyServer
{
    public abstract class FactoryBase
    {
        private class LeaseData
        {
            public ILeasable Lease { get; set; }
            public DateTime LoadTime { get; set; }
        }

        private class Pool
        {
            public LinkedList<LeaseData> Queue { get; set; } = new();
            public int TotalCnt { get; set; }
            public object Lock { get; set; } = new();
        }

        public int MaxShardCount => 4;

        public int PoolCnt => _poolArr.Length - 1;

        protected ILogger Logger { get; }

        public FactoryBase(bool usePooling, int shardCnt, int maxPoolCnt, TimeSpan idleTimeOut, TimeSpan lifeTimeOut, ILogger logger)
        {
            Logger = logger;

            _usePooling = usePooling;
            _maxPoolCnt = maxPoolCnt;
            _idleTimeOut = idleTimeOut;
            _lifeTimeOut = lifeTimeOut;
            _shardCnt = shardCnt;

            var poolArrCnt = _shardCnt + 1;
            _poolArr = new Pool[poolArrCnt]; // NOTE: 0번은 Default Pool
            for (var i = 0; i < poolArrCnt; ++i)
            {
                _poolArr[i] = new Pool()
                {
                    Queue = new(),
                    TotalCnt = 0,
                    Lock = new()
                };
            }
        }

        public ILeasable GetLease(int shardId)
        {
            if (shardId < 0 || shardId >= MaxShardCount)
            {
                // TODO: 로그
                return Rent(-1, shardId);
            }

            var poolIdx = GetPoolIdx(shardId);
            return Rent(poolIdx, shardId);
        }

        public ILeasable Rent(int poolIdx, int shardId)
        {
            if (!_usePooling)
            {
                var lease = Create(poolIdx);
                lease.Factory = this;
                lease.PoolIdx = poolIdx;
                lease.ShardId = shardId;
                lease.CreateTime = DateTime.UtcNow;
                lease.OnLease();
                return lease;
            }

            var pool = GetPool(poolIdx);
            lock (pool.Lock)
            {
                if (!TryDequeuePool(pool, out var lease))
                {
                    // 사용 가능한 것이 없으면 생성
                    var totalCnt = GetTotalPoolCnt();
                    if (totalCnt > _maxPoolCnt)
                    {
                        // TODO: 로그
                        // ??
                    }

                    //Logger.Debug("CreateNewLease PoolIdx({PoolIdx})", poolIdx);
                    lease = Create(poolIdx);
                    lease.Factory = this;
                    lease.PoolIdx = poolIdx;
                    lease.CreateTime = DateTime.UtcNow;
                    ++pool.TotalCnt;
                }

                lease.ShardId = shardId;
                ++lease.LeaseCnt;
                lease.OnLease();
                return lease;
            }
        }

        public virtual void Return(ILeasable lease)
        {
            if (!_usePooling)
            {
                //Logger.Warning("FAILED_RETURN_LEASE PoolIdx({PoolIdx})", lease.PoolIdx);
                return;
            }

            var pool = GetPool(lease.PoolIdx);
            lock (pool.Lock)
            {
                --lease.LeaseCnt;
                var contextData = new LeaseData
                {
                    Lease = lease,
                    LoadTime = DateTime.UtcNow
                };
                pool.Queue.AddLast(contextData);
            }
        }

        public int GetTotalPoolCnt()
        {
            if (!_usePooling)
            {
                return 0;
            }

            return _poolArr.Sum(x =>
            {
                lock (x.Lock)
                {
                    return x.TotalCnt;
                }
            });
        }

        public int GetPoolCnt(int poolIdx)
        {
            if (!_usePooling)
            {
                return 0;
            }

            if (poolIdx >= _poolArr.Length)
            {
                // TODO: 로그
                return 0;
            }

            var pool = _poolArr[poolIdx];
            lock (pool.Lock)
            {
                return pool.TotalCnt;
            }
        }

        public int GetTotalFreePoolCnt()
        {
            if (!_usePooling)
            {
                return 0;
            }

            return _poolArr.Sum(x =>
            {
                lock (x.Lock)
                {
                    return x.Queue.Count;
                }
            });
        }

        public int GetFreePoolCnt(int poolIdx)
        {
            if (!_usePooling)
            {
                return 0;
            }


            if (poolIdx >= _poolArr.Length)
            {
                // TODO: 로그
                return 0;
            }

            var pool = _poolArr[poolIdx];
            lock (pool.Lock)
            {
                return pool.Queue.Count;
            }
        }

        public void Shrink()
        {
            if (!_usePooling)
            {
                return;
            }

            foreach (var pool in _poolArr)
            {
                lock (pool.Lock)
                {
                    ShrinkPool(pool);
                }
            }
        }

        public virtual void Dispose()
        {
            Shrink();
            GC.SuppressFinalize(this);
        }

        protected abstract ILeasable Create(int poolIdx);


        private bool IsDeadLease(ILeasable leasable)
        {
            return false;
        }

        private Pool GetPool(int poolIdx)
        {
            if (poolIdx + 1 >= _poolArr.Length)
            {
                poolIdx = -1;
            }
            return _poolArr[poolIdx + 1];
        }

        private bool TryDequeuePool(Pool pool, out ILeasable? lease)
        {
            while (pool.Queue.Any())
            {
                var leaseData = pool.Queue.First.Value;
                pool.Queue.RemoveFirst();

                if (IsDeadLease(leaseData.Lease))
                {

                    // Logger.Debug("RemoveDeadLease");
                    leaseData.Lease.Dispose();
                    --pool.TotalCnt;
                    continue;
                }

                lease = leaseData.Lease;
                --pool.TotalCnt;
                return true;
            }

            lease = null;
            return false;
        }

        private static void ShrinkPool(Pool pool)
        {
            lock (pool.Lock)
            {
                foreach (var leaseData in pool.Queue)
                {
                    // TODO: 로그
                    leaseData.Lease.Dispose();
                }
                pool.TotalCnt -= pool.Queue.Count;
                pool.Queue.Clear();
            }
        }

        private int GetPoolIdx(int shardId)
        {
            return shardId % _shardCnt;
        }


        private readonly bool _usePooling;
        private readonly int _shardCnt;
        private readonly int _maxPoolCnt;
        private readonly TimeSpan _idleTimeOut;
        private readonly TimeSpan _lifeTimeOut;

        private readonly Pool[] _poolArr;
    }

    public abstract class FactoryBase<TLease> : FactoryBase
      where TLease : ILeasable, IDisposable
    {
        public FactoryBase(int poolNum, bool usePooling, int maxCnt, TimeSpan idleTimeOut, TimeSpan lifeTimeOut, ILogger logger)
            : base(usePooling, poolNum, maxCnt, idleTimeOut, lifeTimeOut, logger) { }
    }
}
