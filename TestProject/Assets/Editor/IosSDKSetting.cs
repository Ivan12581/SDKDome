﻿using UnityEngine;
//#if UNITY_EDITOR_OSX
using UnityEditor.iOS.Xcode;
//#endif
using System.IO;
using System.Linq;
using UnityEditor;
using System;
using UnityEngine.Rendering;
using UnityEditor.Build.Reporting;
using NUnit.Framework;

namespace celia.game.editor
{
    public class IosSDKSetting : Editor
    {
        //#if UNITY_EDITOR_OSX
        public static string path = "/Users/mini/Documents/SDKXCodeProj";
        public static string Otherpath = "/Users/mini/Documents/NewSdkXCodeProj";
        [MenuItem("出包设置/IOS/1.IOSPlayerSettings")]
        public static void IOSPlayerSettings()
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.elex.girlsthrone.tw");
            PlayerSettings.productName = "少女的王座";
            PlayerSettings.companyName = "EMG TECHNOLOGY LIMITED";
            PlayerSettings.bundleVersion = "0.1";

            // 关闭Unity自带的Splash
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            // 强制使用OPENGL ES3
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new GraphicsDeviceType[]
            {
                GraphicsDeviceType.Vulkan,
                GraphicsDeviceType.OpenGLES3
            });
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.iOS, Il2CppCompilerConfiguration.Release);
            PlayerSettings.iOS.scriptCallOptimization = ScriptCallOptimizationLevel.SlowAndSafe;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "HOTFIX_ENABLE;CELIA_RELEASE;AOT");

            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.iOS.appleDeveloperTeamID = "5HK243M76T";
            PlayerSettings.iOS.iOSManualProvisioningProfileID = "cc302810-fe11-4b7d-86e5-df0bf7bd2aac";
            PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Automatic;
            Debug.Log("--**--1.IOSPlayerSettings--**--");
        }


        [MenuItem("出包设置/IOS/2.IOSGameSettings")]
        public static void IOSGameSettings()
        {
            //加密配置文件
            //剧情相关
            //服务器ip端口配置
            SDKParams sdkParams = AssetDatabase.LoadAssetAtPath<SDKParams>("Assets/Resources/SDKParams.asset");
            sdkParams.SDKType = SDKType.None;
            EditorUtility.SetDirty(sdkParams);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("--**--2.IOSGameSettings--**--");
        }


        [MenuItem("出包设置/IOS/3.IOSBuildPlayer")]
        public static bool IOSBuildPlayer()
        {
            BuildPlayerOptions CurIOSOption = new BuildPlayerOptions();
            CurIOSOption.target = BuildTarget.iOS;
            CurIOSOption.targetGroup = BuildTargetGroup.iOS;
            CurIOSOption.locationPathName = path;
            CurIOSOption.options = BuildOptions.StrictMode;
            CurIOSOption.scenes = SetScenes.ProcessScenes().ToArray();
            BuildReport report = BuildPipeline.BuildPlayer(CurIOSOption);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("--**--3.IOSBuildPlayer--**--");
                Debug.Log("Building successed, the total size is:" + summary.totalSize + " bytes" + ";time is:" + summary.totalTime);
                return true;
            }else {
                Debug.Log("--**--3.IOSBuildPlayer--**--");
                Debug.Log("Build failed");
                return false;
            }

        }
        [MenuItem("出包设置/IOS/44.NewIOSXcodeSettings")]
        public static void NewIOSXcodeSettings()
        {

            //添加XCode引用的Framework
            string projPath = PBXProject.GetPBXProjectPath(Otherpath);
            PBXProject proj = new PBXProject();

            proj.ReadFromString(File.ReadAllText(projPath));
            string target = proj.TargetGuidByName("Unity-iPhone");
            // BuildSetting修改
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");//这个好像是bugly需要的
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");//这个google等其他sdk非常需要的
            //proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-all_load");
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
            #endregion

            string plistPath = Path.Combine(Otherpath, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;
            #region 修改Xcode工程Info.plist
            /* iOS9所有的app对外http协议默认要求改成https */
            // Add value of NSAppTransportSecurity in Xcode plist
            PlistElementDict dictTmp = rootDict.CreateDict("NSAppTransportSecurity");
            dictTmp.SetBoolean("NSAllowsArbitraryLoads", true);
            //AIHelp-权限配置
            rootDict.SetString("NSCameraUsageDescription", "是否允许访问相机?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许使用麦克风?");
            rootDict.SetString("NSPhotoLibraryAddUsageDescription", "是否允许添加照片?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许访问相册?");

            rootDict.SetString("CFBundleDevelopmentRegion", "zh_TW");
            rootDict.SetString("CFBundleVersion", "1");
            // SDK相关参数设置
            rootDict.SetString("FacebookAppID", "949004278872387");
            rootDict.SetString("GoogleClientID", "554619719418-0hdrkdprcsksigpldvtr9n5lu2lvt5kn.apps.googleusercontent.com");
            rootDict.SetString("FacebookAppDisplayName", "少女的王座");
            rootDict.SetString("AIHelpAppID", "elextech_platform_15ce9b10-f784-4ab5-8ee4-45efab40bd6a");
            rootDict.SetString("AIHelpAppKey", "ELEXTECH_app_50dd4661c57843778d850769a02f8a09");
            rootDict.SetString("AIHelpDomain", "elextech@aihelp.net");
            rootDict.SetString("AdjustAppToken", "1k2jm7bpansw");
            rootDict.SetString("AdjustAppSecret", "1,750848352-1884995334-181661496-1073918938");
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
            #endregion

            ProjectCapabilityManager projectCapabilityManager = new ProjectCapabilityManager(projPath, "tw.entitlements", PBXProject.GetUnityTargetName());
            projectCapabilityManager.AddGameCenter();
            projectCapabilityManager.AddInAppPurchase();
            plist.WriteToFile(plistPath);
            proj.WriteToFile(projPath);
            Debug.Log("--**--4.IOSXcodeSettings--**--");
        }
        [MenuItem("出包设置/IOS/4.IOSXcodeSettings")]
        public static void IOSXcodeSettings()
        {
            string CurPath = path;
            //if (!string.IsNullOrEmpty(_path))
            //{
            //    CurPath = _path;
            //}
            //添加XCode引用的Framework
            string projPath = PBXProject.GetPBXProjectPath(CurPath);
            PBXProject proj = new PBXProject();

            proj.ReadFromString(File.ReadAllText(projPath));
            string target = proj.TargetGuidByName("Unity-iPhone");
            // BuildSetting修改
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");//这个好像是bugly需要的
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");//这个google等其他sdk非常需要的
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
            #endregion

            string plistPath = Path.Combine(path, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;
            #region 修改Xcode工程Info.plist
            /* iOS9所有的app对外http协议默认要求改成https */
            // Add value of NSAppTransportSecurity in Xcode plist
            PlistElementDict dictTmp = rootDict.CreateDict("NSAppTransportSecurity");
            dictTmp.SetBoolean("NSAllowsArbitraryLoads", true);
            //AIHelp-权限配置
            rootDict.SetString("NSCameraUsageDescription", "是否允许访问相机?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许使用麦克风?");
            rootDict.SetString("NSPhotoLibraryAddUsageDescription", "是否允许添加照片?");
            rootDict.SetString("NSMicrophoneUsageDescription", "是否允许访问相册?");

            rootDict.SetString("CFBundleDevelopmentRegion", "zh_TW");
            rootDict.SetString("CFBundleVersion", "1");
            // SDK相关参数设置
            rootDict.SetString("FacebookAppID", "949004278872387");
            rootDict.SetString("GoogleClientID", "554619719418-0hdrkdprcsksigpldvtr9n5lu2lvt5kn.apps.googleusercontent.com");
            rootDict.SetString("FacebookAppDisplayName", "少女的王座");
            rootDict.SetString("AIHelpAppID", "elextech_platform_15ce9b10-f784-4ab5-8ee4-45efab40bd6a");
            rootDict.SetString("AIHelpAppKey", "ELEXTECH_app_50dd4661c57843778d850769a02f8a09");
            rootDict.SetString("AIHelpDomain", "elextech@aihelp.net");
            rootDict.SetString("AdjustAppToken", "1k2jm7bpansw");
            rootDict.SetString("AdjustAppSecret", "1,750848352-1884995334-181661496-1073918938");
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
            LSApplicationQueriesSchemes.AddString("fbapi20130214");
            LSApplicationQueriesSchemes.AddString("fbapi20130410");
            LSApplicationQueriesSchemes.AddString("fbapi20130702");
            LSApplicationQueriesSchemes.AddString("fbapi20131010");
            LSApplicationQueriesSchemes.AddString("fbapi20131219");
            LSApplicationQueriesSchemes.AddString("fbapi20140410");
            LSApplicationQueriesSchemes.AddString("fbapi20140116");
            LSApplicationQueriesSchemes.AddString("fbapi20150313");
            LSApplicationQueriesSchemes.AddString("fbapi20150629");
            LSApplicationQueriesSchemes.AddString("fbapi20160328");
            LSApplicationQueriesSchemes.AddString("fb-messenger-share-api");
            LSApplicationQueriesSchemes.AddString("fbauth2");
            LSApplicationQueriesSchemes.AddString("fbshareextension");
            // Line接入配置
            LSApplicationQueriesSchemes.AddString("lineauth");
            LSApplicationQueriesSchemes.AddString("line3rdp.$(APP_IDENTIFIER)");
            LSApplicationQueriesSchemes.AddString("line");
            #endregion


            // Capabilitise添加
            //var entitlementsFileName = "tw.entitlements";
            //var entitlementsFilePath = Path.Combine("Assets/Plugins/iOS/SDK/", entitlementsFileName);
            //File.Copy(entitlementsFilePath, Path.Combine(path, entitlementsFileName));
            //proj.AddFileToBuild(target, proj.AddFile(entitlementsFileName, entitlementsFileName, PBXSourceTree.Source));


            proj.AddCapability(target, PBXCapabilityType.GameCenter);
            proj.AddCapability(target, PBXCapabilityType.InAppPurchase);
            //添加推送和其他的有点不一样，需要添加一个文件。这个文件只能考进去。
            //或者事先准备好了Base.entitlements 文件，文件类容 就是手动添加进去的内容，手动添加完成后生成的那个文件
            //proj.AddCapability(target, PBXCapabilityType.PushNotifications, entitlementsFileName);

            //ProjectCapabilityManager projectCapabilityManager = new ProjectCapabilityManager(projPath, "tw.entitlements", PBXProject.GetUnityTargetName());
            //projectCapabilityManager.AddGameCenter();
            //projectCapabilityManager.AddInAppPurchase();
            plist.WriteToFile(plistPath);
            proj.WriteToFile(projPath);
            Debug.Log("--**--4.IOSXcodeSettings--**--");
        }




        [MenuItem("出包设置/IOS/S.一键打IOS测试包")]
        public static void IOSTestBuild()
        {
            IOSPlayerSettings();
            IOSGameSettings();
            IOSBuildPlayer();
            if (IOSBuildPlayer())
            {
                IOSXcodeSettings();
            }
            Debug.Log("--**--S.一键打IOS测试包--**--");
        }

        [MenuItem("出包设置/还原默认配置")]
        public static void IOSRebackSettings()
        {
            SDKParams sdkParams = AssetDatabase.LoadAssetAtPath<SDKParams>("Assets/Resources/SDKParams.asset");
            sdkParams.SDKType = SDKType.None;
            EditorUtility.SetDirty(sdkParams);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("--**--还原默认配置--**--");
        }
        public static void IOSbuildApple()
        {
            /*下面的添加进info.list中
             <key> NSAppTransportSecurity</ key >
                  < dict >
                      < key > NSAllowsArbitraryLoads </ key >
                      < true />
                      < key > NSExceptionDomains </ key >
                      < dict >
                          < key > mydomain.com </ key >
                          < dict >
                              < key > NSIncludesSubdomains </ key >
                              < true />
                              < key > NSTemporaryExceptionAllowsInsecureHTTPLoads </ key >
                              < true />
                              < key > NSTemporaryExceptionMinimumTLSVersion </ key >
                              < string > TLSv1.1 </ string >
                                 < key > NSExceptionRequiresForwardSecrecy </ key >
                                 < false />
                                 < key > NSRequiresCertificateTransparency </ key >
                                 < false />
                             </ dict >
                        </ dict >
                     </ dict >
            */
        }
        //#endif
    }

}