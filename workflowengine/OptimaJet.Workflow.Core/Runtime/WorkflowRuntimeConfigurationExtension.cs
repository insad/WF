using System;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Bus;
using OptimaJet.Workflow.Core.Cache;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Persistence;

namespace OptimaJet.Workflow.Core.Runtime
{
   
    public static class WorkflowRuntimeConfigurationExtension
    {
        public static WorkflowRuntime WithBuilder(this WorkflowRuntime runtime, IWorkflowBuilder builder)
        {
            runtime.Builder = builder;
            return runtime;
        }

        public static WorkflowRuntime WithDefaultBuilder<TSchemeMedium>(this WorkflowRuntime runtime) where TSchemeMedium : class
        {
            runtime.Builder = new WorkflowBuilder<TSchemeMedium>();
            return runtime;
        }

        public static WorkflowRuntime WithRoleProvider(this WorkflowRuntime runtime, IWorkflowRoleProvider roleProvider)
        {
            runtime.RoleProvider = roleProvider;
            return runtime;
        }

        public static WorkflowRuntime WithRuleProvider(this WorkflowRuntime runtime, IWorkflowRuleProvider ruleProvider)
        {
            runtime.RuleProvider = ruleProvider;
            return runtime;
        }

        public static WorkflowRuntime WithPersistenceProvider(this WorkflowRuntime runtime, IPersistenceProvider persistenceProvider)
        {
            runtime.PersistenceProvider = persistenceProvider;
            return runtime;
        }

        public static WorkflowRuntime WithRuntimePersistance(this WorkflowRuntime runtime, IRuntimePersistence persistenceProvider)
        {
            runtime.RuntimePersistence = persistenceProvider;
            return runtime;
        }

        public static WorkflowRuntime WithBus(this WorkflowRuntime runtime, IWorkflowBus bus)
        {
            bus.Initialize();
            runtime.Bus = bus;
            bus.ExecutionComplete += runtime.BusExecutionComplete;
            return runtime;
        }

        public static WorkflowRuntime AttachDeterminingParametersGetter(this WorkflowRuntime runtime, EventHandler<NeedDeterminingParametersEventArgs> determiningParametersGetter)
        {
            runtime.OnNeedDeterminingParameters += determiningParametersGetter;
            return runtime;
        }

        public static WorkflowRuntime SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn(this WorkflowRuntime runtime)
        {
            runtime.IsAutoUpdateSchemeBeforeGetAvailableCommands = true;
            return runtime;
        }

        public static WorkflowRuntime SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn(this WorkflowRuntime runtime, EventHandler<NeedDeterminingParametersEventArgs> determiningParametersGetter)
        {
            runtime.IsAutoUpdateSchemeBeforeGetAvailableCommands = true;
            runtime.OnNeedDeterminingParameters += determiningParametersGetter;
            return runtime;
        }

        public static WorkflowRuntime SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOff(this WorkflowRuntime runtime)
        {
            runtime.IsAutoUpdateSchemeBeforeGetAvailableCommands = false;
            return runtime;
        }

        public static WorkflowRuntime Start(this WorkflowRuntime runtime)
        {
            if (!runtime.ValidateSettings())
                throw new InvalidOperationException();
            runtime.Start();
            return runtime;
        }

        public static WorkflowRuntime ColdStart(this WorkflowRuntime runtime)
        {
            if (!runtime.ValidateSettings())
                throw new InvalidOperationException();
            runtime.ColdStart();
            return runtime;
        }

        public static IWorkflowBuilder WithCache(this IWorkflowBuilder bulder, IParsedProcessCache cache)
        {
            bulder.SetCache(cache);
            return bulder;
        }

        public static IWorkflowBuilder WithDefaultCache(this IWorkflowBuilder bulder)
        {
            bulder.SetCache(new DefaultParcedProcessCache());
            return bulder;
        }

        public static IWorkflowBuilder WithGenerator<TSchemeMedium>(this WorkflowBuilder<TSchemeMedium> bulder, IWorkflowGenerator<TSchemeMedium> generator) where TSchemeMedium : class
        {
            bulder.Generator = generator;
            return bulder;
        }

        public static IWorkflowBuilder WithParser<TSchemeMedium>(this WorkflowBuilder<TSchemeMedium> bulder, IWorkflowParser<TSchemeMedium> parser) where TSchemeMedium : class
        {
            bulder.Parser = parser;
            return bulder;
        }

        public static IWorkflowBuilder WithShemePersistenceProvider<TSchemeMedium>(this WorkflowBuilder<TSchemeMedium> bulder, ISchemePersistenceProvider<TSchemeMedium> schemePersistenceProvider) where TSchemeMedium : class
        {
            bulder.SchemePersistenceProvider = schemePersistenceProvider;
            return bulder;
        }

        public static IWorkflowGenerator<TSchemeMedium> WithMapping<TSchemeMedium> (this IWorkflowGenerator<TSchemeMedium> generator, string processName, object generatorSource) where TSchemeMedium : class
        {
            generator.AddMapping(processName,generatorSource);
            return generator;
        }
    }
}
