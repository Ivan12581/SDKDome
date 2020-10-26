using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace celia.game.editor
{
    public class SetProjectSetting : CommonAction
    {
        public override void PreExcute(CeliaBuildOption option)
        {
            // 应用名
            string productName = CeliaBuilder.GetInputParam("ProductName:", option.Args);
            PlayerSettings.productName = string.IsNullOrEmpty(productName) ? "少女的王座" : productName;
            // 公司名
            string companyName = CeliaBuilder.GetInputParam("CompanyName:", option.Args);
            PlayerSettings.companyName = string.IsNullOrEmpty(companyName) ? "Rastar Games Inc." : companyName;

            // 关闭Unity自带的Splash
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            // 默认包名
            PlayerSettings.SetApplicationIdentifier(option.PlayerOption.targetGroup, "com.xlycs.rastar");

            // ==构建平台==
            option.PlayerOption.target = option.ProcessCfg.Target;
            option.PlayerOption.targetGroup = BuildPipeline.GetBuildTargetGroup(option.PlayerOption.target);
            // ====SDK版本===
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // 强制使用OPENGL ES3
            PlayerSettings.SetGraphicsAPIs(option.PlayerOption.target,new GraphicsDeviceType[] 
            {
                UnityEngine.Rendering.GraphicsDeviceType.Vulkan,
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3
            });

            // ScriptingRuntimeVersion
            PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest;
            PlayerSettings.SetScriptingBackend(option.PlayerOption.targetGroup, ScriptingImplementation.IL2CPP);

            bool isBetaVersion = option.ReleaseLevel == ReleaseLevel.Beta;
            // 设置Il2CppCompilerConfiguration
            Il2CppCompilerConfiguration il2CppCfg = isBetaVersion ? Il2CppCompilerConfiguration.Release : Il2CppCompilerConfiguration.Debug;
            PlayerSettings.SetIl2CppCompilerConfiguration(option.PlayerOption.targetGroup, il2CppCfg);

            Debug.Log("SetProjectSetting PreExcuted!");
        }

        public override void PostExcute(CeliaBuildOption option)
        {
            
            Debug.Log("SetProjectSetting PostExcuted!");
        }
    }
}
