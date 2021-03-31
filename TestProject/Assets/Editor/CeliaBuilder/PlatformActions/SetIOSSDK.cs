using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;
using System.IO;
using System;
#if UNITY_EDITOR_OSX
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
#endif

namespace celia.game.editor
{
    public class SetIOSSDK : PlatformAction, IIOSAction
    {
        private CeliaBuildOption option;

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

                case SDKType.CeliaOversea:
                    SetCeliaOverseaSDK();
                    break;
            }

            // 复原文件夹
            SetSDKFolderBack();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("SetIOSSDK PostExcuted!");
#endif
        }

#if UNITY_EDITOR_OSX
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

        #region PreExcute

        /// <summary>
        /// 删除Assets/Plugins/iOS文件夹文件，复原配置
        /// </summary>
        private void SetSDKFolderBack()
        {
            if (Directory.Exists(pluginIOSPath))
            {
                Directory.Delete(pluginIOSPath, true);
            }

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
                return;
            }
            FileInfo[] files = folder.GetFiles();
            foreach (var file in files)
            {
                PluginImporter importer = AssetImporter.GetAtPath(file.FullName.Replace(Application.dataPath.Replace('/', '\\'), "Assets")) as PluginImporter;
                importer?.SetCompatibleWithPlatform(BuildTarget.iOS, add);
            }

            DirectoryInfo[] subFolders = folder.GetDirectories();
            foreach (var subfolder in subFolders)
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

        #endregion 文件操作

        #endregion PreExcute

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

        #endregion 添加XCode引用的Framework

            // BuildSetting修改
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");

        #region 修改Xcode工程Info.plist

            string plistPath = Path.Combine(path, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;
            // 调整默认配置
            rootDict.SetString("CFBundleDevelopmentRegion", "zh_TW");
            // 权限配置
            rootDict.SetString("NSCameraUsageDescription", "是否允许访问相机?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许使用麦克风?");
            rootDict.SetString("NSPhotoLibraryAddUsageDescription", "是否允许添加照片?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许访问相册?");
            rootDict.SetString("NSLocationUsageDescription", "App需要您的同意,才能访问位置");
            rootDict.SetString("NSLocationWhenInUseUsageDescription", "App需要您的同意,才能在使用期间访问位置");
            rootDict.SetString("NSLocationAlwaysUsageDescription", "App需要您的同意,才能始终访问位置");

            // Set encryption usage boolean
            string encryptKey = "ITSAppUsesNonExemptEncryption";
            rootDict.SetBoolean(encryptKey, false);
            // remove exit on suspend if it exists.ios13新增
            string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
            if (rootDict.values.ContainsKey(exitsOnSuspendKey))
            {
                rootDict.values.Remove(exitsOnSuspendKey);
            }
            plist.WriteToFile(plistPath);

        #endregion 修改Xcode工程Info.plist

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
            //星辉SDK依赖
            proj.AddFrameworkToProject(target, "libsqlite3.tbd", false);
            proj.AddFrameworkToProject(target, "libsqlite3.0.tbd", false);
            proj.AddFrameworkToProject(target, "libicucore.tbd", false);
            proj.AddFrameworkToProject(target, "SafariServices.framework", false);
            proj.AddFrameworkToProject(target, "WebKit.framework", false);
            proj.AddFrameworkToProject(target, "MobileCoreServices.framework", false);
            proj.AddFrameworkToProject(target, "ImageIO.framework", false);
            proj.AddFrameworkToProject(target, "Photos.framework", false);
            // TPNS依赖
            proj.AddFrameworkToProject(target, "CoreTelephony.framework", false);
            proj.AddFrameworkToProject(target, "SystemConfiguration.framework", false);
            proj.AddFrameworkToProject(target, "UserNotifications.framework", false);
            proj.AddFrameworkToProject(target, "CoreData.framework", false);
            proj.AddFrameworkToProject(target, "CFNetwork.framework", false);

        #endregion 添加XCode引用的Framework

            // BuildSetting修改
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-lc++");
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-fprofile-instr-generate");
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-all_load");

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

            //星辉SDK默认为暗黑模式 但是开启之后隐私协议会看不到 所以要关闭暗黑模式
            //rootDict.SetString("UIUserInterfaceStyle", "Light");
            // SDK相关参数设置
            rootDict.SetString("RaStarUMKey", "5bc6b08af1f55681f30000da");
            rootDict.SetString("RaStarWeChatKey", "wxf4ea05dfad6b67f3");
            rootDict.SetString("RaStarWeChatSecret", "ec8b3c05e2d81442c14bbc33450c24e1");
            rootDict.SetString("RaStarWeChatUniversalLink", "https://3rd-sy.rastargame.com/sndwz/");
            rootDict.SetString("RaStarQQID", "101908009");
            rootDict.SetString("RaStarQQSecret", "5cbe0e16d98468e954e14087fb963ffe");
            rootDict.SetString("RaStarWeiboAppKey", "2546046978");
            rootDict.SetString("RaStarWeiboSecret", "ab014360f368b311739e5060256d4284");
            PlistElementArray RaStarShareTypeArray = rootDict.CreateArray("RaStarShareTypeArray");
            RaStarShareTypeArray.AddString("微信");
            RaStarShareTypeArray.AddString("朋友圈");
            RaStarShareTypeArray.AddString("微博");
            RaStarShareTypeArray.AddString("QQ");
            RaStarShareTypeArray.AddString("QQ空间");
            //文件共享
            rootDict.SetBoolean("UIFileSharingEnabled",true);
            // Set encryption usage boolean
            string encryptKey = "ITSAppUsesNonExemptEncryption";
            rootDict.SetBoolean(encryptKey, false);
            // remove exit on suspend if it exists.ios13新增
            string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
            if (rootDict.values.ContainsKey(exitsOnSuspendKey))
            {
                rootDict.values.Remove(exitsOnSuspendKey);
            }
            // URL types配置
            PlistElementArray URLTypes = plist.root.CreateArray("CFBundleURLTypes");
            //weixin
            PlistElementDict wxUrl = URLTypes.AddDict();
            wxUrl.SetString("CFBundleTypeRole", "Editor");
            wxUrl.SetString("CFBundleURLName", "weixin");
            PlistElementArray wxUrlScheme = wxUrl.CreateArray("CFBundleURLSchemes");
            wxUrlScheme.AddString("wxf4ea05dfad6b67f3");
            //weibo
            PlistElementDict weibiUrl = URLTypes.AddDict();
            weibiUrl.SetString("CFBundleTypeRole", "Editor");
            weibiUrl.SetString("CFBundleURLName", "weibo");
            PlistElementArray weibiUrlScheme = weibiUrl.CreateArray("CFBundleURLSchemes");
            weibiUrlScheme.AddString("wb2546046978");
            //tencent
            PlistElementDict tcUrl = URLTypes.AddDict();
            tcUrl.SetString("CFBundleTypeRole", "Editor");
            tcUrl.SetString("CFBundleURLName", "tencent");
            PlistElementArray tcUrlScheme = tcUrl.CreateArray("CFBundleURLSchemes");
            tcUrlScheme.AddString("tencent101908009");
            //QQ
            PlistElementDict qqUrl = URLTypes.AddDict();
            qqUrl.SetString("CFBundleTypeRole", "Editor");
            qqUrl.SetString("CFBundleURLName", "QQ");
            PlistElementArray qqUrlScheme = qqUrl.CreateArray("CFBundleURLSchemes");
            qqUrlScheme.AddString("QQ0612fe29");

        #endregion 修改Xcode工程Info.plist

        #region LSApplicationQueriesSchemes配置

            PlistElementArray LSApplicationQueriesSchemes = plist.root.CreateArray("LSApplicationQueriesSchemes");
            LSApplicationQueriesSchemes.AddString("wechat");
            LSApplicationQueriesSchemes.AddString("weixin");
            LSApplicationQueriesSchemes.AddString("weixinULAPI");
            LSApplicationQueriesSchemes.AddString("sinaweibohd");
            LSApplicationQueriesSchemes.AddString("sinaweibo");
            LSApplicationQueriesSchemes.AddString("sinaweibosso");
            LSApplicationQueriesSchemes.AddString("weibosdk");
            LSApplicationQueriesSchemes.AddString("weibosdk2.5");
            LSApplicationQueriesSchemes.AddString("mqqopensdklaunchminiapp");
            LSApplicationQueriesSchemes.AddString("mqqopensdkminiapp");
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
            LSApplicationQueriesSchemes.AddString("mqzoneopensdkapi");
            LSApplicationQueriesSchemes.AddString("mqqbrowser");
            LSApplicationQueriesSchemes.AddString("mttbrowser");
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

        #endregion LSApplicationQueriesSchemes配置

            //文件追加
            var entitlementsFileName = "sndwz.entitlements";
            var entitlementsFilePath = Path.Combine("Assets/Plugins/iOS/SDK/", entitlementsFileName);
            File.Copy(entitlementsFilePath, Path.Combine(path, entitlementsFileName));
            proj.AddFileToBuild(target, proj.AddFile(entitlementsFileName, entitlementsFileName, PBXSourceTree.Source));
            proj.AddCapability(target, PBXCapabilityType.InAppPurchase);
            proj.AddCapability(target, PBXCapabilityType.AccessWiFiInformation, entitlementsFileName);
            proj.AddCapability(target, PBXCapabilityType.AssociatedDomains, entitlementsFileName);
            proj.AddCapability(target, PBXCapabilityType.PushNotifications, entitlementsFileName);
            proj.WriteToFile(projPath);
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

        #endregion 添加XCode引用的Framework

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

        #endregion 修改Xcode工程Info.plist

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

        #endregion 添加XCode引用的Framework

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

        #endregion 修改Xcode工程Info.plist

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

        void SetCeliaOverseaSDK(){
            string path = GetXcodeProjectPath(option.PlayerOption.locationPathName);
            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            string target = proj.TargetGuidByName("Unity-iPhone");
            // BuildSetting修改
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");
            proj.SetBuildProperty(target, "EMBED_ASSET_PACKS_IN_PRODUCT_BUNDLE", "YES");//FB需要
            proj.SetBuildProperty(target, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");//FB需要

        #region 添加XCode引用的Framework

            // SDK依赖 --AIHelp
            proj.AddFrameworkToProject(target, "libsqlite3.tbd", false);
            proj.AddFrameworkToProject(target, "libresolv.tbd", false);
            proj.AddFrameworkToProject(target, "WebKit.framework", false);
            // SDK依赖 --Google
            proj.AddFrameworkToProject(target, "LocalAuthentication.framework", false);
            proj.AddFrameworkToProject(target, "SafariServices.framework", false);
            proj.AddFrameworkToProject(target, "AuthenticationServices.framework", false);
            proj.AddFrameworkToProject(target, "SystemConfiguration.framework", false);
            // SDK依赖 --Apple
            proj.AddFrameworkToProject(target, "storekit.framework", false);
            proj.AddFrameworkToProject(target, "AuthenticationServices.framework", false);
            proj.AddFrameworkToProject(target, "gamekit.framework", false);
            // SDK依赖 --Adjust
            proj.AddFrameworkToProject(target, "AdSupport.framework", false);
            proj.AddFrameworkToProject(target, "iAd.framework", false);

            //EmbedFrameworks --Add to Embedded Binaries
            string defaultLocationInProj = "Plugins/iOS/SDK";
            string[] frameworkNames = { "FaceBookSDK/FBSDKCoreKit.framework", "FaceBookSDK/FBSDKLoginKit.framework", "FaceBookSDK/FBSDKShareKit.framework", "AdjustSDK/AdjustSdk.framework" };
            foreach (var str in frameworkNames)
            {
                string framework = Path.Combine(defaultLocationInProj, str);
                string fileGuid = proj.AddFile(framework, "Frameworks/" + framework, PBXSourceTree.Sdk);
                PBXProjectExtensions.AddFileToEmbedFrameworks(proj, target, fileGuid);
            }
            proj.SetBuildProperty(target, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");

        #endregion 添加XCode引用的Framework

            string plistPath = Path.Combine(path, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;

        #region 修改Xcode工程Info.plist

            /* 从iOS9开始所有的app对外http协议默认要求改成https 若需要添加http协议支持需要额外添加*/
            // Add value of NSAppTransportSecurity in Xcode plist
            PlistElementDict dictTmp = rootDict.CreateDict("NSAppTransportSecurity");
            dictTmp.SetBoolean("NSAllowsArbitraryLoads", true);
            //AIHelp-权限配置
            rootDict.SetString("NSCameraUsageDescription", "是否允许访问相机?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许使用麦克风?");
            rootDict.SetString("NSPhotoLibraryAddUsageDescription", "是否允许添加照片?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许访问相册?");
            rootDict.SetString("NSLocationUsageDescription", "App需要您的同意,才能访问位置");
            rootDict.SetString("NSLocationWhenInUseUsageDescription", "App需要您的同意,才能在使用期间访问位置");
            rootDict.SetString("NSLocationAlwaysUsageDescription", "App需要您的同意,才能始终访问位置");

            rootDict.SetString("CFBundleDevelopmentRegion", "zh_TW");
            //rootDict.SetString("CFBundleVersion", "1");
            // SDK相关参数设置
            rootDict.SetString("FacebookAppID", "949004278872387");
            rootDict.SetString("GoogleClientID", "554619719418-0hdrkdprcsksigpldvtr9n5lu2lvt5kn.apps.googleusercontent.com");
            rootDict.SetString("FacebookAppDisplayName", "少女的王座");
            rootDict.SetString("AIHelpAppID", "elextech_platform_15ce9b10-f784-4ab5-8ee4-45efab40bd6a");
            rootDict.SetString("AIHelpAppKey", "ELEXTECH_app_50dd4661c57843778d850769a02f8a09");
            rootDict.SetString("AIHelpDomain", "elextech@aihelp.net");
            rootDict.SetString("AdjustAppToken", "1k2jm7bpansw");
            rootDict.SetString("AdjustAppSecret", "1,750848352-1884995334-181661496-1073918938");
            //文件共享
            rootDict.SetBoolean("UIFileSharingEnabled",true);
            // Set encryption usage boolean
            string encryptKey = "ITSAppUsesNonExemptEncryption";
            rootDict.SetBoolean(encryptKey, false);
            // remove exit on suspend if it exists.ios13新增
            string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
            if (rootDict.values.ContainsKey(exitsOnSuspendKey))
            {
                rootDict.values.Remove(exitsOnSuspendKey);
            }
            // URL types配置
            PlistElementArray URLTypes = rootDict.CreateArray("CFBundleURLTypes");
            //Facebook
            PlistElementDict typeRoleFB = URLTypes.AddDict();
            typeRoleFB.SetString("CFBundleTypeRole", "Editor");
            PlistElementArray urlSchemeFB = typeRoleFB.CreateArray("CFBundleURLSchemes");
            urlSchemeFB.AddString("fb949004278872387");
            //Google
            PlistElementDict typeRole = URLTypes.AddDict();
            typeRole.SetString("CFBundleTypeRole", "Editor");
            typeRole.SetString("CFBundleURLName", "com.googleusercontent.apps.554619719418-0hdrkdprcsksigpldvtr9n5lu2lvt5kn");
            PlistElementArray urlScheme = typeRole.CreateArray("CFBundleURLSchemes");
            urlScheme.AddString("com.googleusercontent.apps.554619719418-0hdrkdprcsksigpldvtr9n5lu2lvt5kn");

            // LSApplicationQueriesSchemes配置
            PlistElementArray LSApplicationQueriesSchemes = rootDict.CreateArray("LSApplicationQueriesSchemes");
            // facebook接入配置
            LSApplicationQueriesSchemes.AddString("fbapi");
            LSApplicationQueriesSchemes.AddString("fb-messenger-share-api");
            LSApplicationQueriesSchemes.AddString("fbauth2");
            LSApplicationQueriesSchemes.AddString("fbshareextension");
            // Line接入配置
            LSApplicationQueriesSchemes.AddString("lineauth");
            LSApplicationQueriesSchemes.AddString("line3rdp.$(APP_IDENTIFIER)");
            LSApplicationQueriesSchemes.AddString("line");
            // 文件追加
            var fileName = "GoogleService-Info.plist";
            var filePath = Path.Combine("Assets/Plugins/iOS/SDK/FCM/", fileName);
            File.Copy(filePath, Path.Combine(option.PlayerOption.locationPathName, "GoogleService-Info.plist"), true);
            proj.AddFileToBuild(target, proj.AddFile(fileName, fileName, PBXSourceTree.Source));

        #endregion 修改Xcode工程Info.plist

            // Capabilitise添加
            var entitlementsFileName = "tw.entitlements";
            var entitlementsFilePath = Path.Combine("Assets/Plugins/iOS/SDK/", entitlementsFileName);
            File.Copy(entitlementsFilePath, Path.Combine(option.PlayerOption.locationPathName, entitlementsFileName),true);
            proj.AddFileToBuild(target, proj.AddFile(entitlementsFileName, entitlementsFileName, PBXSourceTree.Source));
            proj.AddCapability(target, PBXCapabilityType.InAppPurchase, entitlementsFileName);
            proj.AddCapability(target, PBXCapabilityType.GameCenter);
            proj.AddCapability(target, PBXCapabilityType.PushNotifications, entitlementsFileName);
            plist.WriteToFile(plistPath);
            proj.WriteToFile(projPath);
            File.WriteAllText(projPath, proj.WriteToString());
        }

        #endregion PostExcute

#endif
    }
}