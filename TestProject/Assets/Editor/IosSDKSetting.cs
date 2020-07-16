using UnityEngine;
//#if UNITY_EDITOR_OSX
using UnityEditor.iOS.Xcode;
//#endif
using System.IO;
using System.Linq;
using UnityEditor;
using System;
public class IosSDKSetting : Editor{
    //#if UNITY_EDITOR_OSX
    public const string path = "/Users/mini/Documents/SDKXCodeProj";
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

    [MenuItem("IosSDKSetting/IOSXcode配置")]
    public static void IOSbuildFBGP()
    {

        //添加XCode引用的Framework
        string projPath = PBXProject.GetPBXProjectPath(path);
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));
        string target = proj.TargetGuidByName("Unity-iPhone");
    #region 添加XCode引用的Framework
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
        // BuildSetting修改
        proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
        proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");
        proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-all_load");

        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        PlistElementDict rootDict = plist.root;
    #region 修改Xcode工程Info.plist
        /* iOS9所有的app对外http协议默认要求改成https */
        // Add value of NSAppTransportSecurity in Xcode plist
        PlistElementDict dictTmp = rootDict.CreateDict("NSAppTransportSecurity");
        dictTmp.SetBoolean("NSAllowsArbitraryLoads", true);
        // 权限配置
        //rootDict.SetString("NSCameraUsageDescription", "是否允许访问相机?");
        //rootDict.SetString("NSMicrophoneUsageDescription", "是否允许使用麦克风?");
        //rootDict.SetString("NSPhotoLibraryAddUsageDescription", "是否允许添加照片?");
        //rootDict.SetString("NSMicrophoneUsageDescription", "是否允许访问相册?");
        rootDict.SetString("CFBundleDevelopmentRegion", "zh_TW");
        rootDict.SetString("CFBundleVersion", "1");
        // SDK相关参数设置
        rootDict.SetString("FacebookAppID", "949004278872387");
        rootDict.SetString("GoogleClientID", "554619719418-0hdrkdprcsksigpldvtr9n5lu2lvt5kn.apps.googleusercontent.com");
        rootDict.SetString("FacebookAppDisplayName", "Girl for the Throne TW");
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
        //LSApplicationQueriesSchemes.AddString("lineauth");
        //LSApplicationQueriesSchemes.AddString("line3rdp.$(APP_IDENTIFIER)");
        //LSApplicationQueriesSchemes.AddString("line");
        #endregion


        // Capabilitise添加
        //var entitlementsFileName = "tw.entitlements";
        //var entitlementsFilePath = Path.Combine("Assets/Plugins/iOS/SDK/", entitlementsFileName);
        //File.Copy(entitlementsFilePath, Path.Combine(path, entitlementsFileName));
        //proj.AddFileToBuild(target, proj.AddFile(entitlementsFileName, entitlementsFileName, PBXSourceTree.Source));


        //var array = rootDict.CreateArray("UIRequiredDeviceCapabilities");
        //array.AddString("armv7");
        //array.AddString("gamekit");
        proj.AddCapability(target, PBXCapabilityType.GameCenter);
        proj.AddCapability(target, PBXCapabilityType.InAppPurchase);
        //添加推送和其他的有点不一样，需要添加一个文件。这个文件只能考进去。
        //或者事先准备好了Base.entitlements 文件，文件类容 就是手动添加进去的内容，手动添加完成后生成的那个文件
        //proj.AddCapability(target, PBXCapabilityType.PushNotifications, entitlementsFileName);


        plist.WriteToFile(plistPath);
        proj.WriteToFile(projPath);
    }

    //#endif
}

