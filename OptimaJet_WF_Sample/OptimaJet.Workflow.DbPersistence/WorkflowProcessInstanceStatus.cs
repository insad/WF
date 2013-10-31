using System;
using System.ComponentModel;
using System.Data.Linq.Mapping;
namespace OptimaJet.Workflow.DbPersistence
{
    [Table(Name = "dbo.WorkflowProcessInstanceStatus")]
    public class WorkflowProcessInstanceStatus : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
        private Guid _Id;
        private byte _Status;
        private Guid _Lock;
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
        [Column(Storage = "_Status", DbType = "TinyInt NOT NULL")]
        public byte Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                if (this._Status != value)
                {
                    this.SendPropertyChanging();
                    this._Status = value;
                    this.SendPropertyChanged("Status");
                }
            }
        }
        [Column(Storage = "_Lock", DbType = "UniqueIdentifier NOT NULL")]
        public Guid Lock
        {
            get
            {
                return this._Lock;
            }
            set
            {
                if (this._Lock != value)
                {
                    this.SendPropertyChanging();
                    this._Lock = value;
                    this.SendPropertyChanged("Lock");
                }
            }
        }
        protected virtual void SendPropertyChanging()
        {
            if (this.PropertyChanging != null)
            {
                this.PropertyChanging(this, WorkflowProcessInstanceStatus.emptyChangingEventArgs);
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
