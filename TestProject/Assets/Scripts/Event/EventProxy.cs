using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace celia.game {
    public class EventProxy
    {
        /// <summary>
        /// 玩家点击应用图标，打开游戏，触发该事件
        /// </summary>
        public virtual void LogLaunchEvent() { }
        /// <summary>
        /// 在“点击屏幕继续”界面完成点击(进入账户登陆界面)，触发该事件
        /// </summary>
        public virtual void LogLoginEvent() { }
        /// <summary>
        /// 玩家每次成功完成付费购买，触发该事件
        /// </summary>
        public virtual void LogPurchaseEvent() { }

    }
}

