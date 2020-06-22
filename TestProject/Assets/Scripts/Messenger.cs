/*
 * Advanced C# messenger by Ilya Suzdalnitski. V1.0
 * 
 * Based on Rod Hyde's "CSharpMessenger" and Magnus Wolffelt's "CSharpMessenger Extended".
 * 
 * Features:
    * Prevents a MissingReferenceException because of a reference to a destroyed message handler.
    * Option to log all messages
    * Extensive error detection, preventing silent bugs
 * 
 * Usage examples:
    1. Messenger.AddEventListener<GameObject>("prop collected", PropCollected);
       Messenger.DispatchEvent<GameObject>("prop collected", prop);
    2. Messenger.AddEventListener<float>("speed changed", SpeedChanged);
       Messenger.DispatchEvent<float>("speed changed", 0.5f);
 * 
 * Messenger cleans up its evenTable automatically upon loading of a new level.
 * 
 * Don't forget that the messages that should survive the cleanup, should be marked with Messenger.MarkAsPermanent(string)
 * 
 */

//#define LOG_ALL_MESSAGES
//#define LOG_ADD_LISTENER
//#define LOG_BROADCAST_MESSAGE
//#define REQUIRE_LISTENER

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public delegate void CallBack();
public delegate void CallBack<T>(T arg1);
public delegate void CallBack<T, U>(T arg1, U arg2);
public delegate void CallBack<T, U, V>(T arg1, U arg2, V arg3);


static internal class Messenger
{
    //Messenger Info
    private class MessengerInfo
    {
        public string eventType;
        public Delegate handler;
    }
    #region Internal variables

    //Disable the unused variable warning
#pragma warning disable 0414
    //Ensures that the MessengerHelper will be created automatically upon start of the game.
    static private MessengerHelper messengerHelper = (new GameObject("MessengerHelper")).AddComponent<MessengerHelper>();
#pragma warning restore 0414

    static public Dictionary<string, List<Delegate>> eventTable = new Dictionary<string, List<Delegate>>();
    static private Dictionary<object, List<MessengerInfo>> msgInfoTable = new Dictionary<object, List<MessengerInfo>>();
    static private Dictionary<Delegate, object> delegateTable = new Dictionary<Delegate, object>();

    //Message handlers that should never be removed, regardless of calling Cleanup
    static public List<string> permanentMessages = new List<string>();
    #endregion
    #region Helper methods
    //Marks a certain message as permanent.
    static public void MarkAsPermanent(string eventType)
    {
#if LOG_ALL_MESSAGES
        Debug.Log("Messenger MarkAsPermanent \t\"" + eventType + "\"");
#endif

        permanentMessages.Add(eventType);
    }


    static public void Cleanup()
    {
#if LOG_ALL_MESSAGES
        Debug.Log("MESSENGER Cleanup. Make sure that none of necessary listeners are removed.");
#endif

        List<string> messagesToRemove = new List<string>();

        foreach (KeyValuePair<string, List<Delegate>> pair in eventTable)
        {
            bool wasFound = false;

            foreach (string message in permanentMessages)
            {
                if (pair.Key == message)
                {
                    wasFound = true;
                    break;
                }
            }

            if (!wasFound)
                messagesToRemove.Add(pair.Key);
        }

        foreach (string message in messagesToRemove)
        {
            eventTable.Remove(message);
        }
    }

    static public void ClearListener(object obj)
    {
        if (msgInfoTable.ContainsKey(obj))
        {
            List<MessengerInfo> remove = new List<MessengerInfo>(msgInfoTable[obj]);
            foreach (MessengerInfo mi in remove)
            {
                RemoveEvent(mi.eventType, mi.handler);
            }
        }
    }

    static public void PrintEventTable()
    {
        Debug.Log("\t\t\t=== MESSENGER PrintEventTable ===");

        foreach (KeyValuePair<string, List<Delegate>> pair in eventTable)
        {
            Debug.Log("\t\t\t" + pair.Key + "\t\t" + pair.Value);
        }

        Debug.Log("\n");
    }
    #endregion

    #region Message logging and exception throwing
    static public void OnListenerAdding(string eventType, object target, Delegate listenerBeingAdded)
    {
#if LOG_ALL_MESSAGES || LOG_ADD_LISTENER
        Debug.Log("MESSENGER OnListenerAdding \t\"" + eventType + "\"\t{" + listenerBeingAdded.Target + " -> " + listenerBeingAdded.Method + "}");
#endif

        if (!eventTable.ContainsKey(eventType))
        {
            eventTable.Add(eventType, new List<Delegate>());
        }
        if (target != null && !msgInfoTable.ContainsKey(target))
        {
            msgInfoTable.Add(target, new List<MessengerInfo>());
        }
    }

