using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Model
{
    public abstract class BaseDefinition
    {
        public virtual string Name { get; internal set; }

        protected BaseDefinition(){}
    }
}
