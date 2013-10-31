using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimaJet.Workflow.Core.Model
{
    public sealed class TimerDefinition : BaseDefinition
    {
        public TimerType Type { get; internal set; }
        public int DelayTimeInMilliseconds { get; internal set; }
        public int IntervalTimeInMilliseconds { get; internal set; }

        public static TimerDefinition Create(string name, string type, string delay, string interval)
        {
            TimerType parsedType;
            Enum.TryParse(type, true, out parsedType);

            int delayTimeInMilliseconds;
            if (!int.TryParse(delay, out delayTimeInMilliseconds))
            {
                throw new InvalidOperationException();
            }

            int intervalTimeInMilliseconds;
            if (!int.TryParse(interval, out intervalTimeInMilliseconds))
            {
                throw new InvalidOperationException();
            }


            return new TimerDefinition { Name = name, IntervalTimeInMilliseconds = intervalTimeInMilliseconds, DelayTimeInMilliseconds = delayTimeInMilliseconds, Type = parsedType };
        }
    }
}
