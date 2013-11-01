using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Bus
{
    /// <summary>
    /// 方法的参数信息
    /// </summary>
    [Serializable]
    public class MethodParameterInfo
    {
        public int Order { get; internal set; }
        public Type Type { get; internal set; }
        public string Name { get; internal set; }
        public object DefaultValue { get; set; }
    }

    /// <summary>
    /// 参数信息
    /// </summary>
    [Serializable]
    public class ParameterContainerInfo
    {
        public object Value { get; set; }
        public Type Type { get; internal set; }
        public string Name { get; internal set; }
    }

    /// <summary>
    /// 执行请求参数
    /// </summary>
    [Serializable]
    public sealed class ExecutionRequestParameters
    {
        public ParameterContainerInfo[] ParameterContainer { get; set; }
       
        [Serializable]
        public  class MethodToExecuteInfo
        {
            public int Order { get; set; }
            public string Type { get;  set; }
            public string MethodName { get;  set; }
            public string ActionName { get;  set; }
            public MethodParameterInfo[] InputParameters { get;  set; }
            public MethodParameterInfo[] OutputParameters { get;  set; }
        }

        public Guid ProcessId { get;  set; }

        public MethodToExecuteInfo[] Methods { get;  set; }

        public ConditionType ConditionType { get;  set; }

        public MethodToExecuteInfo ConditionMethod { get;  set; }

        public bool? ConditionResultOnPreExecution { get; set; }
     
        public string TransitionName { get;  set; }

        public string ActivityName { get; set; }


        public static ExecutionRequestParameters Create(Guid processId, IEnumerable<ParameterDefinitionWithValue> parameters, TransitionDefinition transition)
        {
            return Create( processId,  parameters,  transition, false);
        }

        public static ExecutionRequestParameters Create(Guid processId, IEnumerable<ParameterDefinitionWithValue> parameters, TransitionDefinition transition, bool isPreExecution)
        {
            var ret = Create(processId, parameters, transition.To, transition.Condition, isPreExecution);
            ret.TransitionName = transition.Name;
            ret.ActivityName = transition.To.Name;
            return ret;
        }

        public static ExecutionRequestParameters Create(Guid processId, IEnumerable<ParameterDefinitionWithValue> parameters, ActivityDefinition activityToExecute, ConditionDefinition condition)
        {
            return Create(processId, parameters, activityToExecute, condition, false);
        }

        public static ExecutionRequestParameters Create(Guid processId, IEnumerable<ParameterDefinitionWithValue> parameters, ActivityDefinition activityToExecute, ConditionDefinition condition, bool isPreExecution)
        {
            List<ActionDefinitionForActivity> implementation = isPreExecution
                                                                   ? activityToExecute.PreExecutionImplemementation
                                                                   : activityToExecute.Implemementation;

            if (parameters == null) throw new ArgumentNullException("parameters");
            var parametersList = parameters.ToList();

            var methods = new List<MethodToExecuteInfo>(implementation.Count);
            var executionParameters = new ExecutionRequestParameters
                                          {
                                              ProcessId = processId,
                                              ConditionType = condition.Type,
                                              ConditionResultOnPreExecution = condition.ResultOnPreExecution,
                                              ConditionMethod = condition.Type == ConditionType.Action ? GetMethodToExecuteInfo(parametersList, ActionDefinition.Create(condition.Action, 0)) : null,
                                          };

            methods.AddRange(implementation.Select(method => GetMethodToExecuteInfo(parametersList, method)));

            var parameters1 = new List<ParameterContainerInfo>();

            parameters1.AddRange(parameters.Where(p => p.Name != DefaultDefinitions.ParameterExecutedActivityState.Name).Select(p => new ParameterContainerInfo() { Name = p.Name, Type = p.Type, Value = p.Value }));
            parameters1.Add(new ParameterContainerInfo{Name = DefaultDefinitions.ParameterExecutedActivityState.Name,Type = DefaultDefinitions.ParameterExecutedActivityState.Type,Value = activityToExecute.State});
            executionParameters.ParameterContainer = parameters1.ToArray();
            executionParameters.Methods = methods.ToArray();

            executionParameters.ActivityName = activityToExecute.Name;
            
            return executionParameters;

        }

        private static MethodToExecuteInfo GetMethodToExecuteInfo(IEnumerable<ParameterDefinitionWithValue> parameters, ActionDefinitionForActivity action)
        {
            var inputParameters = new List<MethodParameterInfo>(action.InputParameters.Count());
            var outputParameters = new List<MethodParameterInfo>(action.OutputParameters.Count());
            var method = new MethodToExecuteInfo
                             {
                                 Type = action.Type.AssemblyQualifiedName,
                                 MethodName = action.MethodName,
                                 Order = action.Order,
                                 ActionName = action.Name
                             };

            inputParameters.AddRange(
                action.InputParameters.Select(
                    inParameter =>
                    new MethodParameterInfo
                        {
                            Order = inParameter.Order,
                            Name = inParameter.Name,
                            Type = inParameter.Type,
                            //TODO -  	反序列化
                            DefaultValue = inParameter.SerializedDefaultValue
                        }));

            outputParameters.AddRange(
                action.OutputParameters.Select(
                    outParameter =>
                    new MethodParameterInfo
                        {
                            Order = outParameter.Order,
                            Name = outParameter.Name,
                            Type = outParameter.Type
                        }));
            method.InputParameters = inputParameters.ToArray();
            method.OutputParameters = outputParameters.ToArray();
            return method;
        }

        /// <summary>
        /// 把ParameterDefinitionWithValue参数及其值添加到该ParameterInContainer中
        /// </summary>
        /// <param name="parameter"></param>
        public void AddParameterInContainer (ParameterDefinitionWithValue parameter)
        {
            var parameterInContainer = ParameterContainer.FirstOrDefault(p => p.Name == parameter.Name);

            if (parameterInContainer == null)
            {
                parameterInContainer = new ParameterContainerInfo{Name = parameter.Name};
                var list = ParameterContainer.ToList();
                list.Add(parameterInContainer);
                ParameterContainer = list.ToArray();
            }

            parameterInContainer.Type = parameter.Type;
            parameterInContainer.Value = parameter.Value;
        }
    }
}
