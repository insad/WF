using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using WF.Sample.Business.Properties;

namespace WF.Sample.Business.Helpers
{

    /// <summary>
    /// 流转公文管理：负责查询公文
    /// </summary>
    public class DocumentHelper
    {
        /// <summary>
        /// 获取所有Document记录
        /// </summary>
        /// <returns></returns>
        public static List<Document> GetAll()
        {
            using (var context = new DataModelDataContext())
            {
                context.LoadOptions = GetDefaultDataLoadOptions();
                return context.Documents.OrderBy(c => c.Number).ToList();
            }
        }

        /// <summary>
        /// 根据DocumentId获取所有DocumentTransitionHistory记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public static List<DocumentTransitionHistory> GetHistory(Guid id)
        {
            using (var context = new DataModelDataContext())
            {
                context.LoadOptions = GetDefaultDataLoadOptions();
                return context.DocumentTransitionHistories.Where(h=>h.DocumentId == id).OrderBy(h=>h.TransitionTimeForSort).ThenBy(h=>h.Order).ToList();
            }
        }

        //根据id获取指定文档
        public static Document Get(Guid id)
        {
            using (var context = new DataModelDataContext())
            {
                return Get(id, context);
            }
        }

        public static Document Get(Guid id, DataModelDataContext context)
        {
            context.LoadOptions = GetDefaultDataLoadOptions();
            return context.Documents.OrderBy(c => c.Number).FirstOrDefault(c => c.Id == id);
        }

       

        private static DataLoadOptions GetDefaultDataLoadOptions()
        {
            var lo = new DataLoadOptions();
            lo.LoadWith<Document>(c => c.Employee);
            lo.LoadWith<Document>(c => c.Employee1);
            lo.LoadWith<DocumentTransitionHistory>(c=>c.Employee);
            return lo;
        }

        //删除文档
        public static void Delete(Guid[] ids)
        {
            using (var context = new DataModelDataContext())
            {
                var objs =
                    (from item in context.Documents where ids.Contains(item.Id) select item).ToList();

                foreach (Document item in objs)
                {
                    context.Documents.DeleteOnSubmit(item);
                }

                context.SubmitChanges();
            }
        }
    }
}