    static public void OnListenerRemoving(string eventType, Delegate listenerBeingRemoved)
    {
#if LOG_ALL_MESSAGES
        Debug.Log("MESSENGER OnListenerRemoving \t\"" + eventType + "\"\t{" + listenerBeingRemoved.Target + " -> " + listenerBeingRemoved.Method + "}");
#endif

        if (eventTable.ContainsKey(eventType))
        {
            List<Delegate> d = eventTable[eventType];

            if (d == null)
            {
                throw new ListenerException(string.Format("Attempting to remove listener with for event type \"{0}\" but current listener is null.", eventType));
            }
        }
        else
        {
#if REQUIRE_LISTENER
            throw new ListenerException(string.Format("Attempting to remove listener for type \"{0}\" but Messenger doesn't know about this event type.", eventType));
#endif
        }
    }

    static public void OnListenerRemoved(string eventType, Delegate handler)
    {
        if (eventTable[eventType].Count == 0)
        {
            eventTable.Remove(eventType);
        }
        if (handler.Target != null)
        {
            if (msgInfoTable.ContainsKey(handler.Target) && msgInfoTable[handler.Target].Count == 0)
            {
                msgInfoTable.Remove(handler.Target);
            }
        }
    }

    static public void OnBroadcasting(string eventType)
    {
#if REQUIRE_LISTENER
        if (!eventTable.ContainsKey(eventType))
        {
            throw new BroadcastException(string.Format("Broadcasting message \"{0}\" but no listener found. Try marking the message with Messenger.MarkAsPermanent.", eventType));
        }
#endif
    }

    static public BroadcastException CreateBroadcastSignatureException(string eventType)
    {
        return new BroadcastException(string.Format("Broadcasting message \"{0}\" but listeners have a different signature than the broadcaster.", eventType));
    }

    public class BroadcastException : Exception
    {
        public BroadcastException(string msg)
            : base(msg)
        {
        }
    }

    public class ListenerException : Exception
    {
        public ListenerException(string msg)
            : base(msg)
        {
        }
    }
    #endregion

    #region AddEventListener
    //No parameters
    static public void AddEventListener(string eventType, CallBack handler)
    {
        AddEvent(eventType, handler.Target, handler);
    }
    static public void AddEventListener(string eventType, object target, CallBack handler)
    {
        AddEvent(eventType, target, handler);
    }

    //Single parameter
    static public void AddEventListener<T>(string eventType, CallBack<T> handler)
    {
        AddEvent(eventType, handler.Target, handler);
    }
    static public void AddEventListener<T>(string eventType, object target, CallBack<T> handler)
    {
        AddEvent(eventType, target, handler);
    }

    //Two parameters
    static public void AddEventListener<T, U>(string eventType, CallBack<T, U> handler)
    {
        AddEvent(eventType, handler.Target, handler);
    }
    static public void AddEventListener<T, U>(string eventType, object target, CallBack<T, U> handler)
    {
        AddEvent(eventType, target, handler);
    }

    //Three parameters
    static public void AddEventListener<T, U, V>(string eventType, CallBack<T, U, V> handler)
    {
        AddEvent(eventType, handler.Target, handler);
    }
    static public void AddEventListener<T, U, V>(string eventType, object target, CallBack<T, U, V> handler)
    {
        AddEvent(eventType, target, handler);
    }

    static private void AddEvent(string eventType, object target, Delegate handler)
    {
        OnListenerAdding(eventType, target, handler);

        if (!eventTable[eventType].Contains(handler))
        {
            eventTable[eventType].Add(handler);
        }
        if (target != null)
        {
            msgInfoTable[target].Add(new MessengerInfo() { eventType = eventType, handler = handler });
            delegateTable[handler] = target;
        }
    }
    #endregion

    #region RemoveEventListener
    //No parameters
    static public void RemoveEventListener(string eventType, CallBack handler)
    {
        RemoveEvent(eventType, handler);
    }

    //Single parameter
    static public void RemoveEventListener<T>(string eventType, CallBack<T> handler)
    {
        RemoveEvent(eventType, handler);
    }

    //Two parameters
    static public void RemoveEventListener<T, U>(string eventType, CallBack<T, U> handler)
    {
        RemoveEvent(eventType, handler);
    }

    //Three parameters
    static public void RemoveEventListener<T, U, V>(string eventType, CallBack<T, U, V> handler)
    {
        RemoveEvent(eventType, handler);
    }

    static private void RemoveEvent(string eventType, Delegate handler)
    {
        if (!eventTable.ContainsKey(eventType)) return;

        OnListenerRemoving(eventType, handler);
        eventTable[eventType].Remove(handler);

        if (delegateTable.ContainsKey(handler))
        {
            List<MessengerInfo> remove = new List<MessengerInfo>();
            List<MessengerInfo> mis = msgInfoTable[delegateTable[handler]];
            foreach (MessengerInfo mi in mis)
            {
                if (mi.handler == handler)
                {
                    remove.Add(mi);
                }
            }
            foreach (MessengerInfo mi in remove)
            {
                mis.Remove(mi);
            }

            delegateTable.Remove(handler);
        }

        OnListenerRemoved(eventType, handler);
    }
    #endregion

