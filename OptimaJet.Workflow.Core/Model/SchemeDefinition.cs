using System;

namespace OptimaJet.Workflow.Core.Model
{
    public class SchemeDefinition<T> where T : class 
    {
        public T Scheme { get; private set; }
        public Guid Id { get; private set; }
        public bool  IsObsolete { get; private set; }
        public bool IsDeterminingParametersChanged { get; set; }

        public SchemeDefinition(Guid id, T scheme, bool isObsolete, bool isDeterminingParametersChanged)
        {
            Id = id;
            Scheme = scheme;
            IsObsolete = isObsolete;
            IsDeterminingParametersChanged = isDeterminingParametersChanged;
        }

        public SchemeDefinition(Guid id, T scheme, bool isObsolete) : this (id,scheme,isObsolete,false)
        {
        }

    }
}
