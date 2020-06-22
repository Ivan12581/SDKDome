using UnityEngine;
#if UNITY_EDITOR_OSX
using UnityEditor.iOS.Xcode;
#endif
using System.IO;
using System.Linq;
using UnityEditor;
using System;
using Newtonsoft.Json.Linq;

namespace celia.game.editor
{
    public class SetIOSSDK : PlatformAction, IIOSAction
    {
        CeliaBuildOption option;
        // SDK存放路径;因为是ScriptObject的原因不能调用Application.dataPath作为属性设置
        public string pluginIOSPath;
        public string pluginSavePath;

        public override void PreExcute(CeliaBuildOption option)
        {
            this.option = option;
            pluginIOSPath = Application.dataPath + "/Plugins/iOS";
            pluginSavePath = Application.dataPath + "/Plugins/PlatformSDK/";

#if UNITY_EDITOR_OSX
            PlistElementDict signCfg = GetSDKSign(option);
            JObject sdkParams = GetSDKParams(option);

            SetSDKFolderBack();

            SetIOSSign(signCfg);

            SetSDKFolder(option.SDKType);

            SetSDKInfo(sdkParams, option.SDKType);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif

            Debug.Log("SetIOSSDK PreExcuted!");
        }
        public override void PostExcute(CeliaBuildOption option)
        {

#if UNITY_EDITOR_OSX
            switch (option.SDKType)
            {
                case SDKType.None:
                    SetNoneSDKProjectSetting();
                    break;
                case SDKType.Native:
                    SetNativeSDK();
                    break;
                case SDKType.NativeChukai:
                    SetNativeChukaiSDK();
                    break;
                case SDKType.Oversea:
                    SetOverseaSDK(GetSDKParams(option));
                    break;
            }

            // 复原文件夹
            SetSDKFolderBack();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("SetIOSSDK PostExcuted!");
#endif
        }

        bool IsRelativePath(string outputPath)
        {
            if (outputPath.Contains(":/") || outputPath.StartsWith("//"))
            {
                return false;
            }
            return true;
        }
        string GetXcodeProjectPath(string outputPath)
        {
            if (IsRelativePath(outputPath))
            {
                return Application.dataPath.Replace("Assets", option.PlayerOption.locationPathName);
            }
            else
            {
                return outputPath;
            }
        }
#if UNITY_EDITOR_OSX

        #region PreExcute
        /// <summary>
        /// 删除Assets/Plugins/iOS文件夹文件，复原配置
        /// </summary>
        private void SetSDKFolderBack()
        {
            DeleteFolder(pluginIOSPath);
            
            SDKParams sdkParams = AssetDatabase.LoadAssetAtPath<SDKParams>("Assets/Resources/SDKParams.asset");
            sdkParams.SDKType = SDKType.None;
            sdkParams.AppKey = "";
            sdkParams.PayKey = "";
            EditorUtility.SetDirty(sdkParams);

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.rastargame.celia");
            PlayerSettings.Android.keystoreName = "";
            PlayerSettings.Android.keystorePass = "";
            PlayerSettings.Android.keyaliasName = "";
            PlayerSettings.Android.keyaliasPass = "";
        }

        private PlistElementDict GetSDKSign(CeliaBuildOption option)
        {
            string fileName = CeliaBuilder.GetInputParam("Sign:", option.Args);
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(Path.Combine(Application.dataPath.Replace("Assets", ""), "BuildSettings", "IOSSignature", fileName + ".plist"));
            return plist.root;
        }

        private JObject GetSDKParams(CeliaBuildOption option)
        {
            string fileName = CeliaBuilder.GetInputParam("SDKParams:", option.Args);
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            string jsonStr = File.ReadAllText(Path.Combine(Application.dataPath.Replace("Assets", ""), "BuildSettings", "SDKParams", fileName + ".json"));
            return JObject.Parse(jsonStr);
        }

        /// <summary>
        /// 设置iOS包名、签名
        /// </summary>
        private void SetIOSSign(PlistElementDict signCfg)
        {
            if (signCfg == null)
            {
                return;
            }
            //包名
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, signCfg["pakageName"].AsString());
            Debug.Log("Pakage Name: " + PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS));
            //签名文件
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.iOS.appleDeveloperTeamID = signCfg["teamID"].AsString();
            PlayerSettings.iOS.iOSManualProvisioningProfileID = signCfg["provisioningProfileID"].AsString();
            PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Automatic;

