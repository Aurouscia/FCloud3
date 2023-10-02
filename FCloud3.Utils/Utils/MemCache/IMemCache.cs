using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Utils.Utils.MemCache
{
    public interface IMemCache<T>
    {
        public bool Exists(string key);
        public void Set(string key, T value, int expireSec);
        public T? Get(string key);
        public T? Get(string key, T defaultValue);
        public IDictionary<string, T?> GetAll(IEnumerable<string> keys);
        public void Remove(string key);
        public void RemoveAll(IEnumerable<string> keys);
        public void RemoveAll();
    }
}
