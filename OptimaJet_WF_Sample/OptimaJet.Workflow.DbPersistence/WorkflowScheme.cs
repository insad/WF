using System;
using System.ComponentModel;
using System.Data.Linq.Mapping;
namespace OptimaJet.Workflow.DbPersistence
{
    /// <summary>
    /// WorkflowScheme数据库表映射类
    /// 用于保存整个流程的相应信息,自定义流程就需要在改表中完成
    ///WorkflowScheme工作流模板类
    /// </summary>
    [Table(Name = "dbo.WorkflowScheme")]
    public class WorkflowScheme : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
        private string _Code;
        private string _Scheme;
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
        [Column(Storage = "_Code", DbType = "NVarChar(256) NOT NULL", CanBeNull = false, IsPrimaryKey = true)]
        public string Code
        {
            get
            {
                return this._Code;
            }
            set
            {
                if (this._Code != value)
                {
                    this.SendPropertyChanging();
                    this._Code = value;
                    this.SendPropertyChanged("Code");
                }
            }
        }
        [Column(Storage = "_Scheme", DbType = "NVarChar(MAX) NOT NULL", CanBeNull = false)]
        public string Scheme
        {
            get
            {
                return this._Scheme;
            }
            set
            {
                if (this._Scheme != value)
                {
                    this.SendPropertyChanging();
                    this._Scheme = value;
                    this.SendPropertyChanged("Scheme");
                }
            }
        }
        protected virtual void SendPropertyChanging()
        {
            if (this.PropertyChanging != null)
            {
                this.PropertyChanging(this, WorkflowScheme.emptyChangingEventArgs);
            }
        }
        protected virtual void SendPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
