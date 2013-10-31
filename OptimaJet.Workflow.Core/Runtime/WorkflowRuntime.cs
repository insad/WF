using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Common;

namespace OptimaJet.Workflow.Core.Runtime
{
    public sealed class WorkflowRuntime
    {
        internal bool ValidateSettings ()
        {
            return Bus != null && Builder != null && PersistenceProvider != null && RuntimePersistence != null;
        }


        internal bool IsAutoUpdateSchemeBeforeGetAvailableCommands { get; set; }

        //public TimeSpan TimerOvnershipIgnoranceInterval { get; set; }
        
        internal event EventHandler<NeedDeterminingParametersEventArgs> OnNeedDeterminingParameters;

        public event EventHandler<SchemaWasChangedEventArgs> OnSchemaWasChanged;

        internal IWorkflowBus Bus;
        internal IWorkflowBuilder Builder;
        internal  IPersistenceProvider PersistenceProvider;
        internal  IRuntimePersistence RuntimePersistence;
        private IWorkflowRuleProvider _ruleProvider;

        //private readonly RuntimeTimer _runtimeTimer; 
        public Guid Id { get; private set; }

        public IWorkflowRuleProvider RuleProvider
        {
            get
            {
                if (_ruleProvider == null)
                    throw new InvalidOperationException();
                return _ruleProvider;
            }
            internal set { _ruleProvider = value; }
        }

        private IWorkflowRoleProvider _roleProvider;

        public  IWorkflowRoleProvider RoleProvider
        {
            get
            {
                if (_roleProvider == null)
                    throw new InvalidOperationException();
                return _roleProvider;
            }
            internal set { _roleProvider = value; }
        }

        public event EventHandler<ProcessStatusChangedEventArgs> ProcessSatusChanged;


        public WorkflowRuntime(Guid runtimeId)
        {
            Id = runtimeId;
            
            //_runtimeTimer = _runtimePersistence.LoadTimer(Id);
            //if (_runtimeTimer == null) 
            //    _runtimeTimer = new RuntimeTimer();

            //_runtimeTimer.TimerComplete += TimerComplete;
            //_runtimeTimer.NeedSave += _runtimeTimer_NeedSave;
        }

        //private object _timerSaveLock = new object();

        //void _runtimeTimer_NeedSave(object sender, EventArgs e)
        //{
        //    lock (_timerSaveLock)
        //    {
        //        _runtimePersistence.SaveTimer(Id, _runtimeTimer);
        //    }
        //}

        private void TimerComplete(object sender, RuntimeTimerEventArgs e)
        {
            TransitionDefinition currentTimerTransition;
            ProcessInstance processInstance;
            try
            {
                processInstance = Builder.GetProcessInstance(e.ProcessId);
                PersistenceProvider.FillProcessParameters(processInstance);

                currentTimerTransition =
                    processInstance.ProcessScheme.GetTimerTransitionForActivity(processInstance.CurrentActivity).
                        FirstOrDefault(p => p.Trigger.Timer.Name == e.TimerName);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error Timer Complete Workflow UNKNOWN", ex);
                throw;
            }
          
            if (currentTimerTransition != null)
            {
                try
                {
                    SetProcessNewStatus(processInstance, ProcessStatus.Running);
                    var parametersLocal = new List<ParameterDefinitionWithValue>(); 
                    parametersLocal.Add(ParameterDefinition.Create(DefaultDefinitions.ParameterIdentityId, Guid.Empty));
                    parametersLocal.Add(ParameterDefinition.Create(DefaultDefinitions.ParameterImpersonatedIdentityId, Guid.Empty));
                    parametersLocal.Add(ParameterDefinition.Create(DefaultDefinitions.ParameterSchemeId, processInstance.SchemeId));
                    var newExecutionParameters = new List<ExecutionRequestParameters>();
                    newExecutionParameters.Add(ExecutionRequestParameters.Create(processInstance.ProcessId, processInstance.ProcessParameters, currentTimerTransition));
                    Bus.QueueExecution(newExecutionParameters);

                }
                catch (Exception ex)
                {
                    Logger.Log.Error(string.Format("Error Timer Complete Workflow Id={0}", processInstance.ProcessId), ex);
                    SetProcessNewStatus(processInstance, ProcessStatus.Idled);
                }
            }
        }

