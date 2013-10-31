using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Runtime;

namespace WF.Sample.Business.Workflow
{
    public class WorkflowRole : IWorkflowRoleProvider
    {
        public bool IsInRole(Guid identityId, Guid roleId)
        {
            using (var context = new DataModelDataContext())
            {
                return context.EmployeeRoles.Count(er => er.EmloyeeId == identityId && er.RoleId == roleId) > 0;
            }
        }

        public IEnumerable<Guid> GetAllInRole(Guid roleId)
        {
            using (var context = new DataModelDataContext())
            {
                return context.EmployeeRoles.Where(er => er.RoleId == roleId).Select(er=>er.EmloyeeId).ToList();
            }
        }
    }
}
