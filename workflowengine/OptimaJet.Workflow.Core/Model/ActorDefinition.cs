using System;
using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Model
{
    public abstract class ActorDefinition : BaseDefinition
    {

        public static ActorDefinition CreateRule(string name, string ruleName)
        {
            return new ActorDefinitionExecuteRule { Name = name, RuleName = ruleName, ParametersDictionary = new Dictionary<string, string>()};
        }

        public static ActorDefinition CreateIsIdentity(string name, string identityId)
        {
            return new ActorDefinitionIsIdentity() {Name = name, IdentityId = new Guid(identityId)};
        }

        public static ActorDefinition CreateIsInRole(string name, string roleId)
        {
            return new ActorDefinitionIsInRole() { Name = name, RoleId = new Guid(roleId) };
        }

        public virtual void AddParameter (string key, string value){}

    }

    public class ActorDefinitionExecuteRule : ActorDefinition
    {
        public string RuleName { get; internal set; }

        public IDictionary<string, string> Parameters { get { return ParametersDictionary; } }

        internal Dictionary<string, string> ParametersDictionary { get; set; }

        public override void AddParameter(string key, string value)
        {
            ParametersDictionary.Add(key,value);
        }
    }

    public class ActorDefinitionIsIdentity : ActorDefinition
    {
        public Guid IdentityId { get; internal set; }

      
    }

    public class ActorDefinitionIsInRole : ActorDefinition
    {
        public Guid RoleId { get; internal set; }

        
    }
}
