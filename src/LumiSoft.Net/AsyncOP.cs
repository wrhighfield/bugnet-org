﻿namespace LumiSoft.Net
{
    /// <summary>
    /// This is base class for asynchronous operation.
    /// </summary>
    public abstract class AsyncOp
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected AsyncOp()
        {
        }

        #region Properties implementation

        /// <summary>
        /// Gets if this object is disposed.
        /// </summary>
        public virtual bool IsDisposed { get; }

        /// <summary>
        /// Gets if asynchronous operation has completed.
        /// </summary>
        public abstract bool IsCompleted { get; }

        /// <summary>
        /// Gets if operation completed synchronously.
        /// </summary>
        public abstract bool IsCompletedSynchronously { get; }

        #endregion
    }
}