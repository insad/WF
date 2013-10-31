using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Persistence
{ 
    /// <summary>
    /// 得到流程的具体方案信息
    /// </summary>
    /// <typeparam name="TSchemeMedium"></typeparam>
    public interface ISchemePersistenceProvider<TSchemeMedium> where TSchemeMedium : class
    {

        SchemeDefinition<TSchemeMedium> GetProcessSchemeByProcessId(Guid processId);

      
        SchemeDefinition<TSchemeMedium> GetProcessSchemeBySchemeId(Guid schemeId);


        SchemeDefinition<TSchemeMedium> GetProcessSchemeWithParameters(string processName,
                                                                       IDictionary<string, IEnumerable<object>>
                                                                           parameters,
                                                                       bool ignoreObsolete);

        SchemeDefinition<TSchemeMedium> GetProcessSchemeWithParameters(string processName,
                                                                       IDictionary<string, IEnumerable<object>>
                                                                           parameters);

        void SaveScheme(string processName,
                        Guid schemeId,
                        TSchemeMedium scheme,
                        IDictionary<string, IEnumerable<object>> parameters);

    }
}
