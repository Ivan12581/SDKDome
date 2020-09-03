using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

using UnityEngine;
namespace celia.game.editor
{
    public class PostProcessBuild
    {
        [PostProcessBuild(100)]
        public static void OnPostProcessBuild(BuildTarget _target, string pathToBuildProject)
        {
            Debug.Log("---_target--->" + _target + "---pathToBuildProject--->" + pathToBuildProject);
            if (_target == BuildTarget.iOS)
            {
                string projPath = PBXProject.GetPBXProjectPath(pathToBuildProject);
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

                string plistPath = Path.Combine(pathToBuildProject, "Info.plist");
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
                // Capabilitise添加
                proj.AddCapability(target, PBXCapabilityType.GameCenter);
                proj.AddCapability(target, PBXCapabilityType.InAppPurchase);
                //ProjectCapabilityManager projectCapabilityManager = new ProjectCapabilityManager(projPath, "tw.entitlements", PBXProject.GetUnityTargetName());
                //projectCapabilityManager.AddGameCenter();
                //projectCapabilityManager.AddInAppPurchase();
                plist.WriteToFile(plistPath);
                proj.WriteToFile(projPath);
                Debug.Log("--**--4.IOSXcodeSettings--**--");
            }
            if (_target == BuildTarget.Android)
            {

            }
        }
    }
}

