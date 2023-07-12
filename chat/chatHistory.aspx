<%@ Page Language="C#" AutoEventWireup="true" CodeFile="chatHistory.aspx.cs" Inherits="chat_chatHistory" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link type="text/css" rel="Stylesheet" href="../lib/ligerUI/Aqua/css/ligerui-all.css" />
    <script src="../lib/jquery-1.5.2.min.js" type="text/javascript"></script>
    <script type="text/javascript" src="../lib/ligerUI/ligerui.min.js"></script>

    <style type="text/css">
        .l-grid-hd-cell-inner {
            text-align: left;
            margin-left: 10px;
            overflow: hidden;
        }

        .chatMsgHeader {
            font-size: 12px;
            color: blue;
            margin-left: 15px;
        }

        .chatMsgBody {
            margin-left: 28px;
        }
    </style>

    <script type="text/javascript">
        var dialog = frameElement.dialog;
        var thisUserName = dialog.get('userName');
        $(function () {
            $("#chatGrid").ligerGrid({
                root: 'rows',
                enabledEdit: false, detailToEdit: false,
                fixedCellHeight: false,
                isScroll: false,
                allowUnSelectRow: true,
                usePager: false,
                width: '736',
                columns: [
                    {
                        display: '消息内容', name: 'id', align: 'left',
                        render: function (msg) {
                            html = '<div class="chatMsgHeader">';
                            html += '<span>' + msg.fromUser + '</span><span style="margin-left: 20px;">' + msg.msgTime + '</span>';
                            html += '</div>';
                            html += '<div class="chatMsgBody">' + msg.msg + '</div>';
                            return html;
                        }
                    }
                ]
            });

            $("#chatDel").ligerButton({
                click: function () {
                    var rows = liger.get('chatGrid').getSelectedRows();
                    if (!rows.length) {
                        $.ligerDialog.warn('请选择要删除的行');
                        return;
                    } else {
                        $.ligerDialog.confirm('确定要删除选中数据？', function (result) {
                            if (result) {
                                var ids = '';
                                $(rows).each(function () {
                                    ids += this.id + ",";
                                });
                                ids = ids.substr(0, ids.length - 1);

                                $.post("chatAshx/ChatHistory.ashx", {
                                    'action': 'delete',
                                    'ids': ids
                                }, function () {
                                    $.ligerDialog.success('删除成功！');
                                    query();
                                });
                            }
                        });
                    }
                }
            });

            $("#btnSearch").ligerButton({
                click: function () {
                    query();
                }
            });
            $("#chatDate").ligerDateEditor({
                labelWidth: 0,
                labelAlign: 'center',
                cancelable: true
            });

            query();
        });
        var query = function () {
            $.getJSON('chatAshx/ChatHistory.ashx', {
                'action': 'query',
                'userName': thisUserName,
                'chatDate': $("#chatDate").val(),
                'key': $('#key').val()
            }, function (json) {
                liger.get('chatGrid').loadData(json);
            });
        };
    </script>

</head>
<body>
    <table style="height: 30px; margin-left: 28px; width: 448px">
        <tr>
            <td>时间：
            </td>
            <td>
                <input type="text" id="chatDate" />
            </td>
            <td>关键字：
            </td>
            <td>
                <input type="text" id="key" onkeydown="if(event.keyCode==13)javascript:Query();" />
            </td>
            <td>
                <input type="button" id="btnSearch" value="搜索" />
            </td>
        </tr>
    </table>
    <div id="chatGrid" style="height: 456px">
    </div>
    <div style="text-align: right; padding: 10px 38px 0 0;">
        <input type="button" id="chatDel" value="删除" />
    </div>
</body>
</html>
