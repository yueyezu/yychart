/*
 * 用来管理线程池，管理线程和用户连接，消息转发等一系列操作的工厂类。
 * **/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Runtime.Serialization.Json;

namespace Commen.ChatCore
{
    /// <summary>
    /// CometStateManger Class
    /// 
    /// An instance of the this class is used to control the manager the state of the COMET Application.
    /// This class manages an instance of a ICometStateProvider instance
    /// </summary>
    /// 
    public class CometStateManager
    {
        private ICometStateProvider stateProvider;
        private int workerThreadCount;
        private int maximumTimeSlot;
        private int currentThread = 0;
        private CometWaitThread[] workerThreads;
        private object state = new object();

        /// <summary>
        /// Event that is called when a Client is Initialized
        /// </summary>
        public event CometClientEventHandler ClientInitialized;
        /// <summary>
        /// Event that is called when a Client is killed
        /// </summary>
        public event CometClientEventHandler IdleClientKilled;
        /// <summary>
        /// Event that is called when a Client subscribes to this channel
        /// </summary>
        public event CometClientEventHandler ClientSubscribed;

        /// <summary>
        /// Construct an instane of the CometStateManager class and pass in an
        /// instance of an ICometStateProvider to manage the persistence of the state
        /// </summary>
        /// <param name="stateProvider">An instance of an ICometStateProvider class that manages the persistence of the state</param>
        /// <param name="workerThreadCount">How many worked threads should this CometStateManager initialize</param>
        /// <param name="maximumTimeSlot">The maximum time in milliseconds that should be idle between each COMET client is polled within a worker thread</param>
        public CometStateManager(ICometStateProvider stateProvider, int workerThreadCount, int maximumTimeSlot)
        {
            if (stateProvider == null)
                throw new ArgumentNullException("stateProvider");

            if (workerThreadCount <= 0)
                throw new ArgumentOutOfRangeException("workerThreadCount");

            //  ok, setup the member of this class
            this.stateProvider = stateProvider;
            this.workerThreadCount = workerThreadCount;
            this.maximumTimeSlot = maximumTimeSlot;

            this.workerThreads = new CometWaitThread[5];

            //
            //  ok, lets fireup the threads
            for (int i = 0; i < workerThreadCount; i++)
            {
                this.workerThreads[i] =
                    new CometWaitThread(this);
            }
        }

        /// <summary>
        /// Construct an instane of the CometStateManager class and pass in an
        /// instance of an ICometStateProvider to manage the persistence of the state
        /// 
        /// This calls the main constructor and specifies the default values workerThreadCount = 5 and maximumTimeSlot = 100.  These values
        /// can be tuned for your application by using the main constructor
        /// </summary>
        /// <param name="stateProvider">An instance of an ICometStateProvider class that manages the persistence of the state</param>
        public CometStateManager(ICometStateProvider stateProvider)
            : this(stateProvider, 5, 100)
        {
        }

        /// <summary>
        /// Creates a CometClient instance that is persisted into the ICometStateProvider instance.  This needs to be
        /// called by the server prior to the client connecting from a client application.
        /// </summary>
        /// <remarks>
        /// This method will typically be used after a login script is executed, either from a standard ASP.NET form
        /// or an Ajax Method etc...
        /// 
        /// The server would validate the user information, if successfull initialize a client in the COMET engine ready
        /// for the client to connect.
        /// </remarks>
        /// <param name="publicToken">The public token of the client, this token is used to identify the client to other clients</param>
        /// <param name="privateToken">The private token of the client, this token is used to identify the client to itself</param>
        /// <param name="displayName">The display name of the client, can be used to hold a friendly display name of the client</param>
        /// <param name="connectionTimeoutSeconds">The number of seconds the client will be connected to the server for, until it needs to reestablish a connection becuase no messages have been sent</param>
        /// <param name="connectionIdleSeconds">The number of seconds the server will wait for the client to reconnect before it treats it as an idle connection and removes it from the server</param>
        /// <returns>An initialized CometClient object that represents the initialized client</returns>
        public CometClient InitializeClient(string publicToken, string privateToken, string displayName, int connectionTimeoutSeconds, int connectionIdleSeconds)
        {
            //  validate the parameters
            if (string.IsNullOrEmpty(publicToken))
                throw new ArgumentNullException("publicToken");

            if (string.IsNullOrEmpty(privateToken))
                throw new ArgumentNullException("privateToken");

            if (string.IsNullOrEmpty(displayName))
                throw new ArgumentNullException("displayName");

            if (connectionIdleSeconds <= 0)
                throw new ArgumentOutOfRangeException("connectionIdleSeconds must be greater than 0");

            if (connectionTimeoutSeconds <= 0)
                throw new ArgumentOutOfRangeException("connectionTimeoutSeconds must be greater than 0");

            CometClient cometClient = new CometClient();

            //  ok, set it up
            cometClient.ConnectionIdleSeconds = connectionIdleSeconds;
            cometClient.ConnectionTimeoutSeconds = connectionTimeoutSeconds;
            cometClient.DisplayName = displayName;
            cometClient.LastActivity = DateTime.Now;
            cometClient.PrivateToken = privateToken;
            cometClient.PublicToken = publicToken;

            //
            //  send this to the state provider
            this.stateProvider.InitializeClient(cometClient);

            //  ok, fire the event
            this.FireClientInitialized(cometClient);

            return cometClient;
        }

