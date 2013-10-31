using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Model
{
    public class ActionDefinitionForActivity : ActionDefinition
    {
        public override string Name
        {
            get { return ActionDefinition.Name; }
            internal set { ActionDefinition.Name = value; }
        }

        public override IEnumerable<ParameterDefinitionForAction> InputParameters
        {
            get { return ActionDefinition.InputParameters; }
        }

        public override IEnumerable<ParameterDefinitionForAction> OutputParameters
        {
            get { return ActionDefinition.OutputParameters; }
        }

        public override string MethodName
        {
            get { return ActionDefinition.MethodName; }
            internal set { ActionDefinition.MethodName = value; }
        }

        public override Type Type
        {
            get { return ActionDefinition.Type; }
            internal set { ActionDefinition.Type = value; }
        }

        public ActionDefinition ActionDefinition { get; internal set; }
        public int Order { get; internal set; }
    }
}
