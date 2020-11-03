using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace celia.game.editor
{
    public class SetAndroidSDK : PlatformAction, IAndroidAction
    {
        CeliaBuildOption option;
        // SDK存放路径;因为是ScriptObject的原因不能调用Application.dataPath作为属性设置
        public string pluginAndroidPath;
        public string pluginSavePath;

        public override void PreExcute(CeliaBuildOption option)
        {
            this.option = option;
            pluginAndroidPath = Application.dataPath + "/Plugins/Android";
            pluginSavePath = Application.dataPath + "/Plugins/PlatformSDK/";
            
            JObject signCfg = GetSDKSign(option);
            JObject sdkParams = GetSDKParams(option);

            //先清空
            SetSDKFolderBack();

            SetAndroidSign(signCfg);

            SetSDKFolder(option.SDKType);

            SetSDKInfo(option.SDKType, sdkParams);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("SetAndroidSDK PreExcuted!");
        }

        public override void PostExcute(CeliaBuildOption option)
        {
            // 复原文件夹
            SetSDKFolderBack();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("SetAndroidSDK PostExcuted!");
        }


        private JObject GetSDKSign(CeliaBuildOption option)
        {
            string fileName = CeliaBuilder.GetInputParam("Sign:", option.Args);
            if (string.IsNullOrEmpty(fileName))
            {
                if (option.SDKType == SDKType.None)
                {
                    return null;
                }
                else
                {
                    throw new System.Exception("SetAndroidSDK ERROR: Not Set Signature!");
                }
            }

            string jsonStr = File.ReadAllText(Path.Combine(Application.dataPath.Replace("Assets", ""), "BuildSettings", "AndSignature", fileName+".json"));
            return JObject.Parse(jsonStr);
        }

        private JObject GetSDKParams(CeliaBuildOption option)
        {
            string fileName = CeliaBuilder.GetInputParam("SDKParams:", option.Args);
            if (string.IsNullOrEmpty(fileName))
            {
                if (option.SDKType == SDKType.None)
                {
                    return null;
                }
                else
                {
                    throw new System.Exception($"SetAndroidSDK ERROR: Not Set SDKParams for SDK{option.SDKType}!");
                }
            }

            string jsonStr = File.ReadAllText(Path.Combine(Application.dataPath.Replace("Assets", ""), "BuildSettings", "SDKParams", fileName+".json"));
            return JObject.Parse(jsonStr);
        }

        /// <summary>
        /// 删除Assets/Plugins/Android文件夹文件，复原配置
        /// </summary>
        private void SetSDKFolderBack()
        {
            //DeleteFolder(pluginAndroidPath);
            Directory.Delete(pluginAndroidPath,true);

            SDKParams sdkParams = AssetDatabase.LoadAssetAtPath<SDKParams>("Assets/Resources/SDKParams.asset");
            sdkParams.SDKType = SDKType.None;
            sdkParams.GameId = 193;
            sdkParams.AppKey = "";
            sdkParams.PayKey = "";
            sdkParams.AppId = 101356;
            sdkParams.CchId = 213;
            sdkParams.MdId = 200000;
            EditorUtility.SetDirty(sdkParams);

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.xlycs.rastar");
            PlayerSettings.Android.keystoreName = "";
            PlayerSettings.Android.keystorePass = "";
            PlayerSettings.Android.keyaliasName = "";
            PlayerSettings.Android.keyaliasPass = "";
        }

        /// <summary>
        /// 设置安卓包名、签名
        /// </summary>
        private void SetAndroidSign(JObject signCfg)
        {
            if (signCfg == null)
            {
                return;
            }
            //包名
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, signCfg.Value<string>("PakageName"));
            Debug.Log("Pakage Name: " + PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));

            //签名文件
            string keystorePath = Path.Combine(Application.dataPath.Replace("Assets", ""), "BuildSettings", "AndSignature", signCfg.Value<string>("KeyFileName"));
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = signCfg.Value<string>("KeystorePass");
            PlayerSettings.Android.keyaliasName = signCfg.Value<string>("KeyAliasName");
            PlayerSettings.Android.keyaliasPass = signCfg.Value<string>("KeyAliasPass");

            Debug.Log($"KeystorePath: {PlayerSettings.Android.keystoreName} keystorePass: {PlayerSettings.Android.keystorePass} keyaliasName: {PlayerSettings.Android.keyaliasName} keyaliasPass: {PlayerSettings.Android.keyaliasPass}");
        }

        /// <summary>
        /// 把SDK信息写入配置文件
        /// </summary>
        private void SetSDKInfo(SDKType sdkType, JObject sdkParams)
        {
            if (sdkParams == null)
            {
                return;
            }
            int gameId = sdkParams.Value<int>("GameId");
            string appKey = sdkParams.Value<string>("AppKey");
            string payKey = sdkParams.Value<string>("PayKey");
            int appID = sdkParams.Value<int>("AppId");
            int cchID = sdkParams.Value<int>("CchId");
            int mdID = sdkParams.Value<int>("MdId");
            string AppleAppleID = sdkParams.Value<string>("AppleAppleID");
            // 保存到包内配置
            SDKParams pakageSDKParams = AssetDatabase.LoadAssetAtPath<SDKParams>("Assets/Resources/SDKParams.asset");
            pakageSDKParams.SDKType = sdkType;
            pakageSDKParams.GameId = gameId;
            pakageSDKParams.AppKey = appKey;
            pakageSDKParams.PayKey = payKey;
            pakageSDKParams.AppId = appID;
            pakageSDKParams.CchId = cchID;
            pakageSDKParams.MdId = mdID;
            pakageSDKParams.AppleAppId = AppleAppleID;
            EditorUtility.SetDirty(pakageSDKParams);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            // SDK信息写入Android配置文件
            string filePath = "";
            string debugParamName = "";
            string debugValue = "";
            switch (sdkType)
            {
                case SDKType.None:
                    return;
                case SDKType.Native:
                    filePath = $"{pluginAndroidPath}/assets/sjoys_app.ini";
                    debugParamName = "debug";
                    debugValue = option.ReleaseLevel == ReleaseLevel.Alpha ? "1" : "0";
                    break;
                case SDKType.NativeChukai:
                    filePath = $"{pluginAndroidPath}/assets/sdk_config.ini";
                    debugParamName = "loggerSwitch";
                    debugValue = option.ReleaseLevel == ReleaseLevel.Alpha ? "1" : "0";
                    break;
                case SDKType.Oversea:
                    filePath = $"{pluginAndroidPath}/assets/rsdk/rastar_na_config.ini";
                    debugParamName = "Debug_Switch";
                    debugValue = option.ReleaseLevel == ReleaseLevel.Alpha ? "true" : "false";
                    break;
                case SDKType.CeliaOversea:
                    AssetDatabase.Refresh();
                    return;
            }
            File.AppendAllText(filePath, $"\r\n\r\napp_id={appID}\r\n\r\ncch_id={cchID}\r\n\r\nmd_id={mdID}\r\n\r\napp_key={appKey}\r\n\r\n{debugParamName}={debugValue}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 把SDK文件放入Android文件夹
        /// </summary>
        private void SetSDKFolder(SDKType sdkType)
        {
            // 其他SDK设置为不导入
            SetAndroidPluginImport(pluginSavePath, false);

            if (sdkType == SDKType.None)
            {
                return;
            }
            // 复制要打包的SDK文件
            CopyFolder(pluginSavePath + $"Android{sdkType.ToString()}", pluginAndroidPath);
            // 包内SDK导入
            SetAndroidPluginImport(pluginAndroidPath, true);
        }

        /// <summary>
        /// 安卓SDK文件导入
        /// </summary>
        private void SetAndroidPluginImport(string path, bool add)
        {
            DirectoryInfo folder = new DirectoryInfo(path);
            if (!folder.Exists)
            {
                folder = null;
                return;
            }
            FileInfo[] files = folder.GetFiles();
            foreach (var file in folder.GetFiles())
            {
                PluginImporter importer = AssetImporter.GetAtPath(file.FullName.Replace(Application.dataPath.Replace('/', '\\'), "Assets")) as PluginImporter;
                importer?.SetCompatibleWithPlatform(BuildTarget.Android, add);
            }

            DirectoryInfo[] subFolders = folder.GetDirectories();
            foreach (var subfolder in folder.GetDirectories())
            {
                SetAndroidPluginImport(subfolder.FullName, add);
            }

            AssetDatabase.Refresh();
        }

        #region 文件操作
        public void CopyFolder(string sourcePath, string targetPath)
        {
            DirectoryInfo sourceFolder = new DirectoryInfo(sourcePath);
            if (!sourceFolder.Exists)
            {
                Debug.Log(sourcePath);
                return;
            }

            DirectoryInfo targetFolder = new DirectoryInfo(targetPath);
            if (!targetFolder.Exists)
            {
                Directory.CreateDirectory(targetPath);
            }

            foreach (var file in sourceFolder.GetFiles())
            {
                if (file.Extension.CompareTo(".meta") == 0)
                {
                    continue;
                }

                string fileTargetPath = Path.Combine(targetPath, Path.GetFileName(file.Name));
                File.Copy(file.FullName, fileTargetPath, true);
            }
            foreach (var folder in sourceFolder.GetDirectories())
            {
                string subfolderTargetPath = Path.Combine(targetPath, Path.GetFileName(folder.Name));
                CopyFolder(folder.FullName, subfolderTargetPath);
            }
            AssetDatabase.Refresh();
        }

        public void DeleteFolder(string deletePath)
        {
            DirectoryInfo folder = new DirectoryInfo(deletePath);
            if (!folder.Exists)
            {
                return;
            }
            foreach (var file in folder.GetFiles())
            {
                if (file.Extension.CompareTo(".gitkeeper") == 0)
                {
                    continue;
                }
                file.Delete();
            }

            foreach (var subFolder in folder.GetDirectories())
            {
                DeleteFolder(subFolder.FullName);
            }
            AssetDatabase.Refresh();
        }
        #endregion
    }
}
