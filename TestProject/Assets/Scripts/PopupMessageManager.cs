using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace celia.game
{
    public class PopupMessageManager : SingleMonoBehaviour<PopupMessageManager>
    {
        public enum PanelType
        {
            LOADING,
            /// <summary>
            /// 文字提示
            /// </summary>
            INFO,
            /// <summary>
            /// 约会的解锁
            /// </summary>
            CommonTip,
            /// <summary>
            /// 好感度
            /// </summary>
            GoodFeeling,
            /// <summary>
            /// 角色初见
            /// </summary>
            FirstMeet,
            /// <summary>
            /// 好感度
            /// </summary>
            LikingValue,
            /// <summary>
            /// 解锁
            /// </summary>
            UnlockedInfo,
            /// <summary>
            /// 奖励
            /// </summary>
            RewardPanel,
            /// <summary>
            /// BuleTip
            /// </summary>
            BuleTip,
            /// <summary>
            /// 购买提示
            /// </summary>
            BuyTip,
            /// <summary>
            /// 重连提示
            /// </summary>
            ReConnectTip,
            /// <summary>
            /// 小提示
            /// </summary>
            SmallTip,
        }
    

        public enum BlueTipType
        {
            Confirm,
            Cancel,
            ConfirmAndCancel,
        }

        public enum LoadingType
        {
            Default,
            MSG,
            TCP,
        }


        private void Awake()
        {

        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += SceneLoaded;
            //Messenger.AddEventListener<BaseView>(Notif.VIEW_OPEN, ViewOpen);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            //Messenger.RemoveEventListener<BaseView>(Notif.VIEW_OPEN, ViewOpen);
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //Login MainStage Battle
            //if (ViewMgr.uiRoot != null)
            //{
            //    GameObject obj = GameObject.Find("UITopRoot");
            //    Transform root = SceneMgr.BATTLE == scene.name ? obj!=null? obj.transform: ViewMgr.vTopHolder : ViewMgr.vTopHolder;
            //    PopupView popupView = ViewMgr.ShowView<PopupView>(holder: root);
            //    popupView.transform.SetLayContainChlids(root.gameObject.layer);
           
            //}
        }

        public int curRoleid = 1;

        [ContextMenu("调试")]
        async void Test()
        {
            Wait(waitTime: 9, type: LoadingType.MSG);
            await new WaitForSeconds(2);
            StopWait(type: LoadingType.MSG);
            Wait(waitTime: 9, type: LoadingType.MSG);
            await new WaitForSeconds(1);
            StopWait(type: LoadingType.TCP);

        }

        #region  ShowTipInfo

        public List<PanelTipInfo> needShows = new List<PanelTipInfo>();
        private bool isShowing;
        public int GetTipShowIndex
        {
            get
            {
                for (int i = 0; i < needShows.Count; i++)
                {
                    if (needShows[i].isShowImmediately)
                    {
                        return i;
                    }
                }
                return -1;
            }
        }


        //private void ViewOpen(BaseView baseView)
        //{

        //    if (baseView is StageMapView || baseView is MainView|| baseView is EngagementView)
        //    {
        //        for (int i = 0; i < needShows.Count; i++)
        //        {
        //            needShows[i].isShowImmediately = true;
        //        }
        //    }
        //}

        //private bool PopupViewIsOpen
        //{
        //    get
        //    {
        //        return ViewMgr.GetView<PopupView>() != null && ViewMgr.GetView<PopupView>().gameObject.activeSelf;
        //    }
        //}


        //private bool IsShowImeImmediately
        //{
        //    get
        //    {
        //        return  
        //                (ViewMgr.GetView<StageMapView>() != null && ViewMgr.GetView<StageMapView>().gameObject.activeSelf)
        //                 || (ViewMgr.GetView<MainView>() != null && ViewMgr.GetView<MainView>().gameObject.activeSelf)
        //                 || (ViewMgr.GetView<EngagementView>() != null && ViewMgr.GetView<EngagementView>().gameObject.activeSelf);
        //    }
        //}

        public bool IsHavePopupObj
        {
            get
            {
                return needShows.Count > 0;
            }
        }

        private void LateUpdate()
        {
            UpdateShowInfo();
        }

        public void ShowInfo(PanelType type, params object[] para)
        {
           // Debug.Log("11111111111111");
            PanelTipInfo info = SetShow(type, para);
            info.isShowImmediately = true;

            if (IsAddToNeedShows(type, para))
            {
                needShows.Add(info);
            }


        }
        /// <summary>
        /// 立即显示并且不加入显示队列
        /// </summary>
        /// <param name="type"></param>
        /// <param name="para"></param>
        public void ShowInfoImmediately(PanelType type, params object[] para)
        {
            //PanelTipInfo info = SetShow(type, para);
            //info.isShowImmediately = true;
            //Messenger.DispatchEvent(Notif.POPUPVEIW_SHOW, info);
        }


        public void ShowInfoDelay(PanelType type, params object[] para)
        {

            //PanelTipInfo info = SetShow(type, para);
            //info.isShowImmediately = IsShowImeImmediately;

            //if (IsAddToNeedShows(type, para))
            //{
            //    needShows.Add(info);
            //}

        }

        public void ShowInfoDelayAtFirst(PanelType type,bool isOnly, params object[] para)
        {
            //PanelTipInfo temp = null;
            //PanelTipInfo info = SetShow(type, para);
            //info.isOnly = isOnly;
            //info.isShowImmediately = IsShowImeImmediately;

            //if (isOnly)
            //{
            //    temp = needShows.Find((PanelTipInfo tempInfo) => tempInfo.isOnly && tempInfo.type == type);

            //}
         
            //if (IsAddToNeedShows(type, para)&& temp==null)
            //{
            //    needShows.Insert(0, info);
            //}

        }


        private PanelTipInfo SetShow(PanelType type, params object[] para)
        {

            PanelTipInfo info = new PanelTipInfo();
            info.type = type;
            info.SetParam(para);
            return info;
        }

        private bool IsAddToNeedShows(PanelType type, params object[] para)
        {
            bool flag = true;

            if (type == PanelType.INFO)
            {

                for (int i = 0; i < needShows.Count; i++)
                {
                    string tip = needShows[i].param[0] as string;
                    string curTip = para[0] as string;
                    if (tip == curTip)
                    {
                        return false;
                    }

                }


            }

            return flag;
        }


        private PanelTipInfo showType;

        private void UpdateShowInfo()
        {
          //  Debug.Log(needShows.Count);
            //if (!isShowing && needShows.Count > 0&& PopupViewIsOpen)
            //{
                
            //    int index = GetTipShowIndex;
            //    if (index >= 0)
            //    {
            //        isShowing = true;
            //        showType = needShows[index];
            //        //   Debug.Log(showType.type+":1111111111111111");
            //        if (!showType.isShowing)
            //        {
            //            Messenger.DispatchEvent(Notif.POPUPVEIW_SHOW, showType);
            //        }
                    
            //    }

            //}



        }

        public void OneTipIsFinish()
        {
            if (needShows.Count > 0)
            {
                needShows.Remove(showType);
            }
            isShowing = false;
        }

        public void SetOneTipIsFinish()
        {
            if (isShowing)
            {
                OneTipIsFinish();
            }
        }



        #endregion


        public void CloseTip(PanelType type)
        {
            //PanelTipInfo info = needShows.Find((PanelTipInfo temp) => temp.isShowing);
            //if (info!=null)
            //{
            //    Messenger.DispatchEvent(Notif.POPUPVEIW_CLOSE, info);
            //}
        }


        #region Loading

        int waitPriority = -1;
        private List<LoadingType> loadingTypes = new List<LoadingType>();



        public void Wait(float waitTime = 5, int waitPriority = 0,LoadingType type = LoadingType.Default)
        {
            if (!loadingTypes.Contains(type))
            {
                loadingTypes.Add(type);
            }
            if (waitPriority > this.waitPriority)
            {
                this.waitPriority = waitPriority;
            }
            else
            {
                return;
            }
            ShowInfoImmediately(PanelType.LOADING,true,waitTime,true);
        }


        public void StopWait(int waitPriority = 0, LoadingType type = LoadingType.Default)
        {
            if (loadingTypes.Contains(type))
            {
                loadingTypes.Remove(type);
            }
            if (waitPriority < this.waitPriority)
            {
                return;
            }
            else
            {
                this.waitPriority = -1;
            }

            if (loadingTypes.Count<=0)
            {
                ShowInfoImmediately(PanelType.LOADING, false, 0f, false);
            }
      

        }

        public void StopAllLoading()
        {
            loadingTypes.Clear();
            this.waitPriority = -1;
            ShowInfoImmediately(PanelType.LOADING, false, 0f, false);
        }


        #endregion

        #region info

        public void ShowInfo(string info)
        {
            Debug.Log(info);
        }

        public void ShowInfoDelay(int id, params object[] para)
        {
            //TextModel model = TextDB.gi.GetDataById(id);
            //string tip = string.Empty;
            //if (model != null)
            //{
            //    tip = model.ReplaceParms(para);
            //    ShowInfoDelay(PanelType.INFO, tip);
            //}
            //else
            //{
            //    ShowInfoDelay(PanelType.INFO, "One Tip:" + id);
            //}
        }

        /// <summary>
        /// TextDB表的提示
        /// </summary>
        /// <param name="id"></param>
        public void ShowInfo(int id)
        {
            //TextModel model = TextDB.gi.GetDataById(id);
            //string tip = string.Empty;
            //if (model != null)
            //{
            //    tip = TextDB.gi.GetDataById(id).Text;
            //    ShowInfo(PanelType.INFO, tip);
            //}
            //else
            //{
            //    ShowInfo(PanelType.INFO, "One Tip:" + id);
            //}
        }

        #endregion

        public void ShowPopupWindow(string content, System.Action leftCallback, System.Action middleCallback, System.Action rightCallback, bool middleBtn = false)
        {

        }
    }
    

    public class PanelTipInfo
    {
        public PopupMessageManager.PanelType type;
        public bool isShowImmediately;
        public List<object> param = new List<object>();
        public bool isShowing { set; get; }
        public float showTime { set; get; }
        //当前弹出列表里面同种类型的提示只能有一个为Ture的此标记
        public bool isOnly { set; get; }
        public System.Action callBack { set; get; }

        public void SetParam(params object[] para)
        {
            for (int i = 0; i < para.Length; i++)
            {
                param.Add(para[i]);
            }
        }

    }

}



