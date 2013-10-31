using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Model
{
    public abstract class OnErrorDefinition : BaseDefinition
    {
        public OnErrorActionType ActionType { get; internal set; }
        public int Priority { get; internal set; }
       // public bool IsRethrow { get; internal set; }
        public Type ExceptionType { get; internal set; }

        public static SetActivityOnErrorDefinition CreateSetActivityOnError(string name, string nameRef, string priority, string typeName/*string isExecuteImplementation, string isRethrow*/)
        {
            return new SetActivityOnErrorDefinition
                       {
                           ActionType = OnErrorActionType.SetActivity,
                           //IsExecuteImplementation =
                           //    !string.IsNullOrEmpty(isExecuteImplementation) && bool.Parse(isExecuteImplementation),
                           //IsRethrow =
                           //   !string.IsNullOrEmpty(isRethrow) && bool.Parse(isRethrow),
                           NameRef = nameRef,
                           Name = name,
                           Priority = !string.IsNullOrEmpty(priority) ? int.Parse(priority) : int.MaxValue,
                           ExceptionType = Type.GetType(typeName)
                       };
        }
    }

    public sealed class SetActivityOnErrorDefinition : OnErrorDefinition
    {
        public string NameRef { get; internal set; }
        //public bool IsExecuteImplementation { get; internal set; }
    }
}
