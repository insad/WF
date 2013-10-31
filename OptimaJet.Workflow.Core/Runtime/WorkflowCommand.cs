using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{

    [Serializable]
    [DataContract]
    public sealed class WorkflowCommand //: IEqualityComparer<WorkflowCommand>
    {
        [Serializable]
        [DataContract]
        public class CommandParameter
        {
            [DataMember]
            public string Name { get; internal set; }

            [DataMember]
            public object Value { get; set; }

            [DataMember]
            public Type Type { get; internal set; }
        }

        [DataMember]
        public Guid ProcessId { get; private set; }

        [DataMember]
        public string ValidForActivityName { get; private set; }

        public IEnumerable<Guid> Identities
        {
            get { return IdentitiesList; }
        }

         private  List<Guid> IdentitiesList = new List<Guid>(); 
        
        [DataMember]
        public string CommandName { get; private set; }

        [DataMember]
        public string LocalizedName { get; set; }

        [DataMember] public IEnumerable<CommandParameter> Parameters = new List<CommandParameter>();

        public object GetParameter(string name)
        {
            var parameter = Parameters.SingleOrDefault(p => p.Name == name);
            if (parameter == null)
                return null;
            return parameter.Value;
        }

        public void SetParameter(string name, object value)
        {
            var parameter = Parameters.SingleOrDefault(p => p.Name == name);
            if (parameter == null)
                throw new InvalidOperationException();
            if (parameter.Type != value.GetType())
                throw new InvalidOperationException();
            parameter.Value = value;
        }

        public static WorkflowCommand Create(Guid processId, TransitionDefinition transitionDefinition)
        {
            if (transitionDefinition.Trigger.Type != TriggerType.Command || transitionDefinition.Trigger.Command == null)
                throw new InvalidOperationException();

            var parametrs = new List<CommandParameter>(transitionDefinition.Trigger.Command.InputParameters.Count);
            parametrs.AddRange(
                transitionDefinition.Trigger.Command.InputParameters.Select(
                    p => new CommandParameter {Name = p.Key, Type = p.Value.Type, Value = null}));

            return new WorkflowCommand
                       {
                           CommandName = transitionDefinition.Trigger.Command.Name,
                           Parameters = parametrs,
                           ProcessId = processId,
                           ValidForActivityName = transitionDefinition.From.Name
                       };
        }

        public bool Validate ()
        {
            return Parameters.All(parameter => parameter.Value != null);
        }

        public void AddIdentity (Guid identityId)
        {
            if (!IdentitiesList.Contains(identityId))
                IdentitiesList.Add(identityId);
        }

        //public bool Equals(WorkflowCommand x, WorkflowCommand y)
        //{
        //    if (x == null && y == null)
        //        return true;

        //    if (x == null || y == null)
        //        return false;

        //    return x.CommandName == y.CommandName;
        //}

        //public int GetHashCode(WorkflowCommand obj)
        //{
        //    int hCode = bx.Height ^ bx.Length ^ bx.Width;
        //    return hCode.GetHashCode();
        //}
    }
}
