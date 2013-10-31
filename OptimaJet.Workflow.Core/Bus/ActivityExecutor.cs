using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Bus
{
    public class ActivityExecutor
    {
        private bool ConsiderResultOnPreExecution { get; set; }

        public ActivityExecutor()
        {
            ConsiderResultOnPreExecution = false;
        }

        public ActivityExecutor(bool considerResultOnPreExecution)
        {
            ConsiderResultOnPreExecution = considerResultOnPreExecution;
        }

        public ExecutionResponseParameters Execute(IEnumerable<ExecutionRequestParameters> requestParameters)
        {
            var requestParametersList = requestParameters.ToList();

            var always = !ConsiderResultOnPreExecution
                             ? requestParametersList.SingleOrDefault(rp => rp.ConditionType == ConditionType.Always)
                             : requestParametersList.SingleOrDefault(
                                 rp =>
                                 rp.ConditionType == ConditionType.Always &&
                                 (!rp.ConditionResultOnPreExecution.HasValue || rp.ConditionResultOnPreExecution.Value));

            if (always != null)
            {
                try
                {
                    return ExecuteMethod(always);
                }
                catch (Exception ex)
                {
                    return GetExecutionResponseErrorParameters(always, ex);
                }
            }



            foreach (var condition in requestParametersList.Where(rp => rp.ConditionType == ConditionType.Action))
            {
                try
                {
                    if (CheckCondition(condition))
                    {
                        return ExecuteMethod(condition);
                    }
                }
                catch (Exception ex)
                {
                    return GetExecutionResponseErrorParameters(condition, ex);
                }
            }

            var otherwise = requestParametersList.SingleOrDefault(rp => rp.ConditionType == ConditionType.Otherwise);

            if (otherwise != null)
            {
                try
                {
                    return ExecuteMethod(otherwise);
                }
                catch (Exception ex)
                {
                    return GetExecutionResponseErrorParameters(otherwise, ex);
                }
            }

            var executionParameters =  ExecutionResponseParameters.Empty;
            executionParameters.ProcessId = requestParameters.First().ProcessId;
            return executionParameters;
        }

        private static ExecutionResponseParameters GetExecutionResponseErrorParameters(ExecutionRequestParameters always,
                                                                                       Exception ex)
        {
            var errorResponse = string.IsNullOrEmpty(always.TransitionName)
                                    ? ExecutionResponseParameters.Create(always.ProcessId, always.ActivityName, ex)
                                    : ExecutionResponseParameters.Create(always.ProcessId, always.ActivityName,
                                                                         always.TransitionName, ex);

            return errorResponse;
        }

        private bool CheckCondition(ExecutionRequestParameters parameters)
        {
            if (parameters.ConditionType != ConditionType.Action || parameters.ConditionMethod == null ||
                parameters.ConditionMethod.OutputParameters.Count() != 1)
                throw new InvalidOperationException();

            if (ConsiderResultOnPreExecution && parameters.ConditionResultOnPreExecution.HasValue)
                return parameters.ConditionResultOnPreExecution.Value;

            var response = ExecutionResponseParameters.Create(parameters.ProcessId, parameters.ActivityName, parameters.ParameterContainer);
            ExecuteMethod(parameters.ConditionMethod, response, parameters.ParameterContainer);
            var result = response.ParameterContainer.SingleOrDefault(p => p.Name == DefaultDefinitions.ParameterConditionResult.Name);
            if (result == null || result.Value == null || !(result.Value  is bool))
                throw new InvalidOperationException();

            return (bool)result.Value;

        }

        private ExecutionResponseParameters ExecuteMethod(ExecutionRequestParameters parameters)
        {
            
            var response = string.IsNullOrEmpty(parameters.TransitionName)
                               ? ExecutionResponseParameters.Create(parameters.ProcessId, parameters.ActivityName, parameters.ParameterContainer)
                               : ExecutionResponseParameters.Create(parameters.ProcessId, parameters.ActivityName, parameters.TransitionName, parameters.ParameterContainer);



                foreach (var method in parameters.Methods.OrderBy(m => m.Order))
                {
                    ExecuteMethod(method, response, response.ParameterContainer);
                }
          

            return response;

        }

        private object GetParameterValue(ParameterContainerInfo[] parameterContainer, MethodParameterInfo parameterInfo)
        {
            var parameter = parameterContainer.SingleOrDefault(pc => pc.Name == parameterInfo.Name);
            if (parameter == null || parameter.Value == null)
                return parameterInfo.DefaultValue;

            return parameter.Value;

        }

        private object GetParameterValueNullable(ParameterContainerInfo[] parameterContainer, string name, Type type)
        {
            var p = parameterContainer.SingleOrDefault(pc => pc.Name == name);
            return p == null
                       ? (type.IsValueType
                              ? Activator.CreateInstance(type)
                              : null)
                       : p.Value;
        }


        private  void ExecuteMethod(ExecutionRequestParameters.MethodToExecuteInfo method,
                                         ExecutionResponseParametersComplete response, ParameterContainerInfo[] parameterContainer )
        {
            var type = Type.GetType(method.Type);
            if (type == null)
                throw new InvalidOperationException();

            var types = method.InputParameters.OrderBy(ip => ip.Order).Select(ip => ip.Type).ToList();
            types.AddRange(method.OutputParameters.OrderBy(ip => ip.Order).Select(ip => ip.Type.MakeByRefType()));

            var values = method.InputParameters.OrderBy(ip => ip.Order).Select(ip => GetParameterValue(parameterContainer, ip)).ToList();
            values.AddRange(method.OutputParameters.OrderBy(ip => ip.Order).Select(ip => GetParameterValueNullable(parameterContainer, ip.Name, ip.Type)));

            var valuesArr = values.ToArray();

            var methodInfo = type.GetMethod(method.MethodName, types.ToArray());


            if (methodInfo.IsStatic)
            {
                var a = methodInfo.Invoke(null, valuesArr);
            }
            else
            {
                var obj = Activator.CreateInstance(type, BindingFlags.NonPublic);
                methodInfo.Invoke(obj, valuesArr);
            }

            response.AddNewParameterValues(method.OutputParameters.OrderBy(output => output.Order).ToList(), valuesArr.Skip(method.InputParameters.Count()).ToList());
        }
    }
}
