using FCloud3.Entities.Sys;
using FCloud3.Repos.Sys;
using System.Collections.Concurrent;

namespace FCloud3.Services.Etc.Cache.Base
{
    public abstract class NewestEnsuredCacheHost<T>(
        LastUpdateRepo lastUpdateRepo)
    {
        protected static ConcurrentDictionary<string, TimedValue> Dict { get; } = [];
        protected void Set(string key, T value)
        {
            TimedValue tv = new(DateTime.Now, value);
            Dict.AddOrUpdate(key, tv, (_, _) => tv);
        }
        protected bool TryGet(string key, out T? value, bool checkNewest = true)
        {
            if (checkNewest)
            {
                var lastUpdate = lastUpdateRepo.GetLastUpdateFor(LuTypes);
                var expired = Dict
                    .Where(kvp => kvp.Value.Time < lastUpdate)
                    .Select(x => x.Key)
                    .ToList();
                expired.ForEach(x => Dict.TryRemove(x, out _));
            }
            if (Dict.TryGetValue(key, out var timedValue))
            {
                value = timedValue.Value;
                return true;
            }
            value = default;
            return false;
        }
        protected readonly struct TimedValue(DateTime time, T value)
        {
            public DateTime Time { get; } = time;
            public T Value { get; } = value;
        }
        protected abstract LastUpdateType[] LuTypes { get; }

        public void Clear()
        {
            Dict.Clear();
        }
    }
}
