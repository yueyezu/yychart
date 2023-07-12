/*
 * COMET的服务实体，实例化一个CometClinet的意义在于记录必要的服务信息，比如用户名称超时设置等。
 */
using System;
using System.Runtime.Serialization;

namespace Commen.ChatCore
{
    /// <summary>
    /// CometClient Class
    /// 
    /// This represents a logged in client within the COMET application.  This marked as a DataContract becuase
    /// it can be seralized to the client using JSON
    /// </summary>
    [DataContract]
    public class CometClient
    {
        [DataMember]
        private string privateToken;    //发送数据的标记
        [DataMember]
        private string publicToken;     //接收数据的标记
        [DataMember]
        private string displayName;     //界面显示的标记
        [DataMember]
        private DateTime lastActivity;
        [DataMember]
        private int connectionIdleSeconds;      //设置超时时间
        [DataMember]
        private int connectionTimeoutSeconds;   //等待超时时间

        /// <summary>
        /// Gets or Sets the token used to identify the client to themselves
        /// </summary>
        public string PrivateToken
        {
            get { return this.privateToken; }
            set { this.privateToken = value; }
        }

        /// <summary>
        /// Gets or Sets the token used to identify the client to other clients
        /// </summary>
        public string PublicToken
        {
            get { return this.publicToken; }
            set { this.publicToken = value; }
        }

        /// <summary>
        /// Gets or Sets the display name of the client
        /// </summary>
        public string DisplayName
        {
            get { return this.displayName; }
            set { this.displayName = value; }
        }

        /// <summary>
        /// Gets or Sets the last activity of the client
        /// </summary>
        public DateTime LastActivity
        {
            get { return this.lastActivity; }
            set { this.lastActivity = value; }
        }

        /// <summary>
        /// Gets or Sets the ConnectionIdleSections property which is the number of seconds a connection will remain
        /// alive for without being connected to a client, after this time has expired the client will 
        /// be removed from the state manager
        /// </summary>
        public int ConnectionIdleSeconds
        {
            get { return this.connectionIdleSeconds; }
            set { this.connectionIdleSeconds = value; }
        }

        /// <summary>
        /// Gets or Sets the ConnectionTimeOutSections property which is the number of seconds a connection will remain
        /// alive for whilst being connected to a client, but without receiving any messages.  After a timeout has expired
        /// A client should restablish a connection to the server
        /// </summary>
        public int ConnectionTimeoutSeconds
        {
            get { return this.connectionTimeoutSeconds; }
            set { this.connectionTimeoutSeconds = value; }
        }
    }
}
