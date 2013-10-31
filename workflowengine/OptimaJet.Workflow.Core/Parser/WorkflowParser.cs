using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Parser
{
    /// <summary>
    /// 将工作流WorkflowScheme的Scheme（XML形式）解析出来
    /// </summary>
    /// <typeparam name="TSchemeMedium"></typeparam>
    public abstract class WorkflowParser<TSchemeMedium>  : IWorkflowParser<TSchemeMedium> where TSchemeMedium : class
    {
        /// <summary>
        /// 转换XML中的Timers节点
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <returns></returns>
        public abstract IEnumerable<TimerDefinition> ParseTimers(TSchemeMedium schemeMedium);
        /// <summary>
        ///  转换XML中的Actors节点
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <returns></returns>
        public abstract IEnumerable<ActorDefinition> ParseActors(TSchemeMedium schemeMedium);
        /// <summary>
        ///  转换XML中的Localization节点
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <returns></returns>
        public abstract IEnumerable<LocalizeDefinition> ParseLocalization(TSchemeMedium schemeMedium);
        /// <summary>
        /// 转换XML中的Parameters节点
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <returns></returns>
        public abstract IEnumerable<ParameterDefinition> ParseParameters(TSchemeMedium schemeMedium);
        /// <summary>
        /// 转换XML中的Commands节点
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <param name="parameterDefinitions"></param>
        /// <returns></returns>
        public abstract IEnumerable<CommandDefinition> ParseCommands(TSchemeMedium schemeMedium,
                                                     IEnumerable<ParameterDefinition> parameterDefinitions);
        /// <summary>
        /// 转换XML中的Actions节点
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <param name="parameterDefinitions"></param>
        /// <returns></returns>
        public abstract IEnumerable<ActionDefinition> ParseActions(TSchemeMedium schemeMedium,
                                                   IEnumerable<ParameterDefinition> parameterDefinitions);
        /// <summary>
        /// 转换XML中的Activities节点
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <param name="actionDefinitions"></param>
        /// <returns></returns>
        public abstract IEnumerable<ActivityDefinition> ParseActivities(TSchemeMedium schemeMedium,
                                                        IEnumerable<ActionDefinition> actionDefinitions);
        /// <summary>
        /// 转换XML中的Transitions节点
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <param name="actorDefinitions"></param>
        /// <param name="commandDefinitions"></param>
        /// <param name="actionDefinitions"></param>
        /// <param name="activityDefinitions"></param>
        /// <param name="timerDefinitions"></param>
        /// <returns></returns>
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
