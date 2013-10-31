using System;
using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Bus
{
    public interface IWorkflowBus
    {
        void Initialize();
        void Start();
        void QueueExecution(IEnumerable<ExecutionRequestParameters> requestParameters);
        void QueueExecution(ExecutionRequestParameters requestParameters);
        event EventHandler<ExecutionResponseEventArgs> ExecutionComplete;
        bool IsAsync { get; }
    }
}
