using OptimaJet.Workflow.Core.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
namespace OptimaJet.Workflow.DbPersistence
{
    /// <summary>
    /// 获取WorkflowScheme的数据库序列化类
    /// WorkflowScheme 表中包含了，整个流程的相应细节。
    /// </summary>
    public class DbXmlWorkflowGenerator : DbProvider, IWorkflowGenerator<XElement>
    {
        protected IDictionary<string, string> TemplateTypeMapping = new Dictionary<string, string>();
        public DbXmlWorkflowGenerator(string connectionStringName)
            : base(connectionStringName)
        {
        }

        /// <summary>
        /// 将WorkflowScheme表中的Scheme的XML转换为XElement
        /// 将指定流程转换为XElement
        /// </summary>
        /// <param name="processName">流程名，如：SimpleWF</param>
        /// <param name="schemeId">没有用</param>
        /// <param name="parameters">没用用，但必须要有值</param>
        /// <returns></returns>
        public XElement Generate(string processName, Guid schemeId, IDictionary<string, IEnumerable<object>> parameters)
        {
            if (parameters.Count > 0)
            {
                throw new InvalidOperationException("Parameters not supported");
            }
            // 返回如：SimpleWF
            string code = (!this.TemplateTypeMapping.ContainsKey(processName.ToLower())) ? processName : this.TemplateTypeMapping[processName.ToLower()];
            WorkflowScheme workflowScheme = null;
            using (WorkflowPersistenceModelDataContext workflowPersistenceModelDataContext = base.CreateContext())
            {
                workflowScheme = workflowPersistenceModelDataContext.WorkflowSchemes.FirstOrDefault((WorkflowScheme ws) => ws.Code == code);
            }
            if (workflowScheme == null)
            {
                throw new InvalidOperationException(string.Format("Scheme with Code={0} not found", code));
            }
            return XElement.Parse(workflowScheme.Scheme);
        }
        /// <summary>
        /// 添加相应的流程流程映射
        /// </summary>
        /// <param name="processName">流程名,如：Document</param>
        /// <param name="generatorSource">WorkflowScheme表中Code值,具体的数据源，如:SimpleWF</param>
        public void AddMapping(string processName, object generatorSource)
        {
            string text = generatorSource as string;
            if (text == null)
            {
                throw new InvalidOperationException("Generator source must be a string");
            }
            this.TemplateTypeMapping.Add(processName.ToLower(), text);
        }
    }
}
