using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Cache
{
    //TODO Multithread
    public sealed class DefaultParcedProcessCache : IParsedProcessCache
    {
        private Dictionary<Guid, ProcessDefinition> _cache;

        public void Clear()
        {
            _cache.Clear();
        }

        public ProcessDefinition GetProcessDefinitionBySchemeId(Guid schemeId)
        {
            if (_cache == null)
                return null;
            if (_cache.ContainsKey(schemeId))
                return _cache[schemeId];
            return null;
        }

        public void AddProcessDefinition(Guid schemeId, ProcessDefinition processDefinition)
        {
            if (_cache == null)
            {
                _cache = new Dictionary<Guid, ProcessDefinition> {{schemeId, processDefinition}};
            }
            else
            {
                if (_cache.ContainsKey(schemeId))
                    _cache[schemeId] = processDefinition;
                else
                    _cache.Add(schemeId, processDefinition);
            }
        }
    }
}
