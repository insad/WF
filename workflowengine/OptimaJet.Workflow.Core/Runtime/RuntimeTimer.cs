using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.Runtime
{
    public class RuntimeTimerEventArgs : EventArgs
    {
        public Guid ProcessId { get; set; }
        public string TimerName { get; set; }
    }

    [Serializable]
    public class TimerKey : IEqualityComparer
    {
        public Guid ProcessId { get; set; }
        public string TimerName { get; set; }

      
        public bool Equals(object x, object y)
        {
            var xKey = x as TimerKey;
            var yKey = y as TimerKey;

            if (xKey == null && yKey == null)
                return true;
            if (xKey == null || yKey == null)
                return false;

            return (xKey.ProcessId == yKey.ProcessId) && (xKey.TimerName == yKey.TimerName);
        }

        public int GetHashCode(object obj)
        {
            var key = obj as TimerKey;
            if (key == null)
                throw new NullReferenceException();
            var keyString = key.ProcessId.ToString() + key.TimerName;
            return keyString.ToLower().GetHashCode();
        }
    }

    [Serializable]
    public class TimerValue
    {
        public DateTime Date { get; set; }
        public bool IsInUse { get; set; } 
        [NonSerialized]
        public object Lock = new object();
    }

    //public class TimerKeyValue
    //{
    //    public 
    //}

    //TODO Убрать апдейт и сет оставить один метод
    public sealed class RuntimeTimer
    {
        public event EventHandler<RuntimeTimerEventArgs> TimerComplete;

        public event EventHandler<EventArgs> NeedSave;

        public IDictionary<TimerKey, DateTime> Timers { get; private set; }
        
        private ReaderWriterLock _lock = new ReaderWriterLock();

         private static int _lockTimeout = 600000;

        
        private Timer _timer;


        public bool IsCold { get; set; }

        public RuntimeTimer ()
        {
            _timer = new Timer(OnTimer);
            Timers = new Dictionary<TimerKey, DateTime>();
        }

        public RuntimeTimer(IDictionary<TimerKey,DateTime> timers)
        {
            _timer = new Timer(OnTimer);
            Timers = timers;
        }

        private void OnTimer(object state)
        {
            //try
            //{
            //    _timer.Change(new TimeSpan(0, 0, 0, 0, -1), new TimeSpan(0, 0, 0, 0, -1));
            //}
            //catch (Exception)
            //{
            //    (state as Timer).Dispose();
            //    Logger.Log.Fatal("FATAL TimerError Timer Disposed");
            //    return;
            //}

            var keysToRemove = new List<TimerKey>();
            _lock.AcquireReaderLock(_lockTimeout);
            try
            {
                
                foreach (var processTimerItem in Timers)
                {
                    if (processTimerItem.Value <= DateTime.UtcNow)
                    {
                        keysToRemove.Add(processTimerItem.Key);
                    }
                }

                LockCookie lc = _lock.UpgradeToWriterLock(_lockTimeout);

                try
                {
                    var error = false;

                    foreach (var runtimeTimerEventArgs in keysToRemove)
                    {
                        var args = new RuntimeTimerEventArgs
                                       {
                                           ProcessId = runtimeTimerEventArgs.ProcessId,
                                           TimerName = runtimeTimerEventArgs.TimerName
                                       };

                        try
                        {
                            var value = Timers[runtimeTimerEventArgs];
                            //ThreadPool.QueueUserWorkItem(, runtimeTimerEventArgs);
                            if (TimerComplete != null)
                                TimerComplete(this, args);
                            Timers.Remove(runtimeTimerEventArgs);
                        }
                        catch (Exception)
                        {
                            Timers[runtimeTimerEventArgs] = DateTime.UtcNow.AddMinutes(10);
                            error = true;
                        }

                  

                    }  
                    
                    //if (NeedSave != null)
                    //        NeedSave(this, EventArgs.Empty);

                    if (error)
                    {
                        throw new InvalidOperationException();
                    }
                }
                finally
                {
                    _lock.DowngradeFromWriterLock(ref lc);
                }

            }
            finally
            {
                _lock.ReleaseReaderLock();
            }

            RefreshTimer();
        }

        public void SetTimer (Guid processId, TimerDefinition timerDefinition)
        {
            if (timerDefinition.Type != TimerType.ByProcessInstance)
                throw new NotSupportedException();

            var intervalSeconds = (int)Math.Floor(timerDefinition.IntervalTimeInMilliseconds / (double)1000);
            var intervalMilliseconds = timerDefinition.IntervalTimeInMilliseconds - intervalSeconds * 1000;
            var delaySeconds = (int)Math.Floor(timerDefinition.DelayTimeInMilliseconds / (double)1000);
            var delayMilliseconds = timerDefinition.DelayTimeInMilliseconds - delaySeconds * 1000;
            var interval = new TimeSpan(0, 0, 0, intervalSeconds + delaySeconds,intervalMilliseconds + delayMilliseconds);
            SetTimer(processId, interval, timerDefinition.Name);
        }

        public void UpdateTimer (Guid processId, TimerDefinition timerDefinition)
        {
            if (timerDefinition.Type != TimerType.ByProcessInstance)
                throw new NotSupportedException();
            var intervalSeconds = (int)Math.Floor(timerDefinition.IntervalTimeInMilliseconds/(double)1000);
            var intervalMilliseconds = timerDefinition.IntervalTimeInMilliseconds - intervalSeconds*1000;
            var interval = new TimeSpan(0, 0, 0, intervalSeconds, intervalMilliseconds);
            SetTimer(processId, interval, timerDefinition.Name);
        }

        private void SetTimer (Guid processId, TimeSpan interval, string name)
        {
            _lock.AcquireWriterLock(_lockTimeout);
            try
            {
                IDictionary<string, DateTime> processTimers;
                var key = new TimerKey {ProcessId = processId, TimerName = name};

                if (Timers.ContainsKey(key))
                {
                    Timers[key] = DateTime.UtcNow.Add(interval);
                }
                else
                {
                    Timers.Add(key, DateTime.UtcNow.Add(interval));
                }

                if (NeedSave != null)
                    NeedSave(this, EventArgs.Empty);
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }

            RefreshTimer();
        }

        public  void RefreshTimer ()
        {
            if (IsCold)
                return;

            _lock.AcquireReaderLock(_lockTimeout);
            try
            {
                DateTime? minDateTime = null;
                foreach (var processTimerItem in Timers)
                {
                    if (!minDateTime.HasValue)
                        minDateTime = processTimerItem.Value;
                    else if (minDateTime.Value > processTimerItem.Value)
                        minDateTime = processTimerItem.Value;

                }

                if (minDateTime.HasValue)
                {
                    var interval = minDateTime.Value.Subtract(DateTime.UtcNow);
                    if (interval.TotalMilliseconds < 0)
                        interval = new TimeSpan(0, 0, 0, 0, 0);
                    _timer.Change(interval, new TimeSpan(0, 0, 0, 0, -1));
                }
                else
                {
                    _timer.Change(new TimeSpan(0, 0, 0, 0, -1), new TimeSpan(0, 0, 0, 0, -1));
                }
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }
    }
}
