using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using celia.game;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "GameSetting.asset")]
public class GameSetting : ScriptableObject
{
    public string VERSION = "Develop";
    public string CDN_ADDRESS = "https://xlycs.res.rastargame.com";
    public string NET_ASSETS = "NetAssets_Develop";
    public string ip = "192.168.102.175";
    public uint port = 52000;
    public ZoneType serverType = ZoneType.Official;

    private static GameSetting _gi;
    public static GameSetting gi
    {
        get
        {
            if (_gi == null)
            {
#if UNITY_EDITOR
                _gi = AssetDatabase.LoadAssetAtPath<GameSetting>("Assets/Resources/ScriptableObjects/GameSetting.asset");
#else
                _gi = Resources.Load<GameSetting>("ScriptableObjects/GameSetting");
#endif
            }
            return _gi;
        }
    }
}

namespace celia.game
{
    public enum ZoneType// 用于OpenApi请求（如 公告、礼包等）
    {
        Official = 1,// 正式服
        PublicTest = 2,// 测试服： 国内79 海外183
        PrivateTest = 3,// 内部测试服： 国内175 海外无
    }
}
