using System;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.Persistence
{
    /// <summary>
    /// Provides save and load of workflow runtime parameters. as timers etc
    /// </summary>
    public interface IRuntimePersistence
    {
        /// <summary>
        /// Provides saving of timers in runtime with runtimeId
        /// </summary>
        /// <param name="runtimeId">Id of runtime</param>
        /// <param name="timer">Runtime timer to save</param>
        void SaveTimer(Guid runtimeId, RuntimeTimer timer);
        /// <summary>
        /// Provides loading of timers from persistence store
        /// </summary>
        /// <param name="runtimeId">Id of runtime</param>
        /// <returns>Timer that was saved in persistence store or new empty timer</returns>
        RuntimeTimer LoadTimer(Guid runtimeId);
    }
}
