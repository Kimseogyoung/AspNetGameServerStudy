/*
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Reflection;
using WebStudyServer;
using WebStudyServer.Service.Singleton;
using WebStudyServer.Base;

namespace WebStudyServer
{
    public class DbContextFactory<TContext> : FactoryBase<TContext>
        where TContext : DbContextBase

    {
        private readonly Action<DbContextOptionsBuilder<TContext>, string> _optionBuilderAction;
        private readonly string[] _dbConnectStrArr;

        public DbContextFactory(Action<DbContextOptionsBuilder<TContext>, string> action, ILogger<DbContextFactory<TContext>> logger)
             : base(1, true, 10, TimeSpan.FromDays(1), TimeSpan.FromDays(1), logger) 
        {
            _dbConnectStrArr = new string[1] { CONFIG.U };
            _optionBuilderAction = action;
        }

        protected override TContext Create(int poolIdx)
        {
            var dbConnStr = _dbConnectStrArr[poolIdx];
            var builder = new DbContextOptionsBuilder<TContext>();
            _optionBuilderAction.Invoke(builder, dbConnStr);
            return CreateContext(builder.Options);
        }

        public override void Return(ILeasable lease)
        {
            var context = lease as DbContextBase;
            context?.ChangeTracker.Clear();

            base.Return(lease);
        }

        private static TContext CreateContext(DbContextOptions<TContext> options)
        {
            var ctor = typeof(TContext).GetTypeInfo().GetConstructor(new Type[] { typeof(DbContextOptions<TContext>) });
            if (ctor == null)
            {
                throw new InvalidOperationException($"{typeof(TContext).Name} doesn't have constructor that has options");
            }
            return (TContext)ctor.Invoke(new object[] { options });
        }

    }
}
*/
