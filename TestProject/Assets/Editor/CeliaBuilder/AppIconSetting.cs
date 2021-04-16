using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class AppIconSetting : Editor
{
    private static readonly string logo1_Path = @"Assets/Res/Icons/Splash/公司logo.png";
    private static readonly string logo2_Path = @"Assets/Res/Icons/Splash/星辉logo.png";
    private static readonly string splash_Path = @"Assets/Res/Icons/Splash/公司splash.jpg";
    [MenuItem("IconSet/SetSplashImages")]
    public static void SetSplashImages()
    {
        PlayerSettings.SplashScreen.show = true;
        PlayerSettings.SplashScreen.showUnityLogo = false;
        SDKParams sdkParams = AssetDatabase.LoadAssetAtPath<SDKParams>("Assets/Resources/SDKParams.asset");
        if (sdkParams.SDKType == celia.game.SDKType.Native)
        {
            SetLogos(true);
            SetStaticSplash(false);
        }
        else
        {
            SetLogos(false);
            SetStaticSplash(true);
        }
    }

    public static void SetLogos(bool show)
    {
        if (!show)
        {
            PlayerSettings.SplashScreen.logos = null;
            PlayerSettings.SplashScreen.backgroundColor = Color.black;
            return;
        }

        //设置闪屏背景颜色
        PlayerSettings.SplashScreen.backgroundColor = Color.white;
        var logo1 = new PlayerSettings.SplashScreenLogo
        {
            duration = 2f,
            //设置闪屏logo
            logo = AssetDatabase.LoadAssetAtPath<Sprite>(logo1_Path)
        };
        var logo2 = new PlayerSettings.SplashScreenLogo
        {
            duration = 2f,
            //设置闪屏logo
            logo = AssetDatabase.LoadAssetAtPath<Sprite>(logo2_Path)
        };
        PlayerSettings.SplashScreen.logos = new PlayerSettings.SplashScreenLogo[2] { logo1, logo2 };
    }

    public static void SetStaticSplash(bool Show)
    {
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(splash_Path);

        //加载ProjectSettings
        string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
        UnityEngine.Object obj = AssetDatabase.LoadAllAssetsAtPath(projectSettingsPath)[0];
        SerializedObject psObj = new SerializedObject(obj);
        //获取到androidSplashScreen Property
        string propertyPath = "";
#if UNITY_ANDROID
        propertyPath = "androidSplashScreen.m_FileID";
#endif
#if UNITY_IOS
        propertyPath = "iPhoneSplashScreen.m_FileID";
#endif
        SerializedProperty SplashFileId = psObj.FindProperty(propertyPath);
        if (SplashFileId != null)
        {
            SplashFileId.intValue = Show ? tex.GetInstanceID() : 0;
        }
        //保存修改
        psObj.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#if UNITY_ANDROID
        PlayerSettings.Android.splashScreenScale = AndroidSplashScreenScale.ScaleToFill;
#endif
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    #region IconSet

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
            Debug.Log("iconSize:" + iconSize);
            filename = string.Format(Icon_Path, target, folder, iconSize);
            if (!File.Exists(filename))
            {
                Debug.LogErrorFormat("图片文件不存在, 1路径为:{0}", filename);
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

    [MenuItem("IconSet/Test")]
    public static void test()
    {
        string path = "Assets/Res/Icons/Android/1024.png";
        Texture2D tex_1024 = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        int[] newTex = new int[] { 512,256,128,64,32};
        for (int i = 0; i < newTex.Length; i++)
        {
            int size = newTex[i];
            Texture2D tex_new = ReSetTextureSize(tex_1024, size, size);
            SaveTexture(tex_new, "Assets/Res/Icons/IOS/"+ size+".png");
        }

    }
    public static Texture2D ReSetTextureSize(Texture2D tex, int width, int height)
    {
        var rendTex = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        rendTex.Create();
        Graphics.SetRenderTarget(rendTex);
        GL.PushMatrix();
        GL.Clear(true, true, Color.clear);
        GL.PopMatrix();

        var mat = new Material(Shader.Find("Unlit/Transparent"));
        mat.mainTexture = tex;
        Graphics.SetRenderTarget(rendTex);
        GL.PushMatrix();
        GL.LoadOrtho();
        mat.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.TexCoord2(0, 0);
        GL.Vertex3(0, 0, 0);
        GL.TexCoord2(0, 1);
        GL.Vertex3(0, 1, 0);
        GL.TexCoord2(1, 1);
        GL.Vertex3(1, 1, 0);
        GL.TexCoord2(1, 0);
        GL.Vertex3(1, 0, 0);
        GL.End();
        GL.PopMatrix();

        var finalTex = new Texture2D(rendTex.width, rendTex.height, TextureFormat.ARGB32, false);
        RenderTexture.active = rendTex;
        finalTex.ReadPixels(new Rect(0, 0, finalTex.width, finalTex.height), 0, 0);
        finalTex.Apply();
        return finalTex;
    }
    public static void SaveTexture(Texture2D tex, string toPath)
    {
        using (var fs = File.OpenWrite(toPath))
        {
            var bytes = tex.EncodeToPNG();
            fs.Write(bytes, 0, bytes.Length);
        }
    }
    #endregion IconSet
}