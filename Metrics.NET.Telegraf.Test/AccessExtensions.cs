namespace Metrics.NET.Telegraf.Test
{
    internal static class AccessExtensions
    {
        /// <summary>
        /// Invokes a non-public method with specified arguments.
        /// </summary>
        /// <param name="obj">This.</param>
        /// <param name="methodName">Target method.</param>
        /// <param name="args">Method arguments.</param>
        /// <returns>Value, returned by the method, if the specified method was found, otherwise null.</returns>
        public static object call(this object obj, string methodName, params object[] args)
        {
            var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                return method.Invoke(obj, args);
            }
            return null;
        }
    }
}
