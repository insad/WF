using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Fault;

namespace OptimaJet.Workflow.Core.Model
{

    /// <summary>
    /// 公文审批流程定义，要通过XML解析器解析出所有关系后构建ProcessDefinition
    /// </summary>
    public sealed class ProcessDefinition : BaseDefinition
    {
        public IEnumerable<ActorDefinition> Actors { get; private set; }
        public IEnumerable<ParameterDefinition> Parameters { get; private set; }
        public IEnumerable<CommandDefinition> Commands { get; private set; }
        public IEnumerable<ActionDefinition> Actions { get; private set; }
        public IEnumerable<ActivityDefinition> Activities { get; private set; }
        public IEnumerable<TransitionDefinition> Transitions { get; private set; }
        public IEnumerable<LocalizeDefinition> Localization { get; private set; }

        /// <summary>
        /// 获取流程的第一个初始审批活动
        /// </summary>
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

        /// <summary>
        /// 根据名称name获取流程活动
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ActivityDefinition FindActivity (string name)
        {
            var activity = Activities.SingleOrDefault(a => a.Name == name);
            if (activity == null)
                throw new ActivityNotFoundException();
            return activity;
        }

        /// <summary>
        /// 根据名称获取审批流转过程
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TransitionDefinition FindTransition(string name)
        {
            var transition = Transitions.SingleOrDefault(a => a.Name == name);
            if (transition == null)
                throw new TransitionNotFoundException();
            return transition;
        }

        /// <summary>
        /// 根据activity定义获取其开始的流转过程
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public IEnumerable<TransitionDefinition> GetPossibleTransitionsForActivity (ActivityDefinition activity)
        {
            return Transitions.Where(t => t.From == activity);
        }

        /// <summary>
        /// 根据activity获取，以其为开始的命令类流转过程
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public IEnumerable<TransitionDefinition> GetCommandTransitions(ActivityDefinition activity)
        {
            return Transitions.Where(t => t.From == activity && t.Trigger.Type == TriggerType.Command);
        }

        /// <summary>
        /// 根据activity获取，以其为开始的自动类流转过程，什么是Auto触发类型？
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public IEnumerable<TransitionDefinition> GetAutoTransitionForActivity(ActivityDefinition activity)
        {
            return Transitions.Where(t => t.From == activity && t.Trigger.Type == TriggerType.Auto);
        }

        /// <summary>
        /// 根据活动activity及命令类名称commandName，获取流转过程
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="commandName"></param>
        /// <returns></returns>
        public IEnumerable<TransitionDefinition> GetCommandTransitionForActivity(ActivityDefinition activity, string commandName)
        {
            return Transitions.Where(t => t.From == activity && t.Trigger.Type == TriggerType.Command && t.Trigger.Command.Name == commandName);
        }

        /// <summary>
        /// 根据activity获取定时器类型流转过程
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
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
