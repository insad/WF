using System;
using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Runtime
{
    public class NeedDeterminingParametersEventArgs : EventArgs
    {
        public Guid ProcessId { get; set; }
        public IDictionary<string, IEnumerable<object>> DeterminingParameters { get; set; }
    }
}
