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

    public sealed class WorkflowBuilder<TSchemeMedium> : IWorkflowBuilder where TSchemeMedium : class
    {
        internal IWorkflowGenerator<TSchemeMedium> Generator;

        internal IWorkflowParser<TSchemeMedium> Parser;

        internal ISchemePersistenceProvider<TSchemeMedium> SchemePersistenceProvider;

        private bool _haveCache;

        private IParsedProcessCache _cache;

        internal WorkflowBuilder()
        {
            
        }

        public WorkflowBuilder (IWorkflowGenerator<TSchemeMedium> generator,
                               IWorkflowParser<TSchemeMedium> parser,
                               ISchemePersistenceProvider<TSchemeMedium> schemePersistenceProvider)
        {
            Generator = generator;
            Parser = parser;
            SchemePersistenceProvider = schemePersistenceProvider;
        }


        public ProcessInstance CreateNewProcess(Guid processId,
                                                string processName,
                                                IDictionary<string, IEnumerable<object>> parameters)
        {
            SchemeDefinition<TSchemeMedium> schemeDefinition = null;
            try
            {
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

        public ProcessInstance GetProcessInstance(Guid processId)
        {
            var schemeDefinition = SchemePersistenceProvider.GetProcessSchemeByProcessId(processId);

            return ProcessInstance.Create(schemeDefinition.Id,
                                          processId,
                                          GetProcessDefinition(schemeDefinition),
                                          schemeDefinition.IsObsolete,schemeDefinition.IsDeterminingParametersChanged);
        }

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
