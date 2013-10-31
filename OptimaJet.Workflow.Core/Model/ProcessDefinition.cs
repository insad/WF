using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Fault;

namespace OptimaJet.Workflow.Core.Model
{


    public sealed class ProcessDefinition : BaseDefinition
    {
        public IEnumerable<ActorDefinition> Actors { get; private set; }
        public IEnumerable<ParameterDefinition> Parameters { get; private set; }
        public IEnumerable<CommandDefinition> Commands { get; private set; }
        public IEnumerable<ActionDefinition> Actions { get; private set; }
        public IEnumerable<ActivityDefinition> Activities { get; private set; }
        public IEnumerable<TransitionDefinition> Transitions { get; private set; }
        public IEnumerable<LocalizeDefinition> Localization { get; private set; }

        public ActivityDefinition InitialActivity
        {
            get
            {
                try
                {
                    var initialActivity = Activities.SingleOrDefault(a => a.IsInitial);
                    if (initialActivity == null)
                        throw new InitialActivityNotFoundException();
                    return initialActivity;
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
            }
        }

        public ActivityDefinition FindActivity (string name)
        {
            var activity = Activities.SingleOrDefault(a => a.Name == name);
            if (activity == null)
                throw new ActivityNotFoundException();
            return activity;
        }

        public TransitionDefinition FindTransition(string name)
        {
            var transition = Transitions.SingleOrDefault(a => a.Name == name);
            if (transition == null)
                throw new TransitionNotFoundException();
            return transition;
        }

        public IEnumerable<TransitionDefinition> GetPossibleTransitionsForActivity (ActivityDefinition activity)
        {
            return Transitions.Where(t => t.From == activity);
        }

        public IEnumerable<TransitionDefinition> GetCommandTransitions(ActivityDefinition activity)
        {
            return Transitions.Where(t => t.From == activity && t.Trigger.Type == TriggerType.Command);
        }

        public IEnumerable<TransitionDefinition> GetAutoTransitionForActivity(ActivityDefinition activity)
        {
            return Transitions.Where(t => t.From == activity && t.Trigger.Type == TriggerType.Auto);
        }

        public IEnumerable<TransitionDefinition> GetCommandTransitionForActivity(ActivityDefinition activity, string commandName)
        {
            return Transitions.Where(t => t.From == activity && t.Trigger.Type == TriggerType.Command && t.Trigger.Command.Name == commandName);
        }

        public IEnumerable<TransitionDefinition> GetTimerTransitionForActivity(ActivityDefinition activity)
        {
            return Transitions.Where(t => t.From == activity && t.Trigger.Type == TriggerType.Timer);
        }


        public static ProcessDefinition Create(string name,  IEnumerable<ActorDefinition> actors, IEnumerable<ParameterDefinition> parameters, IEnumerable<CommandDefinition> commands,
           IEnumerable<ActionDefinition> actions, IEnumerable<ActivityDefinition> activities, IEnumerable<TransitionDefinition> transitions, IEnumerable<LocalizeDefinition> localization)
        {
            return new ProcessDefinition
                       {
                           Actions = actions,
                           Activities = activities,
                           Actors = actors,
                           Commands = commands,
                           Name = name,
                           Parameters = parameters,
                           Transitions = transitions,
                           Localization = localization
                       };
        }

        public ParameterDefinition GetParameterDefinition(string name)
        {
            return Parameters.Single(p => p.Name == name);
        }

        public ParameterDefinition GetNullableParameterDefinition(string name)
        {
            return Parameters.SingleOrDefault(p => p.Name == name);
        }

        public IEnumerable<ParameterDefinition> PersistenceParameters
        {
            get { return Parameters.Where(p => p.Purpose == ParameterPurpose.Persistence); }
        }

    }
}
