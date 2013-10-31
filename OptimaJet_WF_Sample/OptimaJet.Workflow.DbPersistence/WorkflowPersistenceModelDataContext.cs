using OptimaJet.Workflow.DbPersistence.Properties;
using System;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Reflection;
namespace OptimaJet.Workflow.DbPersistence
{
    /// <summary>
    /// LINQ 到 Sql的数据库框架数据上下文，负责映射和管理数据表到类
    /// </summary>
    [Database(Name = "BudgetNewWorkflow")]
    public class WorkflowPersistenceModelDataContext : DataContext
    {
        private static MappingSource mappingSource = new AttributeMappingSource();
        public Table<WorkflowProcessTransitionHistory> WorkflowProcessTransitionHistories
        {
            get
            {
                return base.GetTable<WorkflowProcessTransitionHistory>();
            }
        }
        public Table<WorkflowProcessInstancePersistence> WorkflowProcessInstancePersistences
        {
            get
            {
                return base.GetTable<WorkflowProcessInstancePersistence>();
            }
        }
        public Table<WorkflowProcessInstanceStatus> WorkflowProcessInstanceStatus
        {
            get
            {
                return base.GetTable<WorkflowProcessInstanceStatus>();
            }
        }
        public Table<WorkflowRuntime> WorkflowRuntimes
        {
            get
            {
                return base.GetTable<WorkflowRuntime>();
            }
        }
        public Table<WorkflowProcessScheme> WorkflowProcessSchemes
        {
            get
            {
                return base.GetTable<WorkflowProcessScheme>();
            }
        }
        public Table<WorkflowProcessInstance> WorkflowProcessInstances
        {
            get
            {
                return base.GetTable<WorkflowProcessInstance>();
            }
        }
        public Table<WorkflowScheme> WorkflowSchemes
        {
            get
            {
                return base.GetTable<WorkflowScheme>();
            }
        }
        public WorkflowPersistenceModelDataContext()
            : base(Settings.Default.BudgetNewWorkflowConnectionString, WorkflowPersistenceModelDataContext.mappingSource)
        {
        }
        public WorkflowPersistenceModelDataContext(string connection)
            : base(connection, WorkflowPersistenceModelDataContext.mappingSource)
        {
        }
        public WorkflowPersistenceModelDataContext(IDbConnection connection)
            : base(connection, WorkflowPersistenceModelDataContext.mappingSource)
        {
        }
        public WorkflowPersistenceModelDataContext(string connection, MappingSource mappingSource)
            : base(connection, mappingSource)
        {
        }
        public WorkflowPersistenceModelDataContext(IDbConnection connection, MappingSource mappingSource)
            : base(connection, mappingSource)
        {
        }
        [Function(Name = "dbo.spWorkflowProcessResetRunningStatus")]
        public int spWorkflowProcessResetRunningStatus()
        {
            IExecuteResult executeResult = base.ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), new object[0]);
            return (int)executeResult.ReturnValue;
        }
    }
}
