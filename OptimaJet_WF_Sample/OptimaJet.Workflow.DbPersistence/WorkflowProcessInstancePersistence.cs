using System;
using System.ComponentModel;
using System.Data.Linq.Mapping;
namespace OptimaJet.Workflow.DbPersistence
{
    /// <summary>
    /// WorkflowProcessInstance的数据表映射类
    /// </summary>
    [Table(Name = "dbo.WorkflowProcessInstancePersistence")]
    public class WorkflowProcessInstancePersistence : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
        private Guid _Id;
        private Guid _ProcessId;
        private string _ParameterName;
        private string _Value;
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
        [Column(Storage = "_Id", DbType = "UniqueIdentifier NOT NULL", IsPrimaryKey = true)]
        public Guid Id
        {
            get
            {
                return this._Id;
            }
            set
            {
                if (this._Id != value)
                {
                    this.SendPropertyChanging();
                    this._Id = value;
                    this.SendPropertyChanged("Id");
                }
            }
        }
        [Column(Storage = "_ProcessId", DbType = "UniqueIdentifier NOT NULL")]
        public Guid ProcessId
        {
            get
            {
                return this._ProcessId;
            }
            set
            {
                if (this._ProcessId != value)
                {
                    this.SendPropertyChanging();
                    this._ProcessId = value;
                    this.SendPropertyChanged("ProcessId");
                }
            }
        }
        [Column(Storage = "_ParameterName", DbType = "NVarChar(MAX) NOT NULL", CanBeNull = false)]
        public string ParameterName
        {
            get
            {
                return this._ParameterName;
            }
            set
            {
                if (this._ParameterName != value)
                {
                    this.SendPropertyChanging();
                    this._ParameterName = value;
                    this.SendPropertyChanged("ParameterName");
                }
            }
        }
        [Column(Storage = "_Value", DbType = "NText NOT NULL", CanBeNull = false, UpdateCheck = UpdateCheck.Never)]
        public string Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                if (this._Value != value)
                {
                    this.SendPropertyChanging();
                    this._Value = value;
                    this.SendPropertyChanged("Value");
                }
            }
        }
        protected virtual void SendPropertyChanging()
        {
            if (this.PropertyChanging != null)
            {
                this.PropertyChanging(this, WorkflowProcessInstancePersistence.emptyChangingEventArgs);
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
