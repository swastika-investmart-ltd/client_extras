using System;
using System.Collections.Generic;
using System.Linq;
namespace Client.WebApi
{
    public class TimePriorityQueue<T>
    {
        private class Entry
        {
            public T Value { get; set; }
            public DateTime Time { get; set; }
        }

        private Dictionary<string, Entry> data = new();
        private readonly int maxSize;

        public TimePriorityQueue(int maxSize = 5)
        {
            this.maxSize = maxSize;
        }

        public void AddOrUpdate(string key, T value, DateTime time)
        {
            data[key] = new Entry { Value = value, Time = time };
            TrimToLatest();
        }

        public List<T> GetData()
        {
            return data
                .OrderByDescending(kv => kv.Value.Time)
                .Take(maxSize)
                .Select(kv => kv.Value.Value)
                .ToList();
        }

        private void TrimToLatest()
        {
            if (data.Count <= maxSize) return;

            var keysToKeep = data
                .OrderByDescending(kv => kv.Value.Time)
                .Take(maxSize)
                .Select(kv => kv.Key)
                .ToHashSet();

            var keysToRemove = data.Keys.Except(keysToKeep).ToList();

            foreach (var key in keysToRemove)
            {
                data.Remove(key);
            }
        }
    }

}
