using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Persistence
{
    //Все работы связанные с хранилищем возможно рабиение на отдельные интерфейсы

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
