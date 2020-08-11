using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace celia.game {
    public class FBEventProxy : SingleClass<FBEventProxy>
    {
        public FBEventProxy() { 
        
        }
        public void test(FBEventType fBEventType) {
            switch (fBEventType) {
                case FBEventType.ActivatedApp:
                    LogActivatedAppEvent();
                    break;
                case FBEventType.CompletedTutorial:
                    LogCompleteTutorialEvent();
                    break;
                case FBEventType.AchievedLevel:
                    LogAchieveLevelEvent();
                    break;
                case FBEventType.Purchased:
                    LogPurchasedEvent();
                    break;
                case FBEventType.Custom:
                    LogCustomEvent();
                    break;
            }
        }
        /// <summary>
        /// 自定义统计事件
        /// </summary>
        public void LogCustomEvent()
        {

        }
        /// <summary>
        /// 玩家点击应用图标，打开游戏，触发该事件
        /// </summary>
        public void LogActivatedAppEvent() { 
        }

        /// <summary>
        /// 玩家每次成功完成付费购买，触发该事件
        /// </summary>
        public void LogPurchasedEvent() {

        }
        /// <summary>
        /// 玩家通过关卡“1-2”后，触发该事件
        /// </summary>
        public void LogCompleteTutorialEvent()
        {

        }
        /// <summary>
        /// 玩家通过关卡“1-13”后，触发该事件
        /// </summary>
        public void LogAchieveLevelEvent()
        {

        }

    }
    public enum FBEventType
    {
        /// <summary>
        /// 玩家点击应用图标，打开游戏，触发该事件
        /// </summary>
        ActivatedApp,
        /// <summary>
        /// 玩家通过关卡“1-2”后，触发该事件
        /// </summary>
        CompletedTutorial,
        /// <summary>
        /// 玩家通过关卡“1-13”后，触发该事件
        /// </summary>
        AchievedLevel,
        /// <summary>
        /// 玩家每次成功完成付费购买，触发该事件
        /// </summary>
        Purchased,
        Custom
    }
}

