using System;

namespace OptimaJet.Workflow.Core.Model
{
    public class ParameterDefinition : BaseDefinition
    {

        public virtual Type Type { get; internal set; }
        public virtual ParameterPurpose Purpose { get; internal set; }
        public virtual string SerializedDefaultValue { get; internal set; }

        public static ParameterDefinition Create(string name, string type, string purpose, string serializedValue)
        {
            ParameterPurpose parsedPurpose;
            Enum.TryParse(purpose, true, out parsedPurpose);
            return new ParameterDefinition
                       {
                           Name = name,
                           Type = Type.GetType(type),
                           Purpose = parsedPurpose,
                           SerializedDefaultValue = serializedValue
                       };
        }

        public static ParameterDefinition Create(string name, string type, string serializedValue)
        {
            return Create(name, type, ParameterPurpose.Temporary.ToString("G"),serializedValue);
        }



        public static ParameterDefinitionForAction Create(ParameterDefinition parameterDefinition, string order)
        {
            var parsedOrder = int.Parse(order);
            return new ParameterDefinitionForAction {ParameterDefinition = parameterDefinition, Order = parsedOrder};
        }

        public static ParameterDefinitionWithValue Create(ParameterDefinition parameterDefinition, object value)
        {
            if (value != null && !value.GetType().Equals(parameterDefinition.Type) && !parameterDefinition.Type.IsAssignableFrom(value.GetType()))
                throw new InvalidOperationException();
            return new ParameterDefinitionWithValue {ParameterDefinition = parameterDefinition, Value = value};
        }
    }

}
