using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WF.Sample.Business;

namespace WF.Sample.Models
{
    /// <summary>
    /// 文档Document的视图数据模型ViewModel
    /// </summary>
    public class DocumentModel
    {
        public Guid Id { get; set; }
        public int? Number { get; set; }

        [Required]
        [StringLength(256)]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Comment")]
        public string Comment { get; set; }
        
        public Guid AuthorId { get; set; }
        
        [DataType(DataType.Text)]
        [Display(Name = "Author")]
        public string AuthorName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Controller")]
        public Guid? EmloyeeControlerId { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Controller")]
        public string EmloyeeControlerName { get; set; }

        [Display(Name = "State")]
        public string StateName { get; set; }

        [Display(Name = "Sum")]
        public decimal Sum { get; set; }

        public Dictionary<string, string> Commands { get; set; }

        public Dictionary<string, string> AvailiableStates { get; set; }

        public DocumentModel ()
        {
            Commands = new Dictionary<string, string>{};
            AvailiableStates = new Dictionary<string, string>{};
            HistoryModel = new DocumentHistoryModel();
        }

        public string StateNameToSet { get; set; }

        public DocumentHistoryModel HistoryModel { get; set; }
    }
}