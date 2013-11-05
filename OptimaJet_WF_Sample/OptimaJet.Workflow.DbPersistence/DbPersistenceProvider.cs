using OptimaJet.Common;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Xml.Serialization;
namespace OptimaJet.Workflow.DbPersistence
{
    /// <summary>
    /// ProcessInstance数据库序列化类
    /// </summary>
    public sealed class DbPersistenceProvider : DbProvider, IPersistenceProvider
    {
        public DbPersistenceProvider(string connectionString)
            : base(connectionString)
        {
        }
        public void InitializeProcess(ProcessInstance processInstance)
        {
            using (TransactionScope readCommittedSupressedScope = PredefinedTransactionScopes.ReadCommittedSupressedScope)
            {
                using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
                {
                    var workflowProcessInstance = workflowPersistenceModelDataContext.WorkflowProcessInstances.SingleOrDefault((WorkflowProcessInstance wpi) => wpi.Id == processInstance.ProcessId);
                    if (workflowProcessInstance != null)
                    {
                        throw new ProcessAlredyExistsException();
                    }
                    var entity = new WorkflowProcessInstance
                    {
                        Id = processInstance.ProcessId,
                        SchemeId = new Guid?(processInstance.SchemeId),
                        ActivityName = processInstance.ProcessScheme.InitialActivity.Name,
                        StateName = processInstance.ProcessScheme.InitialActivity.State
                    };
                    workflowPersistenceModelDataContext.WorkflowProcessInstances.InsertOnSubmit(entity);
                    workflowPersistenceModelDataContext.SubmitChanges();
                }
                readCommittedSupressedScope.Complete();
            }
        }
        public void BindProcessToNewScheme(ProcessInstance processInstance)
        {
            this.BindProcessToNewScheme(processInstance, false);
        }
        public void BindProcessToNewScheme(ProcessInstance processInstance, bool resetIsDeterminingParametersChanged)
        {
            using (TransactionScope readCommittedSupressedScope = PredefinedTransactionScopes.ReadCommittedSupressedScope)
            {
                using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
                {
                    WorkflowProcessInstance workflowProcessInstance = workflowPersistenceModelDataContext.WorkflowProcessInstances.SingleOrDefault((WorkflowProcessInstance wpi) => wpi.Id == processInstance.ProcessId);
                    if (workflowProcessInstance == null)
                    {
                        throw new ProcessNotFoundException();
                    }
                    workflowProcessInstance.SchemeId = new Guid?(processInstance.SchemeId);
                    if (resetIsDeterminingParametersChanged)
                    {
                        workflowProcessInstance.IsDeterminingParametersChanged = false;
                    }
                    workflowPersistenceModelDataContext.SubmitChanges();
                }
                readCommittedSupressedScope.Complete();
            }
        }
        public void FillProcessParameters(ProcessInstance processInstance)
        {
            processInstance.AddParameters(this.GetProcessParameters(processInstance.ProcessId, processInstance.ProcessScheme));
        }
        public void FillPersistedProcessParameters(ProcessInstance processInstance)
        {
            processInstance.AddParameters(this.GetPersistedProcessParameters(processInstance.ProcessId, processInstance.ProcessScheme));
        }

