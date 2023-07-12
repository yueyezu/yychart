/*
 * 异步请求的管理类，通过继承SYSTEM.IAsyncResult来实现异步。
 * **/

using System;
using System.Web;
using System.Threading;

namespace Commen.ChatCore
{
    /// <summary>
    /// An Implementation of IAsyncResult that enables the use of the custom thread pool
    /// and enables us to use the IHttpAsyncHandler implementation
    /// </summary>
    public class CometAsyncResult : IAsyncResult
    {
        private AsyncCallback callback;
        private object asyncState;
        private bool isCompleted = false;
        private CometMessage[] messages;
        private HttpContext context;

        /// <summary>
        /// Construct a new instance of CometAsyncResult
        /// </summary>
        /// <param name="context">The HTTP Context passed in from the handler</param>
        /// <param name="callback">The AsyncCallback passed in from the handler</param>
        /// <param name="asyncState">The extra data passed in from the handler</param>
        public CometAsyncResult(HttpContext context, AsyncCallback callback, object asyncState)
        {
            this.callback = callback;
            this.asyncState = asyncState;
            this.context = context;
        }

        #region IAsyncResult Members

        /// <summary>
        /// Gets or Sets the extra data associated with this async operation
        /// </summary>
        public object AsyncState
        {
            get { return this.asyncState; }
        }

        /// <summary>
        /// Not Implemented: will throw InvalidOperationException("ASP.NET Should never use this property"); }
        /// </summary>
        public WaitHandle AsyncWaitHandle
        {
            get { throw new InvalidOperationException("ASP.NET Should never use this property"); }
        }

        /// <summary>
        /// Gets a boolean indicating if the operation completed synchronously (always returns false)
        /// </summary>
        public bool CompletedSynchronously
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a boolean indicating if the operation has completed
        /// </summary>
        public bool IsCompleted
        {
            get { return this.isCompleted; }
        }

        /// <summary>
        /// Gets the HttpContext associaetd with this async operation
        /// </summary>
        public HttpContext Context
        {
            get { return this.context; }
        }

        #endregion

        /// <summary>
        /// Gets the Messages that are to be returned upon completion of this Async Operation
        /// </summary>
        public CometMessage[] CometMessages
        {
            get { return this.messages; }
            set { this.messages = value; }
        }

        /// <summary>
        /// Signal this operation has completed
        /// </summary>
        internal void SetCompleted()
        {
            this.isCompleted = true;

            if (callback != null)
                callback(this);
        }
    }
}
