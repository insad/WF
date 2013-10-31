using OptimaJet.Common;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Transactions;
using System.Xml;
namespace OptimaJet.Workflow.DbPersistence
{
    /// <summary>
    /// 定时器的时间数据库序列类
    /// </summary>
    public class RuntimePersistence : DbProvider, IRuntimePersistence
    {
        public RuntimePersistence(string connectionStringName)
            : base(connectionStringName)
        {
        }
        public void SaveTimer(Guid runtimeId, RuntimeTimer timer)
        {
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(Dictionary<TimerKey, DateTime>));
            string timer2;
            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
                {
                    dataContractSerializer.WriteObject(xmlWriter, timer.Timers);
                    xmlWriter.Flush();
                    timer2 = stringWriter.ToString();
                }
            }
            using (TransactionScope readUncommittedSupressedScope = PredefinedTransactionScopes.ReadUncommittedSupressedScope)
            {
                using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
                {
                    WorkflowRuntime workflowRuntime = workflowPersistenceModelDataContext.WorkflowRuntimes.FirstOrDefault((WorkflowRuntime wr) => wr.RuntimeId == runtimeId);
                    if (workflowRuntime == null)
                    {
                        workflowRuntime = new WorkflowRuntime
                        {
                            RuntimeId = runtimeId,
                            Timer = timer2
                        };
                        workflowPersistenceModelDataContext.WorkflowRuntimes.InsertOnSubmit(workflowRuntime);
                    }
                    else
                    {
                        workflowRuntime.Timer = timer2;
                    }
                    workflowPersistenceModelDataContext.SubmitChanges();
                }
                readUncommittedSupressedScope.Complete();
            }
        }
        public RuntimeTimer LoadTimer(Guid runtimeId)
        {
            string timer;
            using (TransactionScope readUncommittedSupressedScope = PredefinedTransactionScopes.ReadUncommittedSupressedScope)
            {
                using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
                {
                    WorkflowRuntime workflowRuntime = workflowPersistenceModelDataContext.WorkflowRuntimes.FirstOrDefault((WorkflowRuntime wr) => wr.RuntimeId == runtimeId);
                    if (workflowRuntime == null)
                    {
                        workflowRuntime = new WorkflowRuntime
                        {
                            RuntimeId = runtimeId,
                            Timer = string.Empty
                        };
                        workflowPersistenceModelDataContext.WorkflowRuntimes.InsertOnSubmit(workflowRuntime);
                    }
                    timer = workflowRuntime.Timer;
                    workflowPersistenceModelDataContext.SubmitChanges();
                }
                readUncommittedSupressedScope.Complete();
            }
            if (string.IsNullOrEmpty(timer))
            {
                return new RuntimeTimer();
            }
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(Dictionary<TimerKey, DateTime>));
            RuntimeTimer result;
            using (StringReader stringReader = new StringReader(timer))
            {
                using (XmlReader xmlReader = XmlReader.Create(stringReader))
                {
                    result = new RuntimeTimer(dataContractSerializer.ReadObject(xmlReader) as IDictionary<TimerKey, DateTime>);
                }
            }
            return result;
        }
    }
}
