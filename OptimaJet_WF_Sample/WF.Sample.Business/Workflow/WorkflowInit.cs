using System;
using System.Configuration;
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.DbPersistence;
using WorkflowRuntime = OptimaJet.Workflow.Core.Runtime.WorkflowRuntime;

namespace WF.Sample.Business.Workflow
{
public static class WorkflowInit
{
    private static volatile WorkflowRuntime _runtime;

    private static readonly object _sync = new object();

    public static WorkflowRuntime Runtime
    {
        get
        {
            if (_runtime == null)
            {
                lock (_sync)
                {
                    if (_runtime == null)
                    {
                        //获取数据库连接字符串
                        var connectionString = ConfigurationManager.ConnectionStrings["WF.Sample.Business.Properties.Settings.WorkflowEngineConnectionString"].ConnectionString;
                        var generator = new DbXmlWorkflowGenerator(connectionString).WithMapping("Document", "SimpleWF");
                            

                        var builder = new WorkflowBuilder<XElement>(generator, new XmlWorkflowParser(),
                                                                    new DbSchemePersistenceProvider(connectionString)).WithDefaultCache();//

                        _runtime = new WorkflowRuntime(new Guid("{8D38DB8F-F3D5-4F26-A989-4FDD40F32D9D}"))
                            .WithBuilder(builder)
                            .WithBus(new NullBus())
                            .WithRoleProvider(new WorkflowRole())
                            .WithRuleProvider(new WorkflowRule())
                            .WithRuntimePersistance(new RuntimePersistence(connectionString))
                            .WithPersistenceProvider(new DbPersistenceProvider(connectionString))
                            .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOff()
                            .Start();

                    }
                }
            }

            return _runtime;
        }
    }
}
}
