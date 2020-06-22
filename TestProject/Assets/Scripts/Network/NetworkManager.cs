using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Google.Protobuf;

namespace celia.game
{
    public enum NetState
    {
        NET_STATE_NULL,
        NET_STATE_AUTH_CONNECTING, // 连接登录服
        NET_STATE_AUTH_REG, // 注册账号
        NET_STATE_AUTH_CHALLENGE, // 账号登陆
        NET_STATE_AUTH_PROOF, // 账号认证
        NET_STATE_AUTH_WAIT_LOGIC_INFO, // 等待Logic连接信息
        NET_STATE_AUTH_LOGIC_INFO, // Logic连接信息
        NET_STATE_LOGIC_CONNECTING, // 连接游戏服
        NET_STATE_LOGIC_PROOF_REQ, // Logic认证
        NET_STATE_LOGIC_IN_GAME, // 游戏中
    }

    public enum ServerType
    {
        /// <summary>
        /// 逻辑服
        /// </summary>
        /// <typeparam name="NetworkManager"></typeparam>
        Logic,
        /// <summary>
        /// 认证服
        /// </summary>
        /// <typeparam name="NetworkManager"></typeparam>
        Authentication,
        /// <summary>
        /// 没服
        /// </summary>
        /// <typeparam name="NetworkManager"></typeparam>
        None
    }

    public enum LoginType
    {
        Account,
        SDKToken,
        Super,
        Apple,
    }

    public class NetworkManager : SingleMonoBehaviour<NetworkManager>
    {
        private const float WAIT_TIME = 3f;
        private const int RECONNECT_LIMIT = 12;

        private bool CheckReLogin(GameMessage msg)
        {
            if (msg.reconnectCount >= RECONNECT_LIMIT)
            {
                return true;
            }
            return false;
        }

        NetState state = NetState.NET_STATE_NULL;
        public ServerType currentServerType = ServerType.None;
        LoginType loginType;
        string account;
        string password;
        string token;

        NetworkManager()
        {
            // 网络模块
            GameTcpClient.Create();
            GameTcpClient.gi.connect_callback += Gi_connect_callback;
            GameTcpClient.gi.close_callback += Gi_close_callback;

            // 认证服消息处理
            AuthProcessor.Create();
            AuthProcessor.gi.state_callback += Gi_state_callback;

            // 游戏服消息处理
            LogicProcessor.Create();
            LogicProcessor.gi.state_callback += Gi_state_callback;
        }

        /// <summary>
        /// 账号密码登录
        /// </summary>
        public void ConnectAuth_Login(string _account, string _password)
        {
            account = _account;
            password = _password;
            loginType = LoginType.Account;

            // 连接认证服
            state = NetState.NET_STATE_AUTH_CONNECTING;
            Debug.Log(Utils.ip + "/" + Utils.port);
            GameTcpClient.gi.Connect(Utils.ip, Utils.port);
        }
        public void ConnectAuth_LoginApple(string _account, string _password)
        {
            account = _account;
            password = _password;
            loginType = LoginType.Apple;

            // 连接认证服
            state = NetState.NET_STATE_AUTH_CONNECTING;
            Debug.Log(Utils.ip + "/" + Utils.port);
            GameTcpClient.gi.Connect(Utils.ip, Utils.port);
        }
        /// <summary>
        /// SDK Token登录
        /// </summary>
        public void ConnectAuth_Login(string _token)
        {
            token = _token;
            loginType = LoginType.SDKToken;

            // 连接认证服
            state = NetState.NET_STATE_AUTH_CONNECTING;
            GameTcpClient.gi.Connect(Utils.ip, Utils.port);
        }

        /// <summary>
        /// 账号登录
        /// </summary>
        public void ConnectAuth_SuperLogin(string _account)
        {
            account = _account;
            loginType = LoginType.Super;

            // 连接认证服
            state = NetState.NET_STATE_AUTH_CONNECTING;
            GameTcpClient.gi.Connect(Utils.ip, Utils.port);
        }

