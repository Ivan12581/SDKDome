using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace celia.game
{
    public class DelayManager : SingleMonoBehaviour<DelayManager>
    {
        private List<DelayMethod> delayList = new List<DelayMethod>();

        public DelayMethod Invoke(Action action, float seconds, Action<object> callBack = null, object callBackParm = null)
        {
            DelayMethod method = new DelayMethod()
            {
                id = DelayMethod.baseId++,
                startTime = Time.time,
                seconds = seconds,
                action = action,
                callBack = callBack
            };
            delayList.Add(method);

            return method;
        }
        public DelayMethod<T> Invoke<T>(Action<T> action, T parm, float seconds, Action<object> callBack = null, object callBackParm = null)
        {
            DelayMethod<T> method = new DelayMethod<T>()
            {
                id = DelayMethod.baseId++,
                startTime = Time.time,
                seconds = seconds,
                parm = parm,
                action = action,
                callBack = callBack
            };
            delayList.Add(method);

            return method;
        }
        public DelayMethod<T1, T2> Invoke<T1, T2>(Action<T1, T2> action, T1 parm1, T2 parm2, float seconds, Action<object> callBack = null, object callBackParm = null)
        {
            DelayMethod<T1, T2> method = new DelayMethod<T1, T2>()
            {
                id = DelayMethod.baseId++,
                startTime = Time.time,
                seconds = seconds,
                parm1 = parm1,
                parm2 = parm2,
                action = action,
                callBack = callBack
            };
            delayList.Add(method);

            return method;
        }
        public DelayMethod<T1, T2, T3> Invoke<T1, T2, T3>(Action<T1, T2, T3> action, T1 parm1, T2 parm2, T3 parm3, float seconds, Action<object> callBack = null, object callBackParm = null)
        {
            DelayMethod<T1, T2, T3> method = new DelayMethod<T1, T2, T3>()
            {
                id = DelayMethod.baseId++,
                startTime = Time.time,
                seconds = seconds,
                parm1 = parm1,
                parm2 = parm2,
                parm3 = parm3,
                action = action,
                callBack = callBack
            };
            delayList.Add(method);

            return method;
        }

        private List<DelayMethod> waitDel = new List<DelayMethod>();

        private void Update()
        {
            for (int i = 0; i < delayList.Count; i++)
            {
                DelayMethod method = delayList[i];
                method.endTime = Time.time;

                if (method.endTime - method.startTime >= method.seconds)
                {
                    try
                    {
                        method.Execute();
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.ToString());
                    }
                    waitDel.Add(method);
                }
            }
        }

        private void LateUpdate()
        {
            foreach (DelayMethod m in waitDel)
            {
                delayList.Remove(m);
            }
        }

        public void Break(DelayMethod method)
        {
            waitDel.Add(method);
        }

        public void BreakAll()
        {
            waitDel.AddRange(delayList);
        }
    }

    public class DelayMethod
    {
        public static int baseId = 0;
        public int id;
        public float startTime;
        public float endTime;
        public float seconds;
        public Action action;
        public object callBackParm;
        public Action<object> callBack;
        public void Execute()
        {
            DoAction();
            if (callBack != null)
            {
                callBack(callBackParm);
            }
        }

        protected virtual void DoAction()
        {
            action();
        }
    }

    public class DelayMethod<T> : DelayMethod
    {
        public T parm;
        new public Action<T> action;
        protected override void DoAction()
        {
            action(parm);
        }
    }

    public class DelayMethod<T1, T2> : DelayMethod
    {
        public T1 parm1;
        public T2 parm2;
        new public Action<T1, T2> action;
        protected override void DoAction()
        {
            action(parm1, parm2);
        }
    }

    public class DelayMethod<T1, T2, T3> : DelayMethod
    {
        public T1 parm1;
        public T2 parm2;
        public T3 parm3;
        new public Action<T1, T2, T3> action;
        protected override void DoAction()
        {
            action(parm1, parm2, parm3);
        }
    }
}
