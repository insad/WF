using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Model
{
    public class RestrictionDefinition
    {
        public RestrictionType Type { get; internal set; }
        public ActorDefinition Actor { get; internal set; }

        public static RestrictionDefinition Create (string type, ActorDefinition actor)
        {
            RestrictionType parsedType;
            Enum.TryParse(type, true, out parsedType);

            return new RestrictionDefinition() { Actor = actor, Type = parsedType };
        }
    }

    public enum RestrictionType
    {
        Allow, Restrict
    }
}
