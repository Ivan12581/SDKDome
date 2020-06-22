using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace celia.game
{
    public class BaseService<T> : SingleClass<T> where T : new()
    {
        private const int WAIT_TIME = 9999;
        private const int REQ_TIME_OUT_MIL = 10000;

        public virtual void Init()
        {

        }
        /*
        public void LogEventRequest(int eventCode, object data, Action<LogEventResponse> callBack)
        {
            PopupMessageManager.gi.Wait(WAIT_TIME);
            if (data == null)
            {
                data = new object();
            }
            new GameSparks.Api.Requests.LogEventRequest().SetEventKey(ServiceKey.SERVICE_EVENT).SetEventAttribute(ServiceKey.DATA, (GSRequestData)GSDataHelpers.ObjectToGSData(data)).SetEventAttribute(ServiceKey.EVENT_CODE, eventCode).Send((dataRes) =>
            {
                PopupMessageManager.gi.StopWait();
                ServiceInfo(eventCode, dataRes);
                callBack?.Invoke(dataRes);
                // UnLockManager.gi.Unlock(dataRes);
            }, REQ_TIME_OUT_MIL);
        }
        public void LogEventRequest(int eventCode, int timeoutMilis, object data, Action<LogEventResponse> callBack)
        {
            PopupMessageManager.gi.Wait(WAIT_TIME);
            if (data == null)
            {
                data = new object();
            }
            new GameSparks.Api.Requests.LogEventRequest().SetEventKey(ServiceKey.SERVICE_EVENT).SetEventAttribute(ServiceKey.DATA, (GSRequestData)GSDataHelpers.ObjectToGSData(data)).SetEventAttribute(ServiceKey.EVENT_CODE, eventCode).Send((dataRes) =>
            {
                PopupMessageManager.gi.StopWait();
                ServiceInfo(eventCode, dataRes);
                callBack?.Invoke(dataRes);
                // UnLockManager.gi.Unlock(dataRes);
            }, timeoutMilis);
        }
        public void LogEventRequest(int eventCode, object data, Action<LogEventResponse> successCallBack, Action<LogEventResponse> errorCallBack)
        {
            PopupMessageManager.gi.Wait(WAIT_TIME);
            if (data == null)
            {
                data = new object();
            }
            new GameSparks.Api.Requests.LogEventRequest().SetEventKey(ServiceKey.SERVICE_EVENT).SetEventAttribute(ServiceKey.DATA, (GSRequestData)GSDataHelpers.ObjectToGSData(data)).SetEventAttribute(ServiceKey.EVENT_CODE, eventCode).Send((dataRes) =>
            {
                PopupMessageManager.gi.StopWait();

                ServiceInfo(eventCode, dataRes);

                if (!dataRes.HasErrors)
                {
                    successCallBack?.Invoke(dataRes);
                    // UnLockManager.gi.Unlock(dataRes);
                }
                else
                {
                    errorCallBack?.Invoke(dataRes);
                }
            });
        }
        public void LogEventRequest(int eventCode, int timeoutMilis, object data, Action<LogEventResponse> successCallBack, Action<LogEventResponse> errorCallBack)
        {
            PopupMessageManager.gi.Wait(WAIT_TIME);
            if (data == null)
            {
                data = new object();
            }
            new GameSparks.Api.Requests.LogEventRequest().SetEventKey(ServiceKey.SERVICE_EVENT).SetEventAttribute(ServiceKey.DATA, (GSRequestData)GSDataHelpers.ObjectToGSData(data)).SetEventAttribute(ServiceKey.EVENT_CODE, eventCode).Send((dataRes) =>
            {
                PopupMessageManager.gi.StopWait();

                ServiceInfo(eventCode, dataRes);

                if (!dataRes.HasErrors)
                {
                    successCallBack?.Invoke(dataRes);
                    // UnLockManager.gi.Unlock(dataRes);
                }
                else
                {
                    errorCallBack?.Invoke(dataRes);
                }
            }, timeoutMilis);
        }

        void ServiceInfo(int eventCode, LogEventResponse dataRes)
        {
            if (!dataRes.HasErrors)
            {
                Debug.Log("response " + eventCode + " is success.");
            }
            else
            {
                Debug.LogError("response " + eventCode + " is error, error : " + dataRes.Errors.JSON);
            }
        }

        public V GetResData<V>(LogEventResponse dataRes)
        {
            string dataStr = dataRes.ScriptData.GetGSData("RESP_DATA").JSON;
            return JsonConvert.DeserializeObject<V>(dataStr);
        }
        */
    }
}