using Neo.SmartContract.Framework;

namespace DataStructure
{
    public partial class DataStructure
    {
        /// <summary>
        /// Check if
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        private static void Assert(bool condition, string message, object data = null)
        {
            if (!condition)
            {
                onFault(message, data);
                ExecutionEngine.Abort();
            }
        }
    }
}
