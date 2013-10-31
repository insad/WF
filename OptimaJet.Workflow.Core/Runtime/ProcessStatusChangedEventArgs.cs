using System;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.Core.Runtime
{
    public class ProcessStatusChangedEventArgs : EventArgs
    {
        public Guid ProcessId { get; private set; }
        public ProcessStatus OldStatus { get; private set; }
        public ProcessStatus NewStatus { get; private set; }

        public ProcessStatusChangedEventArgs (Guid processId, ProcessStatus oldStatus, ProcessStatus newStatus)
        {
            ProcessId = processId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }
}
