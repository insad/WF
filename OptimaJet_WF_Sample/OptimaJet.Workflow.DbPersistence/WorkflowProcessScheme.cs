using System;
using System.ComponentModel;
using System.Data.Linq.Mapping;
namespace OptimaJet.Workflow.DbPersistence
{
    /// <summary>
    /// WorkflowProcessScheme数据库表映射类：可以理解为基于 WorkflowScheme工作流模板类创建的工作流处理模板实例
    ///  如：《建设项目选址意见书》 这个相当于一个类。WorkflowProcessInstance这相当于类中的实例
    /// </summary>
    [Table(Name = "dbo.WorkflowProcessScheme")]
    public class WorkflowProcessScheme : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
        private Guid _Id;
        private string _Scheme;
        private string _DefiningParameters;
        private string _DefiningParametersHash;
        private string _ProcessName;
        private bool _IsObsolete;
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
        [Column(Storage = "_Scheme", DbType = "NText NOT NULL", CanBeNull = false, UpdateCheck = UpdateCheck.Never)]
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
        [Column(Storage = "_DefiningParameters", DbType = "NText NOT NULL", CanBeNull = false, UpdateCheck = UpdateCheck.Never)]
        public string DefiningParameters
        {
            get
            {
                return this._DefiningParameters;
            }
            set
            {
                if (this._DefiningParameters != value)
                {
                    this.SendPropertyChanging();
                    this._DefiningParameters = value;
                    this.SendPropertyChanged("DefiningParameters");
                }
            }
        }
        [Column(Storage = "_DefiningParametersHash", DbType = "NVarChar(1024) NOT NULL", CanBeNull = false)]
        public string DefiningParametersHash
        {
            get
            {
                return this._DefiningParametersHash;
            }
            set
            {
                if (this._DefiningParametersHash != value)
                {
                    this.SendPropertyChanging();
                    this._DefiningParametersHash = value;
                    this.SendPropertyChanged("DefiningParametersHash");
                }
            }
        }
        [Column(Storage = "_ProcessName", DbType = "NVarChar(MAX) NOT NULL", CanBeNull = false)]
        public string ProcessName
        {
            get
            {
                return this._ProcessName;
            }
            set
            {
                if (this._ProcessName != value)
                {
                    this.SendPropertyChanging();
                    this._ProcessName = value;
                    this.SendPropertyChanged("ProcessName");
                }
            }
        }
        [Column(Storage = "_IsObsolete", DbType = "Bit NOT NULL")]
        public bool IsObsolete
        {
            get
            {
                return this._IsObsolete;
            }
            set
            {
                if (this._IsObsolete != value)
                {
                    this.SendPropertyChanging();
                    this._IsObsolete = value;
                    this.SendPropertyChanged("IsObsolete");
                }
            }
        }
        protected virtual void SendPropertyChanging()
        {
            if (this.PropertyChanging != null)
            {
                this.PropertyChanging(this, WorkflowProcessScheme.emptyChangingEventArgs);
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
