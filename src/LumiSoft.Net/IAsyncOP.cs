using System;

namespace LumiSoft.Net
{
    /// <summary>
    /// Represents asynchronous operation.
    /// </summary>
    public interface IAsyncOp
    {
        /// <summary>
        /// Gets asynchronous operation state.
        /// </summary>
        AsyncOpState State { get; }

        /// <summary>
        /// Gets error happened during operation. Returns null if no error.
        /// </summary>
        Exception Error { get; }
    }
}