        /// <summary>
        /// 注册服务器消息监听
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="handler"></param>
        /// <param name="clearOtherHandler">是否清除其他监听以保证这是唯一的handler</param>
        public void RegisterMsgHandler(LogicMsgID msgId, EventHandler<TcpClientEventArgs> handler, bool clearOtherHandler = false)
        {
            RegisterMsgHandler((int)msgId, handler, clearOtherHandler);
        }

        /// <summary>
        /// 注册服务器消息监听
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="handler"></param>
        /// <param name="clearOtherHandler">是否清除其他监听以保证这是唯一的handler</param>
        public void RegisterMsgHandler(AuthMsgID msgId, EventHandler<TcpClientEventArgs> handler, bool clearOtherHandler = false)
        {
            RegisterMsgHandler((int)msgId, handler, clearOtherHandler);
        }

        /// <summary>
        /// 以Lambda形式注册服务器消息监听，以此形式注册的监听默认覆盖其他监听同消息的handler, 如不覆盖需要自己管理此方法的返回值
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="callback"></param>
        /// <param name="clearOtherHandler">是否清除其他监听以保证这是唯一的handler</param>
        public EventHandler<TcpClientEventArgs> RegisterMsgHandler(LogicMsgID msgId, Action<TcpClientEventArgs> callback, bool clearOtherHandler = true)
        {
            return RegisterMsgHandler((int)msgId, callback, clearOtherHandler);
        }

        /// <summary>
        /// 以Lambda形式注册服务器消息监听，以此形式注册的监听默认覆盖其他监听同消息的handler, 如不覆盖需要自己管理此方法的返回值
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="callback"></param>
        /// <param name="clearOtherHandler">是否清除其他监听以保证这是唯一的handler</param>
        public EventHandler<TcpClientEventArgs> RegisterMsgHandler(AuthMsgID msgId, Action<TcpClientEventArgs> callback, bool clearOtherHandler = true)
        {
            return RegisterMsgHandler((int)msgId, callback, clearOtherHandler);
        }

        internal EventHandler<TcpClientEventArgs> RegisterMsgHandler(int msgId, Action<TcpClientEventArgs> callback, bool clearOtherHandler = false)
        {
            EventHandler<TcpClientEventArgs> handler = (s, e) =>
            {
                Debug.Log("客户端收到消息MSG" + msgId);
                callback?.Invoke(e);
            };

            RegisterMsgHandler(msgId, handler, clearOtherHandler);

            return handler;
        }

        /// <summary>
        /// 一直监听的消息
        /// </summary>
        public List<int> alwaysMsg = new List<int>();
        public bool IsAlwaysMsg(MsgWrap wrap)
        {
            return alwaysMsg.Contains(wrap.Id);
        }

        internal void RegisterMsgHandler(int msgId, EventHandler<TcpClientEventArgs> handler, bool clearOtherHandler)
        {
            if (!alwaysMsg.Contains(msgId))
            {
                alwaysMsg.Add(msgId);
            }

            if (clearOtherHandler)
            {
                GameTcpClient.gi.rcv_callback[msgId] = handler;
            }
            else
            {
                if (GameTcpClient.gi.rcv_callback.ContainsKey(msgId))
                {
                    GameTcpClient.gi.rcv_callback[msgId] += handler;
                }
                else
                {
                    GameTcpClient.gi.rcv_callback.Add(
                    (int)msgId, handler);
                }
            }
        }

        /// <summary>
        /// 注册只响应一次的服务器消息监听
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="callback"></param>
        public EventHandler<TcpClientEventArgs> RegisterOnceMsgHandler(LogicMsgID msgId, Action<TcpClientEventArgs> callback)
        {
            return RegisterOnceMsgHandler((int)msgId, callback);
        }

        /// <summary>
        /// 注册只响应一次的服务器消息监听
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="callback"></param>
        public EventHandler<TcpClientEventArgs> RegisterOnceMsgHandler(AuthMsgID msgId, Action<TcpClientEventArgs> callback)
        {
            return RegisterOnceMsgHandler((int)msgId, callback);
        }

