using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Runtime
{
    /// <summary>
    /// 用记角色定义
    /// </summary>
    public interface IWorkflowRoleProvider
    {
        /// <summary>
        /// 该用户是否在这个角色下
        /// </summary>
        /// <param name="identityId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        bool IsInRole(Guid identityId, Guid roleId);
        /// <summary>
        /// 得到该角色下的所有员工
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        IEnumerable<Guid> GetAllInRole(Guid roleId);
    }
}
