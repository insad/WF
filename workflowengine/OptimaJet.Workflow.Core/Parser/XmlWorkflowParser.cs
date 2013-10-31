using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using OptimaJet.Common;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Parser
{
    /// <summary>
    /// 实现对XML数据形式的公文流转审批流程方案的关系解析，XElement表示的只是数据字符串，
    /// XmlWorkflowParser要负责把（WorkflowScheme表中的Scheme）XML里面表达的关系解析出来 
    /// </summary>
    public class XmlWorkflowParser : WorkflowParser<XElement>
    {
        /// <summary>
        /// 解析方案模型中的Timers节点
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <returns></returns>
        public override IEnumerable<TimerDefinition> ParseTimers(XElement schemeMedium)
        {
            if (schemeMedium == null) throw new ArgumentNullException("schemeMedium");

            var timersElement = schemeMedium.SingleOrDefault("Timers");

            if (timersElement == null) throw new ArgumentNullException("");

            return timersElement.Elements().ToList().Select(element => TimerDefinition.Create(GetName(element), GetType(element), GetDelay(element), GetInterval(element))).ToList();
        }

        /// <summary>
        /// 解析方案模型中的Actors节点
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <returns></returns>
        public override IEnumerable<ActorDefinition> ParseActors(XElement schemeMedium)
        {
            if (schemeMedium == null) throw new ArgumentNullException("schemeMedium");

            var actorsElement = schemeMedium.SingleOrDefault("Actors");

            if (actorsElement == null) throw new ArgumentNullException("");

            var actors = new List<ActorDefinition>();

            foreach (var element in actorsElement.Elements().ToList())
            {
                ActorDefinition actor = null;
                if (element.Element("IsIdentity") != null)
                {
                    actor = ActorDefinition.CreateIsIdentity(GetName(element), GetId(element.Element("IsIdentity")));
                }
                else if (element.Element("Rule") != null)
                {
                    actor = ActorDefinition.CreateRule(GetName(element), GetRuleName(element.Element("Rule")));
                    var parameters = element.Element("Rule").Elements("RuleParameter");
                    foreach (var parameter in parameters)
                    {
                        actor.AddParameter(GetName(parameter), GetValue(parameter));
                    }
                }
                else if (element.Element("IsInRole") != null)
                {
                    actor = ActorDefinition.CreateIsInRole(GetName(element), GetId(element.Element("IsInRole")));
                }

                if (actor != null) actors.Add(actor);
            }

            return actors;
        }

        /// <summary>
        /// 解析方案模型中的Localization节点
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <returns></returns>
        public override IEnumerable<LocalizeDefinition> ParseLocalization(XElement schemeMedium)
        {
            if (schemeMedium == null) throw new ArgumentNullException("schemeMedium");

            var localizationElement = schemeMedium.SingleOrDefault("Localization");

            if (localizationElement == null) return new List<LocalizeDefinition>();

            var localization = new List<LocalizeDefinition>();

            foreach (var element in localizationElement.Elements().ToList())
            {
                var localizeDefinition = LocalizeDefinition.Create(GetObjectName(element), GetType(element), GetCulture(element),
                                          GetValue(element), GetIsDefault(element));


                localization.Add(localizeDefinition);
            }

            return localization;
        }

        /// <summary>
        /// 解析方案模型中的Parameters节点
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <returns></returns>
        public override IEnumerable<ParameterDefinition> ParseParameters(XElement schemeMedium)
        {
            if (schemeMedium == null) throw new ArgumentNullException("schemeMedium");
            var parametersElement = schemeMedium.SingleOrDefault("Parameters");
            if (parametersElement == null) throw new ArgumentNullException("");

            var parameters = new List<ParameterDefinition>();

            foreach (var element in parametersElement.Elements().ToList())
            {
                ParameterDefinition parameterDefinition;
                if (element.Attributes("Purpose").Count() == 1)
                {
                    parameterDefinition = ParameterDefinition.Create(GetName(element),
                                                                     element.Attributes("Type").Single().Value,
                                                                     element.Attributes("Purpose").Single().Value, GetDefaultValue(element));
                }
                else
                {
                    parameterDefinition = ParameterDefinition.Create(GetName(element), element.Attributes("Type").Single().Value, GetDefaultValue(element));
                }

                parameters.Add(parameterDefinition);

            }

            parameters.AddRange(DefaultDefinitions.DefaultParameters);

            return parameters;
        }

        /// <summary>
        /// 解析方案模型中的Commands节点及其输出参数，为什么没有输出参数呢？
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <param name="parameterDefinitions"></param>
        /// <returns></returns>
        public override IEnumerable<CommandDefinition> ParseCommands(XElement schemeMedium, IEnumerable<ParameterDefinition> parameterDefinitions)
        {
            if (schemeMedium == null) throw new ArgumentNullException("schemeMedium");
            if (parameterDefinitions == null) throw new ArgumentNullException("parameterDefinitions");
            var commandsElement = schemeMedium.SingleOrDefault("Commands");
            if (commandsElement == null) throw new ArgumentNullException("");

            var parameterDefinitionsList = parameterDefinitions.ToList();

            var commands = new List<CommandDefinition>();

            foreach (var element in commandsElement.Elements().ToList())
            {
                var command = CommandDefinition.Create(GetName(element));

                foreach (var xmlInputParameter in element.Elements("InputParameters").Single().Elements())
                {
                    var parameterRef = parameterDefinitionsList.Single(pd => pd.Name == GetNameRef(xmlInputParameter));
                    command.AddParameterRef(GetName(xmlInputParameter), parameterRef);
                }

                commands.Add(command);
            }

            return commands;
        }

        /// <summary>
        /// 解析方案模型中的Actions节点及其ExecuteMethod，输入输出参数InputParameters和OutputParameters
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <param name="parameterDefinitions"></param>
        /// <returns></returns>
        public override IEnumerable<ActionDefinition> ParseActions(XElement schemeMedium, IEnumerable<ParameterDefinition> parameterDefinitions)
        {
            if (schemeMedium == null) throw new ArgumentNullException("schemeMedium");
            if (parameterDefinitions == null) throw new ArgumentNullException("parameterDefinitions");
            var actionElements = schemeMedium.SingleOrDefault("Actions");
            if (actionElements == null) throw new ArgumentNullException("");

            var parameterDefinitionsList = parameterDefinitions.ToList();

            var actions = new List<ActionDefinition>();

            foreach (var element in actionElements.Elements().ToList())
            {
                var executeMethodElement = element.Elements("ExecuteMethod").Single();

                var action = ActionDefinition.Create(GetName(element), GetType(executeMethodElement), GetMethodName(executeMethodElement));


                var inputParameters = executeMethodElement.Elements("InputParameters").SingleOrDefault();

                if (inputParameters != null)
                    foreach (var xmlInputParameter in inputParameters.Elements())
                    {
                        var parameterRef = parameterDefinitionsList.Single(pd => pd.Name == GetNameRef(xmlInputParameter));
                        var parameterForAction = ParameterDefinition.Create(parameterRef, GetOrder(xmlInputParameter));
                        action.AddInputParameterRef(parameterForAction);
                    }

                var outputParameters = executeMethodElement.Elements("OutputParameters").SingleOrDefault();

                if (outputParameters != null)
                    foreach (var xmlOutputParameter in outputParameters.Elements())
                    {
                        var parameterRef =
                            parameterDefinitionsList.Single(pd => pd.Name == GetNameRef(xmlOutputParameter));
                        var parameterForAction = ParameterDefinition.Create(parameterRef, GetOrder(xmlOutputParameter));
                        action.AddOutputParameterRef(parameterForAction);
                    }

                actions.Add(action);
            }

            return actions;
        }

        /// <summary>
        /// 解析方案模型中的Activities节点及其实现Implementation节点和预处理PreExecutionImplementation节点
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <param name="actionDefinitions"></param>
        /// <returns></returns>
        public override IEnumerable<ActivityDefinition> ParseActivities(XElement schemeMedium, IEnumerable<ActionDefinition> actionDefinitions)
        {
            if (schemeMedium == null) throw new ArgumentNullException("schemeMedium");
            if (actionDefinitions == null) throw new ArgumentNullException("actionDefinitions");
            var activitiesElements = schemeMedium.SingleOrDefault("Activities");
            if (activitiesElements == null) throw new ArgumentNullException("");

            var actionDefinitionsList = actionDefinitions.ToList();

            var activities = new List<ActivityDefinition>();

            foreach (var element in activitiesElements.Elements().ToList())
            {
                var activity = ActivityDefinition.Create(GetName(element), GetState(element), GetIsInitial(element),
                                                         GetIsFinal(element), GetIsForSetState(element), GetIsAutoSchemeUpdate(element));


                var implementation = element.Elements("Implementation").ToList();

                if (implementation.Count() > 0)
                    foreach (var xmlOutputParameter in implementation.Single().Elements())
                    {
                        string nameRef = GetNameRef(xmlOutputParameter);
                        ActionDefinition actionRef = actionDefinitionsList.SingleOrDefault(ad => ad.Name == nameRef);
                        if (actionRef == null)
                            throw new KeyNotFoundException(string.Format("Action {0} not found", nameRef));
                        activity.AddAction(ActionDefinition.Create(actionRef, GetOrder(xmlOutputParameter)));
                    }

                var preExecutionImplementation = element.Elements("PreExecutionImplementation").ToList();

                if (preExecutionImplementation.Count() > 0)
                    foreach (var xmlOutputParameter in preExecutionImplementation.Single().Elements())
                    {
                        var actionRef = actionDefinitionsList.Single(ad => ad.Name == GetNameRef(xmlOutputParameter));
                        activity.AddPreExecutionAction(ActionDefinition.Create(actionRef, GetOrder(xmlOutputParameter)));
                    }

                activities.Add(activity);
            }

            return activities;
        }

        /// <summary>
        /// 解析方案模型中的Transitions节点,该节点最负责，其关联了activityDefinitions，actionDefinitions
        /// commandDefinitions、Triggers、Conditions条件节点和Restrictions权限限制节点
        /// </summary>
        /// <param name="schemeMedium"></param>
        /// <param name="actorDefinitions"></param>
        /// <param name="commandDefinitions"></param>
        /// <param name="actionDefinitions"></param>
        /// <param name="activityDefinitions"></param>
        /// <param name="timerDefinitions"></param>
        /// <returns></returns>
        public override IEnumerable<TransitionDefinition> ParseTransitions(XElement schemeMedium, IEnumerable<ActorDefinition> actorDefinitions,
            IEnumerable<CommandDefinition> commandDefinitions, IEnumerable<ActionDefinition> actionDefinitions,
            IEnumerable<ActivityDefinition> activityDefinitions, IEnumerable<TimerDefinition> timerDefinitions)
        {
            if (schemeMedium == null) throw new ArgumentNullException("schemeMedium");
            if (commandDefinitions == null) throw new ArgumentNullException("commandDefinitions");
            if (actionDefinitions == null) throw new ArgumentNullException("actionDefinitions");
            if (activityDefinitions == null) throw new ArgumentNullException("activityDefinitions");
            var transitionElements = schemeMedium.SingleOrDefault("Transitions");
            if (transitionElements == null) throw new ArgumentNullException("");

            var commandDefinitionsList = commandDefinitions.ToList();
            var actionDefinitionsList = actionDefinitions.ToList();
            var activityDefinitionsList = activityDefinitions.ToList();
            var actorDefinitionsList = actorDefinitions.ToList();
            var timerDefinitionsList = timerDefinitions.ToList();

            var transitions = new List<TransitionDefinition>();

            foreach (var transitionElement in transitionElements.Elements().ToList())
            {
                var fromActivity = activityDefinitionsList.Single(ad => ad.Name == GetFrom(transitionElement));
                var toActivity = activityDefinitionsList.Single(ad => ad.Name == GetTo(transitionElement));

                TriggerDefinition trigger = null;
                var triggersElement = transitionElement.Element("Triggers");
                if (triggersElement != null)
                {
                    var triggerElement = triggersElement.Element("Trigger");
                    if (triggerElement != null)
                    {
                        trigger = TriggerDefinition.Create(GetType(triggerElement));
                        if (trigger.Type == TriggerType.Command)
                        {
                            (trigger as CommandTriggerDefinition).Command =
                                commandDefinitionsList.Single(cd => cd.Name == GetNameRef(triggerElement));
                        }
                        else if (trigger.Type == TriggerType.Timer)
                        {
                            (trigger as TimerTriggerDefinition).Timer =
                               timerDefinitionsList.Single(cd => cd.Name == GetNameRef(triggerElement));
                        }
                    }
                }

                ConditionDefinition condition = null;
                var conditionsElement = transitionElement.Element("Conditions");
                if (conditionsElement != null)
                {
                    var conditionElement = conditionsElement.Element("Condition");
                    if (conditionElement != null)
                    {
                        condition = !string.IsNullOrEmpty(GetNameRefNullable(conditionElement))
                                        ? ConditionDefinition.Create(GetType(conditionElement), actionDefinitionsList.Single(ad => ad.Name == GetNameRef(conditionElement)), GetResultOnPreExecution(conditionElement))
                                        : ConditionDefinition.Create(GetType(conditionElement), GetResultOnPreExecution(conditionElement));

                    }
                }

                var transition = TransitionDefinition.Create(GetName(transitionElement), GetClassifier(transitionElement), fromActivity,
                                                             toActivity, trigger, condition);

                var restrictionsElement = transitionElement.Element("Restrictions");
                if (restrictionsElement != null)
                {
                    foreach (var element in restrictionsElement.Elements("Restriction"))
                    {
                        transition.AddRestriction(RestrictionDefinition.Create(GetType(element), actorDefinitionsList.Single(ad => ad.Name == GetNameRef(element))));
                    }
                }

                var onErrorsElement = transitionElement.Element("OnErrors");
                if (onErrorsElement != null)
                {
                    foreach (var element in onErrorsElement.Elements("OnError"))
                    {
                        //TODO Only One Type Of OnErrorHandler
                        transition.AddOnError(OnErrorDefinition.CreateSetActivityOnError(GetName(element), GetNameRef(element), GetPriority(element), GetTypeName(element)/*, GetIsExecuteImplementation(element),GetIsRethrow(element)*/));
                    }
                }
                transitions.Add(transition);
            }


            return transitions;
        }

        #region 解析XML时的常用获取属性等函数
        public override string GetProcessName(XElement schemeMedium)
        {
            return GetName(schemeMedium);
        }

        private static string GetName(XElement element)
        {
            return element.Attributes("Name").Single().Value;
        }

        private static string GetValue(XElement element)
        {
            return element.Attributes("Value").Single().Value;
        }

        private static string GetRuleName(XElement element)
        {
            return element.Attributes("RuleName").Single().Value;
        }

        private static string GetOrder(XElement element)
        {
            return element.Attributes("Order").Single().Value;
        }

        private static string GetType(XElement element)
        {
            return element.Attributes("Type").Single().Value;
        }

        private static string GetDelay(XElement element)
        {
            return element.Attributes("Delay").Single().Value;
        }

        private static string GetInterval(XElement element)
        {
            return element.Attributes("Interval").Single().Value;
        }

        private static string GetMethodName(XElement element)
        {
            return element.Attributes("MethodName").Single().Value;
        }

        private static string GetTypeName(XElement element)
        {
            return element.Attributes("ExceptionType").Single().Value;
        }

        private static string GetId(XElement element)
        {
            return element.Attributes("Id").Single().Value;
        }

        private static string GetNameRef(XElement element)
        {
            return element.Attributes("NameRef").Single().Value;
        }

        private static string GetDefaultValue(XElement element)
        {
            var attr = element.Attributes("DefaultValue").SingleOrDefault();
            return attr == null ? null : attr.Value;
        }

        private static string GetPriority(XElement element)
        {
            var attr = element.Attributes("Priority").SingleOrDefault();
            return attr == null ? null : attr.Value;
        }

        private static string GetNameRefNullable(XElement element)
        {
            var attr = element.Attributes("NameRef").SingleOrDefault();
            return attr == null ? null : attr.Value;
        }

        private static string GetResultOnPreExecution(XElement element)
        {
            var attr = element.Attributes("ResultOnPreExecution").SingleOrDefault();
            return attr == null ? null : attr.Value;
        }

        private static string GetFrom(XElement element)
        {
            return element.Attributes("From").Single().Value;
        }

        private static string GetTo(XElement element)
        {
            return element.Attributes("To").Single().Value;
        }

        private static string GetIsFinal(XElement element)
        {
            var attr = element.Attributes("IsFinal").SingleOrDefault();
            return attr == null ? null : attr.Value;
        }

        private static string GetIsForSetState(XElement element)
        {
            var attr = element.Attributes("IsForSetState").SingleOrDefault();
            return attr == null ? null : attr.Value;
        }

        private static string GetIsAutoSchemeUpdate(XElement element)
        {
            var attr = element.Attributes("IsAutoSchemeUpdate").SingleOrDefault();
            return attr == null ? null : attr.Value;
        }

        private static string GetIsInitial(XElement element)
        {
            var attr = element.Attributes("IsInitial").SingleOrDefault();
            return attr == null ? null : attr.Value;
        }

        private static string GetClassifier(XElement element)
        {
            var attr = element.Attributes("Classifier").SingleOrDefault();
            return attr == null ? null : attr.Value;
        }

        private static string GetState(XElement element)
        {
            var attr = element.Attributes("State").SingleOrDefault();
            return attr == null ? null : attr.Value;
        }

        private static string GetObjectName(XElement element)
        {
            return element.Attributes("ObjectName").Single().Value;
        }

        private static string GetCulture(XElement element)
        {
            return element.Attributes("Culture").Single().Value;
        }

        private static string GetIsDefault(XElement element)
        {
            var attr = element.Attributes("IsDefault").SingleOrDefault();
            return attr == null ? null : attr.Value;
        }

        private static string GetIsExecuteImplementation(XElement element)
        {
            var attr = element.Attributes("IsExecuteImplementation").SingleOrDefault();
            return attr == null ? null : attr.Value;
        }

        private static string GetIsRethrow(XElement element)
        {
            var attr = element.Attributes("IsRethrow").SingleOrDefault();
            return attr == null ? null : attr.Value;
        }

        #endregion
    }
}