        internal EventHandler<TcpClientEventArgs> RegisterOnceMsgHandler(int msgId, Action<TcpClientEventArgs> callback)
        {
            EventHandler<TcpClientEventArgs> handler = null;
            if (GameTcpClient.gi.rcv_callback.ContainsKey(msgId))
            {
                GameTcpClient.gi.rcv_callback[msgId] += handler = (s, e) =>
                {
                    GameTcpClient.gi.rcv_callback[msgId] -= handler;

                    LogHelper.Log("stop wait msg 247:"+msgId);
                    PopupMessageManager.gi.StopWait(type: PopupMessageManager.LoadingType.MSG);
                    callback?.Invoke(e);
                };
            }
            else
            {
                GameTcpClient.gi.rcv_callback.Add(
                msgId, handler = (s, e) =>
                {
                    GameTcpClient.gi.rcv_callback[msgId] -= handler;
                    LogHelper.Log("stop wait msg 258:"+msgId);
                    PopupMessageManager.gi.StopWait(type: PopupMessageManager.LoadingType.MSG);
                    callback?.Invoke(e);
                });
            }
            return handler;
        }

        public void RemoveMsgHandler(LogicMsgID msgId, EventHandler<TcpClientEventArgs> handler)
        {
            RemoveMsgHandler((int)msgId, handler);
        }

        internal void RemoveMsgHandler(int msgId, EventHandler<TcpClientEventArgs> handler)
        {
            if (GameTcpClient.gi.rcv_callback.ContainsKey((int)msgId))
            {
                GameTcpClient.gi.rcv_callback[msgId] -= handler;
            }
        }

        /// <summary>
        /// 发送消息给逻辑服务器, 逻辑服的请求都会进行网络连接检查以保证玩家游戏流程的连贯性
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="pkt"></param>
        public GameMessage SendPkt(LogicMsgID proto, IMessage pkt, string guid = "")
        {
            return GameTcpClient.gi.SendPkt((int)proto, pkt, guid, true);
        }

        /// <summary>
        /// 发送消息给授权服务器
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="pkt"></param>
        public void SendPkt(AuthMsgID proto, IMessage pkt)
        {
            GameTcpClient.gi.SendPkt((int)proto, pkt);
        }

        public string curReconnectGUID = "";
        /// <summary>
        /// 发送一个消息给服务器，同时注册一个只响应一次的回调, 一对一
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="pkt"></param>
        /// <param name="callbackProto"></param>
        /// <param name="callback"></param>
        async public void SendPktWithCallback(LogicMsgID proto, IMessage pkt, LogicMsgID callbackProto, Action<TcpClientEventArgs> callback, string guid = "")
        {
            PopupMessageManager.gi.Wait(waitTime: WAIT_TIME * 3, type: PopupMessageManager.LoadingType.MSG);

            bool flag = false;

            GameMessage send_msg = SendPkt(proto, pkt, guid);

            EventHandler<TcpClientEventArgs> errorCallback = null;
            EventHandler<TcpClientEventArgs> successCallback = null;

            errorCallback = RegisterOnceMsgHandler(LogicMsgID.LogicMsgL2CError, (args) =>
            {
                l2c_error msg = l2c_error.Parser.ParseFrom(args.msg);

                Debug.Log("收到错误协议， ID为" + msg.Id);
                LogHelper.Log("收到错误协议， ID为" + msg.Id);

                GameTcpClient.gi.SetMsgState(send_msg.guid, MsgHandle.MsgHandleState.IS_OVER);

                // 收到此协议的报错信息, 调用回调, 移除成功的回调
                if (msg.Id == proto)
                {
                    RemoveMsgHandler(callbackProto, successCallback);
                    args.hasErrors = true;
                    args.errorReason = TcpClientEventArgs.ErrorReason.ServerSideError;
                    flag = true;
                    callback?.Invoke(args);
                }
            });

            successCallback = RegisterOnceMsgHandler(callbackProto, (args) =>
            {
                GameTcpClient.gi.SetMsgState(send_msg.guid, MsgHandle.MsgHandleState.IS_OVER);

                LogHelper.Log("收到成功返回的协议， ID为" + ((int)proto));
                // 移除错误回调
                RemoveMsgHandler(LogicMsgID.LogicMsgL2CError, errorCallback);
                flag = true;
                callback?.Invoke(args);
            });

            await new WaitForSeconds(WAIT_TIME * (Time.timeScale == 0 ? 1 : Time.timeScale));

            //检查重发状态
            CheckReSend(flag, send_msg,
            () =>
            {
                //resend
                flag = true;
                RemoveMsgHandler(callbackProto, successCallback);
                RemoveMsgHandler(LogicMsgID.LogicMsgL2CError, errorCallback);

                SendPktWithCallback(proto, pkt, callbackProto, callback, send_msg.guid);
            },
            () =>
            {
                //remove msg handler
                LogHelper.Log("stop wait msg 364:"+((int)proto));
                PopupMessageManager.gi.StopWait(type: PopupMessageManager.LoadingType.MSG);
                flag = true;
                RemoveMsgHandler(callbackProto, successCallback);
                RemoveMsgHandler(LogicMsgID.LogicMsgL2CError, errorCallback);
            });
        }

