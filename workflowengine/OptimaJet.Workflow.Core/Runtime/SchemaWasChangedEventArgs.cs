using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Runtime
{
    public class SchemaWasChangedEventArgs : EventArgs
    {
        public Guid ProcessId { get; set; }
        public Guid SchemeId { get; set; }
        public bool SchemaWasObsolete { get; set; }
        public bool DeterminingParametersWasChanged { get; set; }
    }
}
