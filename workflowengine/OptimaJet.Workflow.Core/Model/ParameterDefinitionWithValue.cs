using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Model
{
    public sealed class ParameterDefinitionWithValue : ParameterDefinition
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

        internal ParameterDefinition ParameterDefinition { private get;  set; }
        public object Value { get; internal set; }
    }
}
