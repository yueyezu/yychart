<%@ Page Language="C#" AutoEventWireup="true" CodeFile="chat.aspx.cs" Inherits="app_main_cooperate_chat" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="../lib/ligerui/Aqua/css/ligerui-all.css" rel="stylesheet" type="text/css" />

    <script src="../lib/jquery-1.5.2.min.js" type="text/javascript"></script>

    <script src="../lib/ligerUI/ligerui.min.js" type="text/javascript"></script>

    <script type="text/javascript" src="xheditor-1.2.1/xheditor-1.2.1.min.js"></script>

    <script type="text/javascript" src="xheditor-1.2.1/zh-cn.js"></script>

    <script src="chat.js" type="text/javascript"></script>

    <script type="text/javascript">
        function playSound(url) {
            var div = document.getElementById('divSound');
            div.innerHTML = '<embed src="' + url + '" loop="0" autostart="true" hidden="true"></embed>';
        }
    </script>

    <style type="text/css">
        .l-tab-content-item {
            width: 100%;
            overflow: auto; /*这里主要改变的这一属性,使聊天信息显示时添加滚动条*/
            position: relative;
        }

        #xhePanel, #xhePanel > div, #xheShadow {
            height: 146px;
        }

        .msgHistorybtn {
            background: transparent url(xheditor-1.2.1/xheditor_skin/msghistorybtn.gif) no-repeat 16px 16px;
            background-position: 2px 2px;
        }

        .chatMsgHeader {
            font-size: 12px;
            color: blue;
            margin: 5px 0 2px 0;
        }

        .sendChatMsgHeader {
            font-size: 12px;
            color: green;
            margin: 5px 0 2px 0;
        }

        .chatMsgBody {
            margin-left: 6px;
        }
    </style>
</head>
<body style="width: 690px; height: 473px; margin: 80px auto">
    <%-- 用于播放声音的一个面板，代码在本页上方--%>
    <div id="divSound">
    </div>
    <div id="chatLayout">
        <div position="center" id="tabShowMsg">
            <div id="pnlShowMsg" title="所有人" tabid="all" style="height: 100%; overflow: auto;">
            </div>
        </div>
        <div position="right" title="用户列表" id="treeUsers">
        </div>
        <div position="bottom">
            <textarea id="msgEdit" name="msgEdit" style="width: 100%; height: 110px;">
            </textarea>
            <div id="bbar" style="float: right; margin-top: 5px; margin-right: 28px">
                <input type="button" id="btnSendMsg" value="发送" />
                <input type="button" id="btnClearMsg" value="清空" />
            </div>
        </div>
    </div>
</body>
</html>
