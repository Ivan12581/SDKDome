using UnityEngine;
#if UNITY_EDITOR_OSX
using UnityEditor.iOS.Xcode;
#endif
using System.IO;
using System.Linq;
using UnityEditor;
using System;
public class IosSDKSetting : Editor{
    #if UNITY_EDITOR_OSX
    public const string path = "/Users/mini/Documents/EmptyXcodeprj";
    [MenuItem("IosSDKSetting/FaceBook配置")]
    public static void IOSbuildFaceBook()
    {
        //添加XCode引用的Framework
        string projPath = PBXProject.GetPBXProjectPath(path);
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));
        string target = proj.TargetGuidByName("Unity-iPhone");
        //添加苹果自带功能
        // proj.AddCapability(target, PBXCapabilityType.GameCenter);
        // proj.AddCapability(target, PBXCapabilityType.InAppPurchase);
        // proj.AddCapability(target, PBXCapabilityType.InAppPurchase);
        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        PlistElementDict rootDict = plist.root;
        rootDict.SetString("FacebookAppID", "949004278872387");
        rootDict.SetString("FacebookAppDisplayName", "Girl for the Throne TW");
        // URL types配置
        PlistElementArray URLTypes = plist.root.CreateArray("CFBundleURLTypes");
        string[] urlSchemes = { "fb949004278872387" };
        foreach (string str in urlSchemes)
        {
            PlistElementDict typeRole = URLTypes.AddDict();
            // typeRole.SetString("CFBundleTypeRole", "Editor");
            PlistElementArray urlScheme = typeRole.CreateArray("CFBundleURLSchemes");
            urlScheme.AddString(str);
        }
        // LSApplicationQueriesSchemes配置
        PlistElementArray LSApplicationQueriesSchemes = plist.root.CreateArray("LSApplicationQueriesSchemes");
        // facebook接入配置为了适配ios9
        LSApplicationQueriesSchemes.AddString("fbapi");
        LSApplicationQueriesSchemes.AddString("fb-messenger-share-api");
        LSApplicationQueriesSchemes.AddString("fbauth2");
        LSApplicationQueriesSchemes.AddString("fbshareextension");
        plist.WriteToFile(plistPath);
    }
    [MenuItem("IosSDKSetting/Google配置")]
    public static void IOSbuildGoogle()
    {
        //添加XCode引用的Framework
        string projPath = PBXProject.GetPBXProjectPath(path);
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));
        string target = proj.TargetGuidByName("Unity-iPhone");
        // SDK依赖
        proj.AddFrameworkToProject(target, "LocalAuthentication.framework", false);
        proj.AddFrameworkToProject(target, "SafariServices.framework", false);
        proj.AddFrameworkToProject(target, "AuthenticationServices.framework", false);
        proj.AddFrameworkToProject(target, "SystemConfiguration.framework", false);
        // BuildSetting修改
        proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
        proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");
        proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-all_load");

        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        PlistElementDict rootDict = plist.root;
        // 调整默认配置
        rootDict.SetString("CFBundleDevelopmentRegion", "zh_CN");
        // URL types配置
        PlistElementArray URLTypes = plist.root.CreateArray("CFBundleURLTypes");
        string[] urlSchemes = { "com.googleusercontent.apps.554619719418-rtqb4au05hj99h8h6n70i6b8i3d91tun" };
        foreach (string str in urlSchemes)
        {
            PlistElementDict typeRole = URLTypes.AddDict();
            typeRole.SetString("CFBundleTypeRole", "Editor");
            PlistElementArray urlScheme = typeRole.CreateArray("CFBundleURLSchemes");
            urlScheme.AddString(str);
        }
        // LSApplicationQueriesSchemes配置
        //PlistElementArray LSApplicationQueriesSchemes = plist.root.CreateArray("LSApplicationQueriesSchemes");
        plist.WriteToFile(plistPath);
    }
    [MenuItem("IosSDKSetting/Apple配置")]
    public static void IOSbuildApple()
    {
        //添加XCode引用的Framework
        string projPath = PBXProject.GetPBXProjectPath(path);
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));
        string target = proj.TargetGuidByName("Unity-iPhone");
        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        PlistElementDict rootDict = plist.root;

        /* iOS9所有的app对外http协议默认要求改成https */
        // Add value of NSAppTransportSecurity in Xcode plist
        PlistElementDict dictTmp = rootDict.CreateDict("NSAppTransportSecurity");
        dictTmp.SetBoolean("NSAllowsArbitraryLoads", true);

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
            plist.WriteToFile(plistPath);

    }


#endif
}

