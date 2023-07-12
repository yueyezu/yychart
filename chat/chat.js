//var dialog = frameElement.dialog;
var showTab = null;
var editor = null;
var comet = null;
var userTree = null;

$(function () {
    $("#chatLayout").ligerLayout({
        height: 470,
        rightWidth: 170,
        bottomHeight: 175
    });

    showTab = $('#tabShowMsg').ligerTab({
        height: 280,
        showSwitch: true,
        dblClickToClose: true
    });

    userTree = $('#treeUsers').ligerTree({
        checkbox: false,
        onclick: function (e) {
            var data = e.data;
            showTab.addTabItem({
                tabid: data.text,
                text: data.text
            });
        }
    });

    editor = $('#msgEdit').xheditor({
        plugins: {
            msgHistory: {
                c: 'msgHistorybtn', t: '聊天历史(Ctrl+h)', s: 'ctrl+h',
                e: function () {
                    window.parent.$.ligerDialog.open({
                        width: 750,
                        height: 600,
                        url: 'chatHistory.aspx',
                        userName: getCurrentUser(),
                        showMax: false,
                        showMin: true,
                        isHidden: false,
                        slide: true,
                        modal: false,
                        title: '聊天历史',
                        winId: 'chathistory'
                    });
                }
            }
        },
        tools: 'Cut,Copy,Paste,|,Bold,Italic,Underline,Strikethrough,|,FontColor,BackColor,|,Blocktag,Fontface,FontSize,|,Align,List,Outdent,Indent,|,Emot,Source,|,msgHistory',
        skin: 'vista',
        forcePtag: false    //不强制P标签
    });

    $('#btnSendMsg').ligerButton({
        click: function () {
            var msg = editor.getSource();
            if (msg == "" || msg == null) {
                window.parent.$.ligerDialog.warn('不能发送空内容！');
                return;
            }
            comet.send({
                'toUser': showTab.getSelectedTabItemID(),
                'msg': msg,
                callback: function (obj) {
                    if (obj.result == '') {
                        msg = msg + "<div style='margin-top:5px; margin-bottom:5px; color:red'>系统出现错误,稍候请重新发送</div>";
                    }
                    if (obj.result == 'senderror') {
                        msg = msg + "<div style='margin-top:5px; margin-bottom:5px; color:red'>信息发送过程中出现错误，对方可能未收到您的消息，请稍后重新发送。</div>";
                    }

                    addShowMsg("send", {
                        'fromUser': comet.publicToken,
                        'toUser': showTab.getSelectedTabItemID(),
                        'msg': msg,
                        'msgTime': getNow()
                    });

                }
            });

            editor.setSource("");
        }
    });

    $('#btnClearMsg').ligerButton({
        click: function () {
            editor.setSource("");
        }
    });

    $(document).ready(function () {
        $('a[cmd="About"]').remove();   //这里将编辑器最后的关于按钮给去除掉
    });

    $(document).ready(function () {      //当界面渲染完成后初始化聊天功能，并上线操作
        comet = new Comet({
            handler: '../DefaultChannel.ashx',
            sendhandler: 'chatAshx/ChatDefault.ashx',
            publicToken: getCurrentUser()
        });
        comet.online();
    });
});

function addShowMsg(type, msg) { //这个函数后期完善时需要修改 使之更人性化
    var select = null;
    var html = null;

    switch (type) {
        case 'send':
            //将相应的内容添加到相对应的面板
            select = $('div[tabid=' + msg.toUser + ']');
            html = '<tr><td>';
            html += '<div class="sendChatMsgHeader">';
            html += '<span>' + msg.fromUser + '</span><span style="margin-left: 20px;">' + msg.msgTime + '</span>';
            html += '</div>';
            html += '<div class="chatMsgBody">' + msg.msg + '</div>';
            html += '</td></tr>';
            break;
        case 'get':
            if (msg.toUser == 'all') {
                showTab.selectTabItem(msg.toUser);
                select = $('div[tabid=' + msg.toUser + ']');
            } else {
                //如果不是当前标签添加到消息盒子
                if (!showTab.isTabItemExist(msg.fromUser)) {
                    showTab.addTabItem({
                        tabid: msg.fromUser,
                        text: msg.fromUser
                    });
                } else {
                    showTab.selectTabItem(msg.fromUser);
                }
                select = $('div[tabid=' + msg.fromUser + ']');
            }
            html = '<tr><td>';
            html += '<div class="chatMsgHeader">';
            html += '<span>' + msg.fromUser + '</span><span style="margin-left: 20px;">' + msg.msgTime + '</span>';
            html += '</div>';
            html += '<div class="chatMsgBody">' + msg.msg + '</div>';
            html += '</td></tr>';
            break;
        default:
    }
    select.append(html);
    select.scrollTop(select[0].scrollHeight);
}