        private void FillParameters(ProcessInstance instance, ExecutionResponseParametersComplete newParameters)
        {
           
            foreach (var parameter in newParameters.ParameterContainer)
            {
                var parameterDefinition = instance.ProcessScheme.GetNullableParameterDefinition(parameter.Name);
                if (parameterDefinition != null)
                {
                    var parameterDefinitionWithValue = ParameterDefinition.Create(parameterDefinition, parameter.Value);

                    instance.AddParameter(parameterDefinitionWithValue);
                }
            }
        }

     
        internal void BusExecutionComplete(object sender, ExecutionResponseEventArgs e)
        {
            var executionResponseParameters = e.Parameters;
            var processInstance = Builder.GetProcessInstance(executionResponseParameters.ProcessId);
            PersistenceProvider.FillSystemProcessParameters(processInstance);
            //TODO Сделать метод филл CurrentActivity
            if (executionResponseParameters.IsEmplty)
            {
                SetProcessNewStatus(processInstance, ProcessStatus.Idled);
                //var timerTransitions =
                //    processInstance.ProcessScheme.GetTimerTransitionForActivity(processInstance.CurrentActivity).ToList();
               
                //timerTransitions.ForEach(p=>_runtimeTimer.UpdateTimer(processInstance.ProcessId,p.Trigger.Timer));

                return;
            }
            if (executionResponseParameters.IsError)
            {
                var executionErrorParameters = executionResponseParameters as ExecutionResponseParametersError;

                Logger.Log.Error(string.Format("Error Execution Complete Workflow Id={0}", processInstance.ProcessId), executionErrorParameters.Exception);

                if (string.IsNullOrEmpty(executionErrorParameters.ExecutedTransitionName))
                {
                    SetProcessNewStatus(processInstance, ProcessStatus.Idled);
                    throw executionErrorParameters.Exception;
                }

                var transition = processInstance.ProcessScheme.FindTransition(executionErrorParameters.ExecutedTransitionName);

                var onErrorDefinition = transition.OnErrors.Where(
                    oe => executionErrorParameters.Exception.GetType().Equals(oe.ExceptionType)).
                                                          OrderBy(oe => oe.Priority).FirstOrDefault() ??
                                                      transition.OnErrors.Where(
                                                          oe => oe.ExceptionType.IsAssignableFrom(executionErrorParameters.Exception.GetType())).
                                                          OrderBy(oe => oe.Priority).FirstOrDefault();
                if (onErrorDefinition == null)
                {
                    SetProcessNewStatus(processInstance, ProcessStatus.Idled);
                    throw executionErrorParameters.Exception;
                }

                if (onErrorDefinition.ActionType == OnErrorActionType.SetActivity)
                {
                    var from = processInstance.CurrentActivity;
                    var to = processInstance.ProcessScheme.FindActivity((onErrorDefinition as SetActivityOnErrorDefinition).NameRef);
                    PersistenceProvider.UpdatePersistenceState(processInstance, TransitionDefinition.Create(from, to));
                    SetProcessNewStatus(processInstance, ProcessStatus.Idled);
                }

                throw executionErrorParameters.Exception;

                //return;
            }


            try
            {

                ActivityDefinition newCurrentActivity;
                if (string.IsNullOrEmpty(executionResponseParameters.ExecutedTransitionName))
                {
                    if (executionResponseParameters.ExecutedActivityName == processInstance.ProcessScheme.InitialActivity.Name)
                        newCurrentActivity = processInstance.ProcessScheme.InitialActivity;
                    else
                    {
                        var from = processInstance.CurrentActivity;
                        var to = processInstance.ProcessScheme.FindActivity(executionResponseParameters.ExecutedActivityName);
                        newCurrentActivity = to;
                        PersistenceProvider.UpdatePersistenceState(processInstance,TransitionDefinition.Create(from,to));
                    }
                }
                else
                {
                    var executedTransition =
                        processInstance.ProcessScheme.FindTransition(executionResponseParameters.ExecutedTransitionName);
                    newCurrentActivity = executedTransition.To;
                    PersistenceProvider.UpdatePersistenceState(processInstance, executedTransition);

                }

                FillParameters(processInstance,(executionResponseParameters as ExecutionResponseParametersComplete));
                PersistenceProvider.SavePersistenceParameters(processInstance);

                var autoTransitions =
                    processInstance.ProcessScheme.GetAutoTransitionForActivity(newCurrentActivity).ToList();
                if (autoTransitions.Count() < 1)
                {
                    SetProcessNewStatus(processInstance,
                                        newCurrentActivity.IsFinal ? ProcessStatus.Finalized : ProcessStatus.Idled);

                    //var timerTransitions =
                    //processInstance.ProcessScheme.GetTimerTransitionForActivity(newCurrentActivity).ToList();

                    //timerTransitions.ForEach(p => _runtimeTimer.SetTimer(processInstance.ProcessId, p.Trigger.Timer));

                    return;
                }

                PersistenceProvider.FillProcessParameters(processInstance);

                var newExecutionParameters = new List<ExecutionRequestParameters>();
                newExecutionParameters.AddRange(
                    autoTransitions.Select(
                        at =>
                        ExecutionRequestParameters.Create(processInstance.ProcessId, processInstance.ProcessParameters,
                                                          at)));
                Bus.QueueExecution(newExecutionParameters);
            }
            catch (ActivityNotFoundException)
            {
                SetProcessNewStatus(processInstance, ProcessStatus.Terminated);
            }
                //TODO Обработка ошибок
            catch (Exception ex)
            {
                Logger.Log.Error(string.Format("Error Execution Complete Workflow Id={0}", processInstance.ProcessId), ex);
                SetProcessNewStatus(processInstance, ProcessStatus.Idled);
            }
        }