        /// <summary>
        /// 根据当前的流程实例填写系统定义的流程参数及参数值，系统要统一维护该参数值
        /// 以保证状态和流程的运转
        /// </summary>
        /// <param name="processInstance"></param>
        public void FillSystemProcessParameters(ProcessInstance processInstance)
        {
            processInstance.AddParameters(this.GetSystemProcessParameters(processInstance.ProcessId, processInstance.ProcessScheme));
        }
        public void SavePersistenceParameters(ProcessInstance processInstance)
        {
            var list = (
                from ptp in processInstance.ProcessParameters
                where ptp.Purpose == ParameterPurpose.Persistence
                select new
                {
                    Parameter = ptp,
                    SerializedValue = this.SerializeParameter(ptp.Value)
                }).ToList();
            List<ParameterDefinition> persistenceParameters = processInstance.ProcessScheme.PersistenceParameters.ToList<ParameterDefinition>();
            using (TransactionScope readUncommittedSupressedScope = PredefinedTransactionScopes.ReadUncommittedSupressedScope)
            {
                using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
                {
                    List<WorkflowProcessInstancePersistence> source = (
                        from wpip in workflowPersistenceModelDataContext.WorkflowProcessInstancePersistences
                        where wpip.ProcessId == processInstance.ProcessId && (
                            from pp in persistenceParameters
                            select pp.Name).Contains(wpip.ParameterName)
                        select wpip).ToList<WorkflowProcessInstancePersistence>();
                    foreach (var parameterDefinitionWithValue in list)
                    {
                        WorkflowProcessInstancePersistence workflowProcessInstancePersistence = source.SingleOrDefault((WorkflowProcessInstancePersistence pp) => pp.ParameterName == parameterDefinitionWithValue.Parameter.Name);
                        if (workflowProcessInstancePersistence == null)
                        {
                            workflowProcessInstancePersistence = new WorkflowProcessInstancePersistence
                            {
                                Id = Guid.NewGuid(),
                                ParameterName = parameterDefinitionWithValue.Parameter.Name,
                                ProcessId = processInstance.ProcessId,
                                Value = parameterDefinitionWithValue.SerializedValue
                            };
                            workflowPersistenceModelDataContext.WorkflowProcessInstancePersistences.InsertOnSubmit(workflowProcessInstancePersistence);
                        }
                        else
                        {
                            workflowProcessInstancePersistence.Value = parameterDefinitionWithValue.SerializedValue;
                        }
                    }
                    workflowPersistenceModelDataContext.SubmitChanges();
                }
                readUncommittedSupressedScope.Complete();
            }
        }
        public void SetWorkflowIniialized(ProcessInstance processInstance)
        {
            using (TransactionScope serializableSupressedScope = PredefinedTransactionScopes.SerializableSupressedScope)
            {
                using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
                {
                    WorkflowProcessInstanceStatus workflowProcessInstanceStatus = workflowPersistenceModelDataContext.WorkflowProcessInstanceStatus.SingleOrDefault((WorkflowProcessInstanceStatus wpis) => wpis.Id == processInstance.ProcessId);
                    if (workflowProcessInstanceStatus == null)
                    {
                        workflowProcessInstanceStatus = new WorkflowProcessInstanceStatus
                        {
                            Id = processInstance.ProcessId,
                            Lock = Guid.NewGuid(),
                            Status = ProcessStatus.Initialized.Id
                        };
                        workflowPersistenceModelDataContext.WorkflowProcessInstanceStatus.InsertOnSubmit(workflowProcessInstanceStatus);
                    }
                    else
                    {
                        workflowProcessInstanceStatus.Status = ProcessStatus.Initialized.Id;
                    }
                    workflowPersistenceModelDataContext.SubmitChanges();
                }
                serializableSupressedScope.Complete();
            }
        }
        public void SetWorkflowIdled(ProcessInstance processInstance)
        {
            this.SetCustomStatus(processInstance.ProcessId, ProcessStatus.Idled);
        }
        public void SetWorkflowRunning(ProcessInstance processInstance)
        {
            using (TransactionScope serializableSupressedScope = PredefinedTransactionScopes.SerializableSupressedScope)
            {
                using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
                {
                    WorkflowProcessInstanceStatus workflowProcessInstanceStatus = workflowPersistenceModelDataContext.WorkflowProcessInstanceStatus.SingleOrDefault((WorkflowProcessInstanceStatus wpis) => wpis.Id == processInstance.ProcessId);
                    if (workflowProcessInstanceStatus == null)
                    {
                        throw new StatusNotDefinedException();
                    }
                    workflowProcessInstanceStatus.Lock = Guid.NewGuid();//为什么要Lock字段
                    workflowProcessInstanceStatus = workflowPersistenceModelDataContext.WorkflowProcessInstanceStatus.SingleOrDefault((WorkflowProcessInstanceStatus wpis) => wpis.Id == processInstance.ProcessId);
                    if (workflowProcessInstanceStatus == null)
                    {
                        throw new StatusNotDefinedException();
                    }
                    if (workflowProcessInstanceStatus.Status == ProcessStatus.Running.Id)
                    {
                        throw new ImpossibleToSetStatusException();
                    }
                    workflowProcessInstanceStatus.Status = ProcessStatus.Running.Id;
                    workflowPersistenceModelDataContext.SubmitChanges();
                }
                serializableSupressedScope.Complete();
            }
        }
        public void SetWorkflowFinalized(ProcessInstance processInstance)
        {
            this.SetCustomStatus(processInstance.ProcessId, ProcessStatus.Finalized);
        }

        /// <summary>
        /// 设置状态为终止
        /// </summary>
        /// <param name="processInstance"></param>
        /// <param name="level"></param>
        /// <param name="errorMessage"></param>
        public void SetWorkflowTerminated(ProcessInstance processInstance, ErrorLevel level, string errorMessage)
        {
            this.SetCustomStatus(processInstance.ProcessId, ProcessStatus.Terminated);
        }

