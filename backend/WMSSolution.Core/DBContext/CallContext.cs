using System.Collections.Concurrent;

namespace WMSSolution.Core.DBContext
{
    /// <summary>
    /// Current connection context
    /// </summary>
    public static class CallContext
    {
        /// <summary>
        /// Static dictionary
        /// </summary>
        static ConcurrentDictionary<string, AsyncLocal<object>> state = new ConcurrentDictionary<string, AsyncLocal<object>>();
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="name">Key</param>
        /// <param name="data">Value</param>
        public static void SetData(string name, object data) =>
            state.GetOrAdd(name, _ => new AsyncLocal<object>()).Value = data;
        /// <summary>
        /// Get value
        /// </summary>
        /// <param name="name">Key</param>
        /// <returns></returns>
        public static object GetData(string name) =>
            state.TryGetValue(name, out AsyncLocal<object> data) ? data.Value : null;
    }
}
