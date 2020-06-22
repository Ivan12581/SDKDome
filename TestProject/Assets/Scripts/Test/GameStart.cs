using celia.game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    void Awake()
    {
        DelayManager.Create();
        SDKManager.Create();
        SDKManager.gi.Init();
        NetworkManager.Create();
        MainThreadDispatcher.Create();
        PopupMessageManager.Create();
        AuthProcessor.Create();
        AuthProcessor.gi.Init();
        LogicProcessor.Create();
        LogicProcessor.gi.Init();
    }

    private void OnApplicationPause(bool pause)
    {
    }
    private void OnApplicationQuit()
    {
        GameTcpClient.gi.OnDestroy();
    }
}