        /// <summary>
        /// 重置所有流程实例的状态，如果流程状态是Running将设置为Idled,通过存储过程
        /// </summary>
        public void ResetWorkflowRunning()
        {
            using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
            {
                workflowPersistenceModelDataContext.spWorkflowProcessResetRunningStatus();
            }
        }

        /// <summary>
        /// 序列化TransitionDefinition的内容到WorkflowProcessTransitionHistory
        /// </summary>
        /// <param name="processInstance"></param>
        /// <param name="transition"></param>
        public void UpdatePersistenceState(ProcessInstance processInstance, TransitionDefinition transition)
        {
            using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
            {
                WorkflowProcessTransitionHistory entity = new WorkflowProcessTransitionHistory
                {
                    ActorIdentityId = Guid.NewGuid(),
                    ExecutorIdentityId = Guid.NewGuid(),
                    Id = Guid.NewGuid(),
                    IsFinalised = false,
                    ProcessId = processInstance.ProcessId,
                    FromActivityName = transition.From.Name,
                    FromStateName = transition.From.State,
                    ToActivityName = transition.To.Name,
                    ToStateName = transition.To.State,
                    TransitionClassifier = transition.Classifier.ToString(),
                    TransitionTime = DateTime.Now
                };
                workflowPersistenceModelDataContext.WorkflowProcessTransitionHistories.InsertOnSubmit(entity);
                workflowPersistenceModelDataContext.SubmitChanges();
            }
        }

        /// <summary>
        /// 检查WorkflowProcessInstance是否存在
        /// </summary>
        /// <param name="processId">Document.id</param>
        /// <returns></returns>
        public bool IsProcessExists(Guid processId)
        {
            bool result;
            using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
            {
                result = (workflowPersistenceModelDataContext.WorkflowProcessInstances.Count((WorkflowProcessInstance wpi) => wpi.Id == processId) > 0);
            }
            return result;
        }

