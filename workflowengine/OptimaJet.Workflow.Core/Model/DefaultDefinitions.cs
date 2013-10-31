using System;
using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// 系统定义的缺省参数，该参数被系统内定义和使用
    /// </summary>
    public static class DefaultDefinitions
    {
        
        public static readonly ParameterDefinition ParameterProcessId = new ParameterDefinition() { Name = "ProcessId", Type = typeof(Guid), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterPreviousState = new ParameterDefinition() { Name = "PreviousState", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterCurrentState = new ParameterDefinition() { Name = "CurrentState", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterPreviousStateForDirect = new ParameterDefinition() { Name = "PreviousStateForDirect", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterPreviousStateForReverse = new ParameterDefinition() { Name = "PreviousStateForReverse", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterPreviousActivity = new ParameterDefinition() { Name = "PreviousActivity", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterCurrentActivity = new ParameterDefinition() { Name = "CurrentActivity", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterPreviousActivityForDirect = new ParameterDefinition() { Name = "PreviousActivityForDirect", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterPreviousActivityForReverse = new ParameterDefinition() { Name = "PreviousActivityForReverse", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterCurrentCommand = new ParameterDefinition() { Name = "CurrentCommand", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterExecutedActivityState = new ParameterDefinition() { Name = "ExecutedActivityState", Type = typeof(string), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterConditionResult = new ParameterDefinition() { Name = "ConditionResult", Type = typeof(bool), Purpose = ParameterPurpose.System };
        public static readonly ParameterDefinition ParameterIdentityId = new ParameterDefinition() { Name = "IdentityId", Type = typeof(Guid), Purpose = ParameterPurpose.Persistence };
        public static readonly ParameterDefinition ParameterImpersonatedIdentityId = new ParameterDefinition() { Name = "ImpersonatedIdentityId", Type = typeof(Guid), Purpose = ParameterPurpose.Persistence };
        public static readonly ParameterDefinition ParameterSchemeId = new ParameterDefinition() { Name = "SchemeId", Type = typeof(Guid), Purpose = ParameterPurpose.System };

        public static readonly ParameterDefinition ParameterIdentityIds = new ParameterDefinition()
                                                                     {
                                                                         Name = "IdentityIds",
                                                                         Type = typeof(IEnumerable<Guid>),
                                                                         Purpose = ParameterPurpose.System
                                                                     };
        public static CommandDefinition CommandAuto = new CommandDefinition() { Name = "Auto" };
        public static CommandDefinition CommandSetState = new CommandDefinition() { Name = "SetState" };


        public static readonly IEnumerable<ParameterDefinition> DefaultParameters = new List<ParameterDefinition>()
                                                                                        {
                                                                                            ParameterProcessId,
                                                                                            ParameterPreviousState,
                                                                                            ParameterCurrentState,
                                                                                            ParameterPreviousStateForDirect,
                                                                                            ParameterPreviousStateForReverse,
                                                                                            ParameterPreviousActivity,
                                                                                            ParameterPreviousActivityForDirect,
                                                                                            ParameterPreviousActivityForReverse,
                                                                                            ParameterCurrentCommand,
                                                                                            ParameterConditionResult,
                                                                                            ParameterIdentityId,
                                                                                            ParameterImpersonatedIdentityId,
                                                                                            ParameterExecutedActivityState,
                                                                                            ParameterCurrentActivity,
                                                                                            ParameterSchemeId,
                                                                                            ParameterIdentityIds
                                                                                        };
    }
}
