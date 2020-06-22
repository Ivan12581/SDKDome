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
                    SevenTestServer();
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

        // 开发服
        private void DevelopServer()
        {
            GameSetting.gi.ip = "192.168.102.175";
            GameSetting.gi.port = 52000;
            Debug.Log("服务器IP设置为" + GameSetting.gi.ip);
        }
        // 正式服
        private void ReleaseServer()
        {
            GameSetting.gi.ip = "182.254.192.79";
            GameSetting.gi.port = 52000;
            Debug.Log("服务器IP设置为" + GameSetting.gi.ip);
        }
        // 正式服 175
        private void Release175Server()
        {
            GameSetting.gi.ip = "192.168.102.175";
            GameSetting.gi.port = 52000;
            Debug.Log("服务器IP设置为" + GameSetting.gi.ip);
        }
        // 审核服
        private void ReviewServer()
        {
            GameSetting.gi.ip = "106.52.28.97";
            GameSetting.gi.port = 52000;
            Debug.Log("服务器IP设置为" + GameSetting.gi.ip);
        }
        // 7日测试服
        private void SevenTestServer()
        {
            GameSetting.gi.ip = "162.14.20.4";
            GameSetting.gi.port = 52001;
            Debug.Log("服务器IP设置为" + GameSetting.gi.ip);
        }
    }
}
