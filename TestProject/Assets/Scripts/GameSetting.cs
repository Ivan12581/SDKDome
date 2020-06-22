using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSetting.asset")]
public class GameSetting : ScriptableObject
{
    public string CDN_ADDRESS = "https://down-mb.resources.3737.com/xly-test";
    public string NET_ASSETS = "NetAssets_Test";
    public string ip = "192.168.102.175";
    public uint port = 52000;
    /// <summary>
    /// 渠道号
    /// </summary>
    public uint pf = 1;

    private static GameSetting _gi;
    public static GameSetting gi
    {
        get
        {
            if (_gi == null)
            {
                _gi = Resources.Load<GameSetting>("ScriptableObjects/GameSetting");
            }
            return _gi;
        }
    }
}
