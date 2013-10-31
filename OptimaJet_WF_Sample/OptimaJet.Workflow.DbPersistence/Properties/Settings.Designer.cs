using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace OptimaJet.Workflow.DbPersistence.Properties
{
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0"), CompilerGenerated]
    internal sealed class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());
        public static Settings Default
        {
            get
            {
                return Settings.defaultInstance;
            }
        }
        [ApplicationScopedSetting, DefaultSettingValue("Data Source=(local);Initial Catalog=Budget2;Integrated Security=True"), SpecialSetting(SpecialSetting.ConnectionString), DebuggerNonUserCode]
        public string Budget2ConnectionString
        {
            get
            {
                return (string)this["Budget2ConnectionString"];
            }
        }
        [ApplicationScopedSetting, DefaultSettingValue("Data Source=(local);Initial Catalog=BudgetNewWorkflow;Integrated Security=True"), SpecialSetting(SpecialSetting.ConnectionString), DebuggerNonUserCode]
        public string BudgetNewWorkflowConnectionString
        {
            get
            {
                return (string)this["BudgetNewWorkflowConnectionString"];
            }
        }
    }
}
