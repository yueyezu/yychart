<%@ WebHandler Language="C#" Class="ChatDefault" %>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Commen.ChatChannels;
using Commen.ChatCore;
using Newtonsoft.Json;

public class ChatDefault : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "text/plain";
        string action = context.Request["action"] ?? "online";
        switch (action)
        {
            case "offline":
                Offline(context);
                break;
            case "send":
                Send(context);
                break;
            case "online":
            default:
                Online(context);
                break;
        }
    }

    private void Send(HttpContext context)
    {
        string privateToken = context.Request["privateToken"];
        string msg = context.Request["msg"];
        string toUser = context.Request["toUser"];

        if (!string.IsNullOrEmpty(toUser))
        {
            ChatMessage chatMessage = new ChatMessage();
            CometClient cometClient = DefaultChannelHandler.StateManager.GetCometClient(privateToken);
            chatMessage.FromUser = cometClient.DisplayName;
            chatMessage.MsgTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            chatMessage.Msg = msg;
            chatMessage.ToUser = toUser;

            //这里用于记录发送历史
            ChatHistoryDal.AddChatHistory(chatMessage);
            object sendResult = new { result = "ok" };

            try
            {
                //处理向所有人发信息
                if (toUser.Equals("all"))
                {
                    DefaultChannelHandler.StateManager.SendMessageNoMe(privateToken, "ChatMessage", chatMessage);

                }
                else
                {
                    DefaultChannelHandler.StateManager.SendMessage(toUser, "ChatMessage", chatMessage);
                }
            }
            catch (Exception)
            {
                sendResult = new { result = "senderror" };
            }
            context.Response.Write(JsonConvert.SerializeObject(sendResult));
        }
    }

    public void Online(HttpContext context)
    {
        string publicToken = context.Request["publicToken"];
        string displayName = context.Request["publicToken"];
        if (!string.IsNullOrEmpty(publicToken))
        {
            string privateToken = string.Empty;

            //这里是使用唯一标识来识别登录的用户可以是程序实现，多地点登陆  但是程序还需要进行修改优化，并且要求session要挂到进程中不失效
            //由于这里使用的用户不会太多，并可以在用户注册时限定住用户名不重复，所以暂时采用简单方式，如下.
            //if (context.Session["client_id"] != null)
            //{
            //    privateToken = context.Session["client_id"].ToString();
            //}
            //else
            //{
            //    privateToken = Guid.NewGuid().ToString();
            //    context.Session["client_id"] = privateToken;
            //}
            privateToken = publicToken;

            CometClient client;
            try
            {
                client = DefaultChannelHandler.StateManager.GetCometClient(publicToken);
            }
            catch (CometException)
            {
                client = DefaultChannelHandler.StateManager.InitializeClient(publicToken, privateToken, displayName, 30, 5);
            }

            List<string> users = DefaultChannelHandler.StateManager.GetLocalUsers().OfType<string>().ToList<string>();
            var obj = new { users = users, privateToken = publicToken };
            context.Response.Write(JsonConvert.SerializeObject(obj));
        }
    }

    public void Offline(HttpContext context)
    {
        string privateToken = context.Request["privateToken"];
        if (!string.IsNullOrEmpty(privateToken))
        {
            DefaultChannelHandler.StateManager.KillIdleCometClient(privateToken);
        }
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}