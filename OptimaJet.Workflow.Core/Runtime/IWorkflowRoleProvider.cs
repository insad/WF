using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Runtime
{
    public interface IWorkflowRoleProvider
    {
        bool IsInRole(Guid identityId, Guid roleId);
        IEnumerable<Guid> GetAllInRole(Guid roleId);
    }
}
