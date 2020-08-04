using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Globalization;
using System.Security.Cryptography;
using UnityEngine;

namespace celia.game
{
    // SRP Protocol Design(http://srp.stanford.edu/design.html)
    class SRP6
    {
        public static BigInteger N = BigInteger.Parse("0894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7", NumberStyles.AllowHexSpecifier);
        // A large safe prime (N = 2q+1, where q is prime)
        // All arithmetic is done modulo N.
        public static BigInteger g = 7;        // A generator modulo N
        public const int k = 3;     // Multiplier parameter (k = H(N, g) in SRP-6a, k = 3 for legacy SRP-6)
        
        public static BigInteger s;        // User's salt
        public static BigInteger A, B;        // Public ephemeral values
        public static BigInteger K, M1;
    };

    // 消息响应参数
    public class AuthEventArgs : EventArgs
    {
        public readonly NetState state;
        public AuthEventArgs(NetState _state)
        {
            state = _state;
        }
    }

    // 认证消息处理
    class AuthProcessor : SingleClass<AuthProcessor>
    {
        SHA1 sha = new SHA1CryptoServiceProvider();
        string I;           // 账号
        string p;           // 密码
        uint account_id;    // 账号ID

        LoginType loginType; // 登录类型
        uint guid;        // username+guid组成SessionKey

        string logic_ip;    // 游戏服ip
        uint logic_port;    // 游戏服端口
        
        // 网络事件回调
        public event EventHandler<AuthEventArgs> state_callback;

        public AuthProcessor()
        {

        }

        public void Init()
        {
            NetworkManager.gi.RegisterMsgHandler(AuthMsgID.AuthMsgA2CRegisterRep, 
            new EventHandler<TcpClientEventArgs>(OnAuthMsgA2CRegisterRep));

            NetworkManager.gi.RegisterMsgHandler(AuthMsgID.AuthMsgA2CLogonAsk, 
            new EventHandler<TcpClientEventArgs>(OnAuthMsgA2CLogonAsk));

            NetworkManager.gi.RegisterMsgHandler(AuthMsgID.AuthMsgA2CLogonRep, 
            new EventHandler<TcpClientEventArgs>(OnAuthMsgA2CLogonRep));

            NetworkManager.gi.RegisterMsgHandler(AuthMsgID.AuthMsgA2CLogonRegion, 
            new EventHandler<TcpClientEventArgs>(OnAuthMsgA2CLogonRegion));
        }

        public string Account
        {
            get { return I; }
        }

        public uint ID
        {
            get { return account_id; }
        }

        public byte[] SessionKey
        {
            get {
                if (loginType == LoginType.Account)
                {
                    return BigInteger2ByteArray(SRP6.K);
                }
                else
                {
                    // 该处MD5 byte[]数组顺序与服务端计算的相反，故进行反转
                    return new MD5CryptoServiceProvider().ComputeHash(new UTF8Encoding().GetBytes(Account + guid)).Reverse().ToArray();
                }
            }
        }