        /// <summary>
        /// WorkflowProcessInstanceStatus表获取状态，为什么把状态放在独立的表里呢？
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public ProcessStatus GetInstanceStatus(Guid processId)
        {
            ProcessStatus result;
            using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
            {
                WorkflowProcessInstanceStatus instance = workflowPersistenceModelDataContext.WorkflowProcessInstanceStatus.FirstOrDefault((WorkflowProcessInstanceStatus wpis) => wpis.Id == processId);
                if (instance == null)
                {
                    result = ProcessStatus.NotFound;
                }
                else
                {
                    ProcessStatus processStatus = ProcessStatus.All.SingleOrDefault((ProcessStatus ins) => ins.Id == instance.Status);
                    if (processStatus == null)
                    {
                        result = ProcessStatus.Unknown;
                    }
                    else
                    {
                        result = processStatus;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 设置WorkflowProcessInstanceStatus新的状态
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="status"></param>
        private void SetCustomStatus(Guid processId, ProcessStatus status)
        {
            using (TransactionScope serializableSupressedScope = PredefinedTransactionScopes.SerializableSupressedScope)
            {
                using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
                {
                    WorkflowProcessInstanceStatus workflowProcessInstanceStatus = workflowPersistenceModelDataContext.WorkflowProcessInstanceStatus.SingleOrDefault((WorkflowProcessInstanceStatus wpis) => wpis.Id == processId);
                    if (workflowProcessInstanceStatus == null)
                    {
                        throw new StatusNotDefinedException();
                    }
                    workflowProcessInstanceStatus.Status = status.Id;
                    workflowPersistenceModelDataContext.SubmitChanges();
                }
                serializableSupressedScope.Complete();
            }
        }
        private IEnumerable<ParameterDefinitionWithValue> GetProcessParameters(Guid processId, ProcessDefinition processDefinition)
        {
            List<ParameterDefinitionWithValue> list = new List<ParameterDefinitionWithValue>(processDefinition.Parameters.Count<ParameterDefinition>());
            list.AddRange(this.GetPersistedProcessParameters(processId, processDefinition));
            list.AddRange(this.GetSystemProcessParameters(processId, processDefinition));
            return list;
        }

        /// <summary>
        /// 获取审批流程实例中系统定义的缺省参数及参数值(DefaultDefinitions中定义)
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="processDefinition"></param>
        /// <returns></returns>
        private IEnumerable<ParameterDefinitionWithValue> GetSystemProcessParameters(Guid processId, ProcessDefinition processDefinition)
        {
            WorkflowProcessInstance processInstance = this.GetProcessInstance(processId);
            List<ParameterDefinition> source = (
                from p in processDefinition.Parameters
                where p.Purpose == ParameterPurpose.System
                select p).ToList<ParameterDefinition>();

            var list = new List<ParameterDefinitionWithValue>(source.Count<ParameterDefinition>());

            list.Add(ParameterDefinition.Create(source.Single((ParameterDefinition sp) => sp.Name == DefaultDefinitions.ParameterProcessId.Name), processId));
            list.Add(ParameterDefinition.Create(source.Single((ParameterDefinition sp) => sp.Name == DefaultDefinitions.ParameterPreviousState.Name), (object)processInstance.PreviousState));

            list.Add(ParameterDefinition.Create(source.Single((ParameterDefinition sp) => sp.Name == DefaultDefinitions.ParameterCurrentState.Name), (object)processInstance.StateName));
            list.Add(ParameterDefinition.Create(source.Single((ParameterDefinition sp) => sp.Name == DefaultDefinitions.ParameterPreviousStateForDirect.Name), (object)processInstance.PreviousStateForDirect));
            list.Add(ParameterDefinition.Create(source.Single((ParameterDefinition sp) => sp.Name == DefaultDefinitions.ParameterPreviousStateForReverse.Name), (object)processInstance.PreviousStateForReverse));
            list.Add(ParameterDefinition.Create(source.Single((ParameterDefinition sp) => sp.Name == DefaultDefinitions.ParameterPreviousActivity.Name), (object)processInstance.PreviousActivity));
            list.Add(ParameterDefinition.Create(source.Single((ParameterDefinition sp) => sp.Name == DefaultDefinitions.ParameterCurrentActivity.Name), (object)processInstance.ActivityName));
            list.Add(ParameterDefinition.Create(source.Single((ParameterDefinition sp) => sp.Name == DefaultDefinitions.ParameterPreviousActivityForDirect.Name), (object)processInstance.PreviousActivityForDirect));
            list.Add(ParameterDefinition.Create(source.Single((ParameterDefinition sp) => sp.Name == DefaultDefinitions.ParameterPreviousActivityForReverse.Name), (object)processInstance.PreviousActivityForReverse));
            return list;
        }
        private IEnumerable<ParameterDefinitionWithValue> GetPersistedProcessParameters(Guid processId, ProcessDefinition processDefinition)
        {
            List<ParameterDefinition> persistenceParameters = processDefinition.PersistenceParameters.ToList<ParameterDefinition>();
            List<ParameterDefinitionWithValue> list = new List<ParameterDefinitionWithValue>(persistenceParameters.Count<ParameterDefinition>());
            List<WorkflowProcessInstancePersistence> list2;
            using (PredefinedTransactionScopes.ReadUncommittedSupressedScope)
            {
                using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
                {
                    list2 = (
                        from wpip in workflowPersistenceModelDataContext.WorkflowProcessInstancePersistences
                        where wpip.ProcessId == processId && (
                            from pp in persistenceParameters
                            select pp.Name).Contains(wpip.ParameterName)
                        select wpip).ToList<WorkflowProcessInstancePersistence>();
                }
            }
            foreach (WorkflowProcessInstancePersistence persistedParameter in list2)
            {
                ParameterDefinition parameterDefinition = persistenceParameters.Single((ParameterDefinition p) => p.Name == persistedParameter.ParameterName);
                list.Add(ParameterDefinition.Create(parameterDefinition, this.DeserializeParameter(persistedParameter.Value, parameterDefinition.Type)));
            }
            return list;
        }
        private object DeserializeParameter(string serializedValue, Type parameterType)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(parameterType);
            object result;
            using (StringReader stringReader = new StringReader(serializedValue))
            {
                result = xmlSerializer.Deserialize(stringReader);
            }
            return result;
        }
        private string SerializeParameter(object value)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(value.GetType());
            string result;
            using (StringWriter stringWriter = new StringWriter())
            {
                xmlSerializer.Serialize(stringWriter, value);
                result = stringWriter.ToString();
            }
            return result;
        }
        private WorkflowProcessInstance GetProcessInstance(Guid processId)
        {
            WorkflowProcessInstance result;
            using (PredefinedTransactionScopes.ReadCommittedSupressedScope)
            {
                using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
                {
                    WorkflowProcessInstance workflowProcessInstance = workflowPersistenceModelDataContext.WorkflowProcessInstances.SingleOrDefault((WorkflowProcessInstance wpi) => wpi.Id == processId);
                    if (workflowProcessInstance == null)
                    {
                        throw new ProcessNotFoundException();
                    }
                    result = workflowProcessInstance;
                }
            }
            return result;
        }
    }
}
