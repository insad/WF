using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace WF.Sample.Business.Workflow
{
    public static class WorkflowActions
    {
        public static void CheckDocumentHasController(Guid processId,  out bool conditionResult)
        {
            using (var context = new DataModelDataContext())
            {
                conditionResult = context.Documents.Count(d => d.Id == processId && d.EmloyeeControlerId.HasValue) > 0;
            }
        }

        public static void CheckDocumentsAuthorIsBoss(Guid processId, out bool conditionResult)
        {
            using (var context = new DataModelDataContext())
            {
                conditionResult = context.Documents.Count(d => d.Id == processId && d.Employee.IsHead) > 0;
            }
        }

        public static void CheckBigBossMustSight(Guid processId, out bool conditionResult)
        {
            using (var context = new DataModelDataContext())
            {
                conditionResult = context.Documents.Count(d => d.Id == processId && d.Sum > 100) > 0;
            }
        }

        public static void DeleteEmptyTransitionHistoryItems(Guid processId)
        {
            using (var context = new DataModelDataContext())
            {
                var unusedHistories =
                    context.DocumentTransitionHistories.Where(
                        h => h.DocumentId == processId && !h.TransitionTime.HasValue);

                context.DocumentTransitionHistories.DeleteAllOnSubmit(unusedHistories);

                context.SubmitChanges();
            }
        }

        public static void WriteTransitionHistory(Guid processId, string currentStateName, string executedStateName, string commandName, IEnumerable<Guid> identities)
        {
            var currentstate = WorkflowInit.Runtime.GetLocalizedStateName(processId, currentStateName);

            var nextState = WorkflowInit.Runtime.GetLocalizedStateName(processId, executedStateName);

            var command = WorkflowInit.Runtime.GetLocalizedCommandName(processId, commandName);

            using (var scope = new TransactionScope())
            {
                using (var context = new DataModelDataContext())
                {
                    GetEmployeesString(identities, context);

                    var historyItem = new DocumentTransitionHistory
                                          {
                                              Id = Guid.NewGuid(),
                                              AllowedToEmployeeNames =  GetEmployeesString(identities, context),
                                              DestinationState = nextState,
                                              DocumentId = processId,
                                              InitialState = currentstate,
                                              Command = command
                                          };
                    context.DocumentTransitionHistories.InsertOnSubmit(historyItem);
                    context.SubmitChanges();
                }

                scope.Complete();
            }
        }

        private static string GetEmployeesString(IEnumerable<Guid> identities, DataModelDataContext context)
        {
            var employees = context.Employees.Where(e => identities.Contains(e.Id)).ToList();

            var sb = new StringBuilder();
            bool isFirst = true;
            foreach (var employee in employees)
            {
                if (!isFirst)
                    sb.Append(",");
                isFirst = false;

                sb.Append(employee.Name);
            }

            return sb.ToString();
        }

        public static void UpdateTransitionHistory(Guid processId, string currentStateName, string executedStateName, string commandName, Guid identityId, Guid impersonatedIdentityId, string comment)
        {
            var currentstate = WorkflowInit.Runtime.GetLocalizedStateName(processId, currentStateName);

            var nextState = WorkflowInit.Runtime.GetLocalizedStateName(processId, executedStateName);

            var command = WorkflowInit.Runtime.GetLocalizedCommandName(processId, commandName);

            using (var scope = new TransactionScope())
            {
                using (var context = new DataModelDataContext())
                {
                    var document = context.Documents.FirstOrDefault(d => d.Id == processId);
                    if (document == null)
                        return;

                     document.State = nextState;

                    var historyItem =
                        context.DocumentTransitionHistories.FirstOrDefault(
                            h => h.DocumentId == processId && !h.TransitionTime.HasValue &&
                                 h.InitialState == currentstate && h.DestinationState == nextState);

                    if (historyItem == null)
                    {
                        historyItem = new DocumentTransitionHistory
                                          {
                                              Id = Guid.NewGuid(),
                                              AllowedToEmployeeNames = string.Empty,
                                              DestinationState = nextState,
                                              DocumentId = processId,
                                              InitialState = currentstate
                                          };

                         context.DocumentTransitionHistories.InsertOnSubmit(historyItem);

                    }

                    historyItem.Command = command;
                    historyItem.TransitionTime = DateTime.Now;
                    historyItem.EmployeeId = identityId;

                    context.SubmitChanges();

                }
                scope.Complete();
            }
        }


    }
}
