using System.Collections.Generic;

namespace OptimaJet.Common
{
    public class MultiThreadedPersistance<TItem>
    {
        private readonly List<TItem> _items = new List<TItem>();

        private volatile object _lock = new object();

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _items.Count;
                }
            }
        }

        public IEnumerable<TItem> Items
        {
            get
            {
                lock (_lock)
                {
                    foreach (TItem item in _items)
                        yield return item;
                }
            }
        }

        public void AddItem(TItem item)
        {
            lock (_lock)
            {
                _items.Add(item);
            }
        }
    }
}