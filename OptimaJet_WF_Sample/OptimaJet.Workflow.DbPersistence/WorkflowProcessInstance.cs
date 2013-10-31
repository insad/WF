using System;
using System.ComponentModel;
using System.Data.Linq.Mapping;
namespace OptimaJet.Workflow.DbPersistence
{
    /// <summary>
    /// WorkflowProcessInstance数据库表映射类
    /// 相当于WorkflowScheme（类）的实例,如具体的那个项目的《建设项目选址意见书》
    /// </summary>
    [Table(Name = "dbo.WorkflowProcessInstance")]
    public class WorkflowProcessInstance : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
        private Guid _Id;
        private string _StateName;
        private string _ActivityName;
        private Guid? _SchemeId;
        private string _PreviousState;
        private string _PreviousStateForDirect;
        private string _PreviousStateForReverse;
        private string _PreviousActivity;
        private string _PreviousActivityForDirect;
        private string _PreviousActivityForReverse;
        private bool _IsDeterminingParametersChanged;
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
        [Column(Storage = "_StateName", DbType = "NVarChar(MAX) NOT NULL", CanBeNull = false)]
        public string StateName
        {
            get
            {
                return this._StateName;
            }
            set
            {
                if (this._StateName != value)
                {
                    this.SendPropertyChanging();
                    this._StateName = value;
                    this.SendPropertyChanged("StateName");
                }
            }
        }
        [Column(Storage = "_ActivityName", DbType = "NVarChar(MAX) NOT NULL", CanBeNull = false)]
        public string ActivityName
        {
            get
            {
                return this._ActivityName;
            }
            set
            {
                if (this._ActivityName != value)
                {
                    this.SendPropertyChanging();
                    this._ActivityName = value;
                    this.SendPropertyChanged("ActivityName");
                }
            }
        }
        [Column(Storage = "_SchemeId", DbType = "UniqueIdentifier")]
        public Guid? SchemeId
        {
            get
            {
                return this._SchemeId;
            }
            set
            {
                if (this._SchemeId != value)
                {
                    this.SendPropertyChanging();
                    this._SchemeId = value;
                    this.SendPropertyChanged("SchemeId");
                }
            }
        }
        [Column(Storage = "_PreviousState", DbType = "NVarChar(MAX)")]
        public string PreviousState
        {
            get
            {
                return this._PreviousState;
            }
            set
            {
                if (this._PreviousState != value)
                {
                    this.SendPropertyChanging();
                    this._PreviousState = value;
                    this.SendPropertyChanged("PreviousState");
                }
            }
        }
        [Column(Storage = "_PreviousStateForDirect", DbType = "NVarChar(MAX)")]
        public string PreviousStateForDirect
        {
            get
            {
                return this._PreviousStateForDirect;
            }
            set
            {
                if (this._PreviousStateForDirect != value)
                {
                    this.SendPropertyChanging();
                    this._PreviousStateForDirect = value;
                    this.SendPropertyChanged("PreviousStateForDirect");
                }
            }
        }
        [Column(Storage = "_PreviousStateForReverse", DbType = "NVarChar(MAX)")]
        public string PreviousStateForReverse
        {
            get
            {
                return this._PreviousStateForReverse;
            }
            set
            {
                if (this._PreviousStateForReverse != value)
                {
                    this.SendPropertyChanging();
                    this._PreviousStateForReverse = value;
                    this.SendPropertyChanged("PreviousStateForReverse");
                }
            }
        }
        [Column(Storage = "_PreviousActivity", DbType = "NVarChar(MAX)")]
        public string PreviousActivity
        {
            get
            {
                return this._PreviousActivity;
            }
            set
            {
                if (this._PreviousActivity != value)
                {
                    this.SendPropertyChanging();
                    this._PreviousActivity = value;
                    this.SendPropertyChanged("PreviousActivity");
                }
            }
        }
        [Column(Storage = "_PreviousActivityForDirect", DbType = "NVarChar(MAX)")]
        public string PreviousActivityForDirect
        {
            get
            {
                return this._PreviousActivityForDirect;
            }
            set
            {
                if (this._PreviousActivityForDirect != value)
                {
                    this.SendPropertyChanging();
                    this._PreviousActivityForDirect = value;
                    this.SendPropertyChanged("PreviousActivityForDirect");
                }
            }
        }
        [Column(Storage = "_PreviousActivityForReverse", DbType = "NVarChar(MAX)")]
        public string PreviousActivityForReverse
        {
            get
            {
                return this._PreviousActivityForReverse;
            }
            set
            {
                if (this._PreviousActivityForReverse != value)
                {
                    this.SendPropertyChanging();
                    this._PreviousActivityForReverse = value;
                    this.SendPropertyChanged("PreviousActivityForReverse");
                }
            }
        }
        [Column(Storage = "_IsDeterminingParametersChanged", DbType = "Bit NOT NULL")]
        public bool IsDeterminingParametersChanged
        {
            get
            {
                return this._IsDeterminingParametersChanged;
            }
            set
            {
                if (this._IsDeterminingParametersChanged != value)
                {
                    this.SendPropertyChanging();
                    this._IsDeterminingParametersChanged = value;
                    this.SendPropertyChanged("IsDeterminingParametersChanged");
                }
            }
        }
        protected virtual void SendPropertyChanging()
        {
            if (this.PropertyChanging != null)
            {
                this.PropertyChanging(this, WorkflowProcessInstance.emptyChangingEventArgs);
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
