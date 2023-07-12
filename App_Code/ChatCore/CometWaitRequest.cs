/*
 * 服务器端线程，监听客户端的message请求。
 * **/
using System;
using System.Web;

namespace Commen.ChatCore
{
    /// <summary>
    /// Class CometWaitRequest
    /// 
    /// This class contains all the information required when queue the request on a specific CometWaitThread
    /// </summary>
    public class CometWaitRequest
    {
        private CometAsyncResult result;
        private DateTime dateTimeAdded = DateTime.Now;
        private string clientPrivateToken;
        private long lastMessageId;
        private DateTime? dateDeactivated = null;

        /// <summary>
        /// Construct a new instance of a CometWaitRequest object
        /// </summary>
        /// <param name="clientPrivateToken"></param>
        /// <param name="lastMessageId"></param>
        /// <param name="context"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public CometWaitRequest(string clientPrivateToken, long lastMessageId, HttpContext context, AsyncCallback callback, object state)
        {
            this.clientPrivateToken = clientPrivateToken;
            this.lastMessageId = lastMessageId;
            this.result = new CometAsyncResult(context, callback, state);
        }

        /// <summary>
        /// Gets the CometAsyncResult object associated with this CometWaitRequest
        /// </summary>
        public CometAsyncResult Result
        {
            get { return this.result; }
        }

        /// <summary>
        /// Gets the Date and time this request was added, so the system knows when to time it out
        /// </summary>
        public DateTime DateTimeAdded
        {
            get { return this.dateTimeAdded; }
        }

        /// <summary>
        /// Gets the private token of the client that is connected to this wait request
        /// </summary>
        public string ClientPrivateToken
        {
            get { return this.clientPrivateToken; }
        }

        /// <summary>
        /// Gets the LastMessage that is specified by the client when it connects and creates this wait request
        /// This is used to identify what messages we are interested in (basically any greater than LastMessageId will be 
        /// returned to the client)
        /// </summary>
        public long LastMessageId
        {
            get { return this.lastMessageId; }
        }

        /// <summary>
        /// Gets a boolean flag indicating if this client is active (has it been disconnected, and is it not idle?)
        /// </summary>
        public bool Active
        {
            get { return !this.dateDeactivated.HasValue; }
        }

        /// <summary>
        /// Gets a DateTime indicating when the client was Deactivated, this is the period where the client can reconnect
        /// and become active again.  If the client does not reconnect within the specified Idle time for the client, it will
        /// be disconnected from the server and removed from the state manager
        /// </summary>
        public DateTime? DateDeactivated
        {
            get { return this.dateDeactivated; }
            set { this.dateDeactivated = value; }
        }
    }
}