        /// <summary>
        /// 分页消息记录列表
        /// </summary>
        private List<LogicMsgID> multipleMsgs = new List<LogicMsgID>();
        public bool IsMultipleMsg(MsgWrap wrap)
        {
            return multipleMsgs.Contains((LogicMsgID)wrap.Id);
        }

        /// <summary>
        /// 发送一个消息给服务器，响应多次相同的协议回调，用以处理分包的情况
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="pkt"></param>
        /// <param name="callbackProto">期望收到的消息的协议</param>
        /// <param name="callback">回调中包含是否需要错误处理，和收集了全部页的字典</param>
        /// <param name="frameFunc">这个要传入一个函数，参数为TcpClientEventArgs, 返回值为基于TcpClientEventArgs求出的当前页的索引和总页数</param>
        public void SendPktWithMultipleCallback(LogicMsgID proto,
                                                IMessage pkt,
                                                LogicMsgID callbackProto,
                                                Action<(bool hasErrors, Dictionary<int, TcpClientEventArgs> contentDic)> callback,
                                                Func<TcpClientEventArgs, (int frameIndex, int frameCount)> frameFunc,
                                                string guid = "")
        {
            var protoList = new List<LogicMsgID> { callbackProto };
            var funcList = new Dictionary<LogicMsgID, Func<TcpClientEventArgs, (int frameIndex, int frameCount)>>();
            funcList[callbackProto] = frameFunc;
            SendPktWithMultipleCallback(proto, pkt, protoList, (args) =>
            {
                callback.Invoke((args.hasErrors, args.dic.ContainsKey(callbackProto) ? args.dic[callbackProto] : new Dictionary<int, TcpClientEventArgs>()));
            }, funcList, guid);
        }