function addAllUser(users) {
    userTree.setData(users);
}

function addNewUser(obj) {
    var nodes = [];
    nodes.push({ text: obj });
    userTree.append(null, nodes);
}

function removeUser(obj) {
    $.each(userTree.getData(), function (n, val) {
        if (val.text == obj) {
            userTree.remove(val.treedataindex);
        }
    });
}

//获取当前的用户
function getCurrentUser() {
    //return dialog.get('data').userName;
    var reg = new RegExp("(^|&)userName=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null)
        return unescape(r[2]);
    return null;
}

function getNow() {
    var now = new Date();
    var year = now.getFullYear();
    var month = now.getMonth();
    var day = now.getDate();
    var hours = now.getHours();
    var minutes = now.getMinutes();
    var seconds = now.getSeconds();
    return year + "-" + month + "-" + day + "  " + hours + ":" + minutes + ":" + seconds;
}

/**
* 下边是前后台连接类——Comet
* 包括函数：connect、send、online、offline
**/
var Comet = function (obj) {
    this.handler = obj.handler;
    this.sendhandler = obj.sendhandler;
    this.publicToken = obj.publicToken;
    this.lastMessageId = 0;
    this.enabled = true;
};

Comet.prototype.connect = function () {
    var me = this;
    $.ajax({
        url: me.handler,
        timeout: 40000,
        dataType: 'json',
        global: false,
        data: {
            privateToken: me.privateToken,
            lastMessageId: me.lastMessageId
        },
        success: function (response) {
            if (response == null) {
                //console.log("无返回信息,重新连接");
                me.reconnect();
            }
            var message = response[0];
            switch (message.name) {
                case 'aspNetComet.error':
                    try {
                        console.log('aspNetComet.error' + new Date().format('Y-m-d H:i:s'));
                    } catch (e) {

                    }
                    me.reconnect();
                    break;
                case 'aspNetComet.timeout':
                    if (me.enabled) {
                        me.connect();
                    }
                    break;
                default:
                    for (var i = 0; i < response.length; i++) {
                        var message = response[i];
                        me.lastMessageId = message.mid;

                        console.log(message);

                        if (message.name === 'online') {
                            addNewUser(message.contents.msg);
                        } else if (message.name === 'offline') {
                            removeUser(message.contents.msg);
                        } else if (message.name === 'ChatMessage') {
                            addShowMsg('get', message.contents);

                            playSound('music/message.wav');
                        }
                    }
                    if (me.enabled) {
                        me.connect();
                    }
                    break;
            }
        },
        failure: function (response) {
        }
    });
};

Comet.prototype.send = function (obj) {
    var me = this;
    $.ajax({
        url: me.sendhandler,
        dataType: 'json',
        global: false,
        data: {
            action: 'send',
            privateToken: me.privateToken,
            toUser: obj.toUser,
            msg: obj.msg
        },
        success: function (response) {
            var result = response;
            obj.callback && obj.callback.call(obj.scope || me, result);
        },
        failure: function () {
            alert('对方可能已经下线，请检查右边的在线列表。');
        }
    });
};
//上线
Comet.prototype.online = function () {
    var me = this;
    $.ajax({
        url: me.sendhandler,
        dataType: 'json',
        global: false,
        data: {
            publicToken: me.publicToken
        },
        success: function (response) {
            me.privateToken = response.privateToken;

            var users = [];
            $.each(response.users, function (n, user) {
                if (user !== me.publicToken) {
                    users.push({ text: user });
                }
            });
            addAllUser(users);
            me.connect();
        },
        failure: function () {
            me.reconnect();
        }
    });
};
Comet.prototype.reconnect = function () {
    var me = this;
    me.lastMessageId = 0;
    setTimeout(function () {
        me.online();
    }, getRandom(30, 90) * 1000);
};
//下线
Comet.prototype.offline = function (callback) {
    var me = this;
    me.enabled = false;
    $.ajax({
        url: me.sendhandler,
        dataType: 'json',
        global: false,
        data: {
            action: 'offline',
            privateToken: me.privateToken
        },
        success: function (response) {
            callback && callback();
        },
        failure: function () {
        }
    });
};

//得到两个数之间的随机数
function getRandom(lowerValue, upperValue) {
    var choices = upperValue - lowerValue + 1;
    var result = Math.floor(Math.random() * choices + lowerValue);
    return result;
}