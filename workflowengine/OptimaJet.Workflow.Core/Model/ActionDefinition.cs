using System;
using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Model
{
    public class ActionDefinition : BaseDefinition
    {
        public virtual Type Type { get; internal set; }

        public virtual string MethodName { get; internal set; }

        public virtual IEnumerable<ParameterDefinitionForAction> InputParameters
        {
            get { return InputParametersList; }
        }

        protected List<ParameterDefinitionForAction> OutputParametersList;

        public virtual IEnumerable<ParameterDefinitionForAction> OutputParameters
        {
            get { return OutputParametersList; }
        }

        protected List<ParameterDefinitionForAction> InputParametersList;

        public static ActionDefinition Create(string name, string type, string metodName)
        {
            return new ActionDefinition
                       {
                           Name = name,
                           InputParametersList = new List<ParameterDefinitionForAction>(),
                           OutputParametersList = new List<ParameterDefinitionForAction>(),
                           Type = Type.GetType(type),
                           MethodName = metodName
                       };
        }

        public static ActionDefinitionForActivity Create(ActionDefinition actionDefinition, string order)
        {
            var parsedOrder = int.Parse(order);
            return new ActionDefinitionForActivity {ActionDefinition = actionDefinition, Order = parsedOrder};
        }

        public static ActionDefinitionForActivity Create(ActionDefinition actionDefinition, int order)
        {
            return new ActionDefinitionForActivity {ActionDefinition = actionDefinition, Order = order};
        }

        public void AddInputParameterRef(ParameterDefinitionForAction parameter)
        {
            InputParametersList.Add(parameter);
        }

        public void AddOutputParameterRef(ParameterDefinitionForAction parameter)
        {
            OutputParametersList.Add(parameter);
        }

        public static ActionDefinition NoAction = new ActionDefinition();
    }

}
