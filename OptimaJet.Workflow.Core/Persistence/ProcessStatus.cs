using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Persistence
{
    public sealed class ProcessStatus
    {
        public byte Id { get; private set; }
        public bool IsAllowedToChangeStatus { get; set; }
        public bool IsAllowedToExecuteCommand { get; set; }

        public static readonly ProcessStatus NotFound = new ProcessStatus
        {
            Id = 255,
            IsAllowedToChangeStatus = false,
            IsAllowedToExecuteCommand = false
        };

        public static readonly ProcessStatus Unknown = new ProcessStatus
        {
            Id = 254,
            IsAllowedToChangeStatus = false,
            IsAllowedToExecuteCommand = false
        };

        public static readonly ProcessStatus Initialized = new ProcessStatus
                                                                {
                                                                    Id = 0,
                                                                    IsAllowedToChangeStatus = false,
                                                                    IsAllowedToExecuteCommand = false
                                                                };

        public static readonly ProcessStatus Running = new ProcessStatus
                                                            {
                                                                Id = 1,
                                                                IsAllowedToChangeStatus = false,
                                                                IsAllowedToExecuteCommand = false
                                                            };

        public static readonly ProcessStatus Idled = new ProcessStatus
                                                          {
                                                              Id = 2,
                                                              IsAllowedToChangeStatus = true,
                                                              IsAllowedToExecuteCommand = true
                                                          };

        public static readonly ProcessStatus Finalized = new ProcessStatus
        {
            Id = 3,
            IsAllowedToChangeStatus = true,
            IsAllowedToExecuteCommand = false
        };

        public static readonly ProcessStatus Terminated = new ProcessStatus
        {
            Id = 4,
            IsAllowedToChangeStatus = false,
            IsAllowedToExecuteCommand = false
        };

        public static readonly IEnumerable<ProcessStatus> All = new List<ProcessStatus>
                                                                     {Initialized, Running, Idled, Finalized, Terminated};
    }
}
