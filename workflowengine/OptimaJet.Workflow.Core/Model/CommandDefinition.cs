using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Model
{
    public class CommandDefinition : BaseDefinition
    {
        public Dictionary<string, ParameterDefinition> InputParameters { get; internal set; }

        public static CommandDefinition Create(string name)
        {
            return new CommandDefinition()
                       {Name = name, InputParameters = new Dictionary<string, ParameterDefinition>()};
        }

        public void AddParameterRef(string name, ParameterDefinition parameter)
        {
            InputParameters.Add(name,parameter);
        }
    }

}
