using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace celia.game.editor
{
    public class SetServerSetting : CommonAction
    {
        // 原数据记录
        string ip = "";
        uint port = 0;

        public override void PreExcute(CeliaBuildOption option)
        {
            ip = GameSetting.gi.ip;
            port = GameSetting.gi.port;
            
            string serverName = CeliaBuilder.GetInputParam("SV:", option.Args); 
            switch (serverName)
            {
                case "rel":
                    ReleaseServer();
                    break;
                case "rel_175":
                    Release175Server();
                    break;
                case "rev":
                    ReviewServer();
                    break;
                case "seven":
                    GameTestServer();
                    break;
                case "dev":
                    DevelopServer();
                    break;
                default:// 未指定服务器时，使用当前项目服务器
                    break;
            }

            EditorUtility.SetDirty(GameSetting.gi);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("SetServerSetting PreExcuted!");
        }

        public override void PostExcute(CeliaBuildOption option)
        {
            GameSetting.gi.ip = ip;
            GameSetting.gi.port = port;

            EditorUtility.SetDirty(GameSetting.gi);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("SetServerSetting PostExcuted!");
        }

        // 内网测试服
        private void DevelopServer()
        {
            GameSetting.gi.serverType = ZoneType.PrivateTest;
            GameSetting.gi.ip = "192.168.102.175";
            GameSetting.gi.port = 52001;
            GameSetting.gi.CDN_ADDRESS = "https://xlycs.res.rastargame.com";
            GameSetting.gi.NET_ASSETS = "NetAssets_Develop";
            Debug.Log("服务器IP设置为" + GameSetting.gi.ip);
        }
        // 外网测试服
        private void ReleaseServer()
        {
            GameSetting.gi.serverType = ZoneType.PublicTest;
            GameSetting.gi.ip = "182.254.192.79";
            GameSetting.gi.port = 52001;
            GameSetting.gi.CDN_ADDRESS = "https://xlycs.res.rastargame.com";
            GameSetting.gi.NET_ASSETS = "NetAssets_PublicNetwork";
            Debug.Log("服务器IP设置为" + GameSetting.gi.ip);
        }

        // 175
        private void Release175Server()
        {
            GameSetting.gi.ip = "192.168.102.175";
            GameSetting.gi.port = 52001;
            GameSetting.gi.CDN_ADDRESS = "https://xlycs.res.rastargame.com";
            GameSetting.gi.NET_ASSETS = "NetAssets_Develop";
            Debug.Log("服务器IP设置为" + GameSetting.gi.ip);
        }
        // 审核服
        private void ReviewServer()
        {
            GameSetting.gi.ip = "106.52.28.97";
            GameSetting.gi.port = 52001;
            GameSetting.gi.CDN_ADDRESS = "https://xlycs.res.rastargame.com";
            GameSetting.gi.NET_ASSETS = "NetAssets_Shenhe";
            Debug.Log("服务器IP设置为" + GameSetting.gi.ip);
        }
        // 10日测试服
        private void GameTestServer()
        {
            GameSetting.gi.ip = "sndwz-login.rastargame.com";
            GameSetting.gi.port = 80;
            GameSetting.gi.CDN_ADDRESS = "https://xlycs.res.rastargame.com";
            GameSetting.gi.NET_ASSETS = "NetAssets_10Days";
            Debug.Log("服务器IP设置为" + GameSetting.gi.ip);
        }
    }
}
