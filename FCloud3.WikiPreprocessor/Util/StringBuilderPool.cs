using System.Text;

namespace FCloud3.WikiPreprocessor.Util
{
    /// <summary>
    /// StringBuilder 线程静态池，用于减少解析过程中的 StringBuilder 分配
    /// </summary>
    internal static class StringBuilderPool
    {
        [ThreadStatic]
        private static StringBuilder? _cached;

        /// <summary>
        /// 获取一个 StringBuilder。优先返回线程缓存的实例，否则创建新的。
        /// </summary>
        public static StringBuilder Rent()
        {
            var sb = _cached;
            if (sb is null)
                return new StringBuilder(4096);
            _cached = null;
            return sb;
        }

        /// <summary>
        /// 归还 StringBuilder 到线程缓存。调用后不应再持有该实例的引用。
        /// </summary>
        public static void Return(StringBuilder sb)
        {
            sb.Clear();
            if (sb.Capacity <= 16384)
                _cached = sb;
        }
    }
}
