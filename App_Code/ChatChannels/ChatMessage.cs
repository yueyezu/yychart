using System.Runtime.Serialization;

namespace Commen.ChatChannels
{
    [DataContract(Name = "cm")]
    public class ChatMessage
    {
        [DataMember(Name = "id")]
        private int id;
        [DataMember(Name = "fromUser")]
        private string fromUser;
        [DataMember(Name = "msg")]
        private string msg;
        [DataMember(Name = "toUser")]
        private string toUser;
        [DataMember(Name = "msgTime")]
        private string msgTime;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public string ToUser
        {
            get { return toUser; }
            set { toUser = value; }
        }

        public string MsgTime
        {
            get { return msgTime; }
            set { msgTime = value; }
        }

        public string FromUser
        {
            get { return fromUser; }
            set { fromUser = value; }
        }

        public string Msg
        {
            get { return msg; }
            set { msg = value; }
        }

    }
}
