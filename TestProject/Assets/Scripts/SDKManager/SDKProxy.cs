using System;
using System.Collections.Generic;
using UnityEngine;

namespace celia.game
{
    public class SDKProxy
    {
        /// <summary>
        /// SDK初始化
        /// </summary>
        public virtual void Init() { }
        /// <summary>
        /// SDK登录
        /// </summary>
        public virtual void Login(SDKLoginType Type) { }
        /// <summary>
        /// SDK切换帐号
        /// </summary>
        public virtual void Switch() { }
        /// <summary>
        /// SDK拉起支付
        /// </summary>
        /// <param name="jsonString">支付需要的参数，json字串</param>
        public virtual void Pay(string jsonString) { }
        /// <summary>
        /// 上报数据
        /// </summary>
        /// <param name="jsonString">上报规定的数据，json字串，uploadType:0创角1进入2升级</param>
        public virtual void UploadInfo(string jsonString) { }
        /// <summary>
        /// 获取SDK、设备信息
        /// </summary>
        public virtual void GetConfigInfo() { }
        /// <summary>
        /// AlHelp客服界面
        /// </summary>
        /// <param name="jsonString">打开指定的界面所需要的参数，json字串，uploadType:0创角1进入2升级</param>
        public virtual void CustomerService(string jsonString){ }
        /// <summary>
        /// 分享
        /// </summary>
        /// <param name="jsonString">分享平台和具体信息等数据</param>
        public virtual void Share(string jsonString) { }
        /// <summary>
        /// 退出游戏
        /// </summary>
        public virtual void ExitGame() { }
    }
}