        /// <summary>
        /// Called from an Asynchronous HttpHandler Method to begin the Subscribe call
        /// </summary>
        /// <param name="context">HttpContext passed in from the handler</param>
        /// <param name="callback">AsyncCallback passed in from the handler</param>
        /// <param name="extraData">AsyncState passed in from the handler</param>
        /// <returns>An IAsyncResult used to identify and control the asynchronous operation</returns>
        public IAsyncResult BeginSubscribe(HttpContext context, AsyncCallback callback, object extraData)
        {
            try
            {
                long lastMessageId;
                string privateToken;

                if (!long.TryParse(context.Request["lastMessageId"] ?? "-1", out lastMessageId))
                    throw CometException.CometHandlerParametersAreInvalidException();

                privateToken = context.Request["privateToken"];

                if (string.IsNullOrEmpty(privateToken))
                    throw CometException.CometHandlerParametersAreInvalidException();


                this.DebugWriteThreadInfo("BeginSubscribe");

                lock (state)
                {
                    //
                    //  get the comet client
                    CometClient cometClient = this.GetCometClient(privateToken);

                    //  ok, fire the event
                    this.FireClientSubscribed(cometClient);

                    //  kill the previous one if one exists
                    //  from the thread pool
                    for (int i = 0; i < this.workerThreadCount; i++)
                    {
                        this.workerThreads[i].DequeueCometWaitRequest(privateToken);
                    }

                    //  ok, this is our result, so lets queue it
                    CometWaitRequest request = new CometWaitRequest(privateToken, lastMessageId, context, callback, extraData);

                    //  we have our request so lets queue it on a thread
                    this.workerThreads[this.currentThread].QueueCometWaitRequest(request);

                    //  cycle the thread count
                    this.currentThread++;

                    if (this.currentThread >= this.workerThreadCount)
                        this.currentThread = 0; //  cycle back to 0

                    return request.Result;
                }
            }
            catch (Exception ex)
            {
                this.WriteErrorToResponse(context, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Called from an Asynchronous HttpHandler Method method to complete the Subscribe call
        /// </summary>
        /// <param name="result">The IAsyncResult instance that was initialized in the BeginSubscribe call</param>
        public void EndSubscribe(IAsyncResult result)
        {
            this.DebugWriteThreadInfo("EndSubscribe");

            CometAsyncResult cometAsyncResult = result as CometAsyncResult;

            if (cometAsyncResult != null)
            {
                try
                {
                    //  get the messages 
                    CometMessage[] messages = cometAsyncResult.CometMessages;
                    //  serialize the messages
                    //  back to the client
                    if (messages != null && messages.Length > 0)
                    {
                        List<Type> knownTypes = new List<Type>();
                        foreach (CometMessage message in messages)
                        {
                            if (message.Contents != null)
                            {
                                Type knownType = message.Contents.GetType();

                                if (!knownTypes.Contains(knownType))
                                {
                                    knownTypes.Add(knownType);
                                }
                            }
                        }

                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(messages.GetType(), knownTypes);
                        serializer.WriteObject(((CometAsyncResult)result).Context.Response.OutputStream, messages);
                    }
                }
                catch (Exception ex)
                {
                    //  write the error out??
                    this.WriteErrorToResponse(((CometAsyncResult)result).Context, ex.Message);
                }
            }
        }

        /// <summary>
        /// Send a message to a specific client
        /// </summary>
        /// <param name="clientPublicToken">The public token of the client</param>
        /// <param name="name">The name of the message</param>
        /// <param name="contents">The contents of the message</param>
        public void SendMessage(string clientPublicToken, string name, object contents)
        {
            this.stateProvider.SendMessage(clientPublicToken, name, contents);
        }

        /// <summary>
        /// Send a message to all clients
        /// </summary>
        /// <param name="name">The name of the message</param>
        /// <param name="contents">The contents of the message</param>
        public void SendMessage(string name, object contents)
        {
            this.stateProvider.SendMessage(name, contents);
        }


        /// <summary>
        /// 向除自己之外的人广播消息
        /// </summary>
        /// <param name="clientPrivateToken"></param>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        public void SendMessageNoMe(string clientPrivateToken, string name, object contents)
        {
            this.stateProvider.SendMessageNoMe(clientPrivateToken, name, contents);
        }


        /// <summary>
        /// Gets the ICometStateProvider instance this manager consumes
        /// </summary>
        internal ICometStateProvider StateProvider
        {
            get { return this.stateProvider; }
        }

        /// <summary>
        /// Register the required javascript for the page
        /// </summary>
        /// <param name="page">The page we want to write the scripts to</param>
        public static void RegisterAspNetCometScripts(Page page)
        {
            page.ClientScript.RegisterClientScriptResource(typeof(CometStateManager), "MethodWorx.AspNetComet.Core.Scripts.AspNetComet.js");
        }

        /// <summary>
        /// Kill an IdleCometClient
        /// </summary>
        /// <param name="clientPrivateToken"></param>
        public void KillIdleCometClient(string clientPrivateToken)
        {
            //  get the comet client
            CometClient cometClient = this.stateProvider.GetCometClient(clientPrivateToken);
            //  ok, tmie the clietn out
            this.stateProvider.KillIdleCometClient(clientPrivateToken);
            //  and fire
            this.FireIdleClientKilled(cometClient);
        }

        public CometClient GetCometClient(string clientPrivateToken)
        {
            return this.stateProvider.GetCometClient(clientPrivateToken);
        }

        internal void DebugWriteThreadInfo(string message)
        {
            int workerAvailable = 0;
            int completionPortAvailable = 0;
            ThreadPool.GetAvailableThreads(out workerAvailable, out completionPortAvailable);

            Debug.WriteLine(string.Format("{0}: {1} {2} out of {3}/{4}", message, Thread.CurrentThread.IsThreadPoolThread, Thread.CurrentThread.ManagedThreadId, workerAvailable, completionPortAvailable));
        }

        internal void FireClientInitialized(CometClient cometClient)
        {
            if (this.ClientInitialized != null)
                this.ClientInitialized(this, new CometClientEventArgs(cometClient));
        }

        internal void FireIdleClientKilled(CometClient cometClient)
        {
            if (this.IdleClientKilled != null)
                this.IdleClientKilled(this, new CometClientEventArgs(cometClient));

        }

        internal void FireClientSubscribed(CometClient cometClient)
        {
            if (this.ClientSubscribed != null)
                this.ClientSubscribed(this, new CometClientEventArgs(cometClient));
        }

        private void WriteErrorToResponse(HttpContext context, string message)
        {
            //
            //  ok, we have had an error so we have to return it
            CometMessage errorMessage = new CometMessage();

            errorMessage.Name = "aspNetComet.error";
            errorMessage.MessageId = 0;
            errorMessage.Contents = message;

            CometMessage[] messages = new CometMessage[] { errorMessage };

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(messages.GetType());
            serializer.WriteObject(context.Response.OutputStream, messages);

            context.Response.End();
        }


        public List<object> GetLocalUsers()
        {
            return this.stateProvider.GetLocalUsers();
        }

        public bool HasUser(string publicToken)
        {
            return this.stateProvider.HasUser(publicToken);
        }
    }
}
