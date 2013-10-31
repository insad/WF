using System;
using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Bus
{
    public class NullBus : IWorkflowBus
    {
        public void Initialize()
        {
        }

        public void Start()
        {
        }

        public void QueueExecution(IEnumerable<ExecutionRequestParameters> requestParameters)
        {
            var executor = new ActivityExecutor();
            var response = executor.Execute(requestParameters);
            if (ExecutionComplete != null)
            {
                var args = new ExecutionResponseEventArgs(response);
                ExecutionComplete(this,args);
            }
        }

        public void QueueExecution(ExecutionRequestParameters requestParameters)
        {
           QueueExecution(new[] {requestParameters});
        }

        public event EventHandler<ExecutionResponseEventArgs> ExecutionComplete;

        public bool IsAsync
        {
            get { return false; }
        }
    }
}