        /// <summary>
        /// 发送一个消息给服务器，响应多次不同的协议回调，用以处理分包的情况
        /// </summary>
        /// <param name="proto">协议枚举</param>
        /// <param name="pkt">协议包</param>
        /// <param name="callbackProto">回调协议</param>
        /// <param name="callback">结果回调</param>
        /// <param name="lengthFunc">用来计算分页信息的方法代理</param>
        public async void SendPktWithMultipleCallback(LogicMsgID proto,
                                                IMessage pkt,
                                                List<LogicMsgID> callbackProtos,
                                                Action<(bool hasErrors, Dictionary<LogicMsgID, Dictionary<int, TcpClientEventArgs>> dic)> callback,
                                                Dictionary<LogicMsgID, Func<TcpClientEventArgs, (int frameIndex, int frameCount)>> frameFuncs,
                                                string guid = "")
        {
            bool flag = false;

            PopupMessageManager.gi.Wait(waitTime: WAIT_TIME * 3, type: PopupMessageManager.LoadingType.MSG);

            GameMessage send_msg = SendPkt(proto, pkt, guid);

            //记录分页消息
            foreach (LogicMsgID logicMsgId in callbackProtos)
            {
                if (!multipleMsgs.Contains(logicMsgId))
                {
                    multipleMsgs.Add(logicMsgId);
                }
            }

            Dictionary<LogicMsgID, EventHandler<TcpClientEventArgs>> errorCallbacks = new Dictionary<LogicMsgID, EventHandler<TcpClientEventArgs>>();
            Dictionary<LogicMsgID, EventHandler<TcpClientEventArgs>> successCallbacks = new Dictionary<LogicMsgID, EventHandler<TcpClientEventArgs>>();

            void ClearSuccessHandlers()
            {
                foreach (var keyValue in successCallbacks)
                {
                    RemoveMsgHandler(keyValue.Key, keyValue.Value);
                }
            }

            void ClearErrorHandlers()
            {
                foreach (var keyValue in errorCallbacks)
                {
                    RemoveMsgHandler(keyValue.Key, keyValue.Value);
                }
            }

            var defaultFailedResult = new Dictionary<LogicMsgID, Dictionary<int, TcpClientEventArgs>>();
            void DealWithError()
            {

                LogHelper.Log("stop wait msg 458:"+((int)proto));
                PopupMessageManager.gi.StopWait(type: PopupMessageManager.LoadingType.MSG);

                flag = true;
                // 一旦发生错误情况，直接清除全部的回调
                ClearSuccessHandlers();
                ClearErrorHandlers();

                // 调用回调以供错误处理
                callback?.Invoke((true, defaultFailedResult));
            }

            var successResult = new Dictionary<LogicMsgID, Dictionary<int, TcpClientEventArgs>>();
            foreach (LogicMsgID logicMsgId in callbackProtos)
            {
                successResult[logicMsgId] = new Dictionary<int, TcpClientEventArgs>();
            }

            // 用来记录各个协议的分包总数量
            var countDic = new Dictionary<LogicMsgID, int>();
            // 用来记录已经收到的各个协议的分包数量
            var receivedDic = new Dictionary<LogicMsgID, int>();

            bool IsFinished()
            {
                foreach (LogicMsgID callbackProto in callbackProtos)
                {
                    if (!countDic.ContainsKey(callbackProto))
                    {
                        return false;
                    }

                    if (!receivedDic.ContainsKey(callbackProto))
                    {
                        return false;
                    }

                    if (countDic[callbackProto] != 0
                    && receivedDic[callbackProto] != countDic[callbackProto])
                    {
                        Debug.Log("收到的数量和总数量不一致, 收到" + receivedDic[callbackProto] + "总数" + receivedDic[callbackProto]);
                        return false;
                    }
                }
                return true;
            }

            foreach (LogicMsgID callbackProto in callbackProtos)
            {
                errorCallbacks[callbackProto] = null;
                successCallbacks[callbackProto] = null;

                var frameFunc = frameFuncs[callbackProto];

                errorCallbacks[callbackProto] = RegisterOnceMsgHandler(LogicMsgID.LogicMsgL2CError, (args) =>
                {
                    if (GameTcpClient.gi.GetMsgHandleState(send_msg.guid) == MsgHandle.MsgHandleState.WAITING)
                    {
                        l2c_error msg = l2c_error.Parser.ParseFrom(args.msg);

                        Debug.Log("收到错误协议， ID为" + msg.Id);

                        GameTcpClient.gi.SetMsgState(send_msg.guid, MsgHandle.MsgHandleState.IS_OVER);

                        // 收到此协议的报错信息, 调用回调, 移除成功的回调
                        if (msg.Id == proto)
                        {
                            DealWithError();
                            return;
                        }
                    }
                });

                successCallbacks[callbackProto] = RegisterMsgHandler(callbackProto, (args) =>
                {
                    var frame = frameFunc(args);
                    if (!countDic.ContainsKey(callbackProto))
                    {
                        countDic[callbackProto] = frame.frameCount;
                    }

                    var frameCount = countDic[callbackProto];
                    Debug.Log("收到消息" + callbackProto.ToString() + "总数为" + frameCount);

                    var result = successResult[callbackProto];
                    if (!result.ContainsKey(frame.frameIndex))
                    {
                        result[frame.frameIndex] = args;
                        if (!receivedDic.ContainsKey(callbackProto))
                        {
                            receivedDic[callbackProto] = 1;
                        }
                        else
                        {
                            receivedDic[callbackProto] = receivedDic[callbackProto] + 1;
                        }

                        // 这个协议收够了, 标为完成，同时检查其他的完成没
                        if (IsFinished())
                        {
                            if (GameTcpClient.gi.GetMsgHandleState(send_msg.guid) == MsgHandle.MsgHandleState.WAITING)
                            {
                                Debug.Log("已处理分页消息: " + (int)proto + ":" + args.wrap.Id + " guid: " + args.wrap.OpId);

                                GameTcpClient.gi.SetMsgState(send_msg.guid, MsgHandle.MsgHandleState.IS_OVER);

                                // 清除全部的回调
                                ClearSuccessHandlers();
                                ClearErrorHandlers();

                                flag = true;

                                // 调用成功回调
                                LogHelper.Log("stop wait msg 571:"+((int)proto));
                                PopupMessageManager.gi.StopWait(type: PopupMessageManager.LoadingType.MSG);
                                callback?.Invoke((false, successResult));
                            }
                            return;
                        }
                    }
                    // 如果重复收到同页，则错误处理
                    else
                    {
                        Debug.Log(callbackProto.ToString() + "收到多次分页, 分页索引为" + frame.frameIndex);
                        DealWithError();
                        return;
                    }
                });
            }

            // 超时处理，如果超过3秒还为触发成功回调，则触发错误处理
            await new WaitForSeconds(WAIT_TIME * (Time.timeScale == 0 ? 1 : Time.timeScale));
            CheckReSend(flag, send_msg,
            () =>
            {
                //resend
                flag = true;
                ClearSuccessHandlers();
                ClearErrorHandlers();

                SendPktWithMultipleCallback(proto, pkt, callbackProtos, callback, frameFuncs, send_msg.guid);
            },
            () =>
            {
                //remove msg handler
                // 一旦发生错误情况，直接清除全部的回调

                LogHelper.Log("stop wait msg 605:"+((int)proto));
                PopupMessageManager.gi.StopWait(type: PopupMessageManager.LoadingType.MSG);
                flag = true;
                ClearSuccessHandlers();
                ClearErrorHandlers();
            });

            return;
        }

