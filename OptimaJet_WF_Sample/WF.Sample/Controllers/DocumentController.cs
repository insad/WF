using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using OptimaJet.Workflow.Core.Runtime;
using WF.Sample.Business;
using WF.Sample.Business.Helpers;
using WF.Sample.Business.Workflow;
using WF.Sample.Helpers;
using WF.Sample.Models;
using ProcessStatus = OptimaJet.Workflow.Core.Persistence.ProcessStatus;

namespace WF.Sample.Controllers
{
    public class DocumentController : Controller
    {
        #region Index
        public ActionResult Index()
        {
            var res = (from d in DocumentHelper.GetAll()
                      select new DocumentModel()
                                 {
                                     Id = d.Id,
                                     AuthorId = d.AuthorId,
                                     AuthorName = d.Employee1.Name,
                                     Comment = d.Comment,
                                     EmloyeeControlerId = d.EmloyeeControlerId,
                                     EmloyeeControlerName = d.EmloyeeControlerId.HasValue ? d.Employee.Name : string.Empty,
                                     Name = d.Name,
                                     Number = d.Number,
                                     StateName = d.State,
                                     Sum = d.Sum
                                 }).ToList();
            return View(res);
        }
        #endregion

        #region Edit
        public ActionResult Edit(Guid? Id)
        {
            DocumentModel model = null;

            if(Id.HasValue) //用户单击某个文档的编辑界面
            {
                var d = DocumentHelper.Get(Id.Value);
                if(d != null)
                {
                    CreateWorkflowIfNotExists(Id.Value);

                    var h = DocumentHelper.GetHistory(Id.Value);
                    model = new DocumentModel()
                               {
                                   Id = d.Id,
                                   AuthorId = d.AuthorId,
                                   AuthorName = d.Employee1.Name,
                                   Comment = d.Comment,
                                   EmloyeeControlerId = d.EmloyeeControlerId,
                                   EmloyeeControlerName =
                                       d.EmloyeeControlerId.HasValue ? d.Employee.Name : string.Empty,
                                   Name = d.Name,
                                   Number = d.Number,
                                   StateName = d.State,
                                   Sum = d.Sum,
                                   Commands = GetCommands(Id.Value),
                                   AvailiableStates = GetStates(Id.Value),
                                   HistoryModel = new DocumentHistoryModel{Items = h} // 关联文档流转历史记录
                               };
                }
                
            }
            else //新建文档的用户界面
            {
               Guid userId = CurrentUserSettings.GetCurrentUser();
                model = new DocumentModel()
                        {
                            AuthorId = userId,
                            AuthorName = EmployeeHelper.GetNameById(userId),
                            StateName = "Draft",
                            Commands = new Dictionary<string, string>{},
                            AvailiableStates = new Dictionary<string, string>()
                        };
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(Guid? Id, DocumentModel model, string button)
        {
            using (var context = new DataModelDataContext())
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                Document target = null;
                if (model.Id != Guid.Empty)
                {
                    target = DocumentHelper.Get(model.Id, context);
                    if (target == null)
                    {
                        ModelState.AddModelError("", "Row not found!");
                        return View(model);
                    }
                }
                else
                {
                    target = new Document();
                    target.Id = Guid.NewGuid();
                    target.AuthorId = model.AuthorId;
                    target.State = model.StateName;
                    context.Documents.InsertOnSubmit(target);
                }

                target.Name = model.Name;
                target.EmloyeeControlerId = model.EmloyeeControlerId;
                target.Comment = model.Comment;
                target.Sum = model.Sum;

                try
                {
                    context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    var sb = new StringBuilder("Ошибка сохранения. " + ex.Message);
                    if (ex.InnerException != null)
                        sb.AppendLine(ex.InnerException.Message);
                    ModelState.AddModelError("", sb.ToString());
                    return View(model);
                }

                if (button == "SaveAndExit")
                    return RedirectToAction("Index");
                if (button != "Save")
                {
                    ExecuteCommand(target.Id, button, model);
                }
                return RedirectToAction("Edit", new {target.Id});
            }
            return View(model);
        }
        #endregion

        #region Delete
        public ActionResult DeleteRows(Guid[] ids)
        {
            if (ids == null || ids.Length == 0)
                return Content("Items not selected");

            try
            {
                DocumentHelper.Delete(ids);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

            return Content("Rows deleted");
        }
        #endregion

        #region Workflow
        /// <summary>
        /// 根据文档ID以及当前的用户ID获取该用户的可执行操作Commands
        /// </summary>
        /// <param name="id">文档ID，该ID作为WorkflowProcessInstance的 ID主键值</param>
        /// <returns></returns>
        private Dictionary<string, string> GetCommands(Guid id)
        {
            var result = new Dictionary<string, string>();
            var commands = WorkflowInit.Runtime.GetAvailableCommands(id, CurrentUserSettings.GetCurrentUser());
            foreach (var workflowCommand in commands)
            {
                if (!result.ContainsKey(workflowCommand.CommandName))
                    result.Add(workflowCommand.CommandName, workflowCommand.LocalizedName);
            }
            return result;
        }

        /// <summary>
        /// 获取进程所有状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetStates(Guid id)
        {

            var result = new Dictionary<string, string>();
            var states = WorkflowInit.Runtime.GetAvailableStateToSet(id);
            foreach (var state in states)
            {
                if (!result.ContainsKey(state.Name))
                    result.Add(state.Name, state.VisibleName);
            }
            return result;

        }

        /// <summary>
        /// 执行某个审批流程的命令
        /// </summary>
        /// <param name="id"></param>
        /// <param name="commandName"></param>
        /// <param name="document"></param>
        private void ExecuteCommand(Guid id, string commandName, DocumentModel document)
        {
            var currentUser = CurrentUserSettings.GetCurrentUser();

            if (commandName.Equals("SetState", StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(document.StateNameToSet))
                    return;

                WorkflowInit.Runtime.SetState(id, currentUser, currentUser, document.StateNameToSet, new Dictionary<string, object> { { "Comment", document.Comment } });
                return;
            }

            if (WorkflowInit.Runtime.GetCurrentStateName(id) == "Draft")
                WorkflowInit.Runtime.PreExecute(id);


            var commands = WorkflowInit.Runtime.GetAvailableCommands(id, currentUser);

            var command =
                commands.FirstOrDefault(
                    c => c.CommandName.Equals(commandName, StringComparison.CurrentCultureIgnoreCase));

            if (command == null)
                return;

            if (command.Parameters.Count(p => p.Name == "Comment") == 1)
                command.Parameters.Single(p => p.Name == "Comment").Value = document.Comment ?? string.Empty;

            WorkflowInit.Runtime.ExecuteCommand(id, currentUser, currentUser, command);
        }

        /// <summary>
        /// 如果对于公文处理的工作流还没有，则创建一个
        /// </summary>
        /// <param name="id"></param>
        private void CreateWorkflowIfNotExists(Guid id)
        {
            if (WorkflowInit.Runtime.IsProcessExists(id))
                return;

            using (var sync = new WorkflowSync(WorkflowInit.Runtime, id))
            {
                WorkflowInit.Runtime.CreateInstance("Document", id);

                sync.StatrtWaitingFor(new List<ProcessStatus> { ProcessStatus.Idled, ProcessStatus.Initialized });

                sync.Wait(new TimeSpan(0, 0, 10));
            }

            WorkflowInit.Runtime.PreExecute(id);
        }
        #endregion
    }
}
