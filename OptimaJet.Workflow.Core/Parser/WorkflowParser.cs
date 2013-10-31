using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Parser
{
    public abstract class WorkflowParser<TSchemeMedium>  : IWorkflowParser<TSchemeMedium> where TSchemeMedium : class
    {
        public abstract IEnumerable<TimerDefinition> ParseTimers(TSchemeMedium schemeMedium);

        public abstract IEnumerable<ActorDefinition> ParseActors(TSchemeMedium schemeMedium);

        public abstract IEnumerable<LocalizeDefinition> ParseLocalization(TSchemeMedium schemeMedium);

        public abstract IEnumerable<ParameterDefinition> ParseParameters(TSchemeMedium schemeMedium);

        public abstract IEnumerable<CommandDefinition> ParseCommands(TSchemeMedium schemeMedium,
                                                     IEnumerable<ParameterDefinition> parameterDefinitions);

        public abstract IEnumerable<ActionDefinition> ParseActions(TSchemeMedium schemeMedium,
                                                   IEnumerable<ParameterDefinition> parameterDefinitions);

        public abstract IEnumerable<ActivityDefinition> ParseActivities(TSchemeMedium schemeMedium,
                                                        IEnumerable<ActionDefinition> actionDefinitions);

        public abstract IEnumerable<TransitionDefinition> ParseTransitions(TSchemeMedium schemeMedium,
                                                           IEnumerable<ActorDefinition> actorDefinitions,
                                                           IEnumerable<CommandDefinition> commandDefinitions,
                                                           IEnumerable<ActionDefinition> actionDefinitions,
                                                           IEnumerable<ActivityDefinition> activityDefinitions,
                                                           IEnumerable<TimerDefinition> timerDefinitions);

        public abstract string GetProcessName(TSchemeMedium schemeMedium);

        public ProcessDefinition Parse(TSchemeMedium schemeMedium)
        {
            var localization = ParseLocalization(schemeMedium).ToList();
            var actors = ParseActors(schemeMedium).ToList();
            var timers = ParseTimers(schemeMedium).ToList();
            var parameters = ParseParameters(schemeMedium).ToList();
            var commands = ParseCommands(schemeMedium, parameters).ToList();
            var actions = ParseActions(schemeMedium, parameters).ToList();
            var activities = ParseActivities(schemeMedium, actions).ToList();
            var transitions = ParseTransitions(schemeMedium, actors, commands, actions, activities,timers).ToList();

            return ProcessDefinition.Create(GetProcessName(schemeMedium),
                                            actors,
                                            parameters,
                                            commands,
                                            actions,
                                            activities,
                                            transitions,
                                            localization
                                            );
        }
    }
}