        /// <summary>
        /// 检查重发的情况
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="send_msg"></param>
        /// <param name="reSendHandler"></param>
        /// <param name="reLoginHandler"></param>
        void CheckReSend(bool flag, GameMessage send_msg, Action reSendHandler, Action reLoginHandler)
        {
            //还没处理并且已经不在待处理的消息列表里
            if (!flag &&
                GameTcpClient.gi.HasMsgHandle(send_msg.guid) &&
                GameTcpClient.gi.GetMsgHandleState(send_msg.guid) == MsgHandle.MsgHandleState.WAITING)
            {
                //如果重连次数超过9次，重连提示只剩下 回到登陆 按钮
                if (!CheckReLogin(send_msg))
                {
                    Debug.Log("3秒内也没收到回复，重发此消息: " + send_msg.proto + " guid: " + send_msg.guid);
                    //每3次提示一次重连界面
                    if (send_msg.reconnectCount % 3 == 0)
                    {
                        //此时先判断最近是否有手动重连过
                        if (curReconnectGUID != "" && curReconnectGUID != send_msg.guid)
                        {
                            Debug.Log("最近有手动重连过: " + curReconnectGUID);
                            ReSend();
                        }
                        else
                        {
                            Debug.Log("NetworkManager:多次重试连接失败, 提示玩家选择操作");
                            //重连界面（重连和登陆 按钮）
                            GameTcpClient.gi.ReconnectPopup(
                            send_msg.guid,
                            () =>
                            {
                                //left button
                                curReconnectGUID = send_msg.guid;
                                ReSend(false);
                            },
                            () =>
                            {
                                //right button
                                reLoginHandler?.Invoke();
                            });
                        }
                    }
                    else
                    {
                        LogHelper.Log("不到3次的默认重发" + send_msg.proto);
                        ReSend();
                    }
                }
                else
                {
                    LogHelper.Log("准备弹出登录界面:" + send_msg.proto);
                    {
                        //提示回到登陆界面
                        reLoginHandler?.Invoke();
                        GameTcpClient.gi.ReturnToLoginPopup();
                    }
                }
            }
            else
            {
                //清除回调
                reLoginHandler?.Invoke();
            }

            void ReSend(bool check = true)
            {
                //如果有其它消息要触发自动重连时，检查一下重连界面是否已经打开，如果已打开的话，则加入此次的重连callback列表里，不自动重连
                //否则自动重连
                if (check && GameTcpClient.gi.reconnectWaitForSel)
                {
                    //reconnect callback
                    GameTcpClient.gi.Add2ReSendCallbacks(
                    send_msg.guid,
                    () =>
                    {
                        ReSendHandler();
                    });
                    //relogin callback
                    GameTcpClient.gi.Add2ReLoginCallbacks(
                    () =>
                    {
                        reLoginHandler?.Invoke();
                    });
                }
                else
                {
                    ReSendHandler();
                }

                void ReSendHandler()
                {
                    send_msg.reconnectCount++;
                    //重发消息
                    reSendHandler?.Invoke();
                }
            }
        }

