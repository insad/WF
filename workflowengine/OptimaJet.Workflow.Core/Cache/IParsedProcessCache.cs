using System;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Cache
{
    public interface IParsedProcessCache
    {
        void Clear();

        ProcessDefinition GetProcessDefinitionBySchemeId(Guid schemeId);

        void AddProcessDefinition(Guid schemeId, ProcessDefinition processDefinition);
    }
}
