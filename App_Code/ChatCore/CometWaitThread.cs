/*
 * 服务器端线程池，排队等待信息请求并调用，同时提供超时等操作。
 * **/
using System;
using System.Collections.Generic;
using System.Threading;

namespace Commen.ChatCore
{
    /// <summary>
    /// Class CometWaitThread
    /// 
    /// This class contains an implementation of the thread pool that controls the
    /// CometWaitRequest objects and returns specified messages, errors or timeout messages
    /// back to the client in a controlled and scalable fasion
    /// </summary>
    public class CometWaitThread
    {
        private object state = new object();
        private List<CometWaitRequest> waitRequests = new List<CometWaitRequest>();
        private CometStateManager stateManager;

        public List<CometWaitRequest> WaitRequests
        {
            get { return this.waitRequests; }
        }

        public CometWaitThread(CometStateManager stateManager)
        {
            //  get the state manager
            this.stateManager = stateManager;

            Thread t = new Thread(new ThreadStart(QueueCometWaitRequest_WaitCallback));
            t.IsBackground = false;
            t.Start();
        }

        internal void QueueCometWaitRequest(CometWaitRequest request)
        {
            lock (this.state)
            {
                waitRequests.Add(request);
            }
        }

        internal void DeactivateCometWaitRequest(CometWaitRequest request)
        {
            lock (state)
            {
                //this.waitRequests.Remove(request);
                //  we disable the request, and we hope the
                //  client should connect immediatly else we time it out!
                request.DateDeactivated = DateTime.Now;
            }
        }

        private void QueueCometWaitRequest_Finished(object target)
        {
            CometWaitRequest request = target as CometWaitRequest;
            request.Result.SetCompleted();
        }

        private void QueueCometWaitRequest_WaitCallback()
        {
            //  here we are...
            //  in a loop

            while (true)
            {
                //Debug.WriteLine(string.Format("QueueCometWaitRequest_WaitCallback Tick: {0} {1} ", Thread.CurrentThread.IsThreadPoolThread, Thread.CurrentThread.ManagedThreadId));

                CometWaitRequest[] processRequest;

                lock (this.state)
                {
                    processRequest = waitRequests.ToArray();
                }

                //  we have no more wait requests left, so we want exis
                /*if (processRequest.Length == 0)
                    break;*/

                if (processRequest.Length == 0)
                {
                    //  sleep for this time
                    Thread.Sleep(100);
                }
                else
                {

                    for (int i = 0; i < processRequest.Length; i++)
                    {
                        try
                        {
                            CometClient cometClient = this.stateManager.StateProvider.GetCometClient(processRequest[i].ClientPrivateToken);

                            if (processRequest[i].Active)
                            {
                                Thread.Sleep(100);

                                //  timed out so remove from the queue
                                if (DateTime.Now.Subtract(processRequest[i].DateTimeAdded).TotalSeconds >= cometClient.ConnectionTimeoutSeconds)
                                {
                                    //  dequeue the request 
                                    DeactivateCometWaitRequest(processRequest[i]);

                                    //  get the message
                                    CometMessage timeoutMessage = new CometMessage()
                                        {
                                            MessageId = 0,
                                            Name = "aspNetComet.timeout",
                                            Contents = null
                                        };

                                    //
                                    //  ok, we we timeout the message
                                    processRequest[i].Result.CometMessages = new CometMessage[] { timeoutMessage };
                                    //  call the message
                                    this.QueueCometWaitRequest_Finished(processRequest[i]);
                                }
                                else
                                {
                                    CometMessage[] messages = this.CheckForServerPushMessages(processRequest[i]);

                                    if (messages != null && messages.Length > 0)
                                    {
                                        //  we have our message
                                        processRequest[i].Result.CometMessages = messages;
                                        //  and return!
                                        //  dequeue the request
                                        DeactivateCometWaitRequest(processRequest[i]);
                                        //  queue the response on another ASP.NET Worker thread
                                        this.QueueCometWaitRequest_Finished(processRequest[i]);
                                    }
                                }
                            }
                            else
                            {
                                //  this is an inactive 
                                this.CheckForIdleCometWaitRequest(processRequest[i], cometClient);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (processRequest[i].Active)
                            {
                                //  ok, this one has screwed up, so
                                //  we need to dequeue the request from ASP.NET, basically disable it and return
                                //  dequeue the request 
                                DeactivateCometWaitRequest(processRequest[i]);

                                //  get the message
                                CometMessage errorMessage = new CometMessage()
                                {
                                    MessageId = 0,
                                    Name = "aspNetComet.error",
                                    Contents = ex.Message
                                };

                                //
                                //  ok, we we timeout the message
                                processRequest[i].Result.CometMessages = new CometMessage[] { errorMessage };
                                //  call the message
                                this.QueueCometWaitRequest_Finished(processRequest[i]);
                            }
                            else
                            {
                                //  this is not an active request, so we dequeue it from the
                                //  thread
                                this.DequeueCometWaitRequest(processRequest[i].ClientPrivateToken);
                            }
                        }
                    }
                }
            }
        }

        private void CheckForIdleCometWaitRequest(CometWaitRequest request, CometClient cometClient)
        {
            lock (state)
            {
                if (DateTime.Now.Subtract(request.DateDeactivated.Value).TotalSeconds >= cometClient.ConnectionIdleSeconds)
                {
                    //  ok, this dude has timed out, so we remove it
                    this.stateManager.KillIdleCometClient(cometClient.PrivateToken);
                    //  and deque the request
                    this.waitRequests.Remove(request);
                }
            }
        }

        private CometMessage[] CheckForServerPushMessages(CometWaitRequest request)
        {
            //
            //  ok, we we need to do is get the messages 
            //  that are stored in the state provider
            return this.stateManager.StateProvider.GetMessages(request.ClientPrivateToken, request.LastMessageId);
        }


        internal void DequeueCometWaitRequest(string privateToken)
        {
            lock (state)
            {
                for (int i = 0; i < this.waitRequests.Count; i++)
                {
                    CometWaitRequest request = this.waitRequests[i];

                    if (request.ClientPrivateToken == privateToken)
                    {
                        //  remove it
                        this.waitRequests.Remove(request);
                        break;
                    }
                }
            }
        }
    }
}
