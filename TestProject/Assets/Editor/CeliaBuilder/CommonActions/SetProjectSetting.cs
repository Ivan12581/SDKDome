using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace celia.game.editor
{
    public class SetProjectSetting : CommonAction
    {
        private readonly string logo1_Path = @"Assets/Res/Icons/Splash/公司logo.png";
        private readonly string logo2_Path = @"Assets/Res/Icons/Splash/星辉logo.png";
        private readonly string splash_Path = @"Assets/Res/Icons/Splash/公司splash.jpg";
        private BuildTargetGroup buildTargetGroup;

        public override void PreExcute(CeliaBuildOption option)
        {
            buildTargetGroup = BuildPipeline.GetBuildTargetGroup(option.ProcessCfg.Target);
            SetProSetting(option);
            SetDefineSymbols(option);
            SetSplash(option);
            SetOutputPath(option);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("SetProjectSetting PreExcuted!");
        }

        private void SetProSetting(CeliaBuildOption option)
        {
            // 应用名
            string productName = CeliaBuilder.GetInputParam("ProductName:", option.Args);
            PlayerSettings.productName = string.IsNullOrEmpty(productName) ? "少女的王座" : productName;
            // 公司名
            string companyName = CeliaBuilder.GetInputParam("CompanyName:", option.Args);
            PlayerSettings.companyName = string.IsNullOrEmpty(companyName) ? "Rastar Games Inc." : companyName;

            // 默认包名
            PlayerSettings.SetApplicationIdentifier(option.PlayerOption.targetGroup, "com.xlycs.rastar");

            // ==构建平台==
            option.PlayerOption.target = option.ProcessCfg.Target;
            option.PlayerOption.targetGroup = BuildPipeline.GetBuildTargetGroup(option.PlayerOption.target);

            // 强制使用OPENGL ES3
            PlayerSettings.SetGraphicsAPIs(option.PlayerOption.target, new GraphicsDeviceType[]
            {
                GraphicsDeviceType.Vulkan,
                GraphicsDeviceType.OpenGLES3
            });

            // ScriptingRuntimeVersion
            PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest;
            PlayerSettings.SetScriptingBackend(option.PlayerOption.targetGroup, ScriptingImplementation.IL2CPP);

            bool isBetaVersion = option.ReleaseLevel == ReleaseLevel.Beta;
            // 设置Il2CppCompilerConfiguration
            Il2CppCompilerConfiguration il2CppCfg = isBetaVersion ? Il2CppCompilerConfiguration.Release : Il2CppCompilerConfiguration.Debug;
            PlayerSettings.SetIl2CppCompilerConfiguration(option.PlayerOption.targetGroup, il2CppCfg);
        }

        private void SetDefineSymbols(CeliaBuildOption option)
        {
            string newDefines = "HOTFIX_ENABLE;AOT";
            if (option.ReleaseLevel == ReleaseLevel.Beta)
            {
                newDefines += ";CELIA_RELEASE";
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
        }

        private void SetOutputPath(CeliaBuildOption option)
        {
            string outputPath = CeliaBuilder.GetInputParam("Path:", option.Args);

            EditorUserBuildSettings.exportAsGoogleAndroidProject = option.OutputProject;
            // iOS不能成包
            if (option.OutputProject || option.ProcessCfg.Target == UnityEditor.BuildTarget.iOS)
            {
                if (!string.IsNullOrEmpty(outputPath))
                {
                    option.PlayerOption.locationPathName = outputPath;
                }
                else
                {
                    option.PlayerOption.locationPathName = $"Outputs/{option.ProcessCfg.Target}/{CeliaBuilder.ApkName}_Project";
                }
            }
            else
            {
                string suffix = option.ProcessCfg.Target == UnityEditor.BuildTarget.Android ? ".apk" : "";
                if (!string.IsNullOrEmpty(outputPath))
                {
                    option.PlayerOption.locationPathName = $"{outputPath}{suffix}";
                }
                else
                {
                    option.PlayerOption.locationPathName = $"Outputs/{option.ProcessCfg.Target}/{CeliaBuilder.ApkName}{suffix}";
                }
            }
        }

        private void SetSplash(CeliaBuildOption option)
        {
            // 关闭Unity自带的Splash
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            if (option.SDKType == SDKType.Native)
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

        private void SetLogos(bool show)
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

        private void SetStaticSplash(bool Show)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(splash_Path);

            //加载ProjectSettings
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            UnityEngine.Object obj = AssetDatabase.LoadAllAssetsAtPath(projectSettingsPath)[0];
            SerializedObject psObj = new SerializedObject(obj);
            //获取到androidSplashScreen Property
            string propertyPath = buildTargetGroup == BuildTargetGroup.Android ? "androidSplashScreen.m_FileID" : "iPhoneSplashScreen.m_FileID";

            SerializedProperty SplashFileId = psObj.FindProperty(propertyPath);
            if (SplashFileId != null)
            {
                SplashFileId.intValue = Show ? tex.GetInstanceID() : 0;
            }
            //保存修改
            psObj.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (buildTargetGroup == BuildTargetGroup.Android)
            {
                PlayerSettings.Android.splashScreenScale = AndroidSplashScreenScale.ScaleToFill;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public override void PostExcute(CeliaBuildOption option)
        {
            Debug.Log("SetProjectSetting PostExcuted!");
        }
    }
}