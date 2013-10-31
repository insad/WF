using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Runtime;

namespace WF.Sample.Business.Workflow
{
    public class WorkflowRule : IWorkflowRuleProvider
    {
        private Dictionary<string, Func<Guid, Guid, bool>> _funcs = new Dictionary<string, Func<Guid, Guid, bool>>();

        private Dictionary<string, Func<Guid, IEnumerable<Guid>>> _getIdentitiesFuncs = new Dictionary<string, Func<Guid, IEnumerable<Guid>>>();

        public WorkflowRule ()
        {
            _funcs.Add("IsDocumentAuthor", IsDocumentAuthor);
            _funcs.Add("IsAuthorsBoss", IsAuthorsBoss);
            _funcs.Add("IsDocumentController", IsDocumentController);
            _getIdentitiesFuncs.Add("IsDocumentAuthor", GetDocumentAuthor);
            _getIdentitiesFuncs.Add("IsDocumentController", GetDocumentController);
            _getIdentitiesFuncs.Add("IsAuthorsBoss", GetAuthorsBoss);
        }

        private bool IsAuthorsBoss(Guid processId, Guid identityId)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processId);
                if (document == null)
                    return false;
                return context.vHeads.Count(h => h.Id == document.AuthorId && h.HeadId == identityId) > 0;
            }
        }

        private IEnumerable<Guid> GetAuthorsBoss(Guid processId)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processId);
                if (document == null)
                    return new List<Guid>{};
                

                return context.vHeads.Where(h=>h.Id == document.AuthorId).Select(h=>h.HeadId).ToList();
            }
        }

        private IEnumerable<Guid> GetDocumentController(Guid processId)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processId);
                if (document == null || !document.EmloyeeControlerId.HasValue)
                    return new List<Guid> {};

                return new List<Guid> {document.EmloyeeControlerId.Value};
            }
        }

        private IEnumerable<Guid> GetDocumentAuthor(Guid processId)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processId);
                if (document == null)
                    return new List<Guid>{};
                return new List<Guid> {document.AuthorId};
            }
        }

        private bool IsDocumentController(Guid processId, Guid identityId)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processId);
                if (document == null)
                    return false;
                return document.EmloyeeControlerId.HasValue && document.EmloyeeControlerId.Value == identityId;
            }
        }

        private bool IsDocumentAuthor(Guid processId, Guid identityId)
        {
            using (var context = new DataModelDataContext())
            {
                var document = context.Documents.FirstOrDefault(d => d.Id == processId);
                if (document == null)
                    return false;
                return document.AuthorId == identityId;
            }
        }

        public bool CheckRule(Guid processId, Guid identityId, string ruleName)
        {
            return _funcs.ContainsKey(ruleName) && _funcs[ruleName].Invoke(processId, identityId);
        }

        public bool CheckRule(Guid processId, Guid identityId, string ruleName, IDictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Guid> GetIdentitiesForRule(Guid processId, string ruleName)
        {
            return !_getIdentitiesFuncs.ContainsKey(ruleName) ? new List<Guid> {} : _getIdentitiesFuncs[ruleName].Invoke(processId);
        }

        public IEnumerable<Guid> GetIdentitiesForRule(Guid processId, string ruleName, IDictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }
    }
}
