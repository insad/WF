using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Cache;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Builder
{
    public interface IWorkflowBuilder
    {
        ProcessInstance CreateNewProcess(Guid processId,
                                         string processName,
                                         IDictionary<string, IEnumerable<object>> parameters);

        ProcessInstance CreateNewProcessScheme(Guid processId,
                                               string processName,
                                               IDictionary<string, IEnumerable<object>> parameters);

        ProcessInstance GetProcessInstance(Guid processId);

        void SetCache(IParsedProcessCache cache);

        void RemoveCache();
    }

}