            Debug.Log($"ProvisioningProfileID: {PlayerSettings.iOS.iOSManualProvisioningProfileID} ProfileType: {PlayerSettings.iOS.iOSManualProvisioningProfileType}");
        }

        /// <summary>
        /// 把SDK信息写入配置文件
        /// </summary>
        private void SetSDKInfo(JObject sdkParams, SDKType sdkType)
        {
            if (sdkParams == null)
            {
                return;
            }
            string appKey = sdkParams.Value<string>("AppKey");
            string payKey = sdkParams.Value<string>("PayKey");
            string appID = sdkParams.Value<string>("AppId");
            string cchID = sdkParams.Value<string>("CchId");
            string mdID = sdkParams.Value<string>("MdId");

            // 保存到包内配置
            SDKParams pakageSDKParams = AssetDatabase.LoadAssetAtPath<SDKParams>("Assets/Resources/SDKParams.asset");
            pakageSDKParams.SDKType = sdkType;
            pakageSDKParams.AppKey = appKey;
            pakageSDKParams.PayKey = payKey;
            EditorUtility.SetDirty(pakageSDKParams);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // SDK信息写入iOS SDK配置文件
            PlistDocument plist = new PlistDocument();
            switch (sdkType)
            {
                case SDKType.None:
                    return;
                case SDKType.Native:
                    plist.ReadFromFile($"{pluginIOSPath}/SDK/Rastar.plist");
                    plist.root.SetString("TTisDeBug", option.ReleaseLevel == ReleaseLevel.Alpha ? "1" : "0");
                    plist.root.SetString("App Key", appKey);
                    plist.root.SetString("App ID", appID);
                    plist.root.SetString("Cch ID", cchID);
                    plist.root.SetString("Md ID", mdID);
                    plist.WriteToFile($"{pluginIOSPath}/SDK/Rastar.plist");
                    break;
                case SDKType.NativeChukai:
                    plist.ReadFromFile($"{pluginIOSPath}/SDK/Rastar.plist");
                    plist.root.SetString("DeBug", option.ReleaseLevel == ReleaseLevel.Alpha ? "1" : "0");
                    plist.root.SetString("App Key", appKey);
                    plist.root.SetString("App ID", appID);
                    plist.root.SetString("Cch ID", cchID);
                    plist.root.SetString("Md ID", mdID);
                    plist.WriteToFile($"{pluginIOSPath}/SDK/Rastar.plist");
                    break;
                case SDKType.Oversea:
                    plist.ReadFromFile($"{pluginIOSPath}/SDK/RSOverseaSDK.plist");
                    plist.root.SetBoolean("AppsFlyer_isDeBug", option.ReleaseLevel == ReleaseLevel.Alpha);
                    plist.root.SetString("RS_AppKey", appKey);
                    plist.root.SetString("RS_AppID", appID);
                    plist.root.SetString("RS_cch_ID", cchID);
                    plist.root.SetString("RS_md_ID", mdID);
                    plist.root.SetString("RS_Schemes", $"rastar{cchID}{appID}");
                    plist.WriteToFile($"{pluginIOSPath}/SDK/RSOverseaSDK.plist");
                    break;
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 把SDK文件放入iOS文件夹
        /// </summary>
        private void SetSDKFolder(SDKType sdkType)
        {
            // 其他SDK设置为不导入
            SetAndroidPluginImport(pluginSavePath, false);
            // 复制要打包的SDK文件
            CopyFolder(pluginSavePath + $"IOS{sdkType.ToString()}", pluginIOSPath);
            // 包内SDK导入
            SetAndroidPluginImport(pluginIOSPath, true);
        }

        /// <summary>
        /// iOS SDK文件导入
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
                importer?.SetCompatibleWithPlatform(BuildTarget.iOS, add);
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
            FileSystemInfo[] fileinfo = folder.GetFileSystemInfos();
            foreach (FileSystemInfo info in fileinfo)
            {
                if (info is DirectoryInfo)
                {
                    DeleteFolder(info.FullName);
                }
                else
                {
                    info.Delete();
                }
            }
            folder.Delete();
            AssetDatabase.Refresh();
        }
        #endregion

        #endregion

        #region PostExcute
        void SetNoneSDKProjectSetting()
        {
            string path = GetXcodeProjectPath(option.PlayerOption.locationPathName);

        #region 添加XCode引用的Framework
            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            string target = proj.TargetGuidByName("Unity-iPhone");

            // 系统框架
            // Bugly依赖
            proj.AddFrameworkToProject(target, "libz.tbd", false);
            proj.AddFrameworkToProject(target, "libc++.tbd", false);
        #endregion

            // BuildSetting修改
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");

        #region 修改Xcode工程Info.plist
            string plistPath = Path.Combine(path, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;
            // 调整默认配置
            rootDict.SetString("CFBundleDevelopmentRegion", "zh_CN");
            // 权限配置
            rootDict.SetString("NSCameraUsageDescription", "是否允许访问相机?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许使用麦克风?");
            rootDict.SetString("NSPhotoLibraryAddUsageDescription", "是否允许添加照片?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许访问相册?");

            plist.WriteToFile(plistPath);
        #endregion

            File.WriteAllText(projPath, proj.WriteToString());
        }

        void SetNativeSDK()
        {
            string path = GetXcodeProjectPath(option.PlayerOption.locationPathName);

        #region 添加XCode引用的Framework
            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            string target = proj.TargetGuidByName("Unity-iPhone");
            
            // Bugly依赖
            proj.AddFrameworkToProject(target, "libz.tbd", false);
            proj.AddFrameworkToProject(target, "libc++.tbd", false);

            // 文件追加
            var fileName = "Rastar.plist";
            var filePath = Path.Combine("Assets/Plugins/iOS/SDK/", fileName);
            File.Copy(filePath, Path.Combine(path, fileName));
            proj.AddFileToBuild(target, proj.AddFile(fileName, fileName, PBXSourceTree.Source));
        #endregion

            // BuildSetting修改
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");

        #region 修改Xcode工程Info.plist
            string plistPath = Path.Combine(path, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;
            // 调整默认配置
            rootDict.SetString("CFBundleDevelopmentRegion", "zh_CN");
            // 权限配置
            rootDict.SetString("NSCameraUsageDescription", "是否允许访问相机?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许使用麦克风?");
            rootDict.SetString("NSPhotoLibraryAddUsageDescription", "是否允许添加照片?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许访问相册?");

            // URL types配置
            PlistElementArray URLTypes = plist.root.CreateArray("CFBundleURLTypes");
            //weixin
            PlistElementDict wxUrl = URLTypes.AddDict();
            wxUrl.SetString("CFBundleTypeRole", "Editor");
            wxUrl.SetString("CFBundleURLName", "weixin");
            PlistElementArray wxUrlScheme = wxUrl.CreateArray("CFBundleURLSchemes");
            wxUrlScheme.AddString("wxb0ffcabf2d045dd7");
            //tencent
            PlistElementDict tcUrl = URLTypes.AddDict();
            tcUrl.SetString("CFBundleTypeRole", "Editor");
            tcUrl.SetString("CFBundleURLName", "tencent");
            PlistElementArray tcUrlScheme = tcUrl.CreateArray("CFBundleURLSchemes");
            tcUrlScheme.AddString("tencent101539443");
            //QQ
            PlistElementDict qqUrl = URLTypes.AddDict();
            qqUrl.SetString("CFBundleTypeRole", "Editor");
            qqUrl.SetString("CFBundleURLName", "QQ");
            PlistElementArray qqUrlScheme = qqUrl.CreateArray("CFBundleURLSchemes");
            qqUrlScheme.AddString("QQ60d5e73");

            // LSApplicationQueriesSchemes配置
            PlistElementArray LSApplicationQueriesSchemes = plist.root.CreateArray("LSApplicationQueriesSchemes");
            LSApplicationQueriesSchemes.AddString("wechat");
            LSApplicationQueriesSchemes.AddString("weixin");
            LSApplicationQueriesSchemes.AddString("mqqapi");
            LSApplicationQueriesSchemes.AddString("mqq");
            LSApplicationQueriesSchemes.AddString("mqqOpensdkSSoLogin");
            LSApplicationQueriesSchemes.AddString("mqqconnect");
            LSApplicationQueriesSchemes.AddString("mqqopensdkdataline");
            LSApplicationQueriesSchemes.AddString("mqqopensdkgrouptribeshare");
            LSApplicationQueriesSchemes.AddString("mqqopensdkfriend");
            LSApplicationQueriesSchemes.AddString("mqqopensdkapi");
            LSApplicationQueriesSchemes.AddString("mqqopensdkapiV2");
            LSApplicationQueriesSchemes.AddString("mqqopensdkapiV3");
            LSApplicationQueriesSchemes.AddString("mqqopensdkapiV4");
            LSApplicationQueriesSchemes.AddString("mqqzoneopensdk");
            LSApplicationQueriesSchemes.AddString("wtloginmqq");
            LSApplicationQueriesSchemes.AddString("wtloginmqq2");
            LSApplicationQueriesSchemes.AddString("mqqwpa");
            LSApplicationQueriesSchemes.AddString("mqzone");
            LSApplicationQueriesSchemes.AddString("mqzoneV2");
            LSApplicationQueriesSchemes.AddString("mqzoneshare");
            LSApplicationQueriesSchemes.AddString("wtloginqzone");
            LSApplicationQueriesSchemes.AddString("mqzonewx");
            LSApplicationQueriesSchemes.AddString("mqzoneopensdkapiV2");
            LSApplicationQueriesSchemes.AddString("mqzoneopensdkapi19");
            LSApplicationQueriesSchemes.AddString("mqzoneopensdkapiV2");
            LSApplicationQueriesSchemes.AddString("mqqbrowser");
            LSApplicationQueriesSchemes.AddString("mttbrowser");
            LSApplicationQueriesSchemes.AddString("TencentWeibo");
            LSApplicationQueriesSchemes.AddString("tencentweiboSdkv2");
            LSApplicationQueriesSchemes.AddString("tim");
            LSApplicationQueriesSchemes.AddString("timapi");
            LSApplicationQueriesSchemes.AddString("timopensdkfriend");
            LSApplicationQueriesSchemes.AddString("timwpa");
            LSApplicationQueriesSchemes.AddString("timegamebindinggroup");
            LSApplicationQueriesSchemes.AddString("timapiwallet");
            LSApplicationQueriesSchemes.AddString("timOpensdkSSoLogin");
            LSApplicationQueriesSchemes.AddString("wtlogintim");
            LSApplicationQueriesSchemes.AddString("timopensdkgrouptribeshare");
            LSApplicationQueriesSchemes.AddString("timopensdkapiV4");
            LSApplicationQueriesSchemes.AddString("timgamebindinggroup");
            LSApplicationQueriesSchemes.AddString("timeopensdkdataline");
            LSApplicationQueriesSchemes.AddString("wtlogintimV1");
            LSApplicationQueriesSchemes.AddString("timpapiV1");

            plist.WriteToFile(plistPath);
        #endregion

            File.WriteAllText(projPath, proj.WriteToString());
        }

        void SetNativeChukaiSDK()
        {
            string path = GetXcodeProjectPath(option.PlayerOption.locationPathName);

        #region 添加XCode引用的Framework
            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            string target = proj.TargetGuidByName("Unity-iPhone");

            // 系统框架
            // Bugly依赖
            proj.AddFrameworkToProject(target, "libz.tbd", false);
            proj.AddFrameworkToProject(target, "libc++.tbd", false);
        
            // SDK依赖
            proj.AddFrameworkToProject(target, "libicucore.tbd", false);
            proj.AddFrameworkToProject(target, "StoreKit.framework", false);
            proj.AddFrameworkToProject(target, "MobileCoreServices.framework", false);
            proj.AddFrameworkToProject(target, "WebKit.framework", false);
            proj.AddFrameworkToProject(target, "SafariServices.framework", true);
            proj.AddFrameworkToProject(target, "AdSupport.framework", false);
            proj.AddFrameworkToProject(target, "CoreTelephony.framework", false);
            
            // 文件追加
            var fileName = "Rastar.plist";
            var filePath = Path.Combine("Assets/Plugins/iOS/SDK/", fileName);
            File.Copy(filePath, Path.Combine(path, fileName));
            proj.AddFileToBuild(target, proj.AddFile(fileName, fileName, PBXSourceTree.Source));
        #endregion

            // BuildSetting修改
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");

        #region 修改Xcode工程Info.plist
            string plistPath = Path.Combine(path, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;
            // 调整默认配置
            rootDict.SetString("CFBundleDevelopmentRegion", "zh_CN");
            // 权限配置
            rootDict.SetString("NSCameraUsageDescription", "是否允许访问相机?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许使用麦克风?");
            rootDict.SetString("NSPhotoLibraryAddUsageDescription", "是否允许添加照片?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许访问相册?");
            // SDK要求的package-id参数，参数为当前时间戳+6个随机⼤写字母，每个游戏包参数保证唯⼀；出包提审前同步给到运营，运营将参数给到后端同事做账号映射
            var pakageID = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds).ToString();
            for (int i = 0; i < 6; i++)
            {
                pakageID += (char)UnityEngine.Random.Range(65, 91);
            }
            rootDict.SetString("package-id", pakageID);

            plist.WriteToFile(plistPath);
        #endregion

            File.WriteAllText(projPath, proj.WriteToString());
        }

        void SetOverseaSDK(JObject sdkParams)
        {
            string appID = "";
            string cchID = "";
            if (sdkParams != null)
            {
                appID = sdkParams.Value<string>("AppId");
                cchID = sdkParams.Value<string>("CchId");
            }

            string path = GetXcodeProjectPath(option.PlayerOption.locationPathName);

        #region 添加XCode引用的Framework
            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            string target = proj.TargetGuidByName("Unity-iPhone");
            // 系统框架
            // Bugly依赖
            proj.AddFrameworkToProject(target, "libz.tbd", false);
            proj.AddFrameworkToProject(target, "libc++.tbd", false);

            // SDK依赖
            proj.AddFrameworkToProject(target, "iAd.framework", false);
            proj.AddFrameworkToProject(target, "StoreKit.framework", false);
            proj.AddFrameworkToProject(target, "AdSupport.framework", false);
            proj.AddFrameworkToProject(target, "UserNotifications.framework", false);
            proj.AddFrameworkToProject(target, "NotificationCenter.framework", false);
            proj.AddFrameworkToProject(target, "MobileCoreServices.framework", false);
            proj.AddFrameworkToProject(target, "SafariServices.framework", true);
            proj.AddFrameworkToProject(target, "ImageIO.framework", false);
            proj.AddFrameworkToProject(target, "Social.framework", false);
            proj.AddFrameworkToProject(target, "MessageUI.framework", false);
            proj.AddFrameworkToProject(target, "WebKit.framework", false);
            proj.AddFrameworkToProject(target, "AssetsLibrary.framework", false);
            proj.AddFrameworkToProject(target, "QuartzCore.framework", false);
            proj.AddFrameworkToProject(target, "ReplayKit.framework", false);
            proj.AddFrameworkToProject(target, "MediaPlayer.framework", false);

            // 文件追加
            var fileName = "RSOverseaSDK.plist";
            var filePath = Path.Combine("Assets/Plugins/iOS/SDK/", fileName);
            File.Copy(filePath, Path.Combine(path, fileName));
            proj.AddFileToBuild(target, proj.AddFile(fileName, fileName, PBXSourceTree.Source));
        #endregion

            // BuildSetting修改
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-all_load");

        #region 修改Xcode工程Info.plist
            string plistPath = Path.Combine(path, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;
            // 调整默认配置
            rootDict.SetString("CFBundleDevelopmentRegion", "zh_CN");
            rootDict.SetString("CFBundleVersion", "1");
            rootDict.SetString("FacebookAppID", "1496435593826813");
            rootDict.SetString("FacebookAppDisplayName", "少女的王座");
            rootDict.SetBoolean("UIViewControllerBasedStatusBarAppearance", false);
            rootDict.CreateArray("UIBackgroundModes").AddString("remote-notification");
            // 权限配置
            rootDict.SetString("NSCameraUsageDescription", "是否允许访问相机?");
            rootDict.SetString("NSPhotoLibraryAddUsageDescription", "是否允许添加照片?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许访问相册?");

            // URL types配置
            PlistElementArray URLTypes = plist.root.CreateArray("CFBundleURLTypes");
            string[] urlSchemes = { "fb1496435593826813", $"rastar{cchID}{appID}", "com.guanghuan.sndwzft", "liveops", "com.guanghuan.sndwzft://" };
            foreach (string str in urlSchemes)
            {
                PlistElementDict typeRole = URLTypes.AddDict();
                typeRole.SetString("CFBundleTypeRole", "Editor");
                PlistElementArray urlScheme = typeRole.CreateArray("CFBundleURLSchemes");
                urlScheme.AddString(str);
            }

            // LSApplicationQueriesSchemes配置
            PlistElementArray LSApplicationQueriesSchemes = plist.root.CreateArray("LSApplicationQueriesSchemes");
            // facebook接入配置
            LSApplicationQueriesSchemes.AddString("fbapi");
            LSApplicationQueriesSchemes.AddString("fb-messenger-share-api");
            LSApplicationQueriesSchemes.AddString("fbauth2");
            LSApplicationQueriesSchemes.AddString("fbshareextension");
            // Navercafe接入配置
            LSApplicationQueriesSchemes.AddString("navercafe");
            LSApplicationQueriesSchemes.AddString("naversearchapp");
            LSApplicationQueriesSchemes.AddString("naversearchthirdlogin");
            // Line接入配置
            LSApplicationQueriesSchemes.AddString("lineauth");
            LSApplicationQueriesSchemes.AddString("line3rdp.$(APP_IDENTIFIER)");
            LSApplicationQueriesSchemes.AddString("line");

            plist.WriteToFile(plistPath);
        #endregion

            // Capabilitise添加
            var entitlementsFileName = "celia.entitlements";
            var entitlementsFilePath = Path.Combine("Assets/Plugins/iOS/SDK/", entitlementsFileName);
            File.Copy(entitlementsFilePath, Path.Combine(path, entitlementsFileName));
            proj.AddFileToBuild(target, proj.AddFile(entitlementsFileName, entitlementsFileName, PBXSourceTree.Source));

            proj.AddCapability(target, PBXCapabilityType.GameCenter);
            var array = rootDict.CreateArray("UIRequiredDeviceCapabilities");
            array.AddString("armv7");
            array.AddString("gamekit");
            plist.WriteToFile(plistPath);
            proj.AddCapability(target, PBXCapabilityType.AssociatedDomains, entitlementsFileName);
            proj.AddCapability(target, PBXCapabilityType.BackgroundModes);
            proj.AddCapability(target, PBXCapabilityType.PushNotifications, entitlementsFileName);

            proj.WriteToFile(projPath);

            // 修改语言支持
            // AddLanguage(path, new string[] { "zh", "zh-Hant", "vi-VN", "th", "ri", "ko", "es-ES", "id-ID" });

            File.WriteAllText(projPath, proj.WriteToString());
        }

        // void AddLanguage(string path, params string[] languages)
        // {
        //     //string[] langs = new string[] { "zh","zh-Hant", "vi-VN", "th", "ri", "ko", "es-ES", "id-ID" };
        //     string plistPath = Path.Combine(path, "Info.plist");
        //     PlistDocument plist = new PlistDocument();
        //     plist.ReadFromFile(plistPath);

        //     var localizationKey = "CFBundleLocalizations";

        //     var localizations = plist.root.values
        //     .Where(kv => kv.Key == localizationKey)
        //     .Select(kv => kv.Value)
        //     .Cast<PlistElementArray>()
        //     .FirstOrDefault();

        //     if (localizations == null)
        //         localizations = plist.root.CreateArray(localizationKey);

        //     foreach (var language in languages)
        //     {
        //         if (localizations.values.Select(el => el.AsString()).Contains(language) == false)
        //             localizations.AddString(language);
        //     }

        //     plist.WriteToFile(plistPath);
        // }
        #endregion
#endif
    }
}