using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Cache;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Builder
{
    /// <summary>
    /// 流程构造器的接口
    /// </summary>
    public interface IWorkflowBuilder
    {
        /// <summary>
        /// 根据已有流程ID、流程名称及参数创建新的流程实例
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="processName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        ProcessInstance CreateNewProcess(Guid processId,
                                         string processName,
                                         IDictionary<string, IEnumerable<object>> parameters);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="processName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        ProcessInstance CreateNewProcessScheme(Guid processId,
                                               string processName,
                                               IDictionary<string, IEnumerable<object>> parameters);

        ProcessInstance GetProcessInstance(Guid processId);

        void SetCache(IParsedProcessCache cache);

        void RemoveCache();
    }

}
