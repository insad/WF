using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Bus
{
    [Serializable]
    public sealed class ExecutionParameters
    {
        [Serializable]
        public class MethodToExecuteParameterInfo
        {
            public int Order { get; internal set; }
            public object Value { get; internal set; }
            public Type Type { get; internal set; }
        }

        [Serializable]
        public  class MethodToExecuteInfo
        {
            public int Order { get; internal set; }
            public string Type { get; internal set; }
            public string MethodName { get; internal set; }
            public IEnumerable<MethodToExecuteParameterInfo> InputParameters { get; internal set; }
            public IEnumerable<MethodToExecuteParameterInfo> OutputParameters { get; internal set; }
        }

        public Guid ProcessId { get; internal set; }

        public IEnumerable<MethodToExecuteInfo> Methods { get; internal set; }
        
        
        public static ExecutionParameters Create (Guid processId, IDictionary<string,object> parameters, ActivityDefinition activityToExecute)
        {
            if (activityToExecute.Implemementation.Count < 1)
                throw new InvalidOperationException();

            var methods = new List<MethodToExecuteInfo>(activityToExecute.Implemementation.Count);
            var executionParameters = new ExecutionParameters
                                          {
                                     ProcessId = processId,
                                     Methods = methods,
                                     
                                 };

            foreach (var action in activityToExecute.Implemementation)
            {
                var inputParameters = new List<MethodToExecuteParameterInfo>(action.InputParameters.Count);
                var outputParameters = new List<MethodToExecuteParameterInfo>(action.OutputParameters.Count);
                var method = new MethodToExecuteInfo
                                 {
                                     Type = action.Type.AssemblyQualifiedName,
                                     MethodName = action.MethodName,
                                     InputParameters = inputParameters,
                                     OutputParameters = outputParameters,
                                     Order = action.Order
                                 };

                inputParameters.AddRange(
                    action.InputParameters.Select(
                        inParameter =>
                        new MethodToExecuteParameterInfo
                            {
                                Order = inParameter.Order,
                                Value = parameters[inParameter.Name],
                                Type = inParameter.ParameterDefinition.Type
                            }));

                outputParameters.AddRange(
                    action.OutputParameters.Select(
                        outParameter =>
                        new MethodToExecuteParameterInfo
                            {
                                Order = outParameter.Order,
                                Value =
                                    parameters.ContainsKey(outParameter.Name)
                                        ? parameters[outParameter.Name]
                                        : null,
                                Type = outParameter.ParameterDefinition.Type
                            }));

                methods.Add(method);
            }

            return executionParameters;
        }
    }
}
