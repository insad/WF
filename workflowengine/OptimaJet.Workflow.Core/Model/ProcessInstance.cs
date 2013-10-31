using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// ProcessInstance是公文流程实例的内存处理数据模型
    /// 审批如何进行和处理都是基于ProcessInstance进行，
    /// 并审批流转关系是从ProcessDefinition获取
    /// </summary>
    public class ProcessInstance
    {
        public Guid ProcessId { get; internal set; }
        public Guid SchemeId { get; internal set; }
        /// <summary>
        /// 流转的定义方案
        /// </summary>
        public ProcessDefinition ProcessScheme { get; internal set; }

        /// <summary>
        /// 流转定义是否已经过时
        /// </summary>
        public bool IsSchemeObsolete { get; internal set; }
        /// <summary>
        /// 关键性参数是否已经发生变化
        /// </summary>
        public bool IsDeterminingParametersChanged { get; internal set; }

        public IEnumerable<ParameterDefinitionWithValue> ProcessParameters
        {
            get { return _processParameters; }
        }

        private readonly List<ParameterDefinitionWithValue> _processParameters = new List<ParameterDefinitionWithValue>();


        public static ProcessInstance Create(Guid schemeId, Guid processId, ProcessDefinition processScheme, bool isSchemeObsolete, bool isDeterminingParametersChanged)
        {
            return new ProcessInstance() {SchemeId = schemeId, ProcessId = processId, ProcessScheme = processScheme, IsSchemeObsolete = isSchemeObsolete, IsDeterminingParametersChanged = isDeterminingParametersChanged};
        }

        public void AddParameter (ParameterDefinitionWithValue parameter)
        {
            _processParameters.RemoveAll(p => p.Name == parameter.Name);
            _processParameters.Add(parameter);
        }

        public void AddParameters(IEnumerable<ParameterDefinitionWithValue> parameters)
        {
            _processParameters.RemoveAll(ep => parameters.Count(p=>p.Name == ep.Name) > 0);
            _processParameters.AddRange(parameters);
        }


        /// <summary>
        /// 获取指定名称参数
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ParameterDefinitionWithValue GetParameter(string name)
        {
            return _processParameters.SingleOrDefault(p => p.Name == name);
        }

        
        /// <summary>
        /// 获取系统定义的当前活动名称
        /// </summary>
        public string CurrentActivityName
        {
            get
            {
                var parameter = GetParameter(DefaultDefinitions.ParameterCurrentActivity.Name);
                return parameter == null ? null : (string) parameter.Value;
            }
        }

        /// <summary>
        /// 获取当前流程实例的当前活动ActivityDefinition，当状态流转时，如何设置当前活动
        /// </summary>
        public ActivityDefinition CurrentActivity
        {
            get { return ProcessScheme.FindActivity(CurrentActivityName); }
        }


        public string GetLocalizedStateName (string stateName, CultureInfo culture)
        {
            return GetLocalizedName(stateName, culture, LocalizeType.State);
        }

        public string GetLocalizedCommandName(string commandName, CultureInfo culture)
        {
            return GetLocalizedName(commandName, culture, LocalizeType.Command);
        }

        /// <summary>
        /// 根据名称获取流程定义中的本地化名称，在ProcessScheme的XML文档中定义的
        /// </summary>
        /// <param name="name"></param>
        /// <param name="culture"></param>
        /// <param name="localizeType"></param>
        /// <returns></returns>
        protected string GetLocalizedName(string name, CultureInfo culture, LocalizeType localizeType)
        {
            var localize =
                ProcessScheme.Localization.FirstOrDefault(
                    l =>
                    l.Type == localizeType && string.Compare(l.Culture, culture.Name, true) == 0 &&
                    l.ObjectName == name);

            if (localize != null)
                return localize.Value;

            localize =
                ProcessScheme.Localization.FirstOrDefault(
                    l =>
                    l.Type == localizeType && l.IsDefault &&
                    l.ObjectName == name);

            if (localize != null)
                return localize.Value;

            return name;
        }
    }
}
