using System;
using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Generator
{
    public interface IWorkflowGenerator<out TSchemeMedium> where TSchemeMedium : class
    {
        TSchemeMedium Generate(string processName, Guid schemeId, IDictionary<string, IEnumerable<object>> parameters);

        void AddMapping(string processName, object generatorSource);
    }
}
