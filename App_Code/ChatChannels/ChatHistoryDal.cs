using System;
using System.Collections.Generic;
using System.Data;

namespace Commen.ChatChannels
{
    public class ChatHistoryDal
    {
        /// <summary>
        /// 添加聊天历史记录
        /// </summary>
        /// <returns></returns>
        public static bool AddChatHistory(ChatMessage message)
        {
            //string sqlStr = "INSERT INTO [ChatManager]([msg],[fromUser],[toUser],[msgTime]) VALUES(@msg,@fromUser,@toUser,@msgTime)";
            //SqlParameter[] parameters =
            //    {
            //        new SqlParameter("@msg",SqlDbType.Text), 
            //        new SqlParameter("@fromUser",SqlDbType.NVarChar), 
            //        new SqlParameter("@toUser",SqlDbType.NVarChar), 
            //        new SqlParameter("@msgTime",SqlDbType.DateTime)
            //    };

            //parameters[0].Value = message.Msg;
            //parameters[1].Value = message.FromUser;
            //parameters[2].Value = message.ToUser;
            //parameters[3].Value = message.MsgTime;

            //int result = DbHelper.ExecuteSql(sqlStr, parameters);
            //if (result == 1)
            //{
            //    return true;
            //}
            //return false;
            return true;
        }

        public bool DeleteChatHistory(string id)
        {
            //string sqlString = "delete from ChatManager where id=" + id;

            //int result = DbHelper.ExecuteSql(sqlString);

            //return (result > 0);
            return true;
        }

        public bool DeleteChatHistorys(string ids)
        {
            //string sqlString = "delete from ChatManager where id in(" + ids + ")";

            //int result = DbHelper.ExecuteSql(sqlString);

            //return (result > 0);
            return true;
        }

        public List<ChatMessage> QueryChatHistory(string userName, string key, string time)
        {
            //string sqlString = "select * from ChatManager where [fromUser]='" + userName +
            //                   "' or toUser ='" + userName + "' or msg like '%" + key + "%' ";
            //if (time != "")
            //{
            //    sqlString += " and  DATEDIFF(day, msgTime, '" + time + "')=0";
            //}

            //DataSet ds = DbHelper.Query(sqlString);

            // return DataTableToList(ds.Tables[0]);

            return new List<ChatMessage>();
        }

        /// <summary>
        /// datatable转化list
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<ChatMessage> DataTableToList(DataTable dt)
        {
            List<ChatMessage> modelList = new List<ChatMessage>();
            int rowsCount = dt.Rows.Count;
            if (rowsCount > 0)
            {
                ChatMessage model;
                for (int n = 0; n < rowsCount; n++)
                {
                    model = new ChatMessage();
                    if (dt.Rows[n]["id"] != null && dt.Rows[n]["id"].ToString() != "")
                    {
                        model.Id = Int32.Parse(dt.Rows[n]["id"].ToString());
                    }
                    if (dt.Rows[n]["fromUser"] != null && dt.Rows[n]["fromUser"].ToString() != "")
                    {
                        model.FromUser = dt.Rows[n]["fromUser"].ToString();
                    }
                    if (dt.Rows[n]["toUser"] != null && dt.Rows[n]["toUser"].ToString() != "")
                    {
                        model.ToUser = dt.Rows[n]["toUser"].ToString();
                    }
                    if (dt.Rows[n]["msg"] != null && dt.Rows[n]["msg"].ToString() != "")
                    {
                        model.Msg = dt.Rows[n]["msg"].ToString();
                    }
                    if (dt.Rows[n]["msgTime"] != null && dt.Rows[n]["msgTime"].ToString() != "")
                    {
                        model.MsgTime = Convert.ToDateTime(dt.Rows[n]["msgTime"]).ToString("yyyy-MM-dd hh:mm:ss");
                    }
                    modelList.Add(model);
                }
            }
            return modelList;
        }
    }
}
