using System;
namespace OptimaJet.Workflow.DbPersistence
{
    public abstract class DbProvider
    {
        protected string ConnectionString
        {
            get;
            set;
        }
        public DbProvider(string connectionString)
        {
            this.ConnectionString = connectionString;
        }
        public WorkflowPersistenceModelDataContext CreateContext()
        {
            return new WorkflowPersistenceModelDataContext(this.ConnectionString)
            {
                CommandTimeout = 600,
                DeferredLoadingEnabled = true
            };
        }
    }
}
