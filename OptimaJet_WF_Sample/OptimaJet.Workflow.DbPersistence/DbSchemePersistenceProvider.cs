using OptimaJet.Common;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml.Linq;
namespace OptimaJet.Workflow.DbPersistence
{
    /// <summary>
    /// 得到流程的具体方案信息
    /// </summary>
    public sealed class DbSchemePersistenceProvider : DbProvider, ISchemePersistenceProvider<XElement>
    {
        public DbSchemePersistenceProvider(string connectionStringName)
            : base(connectionStringName)
        {
        }

        /// <summary>
        /// WorkflowProcessInstances
        /// 根据公文流转实例processId获取其基于的审批流程方案WorkflowProcessScheme，
        /// 并根据其Scheme的XML内容转换为XElement结构
        /// </summary>
        /// <param name="processId">如：具体《建设项目选址意见书》ID</param>
        /// <returns></returns>
        public SchemeDefinition<XElement> GetProcessSchemeByProcessId(Guid processId)
        {
            WorkflowProcessInstance workflowProcessInstance;
            using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
            {
                workflowProcessInstance = workflowPersistenceModelDataContext.WorkflowProcessInstances.FirstOrDefault((WorkflowProcessInstance pis) => pis.Id == processId);
            }
            if (workflowProcessInstance == null)
            {
                throw new ProcessNotFoundException();
            }
            if (!workflowProcessInstance.SchemeId.HasValue) //模版类(WorkflowProcessScheme)不能为空
            {
                throw new SchemeNotFoundException();
            }
            SchemeDefinition<XElement> processSchemeBySchemeId = this.GetProcessSchemeBySchemeId(workflowProcessInstance.SchemeId.Value);
            processSchemeBySchemeId.IsDeterminingParametersChanged = workflowProcessInstance.IsDeterminingParametersChanged;
            return processSchemeBySchemeId;
        }

        /// <summary>
        /// 根据WorkflowProcessScheme的schemeId获取指定的WorkflowProcessScheme
        /// 并根据其Scheme的内容通过构建SchemeDefinition<XElement>的审批方案的XML关系定义
        /// </summary>
        /// <param name="schemeId"></param>
        /// <returns></returns>
        public SchemeDefinition<XElement> GetProcessSchemeBySchemeId(Guid schemeId)
        {
            WorkflowProcessScheme workflowProcessScheme;
            using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
            {
                workflowProcessScheme = workflowPersistenceModelDataContext.WorkflowProcessSchemes.FirstOrDefault((WorkflowProcessScheme pss) => pss.Id == schemeId);
            }
            if (workflowProcessScheme == null || string.IsNullOrEmpty(workflowProcessScheme.Scheme))
            {
                throw new SchemeNotFoundException();
            }

            //XML如何与SchemeDefinition<XElement>关联起来的呢？SchemeDefinition只是简单的保存schemeId，Scheme等的数据，未进行审批关系分析
            return new SchemeDefinition<XElement>(schemeId, XElement.Parse(workflowProcessScheme.Scheme), workflowProcessScheme.IsObsolete);
        }
        public SchemeDefinition<XElement> GetProcessSchemeWithParameters(string processName, IDictionary<string, IEnumerable<object>> parameters)
        {
            return this.GetProcessSchemeWithParameters(processName, parameters, false);
        }

