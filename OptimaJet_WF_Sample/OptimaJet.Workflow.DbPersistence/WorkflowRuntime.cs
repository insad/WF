using System;
using System.ComponentModel;
using System.Data.Linq.Mapping;
namespace OptimaJet.Workflow.DbPersistence
{
    /// <summary>
    /// WorkflowRuntime数据库表映射类
    /// </summary>
    [Table(Name = "dbo.WorkflowRuntime")]
    public class WorkflowRuntime : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
        private Guid _RuntimeId;
        private string _Timer;
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
        [Column(Storage = "_RuntimeId", DbType = "UniqueIdentifier NOT NULL", IsPrimaryKey = true)]
        public Guid RuntimeId
        {
            get
            {
                return this._RuntimeId;
            }
            set
            {
                if (this._RuntimeId != value)
                {
                    this.SendPropertyChanging();
                    this._RuntimeId = value;
                    this.SendPropertyChanged("RuntimeId");
                }
            }
        }
        [Column(Storage = "_Timer", DbType = "NVarChar(MAX) NOT NULL", CanBeNull = false)]
        public string Timer
        {
            get
            {
                return this._Timer;
            }
            set
            {
                if (this._Timer != value)
                {
                    this.SendPropertyChanging();
                    this._Timer = value;
                    this.SendPropertyChanged("Timer");
                }
            }
        }
        protected virtual void SendPropertyChanging()
        {
            if (this.PropertyChanging != null)
            {
                this.PropertyChanging(this, WorkflowRuntime.emptyChangingEventArgs);
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
