using System;
using System.Collections.Generic;
using System.Linq;

namespace OptimaJet.Workflow.Core.Bus
{
    [Serializable]
    public class ExecutionResponseParameters
    {
        public Guid ProcessId { get; set; }
        public string ExecutedTransitionName { get; set; }
        public string ExecutedActivityName { get; set; }

        public virtual bool IsError
        {
            get { return false; }
        }


        public static ExecutionResponseParametersComplete Create(Guid processId,
                                                                 string executedActivityName,
                                                                 IEnumerable<ParameterContainerInfo> parameters)
        {
            return new ExecutionResponseParametersComplete
                       {
                           ProcessId = processId,
                           ParameterContainer = parameters.ToArray(),
                           ExecutedActivityName = executedActivityName
                       };
        }


        public static ExecutionResponseParametersComplete Create(Guid processId,
                                                                 string executedActivityName,
                                                                 string executedTransitionName,
                                                                 IEnumerable<ParameterContainerInfo> parameters)
        {
            return new ExecutionResponseParametersComplete
                       {
                           ProcessId = processId,
                           ExecutedTransitionName = executedTransitionName,
                           IsEmplty = false,
                           ParameterContainer = parameters.ToArray(),
                           ExecutedActivityName = executedActivityName
                       };
        }


        public static ExecutionResponseParametersError Create(Guid processId,
                                                               string executedActivityName,
                                                               Exception exception)
        {
            return new ExecutionResponseParametersError
            {
                ProcessId = processId,
                Exception = exception,
                ExecutedActivityName = executedActivityName
            };
        }


        public static ExecutionResponseParametersError Create(Guid processId,
                                                                 string executedActivityName,
                                                                 string executedTransitionName,
                                                                 Exception exception)
        {
            return new ExecutionResponseParametersError
            {
                ProcessId = processId,
                ExecutedTransitionName = executedTransitionName,
                IsEmplty = false,
                Exception = exception,
                ExecutedActivityName = executedActivityName
            };
        }


        public bool IsEmplty { get; set; }

        public static ExecutionResponseParameters Empty
        {
            get { return new ExecutionResponseParameters() {IsEmplty = true}; }
        }
    }

    [Serializable]
    public sealed class ExecutionResponseParametersComplete : ExecutionResponseParameters
    {
        public ParameterContainerInfo[] ParameterContainer { get; set; }

        public void AddNewParameterValues(List<MethodParameterInfo> parameterInfos, List<object> values)
        {
            if (IsEmplty)
                throw new InvalidOperationException();

            var parameterContainer = ParameterContainer.ToList();
            var skip = 0;
            foreach (var methodParameterInfo in parameterInfos)
            {
                var parameterInContainer = parameterContainer.SingleOrDefault(p => p.Name == methodParameterInfo.Name);
                if (parameterInContainer == null)
                {
                    parameterInContainer = new ParameterContainerInfo()
                                               {Name = methodParameterInfo.Name, Type = methodParameterInfo.Type};
                    parameterContainer.Add(parameterInContainer);
                }
                parameterInContainer.Value = values.Skip(skip).Take(1).SingleOrDefault();
                skip++;
            }

            ParameterContainer = parameterContainer.ToArray();

        }

    }

    [Serializable]
    public sealed class ExecutionResponseParametersError : ExecutionResponseParameters
    {
        public override bool IsError
        {
            get { return true; }
        }

        public Exception Exception { get; set; }
    }
}