        /// <summary>
        /// 根据公文流转处理流程名称processName，以及参数获取适配的SchemeDefinition
        /// 注意这里没有processId
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="parameters">参数，转换为hash在WorkflowProcessScheme中的DefiningParametersHash进行比较</param>
        /// <param name="ignoreObsolete"></param>
        /// <returns></returns>
        public SchemeDefinition<XElement> GetProcessSchemeWithParameters(string processName, IDictionary<string, IEnumerable<object>> parameters, bool ignoreObsolete)
        {
            string definingParameters = this.SerializeParameters(parameters);
            string hash = HashHelper.GenerateStringHash(definingParameters);
            IEnumerable<WorkflowProcessScheme> source;
            using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
            {
                source = (
                    from pss in workflowPersistenceModelDataContext.WorkflowProcessSchemes
                    where pss.ProcessName == processName && pss.DefiningParametersHash == hash && (!ignoreObsolete || !pss.IsObsolete)
                    select pss).ToList<WorkflowProcessScheme>();
            }
            if (!source.Any()) // 表示数据表WorkflowProcessScheme中没有匹配的WorkflowProcessScheme记录，source.Count<WorkflowProcessScheme>() < 1 卢远宗修改
            {
                throw new SchemeNotFoundException();
            }
            if (source.Count<WorkflowProcessScheme>() == 1)
            {
                var workflowProcessScheme = source.First<WorkflowProcessScheme>();
                return new SchemeDefinition<XElement>(workflowProcessScheme.Id, XElement.Parse(workflowProcessScheme.Scheme), workflowProcessScheme.IsObsolete);
            }

            //如果有多个匹配项的WorkflowProcessScheme
            using (IEnumerator<WorkflowProcessScheme> enumerator = (
                from processScheme in source
                where processScheme.DefiningParameters == definingParameters
                select processScheme).GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    WorkflowProcessScheme current = enumerator.Current;
                    return new SchemeDefinition<XElement>(current.Id, XElement.Parse(current.Scheme), current.IsObsolete);
                }
            }
            throw new SchemeNotFoundException();
        }

        /// <summary>
        /// 保存新的WorkflowProcessScheme，可以动态的修改参数后保存
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="schemeId"></param>
        /// <param name="scheme"></param>
        /// <param name="parameters"></param>
        public void SaveScheme(string processName, Guid schemeId, XElement scheme, IDictionary<string, IEnumerable<object>> parameters)
        {
            string text = this.SerializeParameters(parameters);
            string definingParametersHash = HashHelper.GenerateStringHash(text);
            using (TransactionScope serializableSupressedScope = PredefinedTransactionScopes.SerializableSupressedScope)
            {
                using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
                {
                    List<WorkflowProcessScheme> list = (
                        from wps in workflowPersistenceModelDataContext.WorkflowProcessSchemes
                        where wps.DefiningParametersHash == definingParametersHash && wps.ProcessName == processName && !wps.IsObsolete
                        select wps).ToList<WorkflowProcessScheme>();
                    if (list.Count<WorkflowProcessScheme>() > 0)
                    {
                        foreach (WorkflowProcessScheme current in list)
                        {
                            if (current.DefiningParameters == text)
                            {
                                throw new SchemeAlredyExistsException();
                            }
                        }
                    }
                    WorkflowProcessScheme entity = new WorkflowProcessScheme
                    {
                        Id = schemeId,
                        DefiningParameters = text,
                        DefiningParametersHash = definingParametersHash,
                        Scheme = scheme.ToString(),
                        ProcessName = processName
                    };
                    workflowPersistenceModelDataContext.WorkflowProcessSchemes.InsertOnSubmit(entity);
                    workflowPersistenceModelDataContext.SubmitChanges();
                }
                serializableSupressedScope.Complete();
            }
        }

        /// <summary>
        /// 参数对象数组序列化为字符串，parameters需要什么结构，序列化结果
        /// 是什么格式呢？{}样式，一般用在保存到definingParameters的字段里
        /// DefiningParametersHash的字段对definingParameters字符串值进行哈希
        /// 用于简单的识别该参数数组是否已经发生变化
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string SerializeParameters(IDictionary<string, IEnumerable<object>> parameters)
        {
            StringBuilder stringBuilder = new StringBuilder("{");
            bool flag = true;
            foreach (KeyValuePair<string, IEnumerable<object>> current in
                from p in parameters
                orderby p.Key
                select p)
            {
                if (!string.IsNullOrEmpty(current.Key) && current.Value.Count<object>() >= 1)
                {
                    if (!flag)
                    {
                        stringBuilder.Append(",");
                    }
                    stringBuilder.AppendFormat("{0}:[", current.Key);
                    bool flag2 = true;
                    foreach (object current2 in current.Value.OrderBy((object p) => p))
                    {
                        if (!flag2)
                        {
                            stringBuilder.Append(",");
                        }
                        stringBuilder.AppendFormat("\"{0}\"", current2);
                        flag2 = false;
                    }
                    stringBuilder.Append("]");
                    flag = false;
                }
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }
}
