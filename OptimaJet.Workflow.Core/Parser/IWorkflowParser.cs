using System.Collections.Generic;
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Parser
{
    public interface IWorkflowParser<in TSchemeMedium> where TSchemeMedium : class
    {
        ProcessDefinition Parse(TSchemeMedium schemeMedium);

    }


}