    #region Broadcast
    //No parameters
    static public void DispatchEvent(string eventType)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        List<Delegate> CallBack;
        if (eventTable.TryGetValue(eventType, out CallBack))
        {
            if (CallBack != null)
            {
                List<Delegate> removes = new List<Delegate>();
                foreach (CallBack callBack in CallBack)
                {
                    if (delegateTable.ContainsKey(callBack))
                    {
                        if (delegateTable[callBack].Equals(null))
                        {
                            removes.Add(callBack);
                            continue;
                        }
                    }
                    try
                    {
                        callBack();
                    }
                    catch (Exception e)
                    {
                        ExceptionInfo(callBack, e);
                    }
                }
                foreach (CallBack callBack in removes)
                {
                    RemoveEventListener(eventType, callBack);
                }
            }
            else
            {
                throw CreateBroadcastSignatureException(eventType);
            }
        }
    }

    //Single parameter
    static public void DispatchEvent<T>(string eventType, T arg1)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        List<Delegate> CallBack;
        if (eventTable.TryGetValue(eventType, out CallBack))
        {
            if (CallBack != null)
            {
                List<Delegate> removes = new List<Delegate>();
                for (int i = 0; i < CallBack.Count; i++)
                {
                    CallBack<T> callBack = CallBack[i] as CallBack<T>;

                    if (delegateTable.ContainsKey(callBack))
                    {
                        if (delegateTable[callBack].Equals(null))
                        {
                            removes.Add(callBack);
                            continue;
                        }
                    }
                    try
                    {
                        callBack(arg1);
                    }
                    catch (Exception e)
                    {
                        ExceptionInfo(callBack, e);
                    }
                }
                foreach (CallBack<T> callBack in removes)
                {
                    RemoveEventListener(eventType, callBack);
                }
            }
            else
            {
                throw CreateBroadcastSignatureException(eventType);
            }
        }
    }

    //Two parameters
    static public void DispatchEvent<T, U>(string eventType, T arg1, U arg2)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        List<Delegate> CallBack;
        if (eventTable.TryGetValue(eventType, out CallBack))
        {
            if (CallBack != null)
            {
                List<Delegate> removes = new List<Delegate>();
                for (int i = 0; i < CallBack.Count; i++)
                {
                    CallBack<T, U> callBack = CallBack[i] as CallBack<T, U>;

                    if (delegateTable.ContainsKey(callBack))
                    {
                        if (delegateTable[callBack].Equals(null))
                        {
                            removes.Add(callBack);
                            continue;
                        }
                    }
                    try
                    {
                        callBack(arg1, arg2);
                    }
                    catch (Exception e)
                    {
                        ExceptionInfo(callBack, e);
                    }
                }
                foreach (CallBack<T, U> callBack in removes)
                {
                    RemoveEventListener(eventType, callBack);
                }
            }
            else
            {
                throw CreateBroadcastSignatureException(eventType);
            }
        }
    }

    //Three parameters
    static public void DispatchEvent<T, U, V>(string eventType, T arg1, U arg2, V arg3)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        List<Delegate> CallBack;
        if (eventTable.TryGetValue(eventType, out CallBack))
        {
            if (CallBack != null)
            {
                List<Delegate> removes = new List<Delegate>();
                for (int i = 0; i < CallBack.Count; i++)
                {
                    CallBack<T, U, V> callBack = CallBack[i] as CallBack<T, U, V>;

                    if (delegateTable.ContainsKey(callBack))
                    {
                        if (delegateTable[callBack].Equals(null))
                        {
                            removes.Add(callBack);
                            continue;
                        }
                    }
                    try
                    {
                        callBack(arg1, arg2, arg3);
                    }
                    catch (Exception e)
                    {
                        ExceptionInfo(callBack, e);
                    }
                }
                foreach (CallBack<T, U, V> callBack in removes)
                {
                    RemoveEventListener(eventType, callBack);
                }
            }
            else
            {
                throw CreateBroadcastSignatureException(eventType);
            }
        }
    }

    private static void ExceptionInfo(Delegate handler, Exception e)
    {
        Debug.LogError(string.Format("Messenger CallBack \"{0}:{1}\" has a exception : {2} \nStackTrace:{3}", handler.Method.DeclaringType.FullName, handler.Method, e.Message, e.StackTrace));
    }
    #endregion
}

//This manager will ensure that the messenger's eventTable will be cleaned up upon loading of a new level.
public sealed class MessengerHelper : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    //Clean up eventTable every time a new level loads.
    public void OnDisable()
    {
        Messenger.Cleanup();
    }

    public static void ClearListener(object obj)
    {
        Messenger.ClearListener(obj);
    }
}