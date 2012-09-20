using System;
using System.Threading;

namespace ThrottedObject
{
    public class ThrottledObject<T>
    {
        private readonly Func<T> _updateAction;
        private readonly TimeSpan _timeOut;
        
        private DateTime _lastUpdateTime;
        private DateTime LastUpdateTime
        {
            get
            {
                lock (_lock)
                {
                    return _lastUpdateTime;
                }
            }
            set
            {
                lock (_lock)
                {
                    _lastUpdateTime = value;
                }
            }
        }

        private object _lock = new object();

        private T _cache;
        private T Cache
        {
            get
            {
                lock(_lock)
                {
                    return _cache;
                }
            }
            set
            {
                lock(_lock)
                {
                    _cache = value;
                }
            }
        }

        public ThrottledObject(TimeSpan timeout, Func<T> updateAction)
        {
            _timeOut = timeout;
            _updateAction = updateAction;

            UpdateItem();
        }

        private void UpdateItem()
        {
            lock(_lock)
            {
                var item = _updateAction();
                if(item != null)
                {
                    Cache = item;
                    LastUpdateTime = DateTime.Now;
                }
            }
        }

        public T Item
        {
            get
            {
                if(CanUpdate)
                {
                    UpdateItem();
                    return Cache;
                }
                return Cache;
            }
        }

        protected bool CanUpdate
        {
            get
            {
                lock(_lock)
                {
                    return DateTime.Now - LastUpdateTime > _timeOut;
                }
            }
        }

        public T LazyGetItem
        {
            get
            {
                if (CanUpdate)
                {
                    ThreadUtil.SafeQueueUserWorkItem(UpdateItem);
                }
                
                return Cache;
            }
        }

        public void LazyUpdate()
        {
            if (CanUpdate)
            {
                ThreadUtil.SafeQueueUserWorkItem(UpdateItem);
            }
        }
    }

    public static class ThreadUtil
    {
        public static bool SafeQueueUserWorkItem(Action callback)
        {
            return ThreadPool.QueueUserWorkItem(innerState =>
            {
                try
                {
                    callback();
                }
                catch (Exception ex)
                {
                    Console.Write("Exception in thread pool thread {0}", ex);
                }
            });
        }
    }
}
