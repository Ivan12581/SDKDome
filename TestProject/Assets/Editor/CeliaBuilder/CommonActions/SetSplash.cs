using UnityEditor;

using UnityEngine;

namespace celia.game.editor
{
    public class SetSplash : CommonAction
    {
        private  readonly string logo1_Path = @"Assets/Res/Icons/Splash/公司logo.png";
        private  readonly string logo2_Path = @"Assets/Res/Icons/Splash/星辉logo.png";
        private  readonly string splash_Path = @"Assets/Res/Icons/Splash/公司splash.jpg";
        private CeliaBuildOption celiaBuildOption;

        public override void PreExcute(CeliaBuildOption option)
        {
            celiaBuildOption = option;

            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            if (celiaBuildOption.SDKType == SDKType.Native)
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

        public void SetLogos(bool show)
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

        public void SetStaticSplash(bool Show)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(splash_Path);

            //加载ProjectSettings
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            UnityEngine.Object obj = AssetDatabase.LoadAllAssetsAtPath(projectSettingsPath)[0];
            SerializedObject psObj = new SerializedObject(obj);
            //获取到androidSplashScreen Property
            string propertyPath = celiaBuildOption.PlayerOption.target == BuildTarget.Android ? "androidSplashScreen.m_FileID" : "iPhoneSplashScreen.m_FileID";
            SerializedProperty SplashFileId = psObj.FindProperty(propertyPath);
            if (SplashFileId != null)
            {
                SplashFileId.intValue = Show ? tex.GetInstanceID() : 0;
            }
            //保存修改
            psObj.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (celiaBuildOption.PlayerOption.target == BuildTarget.Android)
            {
                PlayerSettings.Android.splashScreenScale = AndroidSplashScreenScale.ScaleToFill;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public override void PostExcute(CeliaBuildOption option)
        {
        }
    }
}