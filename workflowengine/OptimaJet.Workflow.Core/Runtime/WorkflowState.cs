using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OptimaJet.Workflow.Core.Runtime
{

    [Serializable]
    [DataContract]
    public class WorkflowState
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string VisibleName { get; set; }
    }
}