        private bool _isColdStart;

        internal void Start()
        { 
            PersistenceProvider.ResetWorkflowRunning();
            Bus.Start();
           //_runtimeTimer.RefreshTimer();
        }

        internal void ColdStart ()
        {
            _isColdStart = true;
            //_runtimeTimer.IsCold = true;
            Bus.Start();
        }

        public void CreateInstance (string processName, Guid processId)
        {
            CreateInstance(processName,processId,new Dictionary<string, IEnumerable<object>>());
        }

        public void CreateInstance(string processName, Guid processId, IDictionary<string, IEnumerable<object>> parameters)
        {
            var processInstance = Builder.CreateNewProcess(processId, processName, parameters);
            PersistenceProvider.InitializeProcess(processInstance);
            SetProcessNewStatus(processInstance, ProcessStatus.Initialized);
            if (processInstance.ProcessScheme.InitialActivity.HaveImplementation)
            {
                try
                {
                    SetProcessNewStatus(processInstance, ProcessStatus.Running);
                    ExecuteRootActivity(processInstance);
                }
                catch (Exception)
                {
                    //TODO 
                    SetProcessNewStatus(processInstance, ProcessStatus.Idled);
                }
               
            }
            SetProcessNewStatus(processInstance, ProcessStatus.Idled);
        }



        private void ExecuteRootActivity(ProcessInstance processInstance)
        {
            PersistenceProvider.FillProcessParameters(processInstance);
            processInstance.AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterSchemeId, processInstance.SchemeId));