        public void RegistAccount(string name/*字母数字组合,最长16个字符*/, string key)
        {
            //System.Console.WriteLine("RegistAccount({0},{1})", name, key);

            I = name;
            p = key;

            c2a_register_req pkt = new c2a_register_req();
            {
                pkt.Username = name;
                pkt.Phone = "";
                pkt.Email = "";

                BigInteger s;
                {// 生成s
                    System.Random random = new System.Random();
                    byte[] data = new byte[32];
                    random.NextBytes(data);
                    data = data.Concat(new byte[] { 0 }).ToArray();
                    s = new BigInteger(data);

                    string _s = s.ToString("x");
                    pkt.S = (64 < _s.Length) ? _s.Substring(1, 64) : _s;
                }

                BigInteger x;// x>=0
                {// x = H(s,p)
                    byte[] s_p = BigInteger2ByteArray(s).Concat(Encoding.ASCII.GetBytes(key)).ToArray();
                    byte[] _x = sha.ComputeHash(s_p);
                    _x = _x.Concat(new byte[] { 0 }).ToArray();
                    x = new BigInteger(_x);
                }

                // v = g^x
                BigInteger v = BigInteger.ModPow(SRP6.g, x, SRP6.N);

                string _V = v.ToString("x");
                pkt.V = (64 < _V.Length) ? _V.Substring(1, 64) : _V;
            }

            NetworkManager.gi.SendPkt(AuthMsgID.AuthMsgC2ARegisterReq, pkt);

            // 状态变化
            state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_AUTH_REG));
        }

        private void OnAuthMsgA2CRegisterRep(object sender, TcpClientEventArgs e)
        {
            a2c_register_rep msg = a2c_register_rep.Parser.ParseFrom(e.msg);

            if (msg.Result != AccountOpResult.AorOk)
            {
                //System.Console.WriteLine("{0}, register error. reason:{1}", I, msg.Result);
                PopupMessageManager.gi.ShowInfo("注册/登录失败");
            }
            else
            {
                //System.Console.WriteLine("{0} register OK.", I);

                // 状态变化
                state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_AUTH_CHALLENGE));
            }
        }

        public void Login(string name, string key)
        {
            loginType = LoginType.Account;
            I = name;
            p = key;

            c2a_logon_challenge pkt = new c2a_logon_challenge();
            pkt.Username = name;

            NetworkManager.gi.SendPkt(AuthMsgID.AuthMsgC2ALogonChallenge, pkt);

            // 状态变化
            state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_AUTH_CHALLENGE));
        }
        public void LoginApple(string user, string identityTokenStr = "")
        {
            loginType = LoginType.Apple;

            c2a_logon_apple pkt = new c2a_logon_apple();
            pkt.UserIdentifier = user;
            
            string SessionKey = SDKManager.gi.AppleSessionKey;
            if (!string.IsNullOrEmpty(SessionKey))
            {
                pkt.SessionKey = SessionKey;
            }
            Debug.Log("--pkt.SessionKey--"+pkt.SessionKey);
            if (!string.IsNullOrEmpty(identityTokenStr))
            {
                pkt.IdentityToken = identityTokenStr;
            }

            NetworkManager.gi.SendPkt(AuthMsgID.AuthMsgC2ALogonApple, pkt);
            // 状态变化
            state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_AUTH_CHALLENGE));
        }
        public void LoginGameCenter(c2a_logon_apple_gamecenter pkt)
        {
            loginType = LoginType.GameCenter;

            //c2a_logon_apple_gamecenter pkt = new c2a_logon_apple_gamecenter();
            //pkt.UserIdentifier = user;
            //pkt.IdentityToken = identityTokenStr;

            NetworkManager.gi.SendPkt(AuthMsgID.AuthMsgC2ALogonAppleGamecenter, pkt);
            // 状态变化
            state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_AUTH_CHALLENGE));
        }
        public void LoginGoogle(string userID, string TokenStr)
        {
            c2a_logon_google pkt = new c2a_logon_google
            {
                GoogleId = userID,
                IdToken = TokenStr
            };
            loginType = LoginType.Google;

            NetworkManager.gi.SendPkt(AuthMsgID.AuthMsgC2ALogonGoogle, pkt);
            // 状态变化
            state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_AUTH_CHALLENGE));
        }
        public void LoginFaceBook(string userID, string TokenStr)
        {
            c2a_logon_facebook pkt = new c2a_logon_facebook
            {
                FacebookId = userID,
                AccessToken = TokenStr
            };
            loginType = LoginType.FaceBook;

            NetworkManager.gi.SendPkt(AuthMsgID.AuthMsgC2ALogonFacebook, pkt);
            // 状态变化
            state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_AUTH_CHALLENGE));
        }
        public static byte[] BigInteger2ByteArray(BigInteger v)
        {
            byte[] result = v.ToByteArray();

            if (result[result.Length - 1] == 0)
            {
                result = result.Take(result.Length - 1).ToArray();
            }

            return result;
        }

        private void OnAuthMsgA2CLogonAsk(object sender, TcpClientEventArgs e)
        {
            a2c_logon_ask msg = a2c_logon_ask.Parser.ParseFrom(e.msg);
            Debug.LogWarning("OnAuthMsgA2CLogonAsk: " + Newtonsoft.Json.JsonConvert.SerializeObject(msg));

            SRP6.B = BigInteger.Parse("0" + msg.PublicKey, NumberStyles.AllowHexSpecifier);
            SRP6.s = BigInteger.Parse("0" + msg.S, NumberStyles.AllowHexSpecifier);

            {
                BigInteger x;// x>=0
                {// x = H(s,p)
                    byte[] s_p = BigInteger2ByteArray(SRP6.s).Concat(Encoding.ASCII.GetBytes(p)).ToArray();
                    byte[] _x = sha.ComputeHash(s_p);
                    _x = _x.Concat(new byte[] { 0 }).ToArray();
                    x = new BigInteger(_x);
                }

                // v = g^x
                BigInteger v = BigInteger.ModPow(SRP6.g, x, SRP6.N);

                BigInteger a;
                {//(a>=0)
                    System.Random random = new System.Random();
                    byte[] data = new byte[19];
                    random.NextBytes(data);
                    data = data.Concat(new byte[] { 0 }).ToArray();
                    a = new BigInteger(data);
                }

                // A = g^a 
                SRP6.A = BigInteger.ModPow(SRP6.g, a, SRP6.N);

                BigInteger u;
                {// u=H(A,B) (u>=0)
                    sha.Initialize();

                    byte[] AB = BigInteger2ByteArray(SRP6.A).Concat(BigInteger2ByteArray(SRP6.B)).ToArray();
                    byte[] AB_hash = sha.ComputeHash(AB);
                    AB_hash = AB_hash.Concat(new byte[] { 0 }).ToArray();
                    u = new BigInteger(AB_hash);
                }

                // S=(B-kv)^(a+ux) (S为负取模修正)
                BigInteger S = BigInteger.ModPow(SRP6.B - SRP6.k * v, a + u * x, SRP6.N);
                if (S < 0) S += SRP6.N;

                {// K=H(S)
                    byte[] _K = new byte[40];

                    byte[] t = new byte[32];
                    BigInteger2ByteArray(S).CopyTo(t, 0);

                    byte[] _t = new byte[16];
                    byte[] hash_t;

                    // 偶数位
                    for (int i = 0; i < 16; ++i)
                        _t[i] = t[i * 2];

                    hash_t = sha.ComputeHash(_t);

                    for (int i = 0; i < 20; ++i)
                        _K[i * 2] = hash_t[i];

                    // 奇数位
                    for (int i = 0; i < 16; ++i)
                        _t[i] = t[i * 2 + 1];

                    hash_t = sha.ComputeHash(_t);

                    for (int i = 0; i < 20; ++i)
                        _K[i * 2 + 1] = hash_t[i];

                    // 保证_K转hex表示不丢失二进制位
                    _K = _K.Concat(new byte[] { 0 }).ToArray();
                    SRP6.K = new BigInteger(_K);
                }

                byte[] t3;
                {// t3= H(N) ^ H(g)
                    t3 = sha.ComputeHash(BigInteger2ByteArray(SRP6.N));
                    byte[] hash_g = sha.ComputeHash(BigInteger2ByteArray(SRP6.g));

                    for (int i = 0; i < 20; ++i)
                        t3[i] ^= hash_g[i];
                }

                byte[] t4;
                {// t4=_login
                    t4 = sha.ComputeHash(Encoding.ASCII.GetBytes(I.ToUpper()));
                }

                {// M1=t3>t4>s>A>B>K
                    byte[] _s = BigInteger2ByteArray(SRP6.s);
                    byte[] _A = BigInteger2ByteArray(SRP6.A);
                    byte[] _B = BigInteger2ByteArray(SRP6.B);
                    byte[] _K = BigInteger2ByteArray(SRP6.K);

                    byte[] _M1 = sha.ComputeHash(
                        t3.Concat(t4).ToArray().Concat(_s).ToArray().Concat(_A).ToArray().Concat(_B).ToArray().Concat(_K).ToArray());

                    // 保证_M1转hex表示不丢失二进制位
                    _M1 = _M1.Concat(new byte[] { 0 }).ToArray();

                    // 加密校验值
                    SRP6.M1 = new BigInteger(_M1);
                }
            }

            {
                c2a_logon_proof pkt = new c2a_logon_proof();
                pkt.A = SRP6.A.ToString("x");
                pkt.M1 = SRP6.M1.ToString("x");

                NetworkManager.gi.SendPkt(AuthMsgID.AuthMsgC2ALogonProof, pkt);
            }

            // 状态变化
            state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_AUTH_PROOF));
        }

        /// <summary>
        /// 通过SDK Token的登录接口
        /// </summary>
        public void Login(string sdkType, string token, string appKey, string appID, string cchID)
        {
            loginType = LoginType.SDKToken;
            c2a_logon_sdk pkt = new c2a_logon_sdk()
            {
                SdkType = sdkType,
                AccessToken = token,
                AppId = appID,
                AppKey = appKey,
                CchId = cchID
            };

            //Debug.LogWarning("SDK Login: \n" + Newtonsoft.Json.JsonConvert.SerializeObject(pkt));
            NetworkManager.gi.SendPkt(AuthMsgID.AuthMsgC2ALogonSdk, pkt);
            // 状态变化
            state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_AUTH_PROOF));
        }

        /// <summary>
        /// SuperLogin
        /// </summary>
        public void Login(string account)
        {
            loginType = LoginType.Super;
            c2a_logon_super pkt = new c2a_logon_super() { Username = account };

            NetworkManager.gi.SendPkt(AuthMsgID.AuthMsgC2ALogonSuper, pkt);
            // 状态变化
            state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_AUTH_PROOF));
        }

        private void OnAuthMsgA2CLogonRep(object sender, TcpClientEventArgs e)
        {
            a2c_logon_rep msg = a2c_logon_rep.Parser.ParseFrom(e.msg);
            Debug.LogWarning(Newtonsoft.Json.JsonConvert.SerializeObject(msg));

            if (msg.Result != AccountOpResult.AorOk)
            {
                switch (loginType)
                {
                    case LoginType.Account:
                        // 登陆失败进行注册
                        RegistAccount(I, p);
                        break;
                    case LoginType.SDKToken:
                    case LoginType.Super:
                    case LoginType.Apple:
                        if (msg.Result == AccountOpResult.AorPassWrongError)
                        {
                            //apple 登陆auth失败  SessionKey过期 重新登陆
                            SDKManager.gi.AppleSessionKey = "";
                            LoginApple(Account);
                        }
                        break;
                    case LoginType.GameCenter:
                    case LoginType.Google:
                    case LoginType.FaceBook:
                    default:
                        Messenger.DispatchEvent(Notif.LOGIN_FAIL);
                        break;
                }
            }
            else
            {
                account_id = msg.AccountId;
                I = msg.Username;
                guid = msg.Guid;
                if (loginType == LoginType.Apple)
                {
                    string SessionKey = msg.SessionKey;
                    if (!string.IsNullOrEmpty(SessionKey))
                    {
                        SDKManager.gi.AppleSessionKey = SessionKey;
                        //PlayerPrefs.SetString("AppleSessionKey", SessionKey);
                    }
                }

                // 状态变化
                state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_AUTH_WAIT_LOGIC_INFO));
            }
        }

        private void OnAuthMsgA2CLogonRegion(object sender, TcpClientEventArgs e)
        {
            a2c_logon_region msg = a2c_logon_region.Parser.ParseFrom(e.msg);

            logic_ip = msg.Ip;
            logic_port = msg.Port;

            Debug.Log("认证服:获取到逻辑服IP, 即将连接逻辑服  IP : " + logic_ip + " Port: " + logic_port);

            // 状态变化
            state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_AUTH_LOGIC_INFO));

            Debug.Log("认证服:断开到认证服的连接");
            GameTcpClient.gi.Close();
        }

        public void ConnectingLogic()
        {
            Debug.LogWarningFormat("Logic {0}:{1} connecting...", logic_ip, logic_port);
            
            // 状态变化
            state_callback?.Invoke(this, new AuthEventArgs(NetState.NET_STATE_LOGIC_CONNECTING));

            GameTcpClient.gi.Connect(logic_ip, logic_port);
            
        }
    }
}
