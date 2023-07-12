<%@ WebHandler Language="C#" Class="ChatHistory" %>

using System;
using System.Web;
using Commen.ChatChannels;
using Newtonsoft.Json;

public class ChatHistory : IHttpHandler
{
    ChatHistoryDal chatHistoryDal = new ChatHistoryDal();
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "text/plain";
        string action = context.Request["action"] ?? "query";
        switch (action)
        {
            case "query":
                Query(context);
                break;
            case "delete":
                Delete(context);
                break;
            default: break;
        }
    }

    private void Query(HttpContext context)
    {
        string key = context.Request["key"] ?? "";
        string logDate = context.Request["chatData"] ?? "";
        string userName = context.Request["userName"];

        context.Response.Write(JsonConvert.SerializeObject(new { rows = chatHistoryDal.QueryChatHistory(userName,key, logDate) }));
    }

    private void Delete(HttpContext context)
    {
        string ids = context.Request["ids"];
        context.Response.Write(chatHistoryDal.DeleteChatHistorys(ids));
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}