        void Start()
        {
        }

        private void Gi_state_callback(object sender, AuthEventArgs e)
        {
            switch (state = e.state)
            {
                default:
                    {// do something
                    }
                    break;
            }
        }

        private void Gi_connect_callback(object sender, EventArgs e)
        {
            switch (state)
            {
                case NetState.NET_STATE_AUTH_CONNECTING:
                    {
                        currentServerType = ServerType.Authentication;

                        StartRepeatedlyPing();

                        switch (loginType)
                        {
                            case LoginType.Account:
                                AuthProcessor.gi.Login(account, password);
                                break;
                            case LoginType.SDKToken:
                                string sdkName = SDKManager.gi.IsOversea ? "oversea" : "chinese";
                                string appKey = SDKManager.gi.SDKParams.AppKey;
                                string appID = SDKManager.gi.PackageParams.AppID;
                                string cchID = SDKManager.gi.PackageParams.CCHID;
                                AuthProcessor.gi.Login(sdkName, token, appKey, appID, cchID);
                                //AuthProcessor.gi.Login(sdkName, setting.Android.PakageName, setting.Android.AppKey, setting.Android.AppId, setting.Android.CchId);
                                break;
                            case LoginType.Super:
                                AuthProcessor.gi.Login(account);
                                break;
                            case LoginType.Apple:
                                AuthProcessor.gi.LoginApple(account, password);
                                break;
                        }
                    }
                    break;
                case NetState.NET_STATE_LOGIC_CONNECTING:
                    {
                        currentServerType = ServerType.Logic;

                        StartRepeatedlyPing();
                    }
                    break;
            }
        }

        private void Gi_close_callback(object sender, EventArgs e)
        {
            StopRepeatedlyPing();

            switch (state)
            {
                case NetState.NET_STATE_AUTH_LOGIC_INFO:
                    {
                        AuthProcessor.gi.ConnectingLogic();
                    }
                    break;
                case NetState.NET_STATE_LOGIC_IN_GAME:
                    {
                        // 此处进入游戏成功
                        Debug.LogWarningFormat("{0}:{1} login SUCCEED.", account, password);
                        StartRepeatedlyPing();
                    }
                    break;
            }
        }

        void StartRepeatedlyPing()
        {
            GameTcpClient.gi.shouldPing = true;
        }

        void StopRepeatedlyPing()
        {
            GameTcpClient.gi.shouldPing = false;
        }
    }
}
