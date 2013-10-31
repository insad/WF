using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Runtime
{
    public interface IWorkflowRuleProvider
    {
        bool CheckRule(Guid processId, Guid identityId, string ruleName);

        bool CheckRule(Guid processId, Guid identityId, string ruleName, IDictionary<string, string> parameters);

        IEnumerable<Guid> GetIdentitiesForRule(Guid processId, string ruleName);

        IEnumerable<Guid> GetIdentitiesForRule(Guid processId, string ruleName, IDictionary<string, string> parameters);
    }
}
