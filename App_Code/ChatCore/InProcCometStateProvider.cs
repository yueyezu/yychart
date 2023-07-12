/*
 * 继承了ICometStateProvider并实现了消息操作的方法，同时将消息保存在内存中。
 * **/
using System;
using System.Collections.Generic;

namespace Commen.ChatCore
{
    /// <summary>
    /// Class InProcCometStateProvider
    /// 
    /// This class provides an implementation of ICometStateProvider that keeps the 
    /// information in memory.  This provider is not scalable as it will not run on a server
    /// farm but demonstrates how you should implemement the provider.
    /// </summary>
    public class InProcCometStateProvider : ICometStateProvider
    {
        /// <summary>
        /// Private class which holds the state of each connected client
        /// </summary>
        private class InProcCometClient
        {
            public CometClient CometClient;
            public Dictionary<long, CometMessage> Messages = new Dictionary<long, CometMessage>();
            public long NextMessageId = 1;
        }

        /// <summary>
        /// Cache of clients
        /// </summary>
        private Dictionary<string, InProcCometClient> publicClients = new Dictionary<string, InProcCometClient>(); //在发送消息时使用
        private Dictionary<string, InProcCometClient> privateClients = new Dictionary<string, InProcCometClient>(); //在接收消息时使用

        private static object state = new object();


        #region ICometStateProvider Members

        /// <summary>
        /// Store the new client in memory
        /// </summary>
        /// <param name="cometClient"></param>
        public void InitializeClient(CometClient cometClient)
        {
            if (cometClient == null)
                throw new ArgumentNullException("cometClient");

            lock (state)
            {

                //  ok, ensure we dont already exist
                if (publicClients.ContainsKey(cometClient.PublicToken) || privateClients.ContainsKey(cometClient.PrivateToken))
                    throw CometException.CometClientAlreadyExistsException();

                InProcCometClient inProcCometClient = new InProcCometClient()
                    {
                        CometClient = cometClient
                    };

                //  stick the client int he arrays
                //  ready to be used
                publicClients.Add(cometClient.PublicToken, inProcCometClient);
                privateClients.Add(cometClient.PrivateToken, inProcCometClient);
            }

            //  ok, they are in there ready to be used
        }

        /// <summary>
        /// Get the messages for a specific client
        /// </summary>
        /// <param name="clientPrivateToken"></param>
        /// <param name="lastMessageId"></param>
        /// <returns></returns>
        public CometMessage[] GetMessages(string clientPrivateToken, long lastMessageId)
        {
            if (string.IsNullOrEmpty(clientPrivateToken))
                throw new ArgumentNullException("clientPrivateToken");

            lock (state)
            {
                if (!privateClients.ContainsKey(clientPrivateToken))
                    throw CometException.CometClientDoesNotExistException();

                //
                //  ok, get the client
                InProcCometClient cometClient = privateClients[clientPrivateToken];

                List<long> toDelete = new List<long>();
                List<long> toReturn = new List<long>();

                //  wicked, we have the client, so we can get its messages from our list
                //  we delete any before the last messageId becuase we dont want them
                foreach (long key in cometClient.Messages.Keys)
                {
                    if (key <= lastMessageId)
                        toDelete.Add(key);
                    else
                        toReturn.Add(key);
                }

                //  delete the ones from the messages
                foreach (long key in toDelete)
                {
                    cometClient.Messages.Remove(key);
                }

                //  and return the ones in the toReturn array
                List<CometMessage> cometMessages = new List<CometMessage>();
                foreach (long key in toReturn)
                {
                    cometMessages.Add(cometClient.Messages[key]);
                }

                return cometMessages.ToArray();


            }
        }

        /// <summary>
        /// Send a message to a specific client
        /// </summary>
        /// <param name="clientPublicToken"></param>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        public void SendMessage(string clientPublicToken, string name, object contents)
        {
            if (string.IsNullOrEmpty(clientPublicToken))
                throw new ArgumentNullException("clientPublicToken");

            if (contents == null)
                throw new ArgumentNullException("contents");

            lock (state)
            {
                if (!publicClients.ContainsKey(clientPublicToken))
                    throw CometException.CometClientDoesNotExistException();

                //
                //  ok, get the client
                InProcCometClient cometClient = publicClients[clientPublicToken];

                // ok, stick the message in the array
                CometMessage message = new CometMessage();

                message.Contents = contents;
                message.Name = name;
                message.MessageId = cometClient.NextMessageId;

                //  increment
                cometClient.NextMessageId++;
                cometClient.Messages.Add(message.MessageId, message);
            }

        }


        /// <summary>
        /// Send a message to all the clients
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        public void SendMessage(string name, object contents)
        {
            if (contents == null)
                throw new ArgumentNullException("contents");

            lock (state)
            {
                foreach (InProcCometClient cometClient in publicClients.Values)
                {
                    // ok, stick the message in the array
                    CometMessage message = new CometMessage();

                    message.Contents = contents;
                    message.Name = name;
                    message.MessageId = cometClient.NextMessageId;

                    //  increment
                    cometClient.NextMessageId++;
                    cometClient.Messages.Add(message.MessageId, message);
                }
            }
        }

        public void SendMessageNoMe(string clientPrivateToken, string name, object contents)
        {
            if (contents == null)
                throw new ArgumentNullException("contents");

            lock (state)
            {
                foreach (InProcCometClient cometClient in publicClients.Values)
                {
                    if (!cometClient.CometClient.PrivateToken.Equals(clientPrivateToken))
                    {
                        // ok, stick the message in the array
                        CometMessage message = new CometMessage();

                        message.Contents = contents;
                        message.Name = name;
                        message.MessageId = cometClient.NextMessageId;

                        //  increment
                        cometClient.NextMessageId++;
                        cometClient.Messages.Add(message.MessageId, message);
                    }
                }
            }
        }

        /// <summary>
        /// Get the client from the state provider
        /// </summary>
        /// <param name="clientPrivateToken"></param>
        /// <returns></returns>
        public CometClient GetCometClient(string clientPrivateToken)
        {
            if (!this.privateClients.ContainsKey(clientPrivateToken))
                throw CometException.CometClientDoesNotExistException();

            //  return the client private token
            return this.privateClients[clientPrivateToken].CometClient;
        }

        /// <summary>
        /// Remove an idle client from the memory
        /// </summary>
        /// <param name="clientPrivateToken"></param>
        public void KillIdleCometClient(string clientPrivateToken)
        {
            if (!this.privateClients.ContainsKey(clientPrivateToken))
                throw CometException.CometClientDoesNotExistException();

            //  get the client
            InProcCometClient ipCometClient = this.privateClients[clientPrivateToken];

            //  and remove the dictionarys
            this.privateClients.Remove(ipCometClient.CometClient.PrivateToken);
            this.publicClients.Remove(ipCometClient.CometClient.PublicToken);
        }


        public List<object> GetLocalUsers()
        {
            List<object> list = new List<object>();
            foreach (string status in publicClients.Keys)
            {
                list.Add(status);
            }

            return list;
        }
        public bool HasUser(string publicToken)
        {
            return this.publicClients.ContainsKey(publicToken);
        }

        #endregion
    }
}
