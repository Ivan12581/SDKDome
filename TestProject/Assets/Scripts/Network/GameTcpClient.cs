using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Google.Protobuf;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace celia.game
{
    // 消息响应参数
    public class TcpClientEventArgs : EventArgs
    {
        public enum ErrorReason
        {
            ServerSideError,
            TimeoutError,
            Null
        }

        public readonly MsgWrap wrap;
        public readonly ByteString msg;
        public bool hasErrors = false;
        public ErrorReason errorReason = ErrorReason.Null;
        public TcpClientEventArgs(MsgWrap msgWrap)
        {
            this.wrap = msgWrap;
            this.msg = msgWrap.Msg;
        }
    }

    public class MsgHandle
    {
        /// <summary>
        /// 缓存消息的长度
        /// </summary>
        public const int LEN = 200;

        public MsgWrap wrap;
        public GameMessage msg;
        public MsgHandleState state;
        public List<int> completed_idx = new List<int>();

        public bool Add2Completed(MsgWrap wrap)
        {
            if (!completed_idx.Contains(wrap.OpIdIdx))
            {
                completed_idx.Add(wrap.OpIdIdx);
                return true;
            }
            return false;
        }

        public bool CheckWrap(MsgWrap wrap)
        {
            return completed_idx.Contains(wrap.Id);
        }

        public enum MsgHandleState
        {
            /// <summary>
            /// 等待处理
            /// </summary>
            WAITING,
            /// <summary>
            /// 已经处理
            /// </summary>
            IS_OVER,
        }
    }

    // 游戏网络客户端
    class GameTcpClient : SingleClass<GameTcpClient>
    {
        sbyte _receive_idx;           // 接收消息索引
        sbyte _send_idx;		        // 发送消息索引

        MessageQue<GameMessage> rcv_msg = new MessageQue<GameMessage>();// 接收消息队列
        MessageQue<GameMessage> snd_pkt = new MessageQue<GameMessage>();// 发送消息队列

        Thread readThread;             // 读线程
        Thread writeThread;            // 写线程
        Thread networkThread;          // 网络线程， 负责建立连接，启动Ping计时器和Update计时器

        Thread updateThread;             // Update线程

        System.Timers.Timer pingTimer = new System.Timers.Timer(); //ping timer

        // 接收消息事件回调
        public Dictionary<int, EventHandler<TcpClientEventArgs>> rcv_callback =
            new Dictionary<int, EventHandler<TcpClientEventArgs>>();

        // 网络事件回调
        public event EventHandler<EventArgs> connect_callback;
        public event EventHandler<EventArgs> close_callback;

        string remote_ip;           // 服务器地址
        uint remote_port;           // 服务器端口
        TcpClient tcpClient;        // Tcp网络客户端
        bool closed = true;         // 客户端关闭状态(true:关闭,false:开启)
        bool close_call = false;

        int _ping_out;              // ping超时次数(发送递增,接收递减.超过三次断线)
        const int kick_out_time = 5; // 断开服务器的ping收发差值次数

        public void Connect(string ip, uint port)
        {
            LogHelper.Log("Connect...");
            Close();

            remote_ip = ip;
            remote_port = port;

            reset();
        }

        /// <summary>
        /// 重新连接，需要注意，重新连接逻辑服务器会触发LogicProcessor下的OnLogicMsgL2CInitialize监听方法
        /// TODO 目前该方法对认证服务器的重连行为未做考虑和处理
        /// 需要注意的是，并不是任何时候的断线状态都可以发起重连，如果已经长时期掉线, 此次的key已经变化，就需要重新登陆
        /// </summary>
        public void Reconnect()
        {
            LogHelper.Log("Reconnect close");
            Close();
            reset(false);
        }

        /// <summary>
        /// 判断是否可以重新连接
        /// </summary>
        /// <returns></returns>
        public bool CanReconnect()
        {
            // 如果已经超时5分钟以上, 则判断不可以重连
            if (_ping_out >= 60 * 5)
            {
                return false;
            }
            return true;
        }

        public bool reconnectWaitForSel = false;

        public void ReturnToLoginPopup(string tip = "连接超时，请重新登陆")
        {
            Debug.Log("连接超时，请重新登陆!!!!");

            reconnectWaitForSel = true;

            MainThreadDispatcher.gi.Enqueue(() =>
            {
                string content = tip;

                reSendCallbacks.Clear();
                msgHandleMap.Clear();
                msgHandleList.Clear();
                waitForSend.Clear();

                // 确保关闭了转圈圈
                LogHelper.Log("stop wait all 163");
                PopupMessageManager.gi.StopAllLoading();
                if (networkThread != null && hasStarted)
                {
                    networkThread.Abort();
                }

                tcpClient = null;
                tryConnect = false;

                PopupMessageManager.gi.ShowPopupWindow(
                content,
                null,
                () =>
                {
                    //回到登录界面
                    DoReturnLogin();
                },
                null, true);
            });
        }

        /// <summary>
        /// 为了处理已经回到登陆界面时，之前的待处理消息才返回的情况
        /// </summary>
        public bool HasMsgHandle(string guid)
        {
            return msgHandleMap.ContainsKey(guid);
        }

        public MsgHandle.MsgHandleState GetMsgHandleState(string guid)
        {
            //消息是否处理过
            if (msgHandleMap.ContainsKey(guid))
            {
                return msgHandleMap[guid].state;
            }
            return MsgHandle.MsgHandleState.WAITING;
        }

        public void SetMsgState(string guid, MsgHandle.MsgHandleState state)
        {
            if (msgHandleMap.ContainsKey(guid))
            {
                msgHandleMap[guid].state = state;
                if (state == MsgHandle.MsgHandleState.IS_OVER &&
                   guid == NetworkManager.gi.curReconnectGUID)
                {
                    NetworkManager.gi.curReconnectGUID = "";
                }
            }
        }

        Dictionary<string, Action> reSendCallbacks = new Dictionary<string, Action>();
        public void Add2ReSendCallbacks(string msgGUID, Action callBack)
        {
            if (!reSendCallbacks.ContainsKey(msgGUID))
            {
                reSendCallbacks[msgGUID] = callBack;
            }
        }

        List<Action> reLoginCallbacks = new List<Action>();
        public void Add2ReLoginCallbacks(Action callBack)
        {
            if (!reLoginCallbacks.Contains(callBack))
            {
                reLoginCallbacks.Add(callBack);
            }
        }

        public void ReconnectPopup(string msgGUID, Action leftCallBack, Action rightCallBack)
        {
            Debug.Log("网络异常，重连提示");

            reconnectWaitForSel = true;

            //重发的回调列表
            Add2ReSendCallbacks(msgGUID, leftCallBack);
            //重登的回调列表
            Add2ReLoginCallbacks(rightCallBack);

            //如果提示界面同时只打开一次
            MainThreadDispatcher.gi.Enqueue(() =>
            {
                string content = "网络异常，是否重连?";
                string confirm = "重连";
                string cancel = "重登";

                // 确保关闭了转圈圈
                LogHelper.Log("stop wait all 260");
                PopupMessageManager.gi.StopAllLoading();
                //reconnectIsOpen = true;
                if (networkThread != null && hasStarted)
                {
                    networkThread.Abort();
                }

                tcpClient = null;
                tryConnect = false;

                PopupMessageManager.gi.ShowPopupWindow(content,
                () =>
                {
                    //left button
                    reconnectWaitForSel = false;
                    List<Action> callbacks = new List<Action>(reSendCallbacks.Values);
                    reSendCallbacks.Clear();
                    foreach (Action action in callbacks)
                    {
                        action?.Invoke();
                    }
                    callbacks.Clear();
                },
                () =>
                {
                    //mid button
                    Return2Login();
                },
                () =>
                {
                    //right button
                    Return2Login();
                },
                false);
            });

            void Return2Login()
            {
                reSendCallbacks.Clear();
                msgHandleMap.Clear();
                msgHandleList.Clear();
                waitForSend.Clear();

                reconnect_time = 0;
                reconnectWaitForSel = false;

                List<Action> callbacks = new List<Action>(reLoginCallbacks);
                reLoginCallbacks.Clear();
                foreach (Action action in callbacks)
                {
                    action?.Invoke();
                }
                callbacks.Clear();

                DoReturnLogin();
            }
        }

        void DoReturnLogin()
        {
            reconnect_time = 0;
            reconnectWaitForSel = false;
            NetworkManager.gi.curReconnectGUID = "";
            Close();
            /*
            //如果不是在登陆界面，则回到登录界面
            if (SceneManager.GetActiveScene().name != SceneMgr.LOGIN)
            {
                SceneTransition.gi.ToScene(SceneMgr.LOGIN, completedCallBack: (scene, obj) =>
                {
                    LogicProcessor.OnDisconnectReturnToLoginResetData();
                });
            }
            else
            {
                //检查是否有其它界面打开着
                if (View.LoginView.BindCodeorChangingNameIsOpen())
                {
                    SceneTransition.gi.ToScene(SceneMgr.LOGIN, completedCallBack: (scene, obj) =>
                    {
                        LogicProcessor.OnDisconnectReturnToLoginResetData();
                    });
                }
                else
                {
                    LogicProcessor.OnDisconnectReturnToLoginResetData();
                }
            }*/
        }

        public void Close()
        {
            if (tcpClient == null) return;

            lock (tcpClient)
            {
                if (closed) return;

                try
                {
                    tcpClient.Close();
                    closed = true;
                    close_call = true;
                    isInitReady = false;

                    LogHelper.Log("TCP close()");
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Close() Exception:{0}", e.ToString());
                }
            }
        }

        private Dictionary<string, MsgHandle> msgHandleMap = new Dictionary<string, MsgHandle>();
        private List<MsgHandle> msgHandleList = new List<MsgHandle>();
        public bool CheckMsgHandle(MsgWrap wrap)
        {
            //消息是否处理过
            if (!msgHandleMap.ContainsKey(wrap.OpId))
            {
                //真翔警告！！
                Debug.LogWarning("真香警告！！理论上来说，应该很小几率走到这逻辑！！");
                GameMessage msg = new GameMessage(wrap.ToByteArray())
                {
                    proto = wrap.Id,
                    guid = wrap.OpId,
                    connectionCheck = true
                };
                MsgHandle mhs = new MsgHandle { wrap = wrap, msg = msg, state = MsgHandle.MsgHandleState.WAITING };
                msgHandleMap[wrap.OpId] = mhs;
                msgHandleList.Add(mhs);
            }
            else
            {
                Debug.Log("查找到有未处理的消息: " + wrap.Id + " guid: " + wrap.OpId + " index: " + wrap.OpIdIdx);
            }
            //opid 为空的应该是服务器主动推过来的登录询问协议之类的
            if (wrap.OpId != "")
            {
                return msgHandleMap[wrap.OpId].Add2Completed(wrap);
            }
            else
            {
                return true;
            }
        }

        private float ping_time = -1;
        const string HEART_BEAT = "HEART_BEAT";
        public void Ping()
        {
            if (!shouldPing)
            {
                return;
            }

            ++_ping_out;
            //Debug.Log("Ping Out值为" + _ping_out);
         //   LogHelper.Log("Ping: " + _ping_out);
            SendPkt(new GameMessage() { guid = HEART_BEAT });
        }

        public void OnDestroy()
        {
            CloseAllTimer();
            Close();

            if (readThread != null)
            {
                readThread.Abort();
            }
            if (writeThread != null)
            {
                writeThread.Abort();
            }
            if (updateThread != null)
            {
                updateThread.Abort();
            }
            if (networkThread != null)
            {
                networkThread.Abort();
            }

            shouldPing = false;
        }

        /// <summary>
        /// 连入服务器，重设所有连接状态
        /// </summary>
        /// <param name="clearKeyMessage">是否清空关键消息，用以在重连时保留未能成功发送出去的请求</param>
        void reset(bool clearKeyMessage = true)
        {
            CloseAllTimer();

            // 重置消息的标记
            _receive_idx = 0;
            _send_idx = 0;

            // 清空消息队列
            {
                if (readThread != null && hasStarted)
                {
                    Debug.Log("中止线程 ReadThread");
                    readThread.Abort();
                }
                if (writeThread != null && hasStarted)
                {
                    Debug.Log("中止线程 WriteThread");
                    writeThread.Abort();
                }
                if (networkThread != null && hasStarted)
                {
                    Debug.Log("中止线程 NetWorkThread");
                    networkThread.Abort();
                    tcpClient = null;
                    tryConnect = false;
                }

                hasStarted = false;

                rcv_msg.Clear();

                //清空消息队列
                if (clearKeyMessage)
                {
                    snd_pkt.Clear();
                    msgHandleMap.Clear();
                    msgHandleList.Clear();
                }
                else
                {
                    var temp_queue = snd_pkt;
                    snd_pkt = new MessageQue<GameMessage>();
                    while (!temp_queue.Empty())
                    {
                        if (temp_queue.Peek().connectionCheck)
                        {
                            Debug.Log("有关键消息尚未发出，在重连时重新加入等待发送队列");
                            GameMessage msg = temp_queue.Peek();
                            Add2WaitForSend(msg);
                        }
                        temp_queue.Dequeue();
                    }
                }
            }

            // 读线程
            {
                readThread = new Thread(new ThreadStart(do_read));
                readThread.Name = "readThread";
                readThread.IsBackground = true;
            }

            // 写线程
            {
                writeThread = new Thread(new ThreadStart(do_write));
                writeThread.Name = "writeThread";
                writeThread.IsBackground = true;
            }

            // update线程
            {
                if (updateThread == null)
                {
                    updateThread = new Thread(new ThreadStart(Update));
                    updateThread.Name = "updateThread";
                    updateThread.IsBackground = true;
                }
            }

            //ping timer
            {
                pingTimer.Enabled = true;
                // 间隔为x秒
                pingTimer.Interval = 1000;
                pingTimer.Elapsed += PingTimerElapsedHandler;
                pingTimer.Start();
            }

            // 网络线程来管理读写线程 
            {
                networkThread = new Thread(new ThreadStart(NetworkThreadStart));
                networkThread.Name = "networkThread";
                networkThread.IsBackground = true;
                networkThread.Start();
            }
        }

        void PingTimerElapsedHandler(object sender, System.Timers.ElapsedEventArgs e)
        {
            Ping();
        }

        public void CloseAllTimer()
        {
            // 关闭所有Timer保证状态重置
            pingTimer.Stop();
            pingTimer.Elapsed -= PingTimerElapsedHandler;

            pingUnpackStarted = false;
            ping_time = unpack_time = 0;
        }

        bool hasStarted = false;
        bool pingUnpackStarted = false;

        int wait_time = 3500;

        //用户此次选择重连的次数
        int reconnect_time = 0;
        int reConnectFaildTimesLimit = 3;

        public bool shouldPing = true;
        /// <summary>
        /// 用来标记是否正在进行连接的变量
        /// </summary>
        public bool tryConnect = false;
        /// <summary>
        /// 角色创建状态
        /// </summary>
        public bool isNoCharacter = false;
        /// <summary>
        /// time scale
        /// </summary>
        public float timeScale = 1;

        void NetworkThreadStart()
        {
            Debug.Log("网络线程:开始运行");
            hasStarted = true;
            tryConnect = true;
            isInitReady = false;
            isNoCharacter = false;

            LogHelper.Log("网络线程开始");

            /// 尝试重新连接的次数
            int reconnectTimes = 0;

            TryConnect2Server();

            void TryConnect2Server()
            {
                try
                {
                    if (reconnectWaitForSel) return;

                    tcpClient = new TcpClient(remote_ip, (int)remote_port);

                    MainThreadDispatcher.gi.Enqueue(() =>
                    {
                        LogHelper.Log("stop wait tcp 582");
                        PopupMessageManager.gi.StopWait(type: PopupMessageManager.LoadingType.TCP);
                    });

                    Debug.Log("网络线程:TCP连接成功");

                    closed = false;
                    tryConnect = false;

                    readThread.Start();
                    writeThread.Start();

                    if (!updateThread.IsAlive)
                    {
                        updateThread.Start();
                    }

                    pingUnpackStarted = true;

                    _ping_out = 0;
                    reconnect_time = 0;

                    connect_callback?.Invoke(this, null);
                }
                catch (Exception e)
                {
                    Debug.Log("start() Exception:{0}" + e.ToString());

                    tcpClient = null;
                    tryConnect = false;

                    if (reconnectWaitForSel) return;

                    // 如果逻辑服重连9次失败，要回到主界面，重新连认证服
                    if (reconnect_time >= reConnectFaildTimesLimit + 1)
                    {
                        Debug.Log("网络线程:逻辑服连接失败次数过多，返回登陆界面");

                        string tip = "";

                        switch (NetworkManager.gi.currentServerType)
                        {
                            case ServerType.Logic:
                                tip = "连接超时，请重新登陆";
                                break;
                            case ServerType.Authentication:
                            case ServerType.None:
                                tip = "连接超时，请重试";
                                break;
                            default:
                                break;
                        }

                        ReturnToLoginPopup(tip);
                    }
                    else
                    {
                        //每3次进行一次真香连接超时警告
                        if (reconnectTimes != 0 && reconnectTimes % 3 == 0)
                        {
                            Debug.Log("网络线程:多次重试连接失败,提示玩家选择操作");
                            //重连提示界面
                            ReconnectPopup(
                            "tcp_reconnect",
                            () =>
                            {
                                //left button
                                Debug.Log("网络线程:逻辑服连接，用户选择尝试重连");
                                reconnect_time++;
                                Reconnect();
                            },
                            () =>
                            {
                                //right button
                                if (networkThread != null && hasStarted)
                                {
                                    networkThread.Abort();
                                }
                            });
                        }
                        else
                        {
                            MainThreadDispatcher.gi.Enqueue(() =>
                            {
                                PopupMessageManager.gi.Wait(waitTime: wait_time * 3 / 1000, type: PopupMessageManager.LoadingType.TCP);
                                Debug.Log("当前服务器类型为" + NetworkManager.gi.currentServerType);
                                Debug.Log("网络线程:TCP连接失败，三秒后尝试重连");
                            });

                            //等待 x 秒
                            Thread.Sleep(wait_time);

                            //默认重连
                            Debug.Log("网络线程:连接，尝试重连");
                            ReConnect2Server();
                        }
                    }
                }
            }

            void ReConnect2Server()
            {
                reconnectTimes += 1;
                TryConnect2Server();
            }
        }

        void Update()
        {
            while (true)
            {
                Thread.Sleep(1);

                if (!pingUnpackStarted) continue;

                UnpackHandler();
            }
        }

        public List<GameMessage> waitForSend = new List<GameMessage>();
        public bool isInitReady = false;
        public static int OPID = 0;//调试查看消息时可以使用这个代替Guid
        /// <summary>
        /// 发包的方法，包含了自动重连相关的逻辑，用以保证流程的可靠性
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="pkt"></param>
        /// <param name="connectionCheck">是否进行网络连接检查，如果进行检查，则会在网络连接断开时进行自动重连相关逻辑</param>
        public GameMessage SendPkt(int proto, IMessage pkt, string guid = "", bool connectionCheck = false)
        {
            Debug.Log("预定发送:" + proto + " guid: " + guid);

            MsgWrap wrap = null;
            GameMessage msg = null;

            //新发的消息
            if (guid == "")
            {
                //msg wrap
                wrap = new MsgWrap
                {
                    Id = proto,
                    OpId = Guid.NewGuid().ToString(),//(++OPID).ToString(),
                    Msg = pkt.ToByteString()
                };

                //msg
                msg = new GameMessage(wrap.ToByteArray())
                {
                    proto = proto,
                    guid = wrap.OpId,
                    connectionCheck = connectionCheck
                };

                //记录消息处理情况
                if (!msgHandleMap.ContainsKey(wrap.OpId))
                {
                    MsgHandle mhs = new MsgHandle { wrap = wrap, msg = msg, state = MsgHandle.MsgHandleState.WAITING };
                    msgHandleMap[wrap.OpId] = mhs;
                    msgHandleList.Add(mhs);

                    //如果超出缓存的限制,则清除第一个
                    if (msgHandleMap.Count > MsgHandle.LEN)
                    {
                        msgHandleMap.Remove(msgHandleList[0].wrap.OpId);
                        msgHandleList.RemoveAt(0);
                    }
                }
            }
            else
            {
                //重发的
                MsgHandle msgHandle = msgHandleMap[guid];
                wrap = msgHandle.wrap;
                msg = msgHandle.msg;
            }

            // 如果开启网络检查，需要先保证网络的连接状态，直到确认网络连接畅通后再进行实际的发包
            if (connectionCheck)
            {
                LogHelper.Log("closed & tryConnect:" + closed + ":" + tryConnect + ":" + isInitReady);
                // 如果服务器已经关闭了，要进行重连，tryConnect 的判断是为了同时多条要重发的情况下，只存在一次重连尝试，其它重发的消息将缓存到waitforsend列表里
                if (closed && !tryConnect)
                {
                    Debug.Log("发送此关键消息时服务器已经关闭，需开始重连，在重连的回调中进行消息发送");
                    if (CanReconnect())
                    {
                        Reconnect();
                    }
                    else
                    {
                        ReturnToLoginPopup();
                        return msg;
                    }

                    //本次没发出去的
                    Add2WaitForSend(msg);
                }
                else
                {
                    //服务器准备好可以拉数据了 或 本身就是登陆协议，新用户起名协议等，直接发
                    if (isInitReady ||
                        wrap.Id == (int)LogicMsgID.LogicMsgC2LLogonProof)
                    {
                        //Debug.Log("网络畅通，直接发送关键消息");
                        SendPkt(msg);
                    }
                    else if (isNoCharacter && 
                             wrap.Id == (int)LogicMsgID.LogicMsgC2LCreateCharacterReq)
                    {
                        Debug.Log("网络畅通，直接发送创建角色消息");
                        SendPkt(msg);
                    }
                    else
                    {
                        //连上了但是还没有初始化完成时，加入等待
                        Debug.Log("网络不畅通，缓存住待发送的消息");
                        Add2WaitForSend(msg);
                    }
                }
            }
            else
            {
                // 不进行网络检查的话就直接发
                SendPkt(msg);
            }

            return msg;
        }

        void SendPkt(GameMessage pkt)
        {
            //心跳包或没有在发送等待队列里，则加入发送等待队列
            if (pkt.guid == HEART_BEAT ||
                !snd_pkt.Contains(pkt, (item1, item2) =>
                {
                    return item1.guid == item2.guid;
                }))
            {
                //Debug.Log("加入消息队列:" + pkt.guid);
                snd_pkt.Enqueue(pkt);
            }
        }

        bool Add2WaitForSend(GameMessage msg)
        {
            if (!waitForSend.Contains(msg))
            {
                Debug.Log("加入缓存发送的消息: " + msg.proto + " guid: " + msg.guid);
                waitForSend.Add(msg);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 发送缓存中的未发送消息
        /// </summary>
        public void SendWaitingMsg()
        {
            //全部缓存的消息
            foreach (GameMessage w_msg in waitForSend)
            {
                Debug.Log("重连成功，发送等待的关键消息: " + w_msg.proto + " guid: " + w_msg.guid);
                SendPkt(w_msg);
            }
        }

        /// <summary>
        /// 发送等待中的角色创建消息
        /// </summary>
        public void SendWaitingCreateCharacterReq()
        {
            foreach (GameMessage w_msg in waitForSend)
            {
                if (w_msg.proto == (int)LogicMsgID.LogicMsgC2LCreateCharacterReq)
                {
                    Debug.Log("重连成功，发送等待的创建角色消息: " + w_msg.proto + " guid: " + w_msg.guid);
                    SendPkt(w_msg);
                    break;
                }
            }
        }

        void do_read()
        {
            while (true)
            {
                Thread.Sleep(10);

                lock (tcpClient)
                {
                    if (closed) return;

                    try
                    {
                        NetworkStream stream = tcpClient.GetStream();
                        if (!stream.DataAvailable)
                            continue;

                        GameMessage _new_msg = new GameMessage();    // 读取中的消息缓存

                        // 读消息头
                        Read(stream, _new_msg, 0, GameMessage.header_length, () =>
                        {
                            // 解密消息头
                            _new_msg.decode_header(++_receive_idx);

                            if (_new_msg.BodyLength > 0)
                            {
                                // 读消息体
                                Read(stream, _new_msg, GameMessage.header_length, _new_msg.BodyLength, () =>
                                {
                                    // 读取完毕
                                    rcv_msg.Enqueue(_new_msg);
                                });
                            }
                            else
                            {
                                rcv_msg.Enqueue(_new_msg);
                            }
                        });

                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine("do_read() Exception:{0}", e.ToString());

                        LogHelper.Log("读线程异常");
                        Close();
                    }
                }
            }
        }

        void Read(NetworkStream stream, GameMessage msg, int offset, int length, Action callBack)
        {
            int readLength = stream.Read(msg.Data, offset, length);
            if (readLength < length)
            {
                Read(stream, msg, offset + readLength, length - readLength, callBack);
            }
            else
            {
                callBack.Invoke();
            }
        }

        private float unpack_time = -1;
        void UnpackHandler()
        {
            unpack_time++;

            if (unpack_time >= 30)
            {
                unpack_time = 0;

                // 消息检测
                if (!rcv_msg.Empty())
                {
                    do
                    {
                        if (0 < rcv_msg.Peek().BodyLength)
                        {
                            // 解压
                            MsgWrap wrap = MsgWrap.Parser.ParseFrom(rcv_msg.Peek().Data, 4, rcv_msg.Peek().BodyLength);

                            //Debug.Log("rev msg: " + wrap.Id + " guid: " + wrap.OpId + " index: " + wrap.OpIdIdx);

                            // 消息处理
                            if (rcv_callback.ContainsKey(wrap.Id))
                            {
                                //在这里处理完成标识
                                if (NetworkManager.gi.IsMultipleMsg(wrap) || CheckMsgHandle(wrap))
                                {
                                    Debug.Log("已处理消息: " + wrap.Id + " guid: " + wrap.OpId + " index: " + wrap.OpIdIdx);

                                    //如果重连界面打开着，则关闭
                                    if (reconnectWaitForSel)
                                    {
                                        reconnectWaitForSel = false;
                                        PopupMessageManager.gi.CloseTip(PopupMessageManager.PanelType.ReConnectTip);
                                    }

                                    MainThreadDispatcher.gi.Enqueue(() =>
                                    {
                                        if (rcv_callback[wrap.Id] == null)
                                        {
                                            Debug.Log("---rcv_callback[wrap.Id] == null--- ");
                                        }
                                        rcv_callback[wrap.Id]?.Invoke(this, new TcpClientEventArgs(wrap));
                                    });
                                }
                            }
                            else
                            {
                                UnityEngine.Debug.Log("未监听的消息:" + wrap.Id.ToString());
                            }
                        }
                        else
                        {
                            // ping 处理
                            _ping_out = Mathf.Max(0, --_ping_out);
                        }

                        // 处理完毕
                        rcv_msg.Dequeue();

                    } while (0 < rcv_msg.Count);
                }
                // ping超时检测
                if (!closed && kick_out_time <= _ping_out)
                {
                    Debug.LogError("TCP Client: Ping超时！关闭TCP Client");
                    LogHelper.Log("TCP Client: Ping超时");

                    Close();

                    _ping_out = 0;
                }
                // 关闭处理
                if (closed && close_call)
                {
                    close_callback?.Invoke(this, null);
                    close_call = false;
                }
            }
        }

        void do_write()
        {
            while (true)
            {
                Thread.Sleep(10);
                lock (tcpClient)
                {
                    if (closed)
                    {
                        return;
                    }

                    GameMessage currentMsg = null;

                    try
                    {
                        if (!snd_pkt.Empty())
                        {
                            NetworkStream stream = tcpClient.GetStream();
                            do
                            {
                                currentMsg = snd_pkt.Peek();
                                // 加密消息头
                                currentMsg.encode_header(++_send_idx);

                                MsgWrap wrap = MsgWrap.Parser.ParseFrom(currentMsg.Data, 4, currentMsg.BodyLength);
                                wrap.OpId = currentMsg.guid;

                                //Debug.Log("send msg: " + wrap.Id + " guid: " + wrap.OpId);

                                // 写消息
                                stream.Write(currentMsg.Data, 0, currentMsg.Length);

                                // 写完毕
                                // 此处需注意，会出现此时可能网络连接已经断开，
                                // 从等待列表里删除
                                if (waitForSend.Contains(currentMsg))
                                {
                                    Debug.Log("remove for wait send: " + currentMsg.proto + " guid: " + currentMsg.guid);
                                    waitForSend.Remove(currentMsg);
                                }

                                snd_pkt.Dequeue();

                                //调试代码
                              //  Messenger.DispatchEvent(Notif.TEST_SEND_MSG_TCP_CLOSE, wrap.Id);

                            } while (0 < snd_pkt.Count);
                        }
                    }
                    catch (Exception e)
                    {
                        // 因被服务器踢出而掉线一般会出现此异常，被远程主机拒绝
                        Debug.LogError("写线程异常 do_write() Exception:{0}" + e.ToString());
                        LogHelper.Log("写线程异常");
                        // 直接关了就可以了
                        Close();
                    }
                }
            }
        }
    }
}
