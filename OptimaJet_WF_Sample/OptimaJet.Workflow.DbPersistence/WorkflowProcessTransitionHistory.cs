using System;
using System.ComponentModel;
using System.Data.Linq.Mapping;
namespace OptimaJet.Workflow.DbPersistence
{
    /// <summary>
    /// WorkflowProcessTransitionHistory数据库表映射类
    /// </summary>
    [Table(Name = "dbo.WorkflowProcessTransitionHistory")]
    public class WorkflowProcessTransitionHistory : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
        private Guid _Id;
        private Guid _ProcessId;
        private Guid _ExecutorIdentityId;
        private Guid _ActorIdentityId;
        private string _FromActivityName;
        private string _ToActivityName;
        private string _ToStateName;
        private DateTime _TransitionTime;
        private string _TransitionClassifier;
        private bool _IsFinalised;
        private string _FromStateName;
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
        [Column(Storage = "_ExecutorIdentityId", DbType = "UniqueIdentifier NOT NULL")]
        public Guid ExecutorIdentityId
        {
            get
            {
                return this._ExecutorIdentityId;
            }
            set
            {
                if (this._ExecutorIdentityId != value)
                {
                    this.SendPropertyChanging();
                    this._ExecutorIdentityId = value;
                    this.SendPropertyChanged("ExecutorIdentityId");
                }
            }
        }
        [Column(Storage = "_ActorIdentityId", DbType = "UniqueIdentifier NOT NULL")]
        public Guid ActorIdentityId
        {
            get
            {
                return this._ActorIdentityId;
            }
            set
            {
                if (this._ActorIdentityId != value)
                {
                    this.SendPropertyChanging();
                    this._ActorIdentityId = value;
                    this.SendPropertyChanged("ActorIdentityId");
                }
            }
        }
        [Column(Storage = "_FromActivityName", DbType = "NVarChar(MAX) NOT NULL", CanBeNull = false)]
        public string FromActivityName
        {
            get
            {
                return this._FromActivityName;
            }
            set
            {
                if (this._FromActivityName != value)
                {
                    this.SendPropertyChanging();
                    this._FromActivityName = value;
                    this.SendPropertyChanged("FromActivityName");
                }
            }
        }
        [Column(Storage = "_ToActivityName", DbType = "NVarChar(MAX) NOT NULL", CanBeNull = false)]
        public string ToActivityName
        {
            get
            {
                return this._ToActivityName;
            }
            set
            {
                if (this._ToActivityName != value)
                {
                    this.SendPropertyChanging();
                    this._ToActivityName = value;
                    this.SendPropertyChanged("ToActivityName");
                }
            }
        }
        [Column(Storage = "_ToStateName", DbType = "NVarChar(MAX)")]
        public string ToStateName
        {
            get
            {
                return this._ToStateName;
            }
            set
            {
                if (this._ToStateName != value)
                {
                    this.SendPropertyChanging();
                    this._ToStateName = value;
                    this.SendPropertyChanged("ToStateName");
                }
            }
        }
        [Column(Storage = "_TransitionTime", DbType = "DateTime NOT NULL")]
        public DateTime TransitionTime
        {
            get
            {
                return this._TransitionTime;
            }
            set
            {
                if (this._TransitionTime != value)
                {
                    this.SendPropertyChanging();
                    this._TransitionTime = value;
                    this.SendPropertyChanged("TransitionTime");
                }
            }
        }
        [Column(Storage = "_TransitionClassifier", DbType = "NVarChar(MAX) NOT NULL", CanBeNull = false)]
        public string TransitionClassifier
        {
            get
            {
                return this._TransitionClassifier;
            }
            set
            {
                if (this._TransitionClassifier != value)
                {
                    this.SendPropertyChanging();
                    this._TransitionClassifier = value;
                    this.SendPropertyChanged("TransitionClassifier");
                }
            }
        }
        [Column(Storage = "_IsFinalised", DbType = "Bit NOT NULL")]
        public bool IsFinalised
        {
            get
            {
                return this._IsFinalised;
            }
            set
            {
                if (this._IsFinalised != value)
                {
                    this.SendPropertyChanging();
                    this._IsFinalised = value;
                    this.SendPropertyChanged("IsFinalised");
                }
            }
        }
        [Column(Storage = "_FromStateName", DbType = "NVarChar(MAX)")]
        public string FromStateName
        {
            get
            {
                return this._FromStateName;
            }
            set
            {
                if (this._FromStateName != value)
                {
                    this.SendPropertyChanging();
                    this._FromStateName = value;
                    this.SendPropertyChanged("FromStateName");
                }
            }
        }
        protected virtual void SendPropertyChanging()
        {
            if (this.PropertyChanging != null)
            {
                this.PropertyChanging(this, WorkflowProcessTransitionHistory.emptyChangingEventArgs);
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
