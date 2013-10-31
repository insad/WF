using System;

namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// 审批流程方案数据模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SchemeDefinition<T> where T : class 
    {
        public T Scheme { get; private set; }
        public Guid Id { get; private set; }
        public bool  IsObsolete { get; private set; }
        /// <summary>
        /// 确定流程参数是否改变
        /// </summary>
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
