using System;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Persistence
{
    //Все работы связанные с хранилищем возможно рабиение на отдельные интерфейсы

    public interface ISchemaPersistenceProvider<TSchemaMedium> where TSchemaMedium : class
    {

        SchemeDefinition<TSchemaMedium> GetProcessSchemeByProcessId(Guid processId);

      
        SchemeDefinition<TSchemaMedium> GetProcessSchemeBySchemeId(Guid schemeId);


        SchemeDefinition<TSchemaMedium> GetProcessSchemeWithParameters(string processName,
                                                                       IDictionary<string, IEnumerable<object>>
                                                                           parameters,
                                                                       bool ignoreObsolete);

        SchemeDefinition<TSchemaMedium> GetProcessSchemeWithParameters(string processName,
                                                                       IDictionary<string, IEnumerable<object>>
                                                                           parameters);

        void SaveSchema(string processName,
                        Guid schemaId,
                        TSchemaMedium schema,
                        IDictionary<string, IEnumerable<object>> parameters);

    }
}
