using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Model
{
    public enum LocalizeType
    {
        Command, State
    }

    public sealed class LocalizeDefinition
    {
        public LocalizeType Type { get; internal set; }

        public bool IsDefault { get; internal set; }

        public string ObjectName { get; internal set; }

        public string Culture { get; internal set; }

        public string Value { get; internal set; }


        public static LocalizeDefinition Create(string objectName, string type, string culture, string value, string isDefault)
        {
            LocalizeType parsedType;
            Enum.TryParse(type, true, out parsedType);

            return new LocalizeDefinition
                       {
                           Culture = culture,
                           IsDefault = !string.IsNullOrEmpty(isDefault) && bool.Parse(isDefault),
                           ObjectName = objectName,
                           Type = parsedType,
                           Value = value
                       };

        }
    }
}
