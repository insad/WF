using System;

namespace OptimaJet.Workflow.Core.Model
{

    public abstract class TriggerDefinition
    {
        public abstract TriggerType Type { get; }
      

        public static TriggerDefinition Create(string type)
        {
            TriggerType parsedType;
            Enum.TryParse(type, true, out parsedType);

            switch (parsedType)
            {
                case TriggerType.Auto:
                    return Auto;
                    break;
                case TriggerType.Command:
                    return new CommandTriggerDefinition();
                case TriggerType.Timer:
                    return new TimerTriggerDefinition();
            }

            return null;
        }

        public CommandDefinition Command
        {
            get { return (this as CommandTriggerDefinition).Command; }
        }

        public TimerDefinition Timer
        {
            get { return (this as TimerTriggerDefinition).Timer; }
        }

        public static TriggerDefinition Auto
        {
            get { return new AutoTriggerDefinition();}
        }
    }

    public class CommandTriggerDefinition : TriggerDefinition
    {
        public override TriggerType Type
        {
            get { return TriggerType.Command; }
        }

        public CommandDefinition Command { get; set; }
    }

    public class TimerTriggerDefinition : TriggerDefinition
    {
        public override TriggerType Type
        {
            get { return TriggerType.Timer; }
        }

        public TimerDefinition Timer { get; set; }
    }

    public class AutoTriggerDefinition : TriggerDefinition
    {
        public override TriggerType Type
        {
            get { return TriggerType.Auto; }
        }
    }



    public enum TriggerType
    {
        Command,
        Auto,
        Timer
    }


    public sealed class ConditionDefinition
    {
        private ConditionDefinition()
        {
        }

        public ConditionType Type { get; internal set; }
        public ActionDefinition Action { get; internal set; }
        public bool? ResultOnPreExecution { get; internal set; }

        public static ConditionDefinition Create(string type)
        {
            return Create(type, null, null);
        }

        public static ConditionDefinition Create(string type, string resultOnPreExecution)
        {
            return Create(type, null, resultOnPreExecution);
        }

        public static ConditionDefinition Create(string type, ActionDefinition action, string resultOnPreExecution)
        {
            ConditionType parsedType;
            Enum.TryParse(type, true, out parsedType);

            return new ConditionDefinition() { Action = action, Type = parsedType, ResultOnPreExecution = string.IsNullOrEmpty(resultOnPreExecution) ? (bool?)null : bool.Parse(resultOnPreExecution) };
        }

        public static ConditionDefinition Always
        {
           get { return new ConditionDefinition() {Type = ConditionType.Always}; }
        }

    }

    public enum ConditionType
    {
        Action,
        Always,
        Otherwise

    }

    
}
