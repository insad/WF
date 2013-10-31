using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Cache;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.Core.Builder
{

    /// <summary>
    /// ProcessInstance流程实例的构造类：负责获取创建流程实例
    /// </summary>
    /// <typeparam name="TSchemeMedium"></typeparam>
    public sealed class WorkflowBuilder<TSchemeMedium> : IWorkflowBuilder where TSchemeMedium : class
    {
        internal IWorkflowGenerator<TSchemeMedium> Generator;

        //工作流方案解析器
        internal IWorkflowParser<TSchemeMedium> Parser;

        //工作流方案提供器
        internal ISchemePersistenceProvider<TSchemeMedium> SchemePersistenceProvider;

        private bool _haveCache;

        private IParsedProcessCache _cache;

        internal WorkflowBuilder()
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="generator">流程的XML数据源</param>
        /// <param name="parser">是流程中XML结点中的转换器，将节点的具体内容转换出来</param>
        /// <param name="schemePersistenceProvider">获取方案的具体操作</param>
        public WorkflowBuilder (IWorkflowGenerator<TSchemeMedium> generator,
                               IWorkflowParser<TSchemeMedium> parser,
                               ISchemePersistenceProvider<TSchemeMedium> schemePersistenceProvider)
        {
            Generator = generator;
            Parser = parser;
            SchemePersistenceProvider = schemePersistenceProvider;
        }

        //todo 10.31 看到这个地方了
        //根据公文射频流程实例processId创建其实例，该实例包括了审批流程的公文流转审批流程ProcessDefinition
        /// <summary>
        /// 根据公文射频流程实例processId创建其实例，该实例包括了审批流程的公文流转审批流程ProcessDefinition
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="processName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public ProcessInstance CreateNewProcess(Guid processId,
                                                string processName,
                                                IDictionary<string, IEnumerable<object>> parameters)
        {
            SchemeDefinition<TSchemeMedium> schemeDefinition = null;
            try
            {
                //根据参数和处理实例类别processName，构建工作流处理实例定义
                schemeDefinition = SchemePersistenceProvider.GetProcessSchemeWithParameters(processName,
                                                                                             parameters,
                                                                                             true);
            }
            catch (SchemeNotFoundException)
            {
                var schemeId = Guid.NewGuid();
                var newScheme = Generator.Generate(processName, schemeId, parameters);
                try
                {
                    SchemePersistenceProvider.SaveScheme(processName, schemeId, newScheme, parameters);
                    schemeDefinition = new SchemeDefinition<TSchemeMedium>(schemeId, newScheme, false, false);
                }
                catch (SchemeAlredyExistsException)
                {
                    schemeDefinition = SchemePersistenceProvider.GetProcessSchemeWithParameters(processName, parameters, true);
                }
            }

            return ProcessInstance.Create(schemeDefinition.Id,
                                          processId,
                                          GetProcessDefinition(schemeDefinition),
                                          schemeDefinition.IsObsolete, schemeDefinition.IsDeterminingParametersChanged);
        }

        //根据审批流程方案实例SchemeDefinition的模型定义及其内容转换为ProcessDefinition
        /// <summary>
        /// 根据审批流程方案实例SchemeDefinition的模型定义及其内容转换为ProcessDefinition
        /// </summary>
        /// <param name="schemeDefinition"></param>
        /// <returns></returns>
        private ProcessDefinition GetProcessDefinition (SchemeDefinition<TSchemeMedium> schemeDefinition  )
        {
            if (_haveCache)
            {
                var cachedDefinition = _cache.GetProcessDefinitionBySchemeId(schemeDefinition.Id);
                if (cachedDefinition != null)
                    return cachedDefinition;
                var processDefinition = Parser.Parse(schemeDefinition.Scheme);
                _cache.AddProcessDefinition(schemeDefinition.Id, processDefinition);
                return processDefinition;
            }
            
            return Parser.Parse(schemeDefinition.Scheme);
        }

        /// <summary>
        /// 根据ProcessInstance的ID从数据库获取ProcessInstance实例
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public ProcessInstance GetProcessInstance(Guid processId)
        {
            //根据ProcessInstance实例processId获取该实例基于的ProcessScheme的审批流程方案ProcessScheme，ProcessScheme是可以基于
            //WorkflowScheme模板派生出多种变化
            var schemeDefinition = SchemePersistenceProvider.GetProcessSchemeByProcessId(processId);

            return ProcessInstance.Create(schemeDefinition.Id,
                                          processId,
                                          GetProcessDefinition(schemeDefinition),
                                          schemeDefinition.IsObsolete,schemeDefinition.IsDeterminingParametersChanged);
        }

        /// <summary>
        /// 根据公文审批流程名称processName，创建新并保存的SchemeDefinition，
        /// 同时返回与processId对应的ProcessInstance实例
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="processName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public ProcessInstance CreateNewProcessScheme(Guid processId,
                                                      string processName,
                                                      IDictionary<string, IEnumerable<object>> parameters)
        {
            SchemeDefinition<TSchemeMedium> schemeDefinition = null;
            var schemeId = Guid.NewGuid();
            var newScheme = Generator.Generate(processName, schemeId, parameters);
            try
            {
                SchemePersistenceProvider.SaveScheme(processName, schemeId, newScheme, parameters);
                schemeDefinition = new SchemeDefinition<TSchemeMedium>(schemeId, newScheme, false, false);
            }
            catch (SchemeAlredyExistsException)
            {
                schemeDefinition = SchemePersistenceProvider.GetProcessSchemeWithParameters(processName, parameters,true);
            }

            return ProcessInstance.Create(schemeDefinition.Id,
                                          processId,
                                          GetProcessDefinition(schemeDefinition),
                                          schemeDefinition.IsObsolete,schemeDefinition.IsDeterminingParametersChanged);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="cache"></param>
        public void SetCache(IParsedProcessCache cache)
        {
            _cache = cache;
            _haveCache = true;
        }

        public void RemoveCache()
        {
            _haveCache = false;
            _cache.Clear();
            _cache = null;
        }
    }
}
