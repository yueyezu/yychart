/*
 * 包含对服务信息和相信属性的实体类，是消息传输的主体。
 * **/
using System.Runtime.Serialization;

namespace Commen.ChatCore
{
    /// <summary>
    /// CometMessage Class
    /// 
    /// This is a CometMessage that has been sent to the client, the DataContract names have been
    /// shortened to remove any bytes we dont need from the message (ok, did'nt save much, but we can do it!)
    /// </summary>'
    [DataContract(Name = "cm")]
    public class CometMessage
    {
        [DataMember(Name = "mid")]
        private long messageId;
        [DataMember(Name = "name")]
        private string name;
        [DataMember(Name = "contents")]
        private object contents;

        /// <summary>
        /// Gets or Sets the MessageId, used to track which message the Client last received
        /// </summary>
        public long MessageId
        {
            get { return this.messageId; }
            set { this.messageId = value; }
        }

        /// <summary>
        /// Gets or Sets the Content of the Message
        /// </summary>
        public object Contents
        {
            get { return this.contents; }
            set { this.contents = value; }
        }

        /// <summary>
        /// Gets or Sets the error message if this is a failure
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
    }
}