            //TODO Убрать после обработки команд
            try
            {
                Bus.QueueExecution(ExecutionRequestParameters.Create(processInstance.ProcessId,
                                                                processInstance.ProcessParameters,
                                                                processInstance.ProcessScheme.InitialActivity,
                                                                ConditionDefinition.Always));
            }
            catch (Exception ex)
            {
                Logger.Log.Error(string.Format("Error Execute Root Workflow Id={0}", processInstance.ProcessId),ex);
                SetProcessNewStatus(processInstance, ProcessStatus.Idled);
                throw;
            }
          
        }


        public ProcessDefinition GetProcessScheme(Guid processId)
        {
            return Builder.GetProcessInstance(processId).ProcessScheme;
        }



        public IEnumerable<WorkflowCommand> GetAvailableCommands(Guid processId, IEnumerable<Guid> identityIds, string commandNameFilter = null, Guid? mainIdentityId = null)
        {
            var identityIdsList = mainIdentityId.HasValue
                                      ? identityIds.Except(new List<Guid> {mainIdentityId.Value}).ToList()
                                      : identityIds.ToList();

            var processInstance = Builder.GetProcessInstance(processId);
           PersistenceProvider.FillSystemProcessParameters(processInstance);

           
            if (IsAutoUpdateSchemeBeforeGetAvailableCommands)
                processInstance = UpdateScheme(processId, processInstance);

            var currentActivity = processInstance.ProcessScheme.FindActivity(processInstance.CurrentActivityName);

            List<TransitionDefinition> commandTransitions;
            if (string.IsNullOrEmpty(commandNameFilter))
                commandTransitions = processInstance.ProcessScheme.GetCommandTransitions(currentActivity).ToList();
            else
            {
                commandTransitions = processInstance.ProcessScheme.GetCommandTransitions(currentActivity).Where(c=>c.Trigger.Command.Name == commandNameFilter).ToList();
            }

            var commands = new List<WorkflowCommand>();

            foreach (var transitionDefinition in commandTransitions)
            {
                List<Guid> availiableIds = null;
                if (mainIdentityId.HasValue && ValidateActor(processId, mainIdentityId.Value, transitionDefinition))
                    availiableIds = new List<Guid>(){mainIdentityId.Value};
                
                if (availiableIds == null)
                    availiableIds = identityIdsList.Where(id => ValidateActor(processId, id, transitionDefinition)).ToList();

                if (availiableIds.Count() > 0)
                {
                    var command = WorkflowCommand.Create(processId, transitionDefinition);
                    foreach (var availiableId in availiableIds)
                    {
                        command.AddIdentity(availiableId);
                    }

                    command.LocalizedName = processInstance.GetLocalizedCommandName(command.CommandName,
                                                                                    CultureInfo.CurrentCulture);

                    commands.Add(command);
                }
            }
           
 
           
            return commands;

        }

       


        public IEnumerable<WorkflowCommand> GetAvailableCommands(Guid processId, Guid identityId)
        {
            return GetAvailableCommands(processId, new List<Guid>() {identityId});
        }


        public void ExecuteCommand(Guid processId, Guid identityId, Guid impersonatedIdentityId, WorkflowCommand command)
        {
            //TODO Workflow Temporary
            //if (!command.Validate())
            //    throw new CommandNotValidException();

            var processInstance = Builder.GetProcessInstance(processId);

            SetProcessNewStatus(processInstance, ProcessStatus.Running);

            IEnumerable<TransitionDefinition> transitions;


            try
            {
                PersistenceProvider.FillSystemProcessParameters(processInstance);

                if (processInstance.CurrentActivityName != command.ValidForActivityName)
                {

                    throw new CommandNotValidForStateException();
                }

                transitions =
                    processInstance.ProcessScheme.GetCommandTransitionForActivity(
                        processInstance.ProcessScheme.FindActivity(processInstance.CurrentActivityName),
                        command.CommandName);

                if (transitions.Count() < 1)
                {
                    throw new InvalidOperationException();
                }
            }
            catch (Exception ex)
            {
                SetProcessNewStatus(processInstance, ProcessStatus.Idled);
                throw;
            }


            var parametersLocal = new List<ParameterDefinitionWithValue>();

            try
            {

                foreach (var commandParameter in command.Parameters)
                {
                    var parameterDefinition = processInstance.ProcessScheme.GetParameterDefinition(commandParameter.Name);

                    if (parameterDefinition != null)
                        parametersLocal.Add(ParameterDefinition.Create(parameterDefinition, commandParameter.Value));

                }

                parametersLocal.Add(ParameterDefinition.Create(DefaultDefinitions.ParameterCurrentCommand,
                                                               (object) command.CommandName));
                parametersLocal.Add(ParameterDefinition.Create(DefaultDefinitions.ParameterIdentityId, identityId));
                parametersLocal.Add(ParameterDefinition.Create(DefaultDefinitions.ParameterImpersonatedIdentityId,
                                                               impersonatedIdentityId));
                parametersLocal.Add(ParameterDefinition.Create(DefaultDefinitions.ParameterSchemeId,
                                                               processInstance.SchemeId));

                parametersLocal.ForEach(processInstance.AddParameter);

                PersistenceProvider.SavePersistenceParameters(processInstance);
                PersistenceProvider.FillPersistedProcessParameters(processInstance);
            }
            catch (Exception)
            {
                SetProcessNewStatus(processInstance, ProcessStatus.Idled);
                throw;
            }

            var newExecutionParameters = new List<ExecutionRequestParameters>();
            newExecutionParameters.AddRange(
                transitions.Select(
                    at =>
                    ExecutionRequestParameters.Create(processInstance.ProcessId, processInstance.ProcessParameters, at)));

            try
            {
                Bus.QueueExecution(newExecutionParameters);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(string.Format("Error Execute Command Workflow Id={0}", processInstance.ProcessId), ex);
                SetProcessNewStatus(processInstance, ProcessStatus.Idled);
                throw;
            }


        }

        public IEnumerable<WorkflowState> GetAvailableStateToSet (Guid processId, CultureInfo culture)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            var activities = processInstance.ProcessScheme.Activities.Where(a => a.IsForSetState && a.IsState);
            return activities.Select(activity => new WorkflowState {Name = activity.State, VisibleName = processInstance.GetLocalizedStateName(activity.State, culture)}).ToList();
        }


        public string  GetCurrentStateName (Guid processId)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            PersistenceProvider.FillSystemProcessParameters(processInstance);

            return processInstance.GetParameter(DefaultDefinitions.ParameterCurrentState.Name).Value.ToString();
        }

        public string GetCurrentActivityName(Guid processId)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            PersistenceProvider.FillSystemProcessParameters(processInstance);

            return processInstance.GetParameter(DefaultDefinitions.ParameterCurrentActivity.Name).Value.ToString();
        }

        public IEnumerable<WorkflowState> GetAvailableStateToSet(Guid processId)
        {
            return GetAvailableStateToSet(processId, CultureInfo.CurrentCulture);
        }

        public void SetState(Guid processId, Guid identityId, Guid impersonatedIdentityId, string stateName, IDictionary<string, object> parameters, bool preventExecution)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            var activityToSet =
                processInstance.ProcessScheme.Activities.FirstOrDefault(
                    a => a.IsState && a.IsForSetState && a.State == stateName);

            if (activityToSet == null)
                throw new ActivityNotFoundException();

            if (!preventExecution)
                SetStateWithExecution(identityId, impersonatedIdentityId, parameters, activityToSet, processInstance);
            else
                SetStateWithoutExecution(activityToSet, processInstance);
        }

        private void SetStateWithoutExecution(ActivityDefinition activityToSet, ProcessInstance processInstance)
        {
            SetProcessNewStatus(processInstance, ProcessStatus.Running);

            IEnumerable<TransitionDefinition> transitions;
            try
            {
                PersistenceProvider.FillSystemProcessParameters(processInstance);
                var from = processInstance.CurrentActivity;
                var to = activityToSet;
                PersistenceProvider.UpdatePersistenceState(processInstance, TransitionDefinition.Create(from, to));
            }
            catch (Exception ex)
            {
                Logger.Log.Error(string.Format("Workflow Id={0}", processInstance.ProcessId), ex);
                throw;
            }
            finally
            {
                SetProcessNewStatus(processInstance, ProcessStatus.Idled);
            }
        }

        private void SetStateWithExecution(Guid identityId,
                                           Guid impersonatedIdentityId,
                                           IDictionary<string, object> parameters,
                                           ActivityDefinition activityToSet,
                                           ProcessInstance processInstance)
        {
            SetProcessNewStatus(processInstance, ProcessStatus.Running);

            IEnumerable<TransitionDefinition> transitions;


            try
            {
                PersistenceProvider.FillSystemProcessParameters(processInstance);
                PersistenceProvider.FillPersistedProcessParameters(processInstance);

                var parametersLocal = new List<ParameterDefinitionWithValue>();
                foreach (var commandParameter in parameters)
                {
                    var parameterDefinition = processInstance.ProcessScheme.GetParameterDefinition(commandParameter.Key);

                    if (parameterDefinition != null)
                        parametersLocal.Add(ParameterDefinition.Create(parameterDefinition, commandParameter.Value));
                }
                parametersLocal.Add(ParameterDefinition.Create(DefaultDefinitions.ParameterCurrentCommand,
                                                               (object) DefaultDefinitions.CommandSetState.Name));
                parametersLocal.Add(ParameterDefinition.Create(DefaultDefinitions.ParameterIdentityId, identityId));
                parametersLocal.Add(ParameterDefinition.Create(DefaultDefinitions.ParameterImpersonatedIdentityId,
                                                               impersonatedIdentityId));
                parametersLocal.Add(ParameterDefinition.Create(DefaultDefinitions.ParameterSchemeId, processInstance.SchemeId));

                parametersLocal.ForEach(processInstance.AddParameter);

            }
            catch (Exception)
            {
                SetProcessNewStatus(processInstance, ProcessStatus.Idled);
                throw;
            }

            //TODO Убрать после обработки команд
            try
            {
                Bus.QueueExecution(ExecutionRequestParameters.Create(processInstance.ProcessId,
                                                                      processInstance.ProcessParameters,
                                                                      activityToSet,
                                                                      ConditionDefinition.Always));
            }
            catch (Exception ex)
            {
                Logger.Log.Error(string.Format("Workflow Id={0}", processInstance.ProcessId), ex);
                SetProcessNewStatus(processInstance, ProcessStatus.Idled);
                throw;
            }
        }

        public void SetState (Guid processId, Guid identityId, Guid impersonatedIdentityId, string stateName, IDictionary<string,object> parameters )
        {

           SetState(processId,identityId,impersonatedIdentityId,stateName,parameters,false);
            
        }

        private IEnumerable<Guid> GetActors(Guid processId, TransitionDefinition transition)
        {
            if (transition.Restrictions.Count() < 1)
                return new List<Guid>();

            List<Guid> result = null;
            //TODO Здесь возможно обрабатывать случай - запрещено только одному
            foreach (var restrictionDefinition in transition.Restrictions.Where(r=>r.Type == RestrictionType.Allow))
            {
                var allowed = new List<Guid>();
                var actorDefinitionIsIdentity = restrictionDefinition.Actor as ActorDefinitionIsIdentity;
                if (actorDefinitionIsIdentity != null)
                    allowed.Add(actorDefinitionIsIdentity.IdentityId);

                var actorDefinitionIsInRole = restrictionDefinition.Actor as ActorDefinitionIsInRole;
                if (actorDefinitionIsInRole != null)
                    allowed.AddRange(RoleProvider.GetAllInRole(actorDefinitionIsInRole.RoleId));

                var actorDefinitionExecute = restrictionDefinition.Actor as ActorDefinitionExecuteRule;
                if (actorDefinitionExecute != null && actorDefinitionExecute.Parameters.Count < 1)
                    allowed.AddRange(RuleProvider.GetIdentitiesForRule(processId, actorDefinitionExecute.RuleName));
                else if (actorDefinitionExecute != null && actorDefinitionExecute.Parameters.Count > 0)
                    allowed.AddRange(RuleProvider.GetIdentitiesForRule(processId, actorDefinitionExecute.RuleName, actorDefinitionExecute.Parameters));

                if (result == null || result.Count() < 1)
                    result = allowed;
                else
                    result = result.Intersect(allowed).ToList();
            }

            if (result == null)
                return new List<Guid>();
            if (result.Count() < 1)
                return result;

            foreach (var restrictionDefinition in transition.Restrictions.Where(r => r.Type == RestrictionType.Restrict))
            {
                var restricted = new List<Guid>();
                var actorDefinitionIsIdentity = restrictionDefinition.Actor as ActorDefinitionIsIdentity;
                if (actorDefinitionIsIdentity != null)
                    restricted.Add(actorDefinitionIsIdentity.IdentityId);

                var actorDefinitionIsInRole = restrictionDefinition.Actor as ActorDefinitionIsInRole;
                if (actorDefinitionIsInRole != null)
                    restricted.AddRange(RoleProvider.GetAllInRole(actorDefinitionIsInRole.RoleId));

                var actorDefinitionExecute = restrictionDefinition.Actor as ActorDefinitionExecuteRule;
                if (actorDefinitionExecute != null && actorDefinitionExecute.Parameters.Count < 1)
                    restricted.AddRange(RuleProvider.GetIdentitiesForRule(processId, actorDefinitionExecute.RuleName));
                else if (actorDefinitionExecute != null && actorDefinitionExecute.Parameters.Count > 0)
                    restricted.AddRange(RuleProvider.GetIdentitiesForRule(processId, actorDefinitionExecute.RuleName, actorDefinitionExecute.Parameters));

                result.RemoveAll(p=>restricted.Contains(p));
                if (result.Count() < 1)
                    return result;
            }

            return result;

        }
        
        private bool ValidateActor (Guid processId, Guid identityId, TransitionDefinition transition)
        {
            if (transition.Restrictions.Count() < 1)
                return true;

            foreach (var restrictionDefinition in transition.Restrictions)
            {
                var actorDefinitionIsIdentity = restrictionDefinition.Actor as ActorDefinitionIsIdentity;
                if (actorDefinitionIsIdentity != null)
                {
                    if ((actorDefinitionIsIdentity.IdentityId != identityId &&
                         restrictionDefinition.Type == RestrictionType.Allow) ||
                        (actorDefinitionIsIdentity.IdentityId == identityId &&
                         restrictionDefinition.Type == RestrictionType.Restrict))
                        return false;
                    continue;
                }

                var actorDefinitionIsInRole = restrictionDefinition.Actor as ActorDefinitionIsInRole;
                if (actorDefinitionIsInRole != null)
                {
                    if ((restrictionDefinition.Type == RestrictionType.Allow &&
                         !RoleProvider.IsInRole(identityId, actorDefinitionIsInRole.RoleId)) ||
                        (restrictionDefinition.Type == RestrictionType.Restrict &&
                         RoleProvider.IsInRole(identityId, actorDefinitionIsInRole.RoleId)))
                        return false;
                    continue;
                }

                var actorDefinitionExecute = restrictionDefinition.Actor as ActorDefinitionExecuteRule;
                if (actorDefinitionExecute != null && actorDefinitionExecute.Parameters.Count < 1)
                {
                    if ((restrictionDefinition.Type == RestrictionType.Allow &&
                         !RuleProvider.CheckRule(processId, identityId, actorDefinitionExecute.RuleName)) ||
                        (restrictionDefinition.Type == RestrictionType.Restrict &&
                         RuleProvider.CheckRule(processId, identityId, actorDefinitionExecute.RuleName)))
                        return false;
                    continue;
                }
                if (actorDefinitionExecute != null && actorDefinitionExecute.Parameters.Count > 0)
                {
                    if ((restrictionDefinition.Type == RestrictionType.Allow &&
                         !RuleProvider.CheckRule(processId, identityId, actorDefinitionExecute.RuleName,
                                                 actorDefinitionExecute.Parameters)) ||
                        (restrictionDefinition.Type == RestrictionType.Restrict &&
                         RuleProvider.CheckRule(processId, identityId, actorDefinitionExecute.RuleName,
                                                actorDefinitionExecute.Parameters)))
                        return false;
                    continue;
                }
            }

            return true;
        }


        public bool IsProcessExists (Guid processId)
        {
            return PersistenceProvider.IsProcessExists(processId);
        }

        public ProcessStatus GetProcessStatus (Guid processId)
        {
            return PersistenceProvider.GetInstanceStatus(processId);
        }

        private void SetProcessNewStatus (ProcessInstance processInstance, ProcessStatus newStatus)
        {

            var oldStatus = PersistenceProvider.GetInstanceStatus(processInstance.ProcessId);
            if (newStatus == ProcessStatus.Finalized)
                PersistenceProvider.SetWorkflowFinalized(processInstance);
            else if (newStatus == ProcessStatus.Idled)
                PersistenceProvider.SetWorkflowIdled(processInstance);
            else if (newStatus == ProcessStatus.Initialized)
                PersistenceProvider.SetWorkflowIniialized(processInstance);
            else if (newStatus == ProcessStatus.Running)
                PersistenceProvider.SetWorkflowRunning(processInstance);
            else if (newStatus == ProcessStatus.Terminated)
                PersistenceProvider.SetWorkflowTerminated(processInstance,ErrorLevel.Critical,"Terminated");
            else
            {
                return;
            }

            if (ProcessSatusChanged != null)
                ProcessSatusChanged(this,new ProcessStatusChangedEventArgs(processInstance.ProcessId, oldStatus, newStatus));
        }

        public void PreExecute(Guid processId)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            PersistenceProvider.FillSystemProcessParameters(processInstance);

            var activity = processInstance.ProcessScheme.FindActivity(processInstance.CurrentActivityName);
            var currentActivity = processInstance.ProcessScheme.InitialActivity;
            if (activity.State != currentActivity.State)
                return; //TODO Workflow Temporary

            var executor = new ActivityExecutor(true);
            

            processInstance.AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterProcessId, processId));
            processInstance.AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterSchemeId, processInstance.SchemeId));

            if (currentActivity.HavePreExecutionImplementation)
            {
                var response = executor.Execute(new List<ExecutionRequestParameters>
                                     {
                                         ExecutionRequestParameters.Create(processInstance.ProcessId,
                                                                           processInstance.ProcessParameters,
                                                                           processInstance.ProcessScheme.InitialActivity,
                                                                           ConditionDefinition.Always,
                                                                           true)
                                     });

                if (PreExecuteProcessResponse(processInstance, response)) return;
            }

            do
            {
                if (!string.IsNullOrEmpty(currentActivity.State))
                    processInstance.AddParameter(ParameterDefinition.Create(DefaultDefinitions.ParameterCurrentState, (object)currentActivity.State));

                var transitions =
                    processInstance.ProcessScheme.GetPossibleTransitionsForActivity(currentActivity).Where(t => t.Classifier == TransitionClassifier.Direct);

                currentActivity = null;

                var autotransitions = transitions.Where(t => t.Trigger.Type == TriggerType.Auto);

                var newExecutionParameters = FillExecutionRequestParameters(processId, processInstance, autotransitions);

                if (newExecutionParameters.Count > 0)
                {
                    var response = executor.Execute(newExecutionParameters);

                    if (!PreExecuteProcessResponse(processInstance, response))
                    {
                        currentActivity =
                            processInstance.ProcessScheme.FindTransition(response.ExecutedTransitionName).To;
                    }
                }

                if (currentActivity == null)
                {
                    var commandTransitions = transitions.Where(t => t.Trigger.Type == TriggerType.Command);

                    if (commandTransitions.Count(t => t.Condition.Type == ConditionType.Always && !t.Condition.ResultOnPreExecution.HasValue) < 2) //Это не является ошибкой валидациии при разных командах
                    {
                        newExecutionParameters = FillExecutionRequestParameters(processId, processInstance,
                                                                                commandTransitions);

                        if (newExecutionParameters.Count > 0)
                        {
                            var response = executor.Execute(newExecutionParameters);

                            if (!PreExecuteProcessResponse(processInstance, response))
                            {
                                currentActivity =
                                    processInstance.ProcessScheme.FindTransition(response.ExecutedTransitionName).To;

                            }
                        }
                    }
                }

            } while (currentActivity != null && !currentActivity.IsFinal);

           
        }

        private bool PreExecuteProcessResponse(ProcessInstance processInstance, ExecutionResponseParameters response)
        {
            if (response.IsEmplty)
                return true;

            if (!response.IsError)
                FillParameters(processInstance, response as ExecutionResponseParametersComplete);
            else
            {
                throw (response as ExecutionResponseParametersError).Exception;
            }
            return false;
        }

        private List<ExecutionRequestParameters> FillExecutionRequestParameters(Guid processId, ProcessInstance processInstance, IEnumerable<TransitionDefinition> transitions)
        {
            var newExecutionParameters = new List<ExecutionRequestParameters>();

            foreach (var transition in transitions)
            { 
                var parametersLocal = ExecutionRequestParameters.Create(processInstance.ProcessId,
                                                                        processInstance.ProcessParameters,
                                                                        transition, true);

                if (transition.Trigger.Type != TriggerType.Auto || transition.Restrictions.Count() > 0)
                {
                    var actors = GetActors(processId, transition);

                    parametersLocal.AddParameterInContainer(
                        ParameterDefinition.Create(DefaultDefinitions.ParameterIdentityIds,
                                                   actors));
                }

                if (transition.Trigger.Type == TriggerType.Command)
                    parametersLocal.AddParameterInContainer(ParameterDefinition.Create(DefaultDefinitions.ParameterCurrentCommand,(object) transition.Trigger.Command.Name));

                
                newExecutionParameters.Add(parametersLocal);
            }
            return newExecutionParameters;
        }

        /// <summary>
        /// If the scheme is in scheme persistent store marked as obsolete. Upgrades scheme.
        /// </summary>
        /// <param name="processId">Process instance id</param>
        public void UpdateSchemeIfObsolete(Guid processId)
        {
            UpdateSchemeIfObsolete(processId,new Dictionary<string, IEnumerable<object>>());
        }

        /// <summary>
        /// If the scheme is in scheme persistent store marked as obsolete. Upgrades scheme.
        /// </summary>
        /// <param name="processId">Process instance id</param>
        /// <param name="parameters">Defining parameters of process</param>
        public void UpdateSchemeIfObsolete(Guid processId, IDictionary<string, IEnumerable<object>> parameters)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            var isSchemeObsolete = processInstance.IsSchemeObsolete;
            var isDeterminingParametersChanged = processInstance.IsDeterminingParametersChanged;

            if (!isSchemeObsolete && !isDeterminingParametersChanged)
                return;

            SetProcessNewStatus(processInstance, ProcessStatus.Running);

            try
            {
                processInstance = Builder.CreateNewProcessScheme(processId, processInstance.ProcessScheme.Name, parameters);
                PersistenceProvider.BindProcessToNewScheme(processInstance,true);
                if (OnSchemaWasChanged != null)
                    OnSchemaWasChanged(this,new SchemaWasChangedEventArgs{DeterminingParametersWasChanged = isDeterminingParametersChanged,ProcessId = processId, SchemaWasObsolete = isSchemeObsolete});
            }
            finally
            {
                SetProcessNewStatus(processInstance, ProcessStatus.Idled);
            }

        }


        private ProcessInstance UpdateScheme(Guid processId, ProcessInstance processInstance)
        {
            if (processInstance.CurrentActivity.IsAutoSchemeUpdate && (processInstance.IsSchemeObsolete || processInstance.IsDeterminingParametersChanged) &&
                OnNeedDeterminingParameters != null)
            {
                try
                {
                    SetProcessNewStatus(processInstance, ProcessStatus.Running);
                    processInstance = Builder.GetProcessInstance(processId);
                    PersistenceProvider.FillSystemProcessParameters(processInstance);

                    var isSchemeObsolete = processInstance.IsSchemeObsolete;
                    var isDeterminingParametersChanged = processInstance.IsDeterminingParametersChanged;

                    if (processInstance.CurrentActivity.IsAutoSchemeUpdate && (isSchemeObsolete || isDeterminingParametersChanged) &&
                        OnNeedDeterminingParameters != null)
                    {
                        var args = new NeedDeterminingParametersEventArgs { ProcessId = processId };
                        OnNeedDeterminingParameters(this, args);
                        if (args.DeterminingParameters == null)
                            args.DeterminingParameters = new Dictionary<string, IEnumerable<object>>();

                        processInstance = Builder.CreateNewProcessScheme(processId, processInstance.ProcessScheme.Name,
                                                                          args.DeterminingParameters);
                        PersistenceProvider.BindProcessToNewScheme(processInstance,true);
                        if (OnSchemaWasChanged != null)
                            OnSchemaWasChanged(this,
                                               new SchemaWasChangedEventArgs
                                                   {
                                                       DeterminingParametersWasChanged = isDeterminingParametersChanged,
                                                       ProcessId = processId,
                                                       SchemeId = processInstance.SchemeId,
                                                       SchemaWasObsolete = isSchemeObsolete
                                                   });
                        PersistenceProvider.FillSystemProcessParameters(processInstance);
                    }
                }
                finally
                {
                    SetProcessNewStatus(processInstance, ProcessStatus.Idled);
                }
            }
            return processInstance;
        }

        public string GetLocalizedStateName (Guid processId, string stateName)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            return processInstance.GetLocalizedStateName(stateName,CultureInfo.CurrentCulture);
        }

        public string GetLocalizedCommandName (Guid processId, string commandName)
        {
            var processInstance = Builder.GetProcessInstance(processId);
            return processInstance.GetLocalizedCommandName(commandName,CultureInfo.CurrentCulture);
        }
    }
}
