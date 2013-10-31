using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Model
{
    public class ActivityDefinition : BaseDefinition
    {
        public string State { get; internal set; }
        /// <summary>
        /// IsInitial表示为流程的其实动作，启动流程时，如果为 true则立即运行该操作
        /// </summary>
        public bool IsInitial { get; internal set; }
        /// <summary>
        /// 是否为最后一个流程节点操作
        /// </summary>
        public bool IsFinal { get; internal set; }
        public bool IsForSetState { get; internal set; }
        public bool IsAutoSchemeUpdate { get; internal set; }

        public bool HaveImplementation
        {
            get { return Implemementation.Count > 0; }
        }

        public bool HavePreExecutionImplementation
        {
            get { return PreExecutionImplemementation.Count > 0; }
        }

        public List<ActionDefinitionForActivity> Implemementation { get; internal set; }

        public List<ActionDefinitionForActivity> PreExecutionImplemementation { get; internal set; } 

        public bool IsState
        {
            get { return !string.IsNullOrEmpty(Name); }
        }

        public static ActivityDefinition Create(string name, string stateName, string isInitial, string isFinal, string isForSetState, string isAutoSchemeUpdate)
        {
            return new ActivityDefinition()
                       {
                           IsFinal = !string.IsNullOrEmpty(isFinal) && bool.Parse(isFinal),
                           IsInitial = !string.IsNullOrEmpty(isInitial) && bool.Parse(isInitial),
                           IsForSetState = !string.IsNullOrEmpty(isForSetState) && bool.Parse(isForSetState),
                           IsAutoSchemeUpdate = !string.IsNullOrEmpty(isAutoSchemeUpdate) && bool.Parse(isAutoSchemeUpdate),
                           Name = name,
                           State = stateName,
                           Implemementation = new List<ActionDefinitionForActivity>(),
                           PreExecutionImplemementation = new List<ActionDefinitionForActivity>()
                       };
        }

        public void AddAction(ActionDefinitionForActivity action)
        {
            Implemementation.Add(action);
        }

        public void AddPreExecutionAction(ActionDefinitionForActivity action)
        {
            PreExecutionImplemementation.Add(action);
        }
    }
}
