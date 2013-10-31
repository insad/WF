using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Model
{
    public sealed class ParameterDefinitionForAction : ParameterDefinition
    {
        public override string Name
        {
            get
            {
                return ParameterDefinition.Name;
            }
            internal set
            {
                ParameterDefinition.Name = value;
            }
        }

        public override ParameterPurpose Purpose
        {
            get
            {
                return ParameterDefinition.Purpose;
            }
            internal set
            {
                ParameterDefinition.Purpose = value;
            }
        }

        public override Type Type
        {
            get
            {
                return ParameterDefinition.Type;
            }
            internal set
            {
                ParameterDefinition.Type = value;
            }
        }

        public override string SerializedDefaultValue
        {
            get
            {
                return ParameterDefinition.SerializedDefaultValue;
            }
            internal set
            {
                ParameterDefinition.SerializedDefaultValue = value;
            }
        }

        public ParameterDefinition ParameterDefinition { get; internal set; }
        public int Order { get; internal set; }
    }
}
