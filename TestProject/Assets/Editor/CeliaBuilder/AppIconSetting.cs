using UnityEngine;
using UnityEditor;
using System.IO;

public class AppIconSetting : Editor
{
    private static readonly string Icon_Path = @"Assets\Res\Icons\{0}\{1}\{2}.png";

    [MenuItem("IconSet/Android/Adaptive")]
    public static void SetAndroidAdaptive()
    {
        SetIcons(BuildTargetGroup.Android, UnityEditor.Android.AndroidPlatformIconKind.Adaptive);
    }

    [MenuItem("IconSet/Android/Round")]
    public static void SetAndroidRound()
    {
        SetIcons(BuildTargetGroup.Android, UnityEditor.Android.AndroidPlatformIconKind.Round);
    }

    [MenuItem("IconSet/Android/Legacy")]
    public static void SetAndroidLegacy()
    {
        SetIcons(BuildTargetGroup.Android, UnityEditor.Android.AndroidPlatformIconKind.Legacy);
    }

    [MenuItem("IconSet/iOS/Application")]
    public static void SetiOSApplication()
    {
        SetIcons(BuildTargetGroup.iOS, UnityEditor.iOS.iOSPlatformIconKind.Application);
    }

    [MenuItem("IconSet/iOS/Spotlight")]
    public static void SetiOSSpotlight()
    {
        SetIcons(BuildTargetGroup.iOS, UnityEditor.iOS.iOSPlatformIconKind.Spotlight);
    }

    [MenuItem("IconSet/iOS/Settings")]
    public static void SetiOSSettings()
    {
        SetIcons(BuildTargetGroup.iOS, UnityEditor.iOS.iOSPlatformIconKind.Settings);
    }

    [MenuItem("IconSet/iOS/Notifications")]
    public static void SetiOSNotifications()
    {
        SetIcons(BuildTargetGroup.iOS, UnityEditor.iOS.iOSPlatformIconKind.Notification);
    }

    [MenuItem("IconSet/iOS/Marketing")]
    public static void SetiOSMarketing()
    {
        SetIcons(BuildTargetGroup.iOS, UnityEditor.iOS.iOSPlatformIconKind.Marketing);
    }

    private static Texture2D[] GetIconsFromAsset(BuildTargetGroup target, PlatformIconKind kind, PlatformIcon[] icons)
    {
        Texture2D[] texArray = new Texture2D[icons.Length];

        //因为Android设置会带有" API (xx)"等附加信息，为了文件夹不出现空格，只取空格前单词
        string folder = kind.ToString().Split(' ')[0];
        string filename;
        for (int i = 0; i < texArray.Length; ++i)
        {
            //不需要再通过GetIconSizesForTargetGroup了来获得Icon尺寸数组，
            //直接由对应的PlatformIcon.width来获取Icon大小
            int iconSize = icons[i].width;
            filename = string.Format(Icon_Path, target, folder, iconSize);
            if (!File.Exists(filename))
            {
                Debug.LogErrorFormat("图片文件不存在, 路径为:{0}", filename);
                continue;
            }
            Texture2D tex2D = AssetDatabase.LoadAssetAtPath(filename,
                typeof(Texture2D)) as Texture2D;
            texArray[i] = tex2D;
        }
        return texArray;
    }

    private static void SetIcons(BuildTargetGroup platform, PlatformIconKind kind)
    {
        //获得当前平台和当前Icon类型的PlatformIcon数组
        PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(platform, kind);

        //将Asset转为Texture2D
        Texture2D[] iconSources = GetIconsFromAsset(platform, kind, icons);

        for (int i = 0, length = icons.Length; i < length; ++i)
        {
            icons[i].SetTexture(iconSources[i]);
        }

        PlayerSettings.SetPlatformIcons(platform, kind, icons);

        AssetDatabase.SaveAssets();
        Debug.LogFormat("Set {0}/{1} Icon Complete", platform, kind);
    }
}